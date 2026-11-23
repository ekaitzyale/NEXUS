namespace WindowsForms;

internal enum CombatAction { Attack, Defend, Reserve, UseKit }

internal sealed record TurnResult(
    IReadOnlyList<CombatAction> Player, IReadOnlyList<CombatAction> Enemy,
    int DamageDealt, int DamageTaken, int Healing, int PlayerBlocks, int EnemyBlocks, int NextPlayerPa, int NextEnemyPa);

internal sealed class CombatTurn(int hp, int kits, int charges, int enemyMaxHp = 40, int enemyDamage = 10)
{
    public const int BasePa = 3;
    public const int MaxPa = 10;
    public const int PlayerDamage = 20;
    public const int KitHealing = 30;

    private readonly List<CombatAction> actions = new();
    private int enemyPa = BasePa;
    private TurnResult? result;
    public IReadOnlyList<CombatAction> Actions => actions;
    public int Hp { get; private set; } = hp;
    public int Kits { get; private set; } = kits;
    public int Charges { get; private set; } = charges;
    public int EnemyHp { get; private set; } = enemyMaxHp;
    public int EnemyMaxHp { get; } = enemyMaxHp;
    public int EnemyDamage => enemyDamage;
    public int RemainingPa { get; private set; } = BasePa;
    public int Number { get; private set; } = 1;
    public bool UsedCharge { get; private set; }
    public int SelectableCharges => result is null ? Math.Min(Charges, MaxPa - RemainingPa - actions.Count) : 0;
    public bool CanUseKit => Hp < GameSession.MaxHp && Kits > actions.Count(a => a == CombatAction.UseKit);

    public bool AllocateCharges(int count)
    {
        if (count <= 0 || count > SelectableCharges) return false;
        Charges -= count;
        RemainingPa += count;
        UsedCharge = true;
        return true;
    }

    public bool Choose(CombatAction action)
    {
        if (result is not null || RemainingPa <= 0 || (action == CombatAction.UseKit && !CanUseKit)) return false;
        actions.Add(action);
        RemainingPa--;
        return true;
    }

    public void Undo()
    {
        if (result is not null || actions.Count == 0) return;
        actions.RemoveAt(actions.Count - 1);
        RemainingPa++;
    }

    public TurnResult Resolve(Random random)
    {
        if (result is not null || RemainingPa != 0) throw new InvalidOperationException();
        int attacks = actions.Count(a => a == CombatAction.Attack);
        int defenses = actions.Count(a => a == CombatAction.Defend);
        int reserved = actions.Count(a => a == CombatAction.Reserve);
        int usedKits = actions.Count(a => a == CombatAction.UseKit);
        int healing = Math.Min(GameSession.MaxHp - Hp, usedKits * KitHealing);
        Hp += healing;
        Kits -= usedKits;

        var enemyActions = new List<CombatAction>();
        for (int i = 0; i < enemyPa; i++)
        {
            int roll = random.Next(100);
            if (EnemyHp <= PlayerDamage && attacks > 0 && roll < 65)
                enemyActions.Add(CombatAction.Defend);
            else if (defenses >= 2 && roll < 45)
                enemyActions.Add(CombatAction.Reserve);
            else if (roll < 62)
                enemyActions.Add(CombatAction.Attack);
            else if (roll < 82)
                enemyActions.Add(CombatAction.Defend);
            else
                enemyActions.Add(CombatAction.Reserve);
        }

        int enemyAttacks = enemyActions.Count(a => a == CombatAction.Attack);
        int playerBlocks = Math.Min(enemyAttacks, defenses);
        int enemyBlocks = Math.Min(attacks, enemyActions.Count(a => a == CombatAction.Defend));
        int dealt = Math.Min(EnemyHp, (attacks - enemyBlocks) * PlayerDamage);
        int received = Math.Min(Hp, (enemyAttacks - playerBlocks) * EnemyDamage);
        EnemyHp -= dealt;
        Hp -= received;

        result = new TurnResult(actions.ToArray(), enemyActions.AsReadOnly(), dealt, received, healing,
            playerBlocks, enemyBlocks, Math.Min(MaxPa, BasePa + reserved), BasePa + enemyActions.Count(a => a == CombatAction.Reserve));
        return result;
    }

    public void Next()
    {
        if (result is null || Hp == 0 || EnemyHp == 0) throw new InvalidOperationException();
        RemainingPa = result.NextPlayerPa;
        enemyPa = result.NextEnemyPa;
        Number++;
        actions.Clear();
        result = null;
    }
}
