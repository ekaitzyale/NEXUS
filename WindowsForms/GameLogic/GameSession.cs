using System.Text.Json.Serialization;

namespace WindowsForms;

internal sealed record PhaseResources(int Hp, int Kits, int Charges);
internal sealed record RoomEnemy(string Id, Point Position, int MaxHp = 40, int Damage = 10)
{
    public string Name => Id == "guardian-01" ? "GUARDIÁN-01" : "ROBOT DE SEGURIDAD";
}

internal sealed class RoomProgress
{
    public HashSet<string> Objectives { get; init; } = new(StringComparer.Ordinal);
    public HashSet<string> DefeatedEnemies { get; init; } = new(StringComparer.Ordinal);
    public HashSet<string> ReadDocuments { get; init; } = new(StringComparer.Ordinal);
}

internal sealed class GameSession
{
    public const int MaxHp = 100;
    public const int MaxKits = 5;
    public const int InitialCharges = 6;
    public static readonly Point Spawn = new(1, 3);
    public static readonly Point Exit = new(7, 3);
    public static readonly Point Kit = new(4, 3);
    public static readonly Point Terminal = new(3, 1);
    public static readonly Point Device = new(5, 3);
    public static readonly Point Generator = new(5, 2);

    public required Guid Id { get; init; }
    public required string Name { get; init; }
    [JsonInclude] public int Phase { get; private set; }
    [JsonInclude] public PhaseResources? EntryResources { get; private set; }
    public int Room { get; private set; } = 1;
    public int X { get; private set; } = Spawn.X;
    public int Y { get; private set; } = Spawn.Y;
    public int Hp { get; private set; } = MaxHp;
    public int Kits { get; private set; }
    public int Charges { get; private set; } = InitialCharges;
    public HashSet<int> CompletedPhases { get; init; } = new();
    public HashSet<string> Flags { get; init; } = new(StringComparer.Ordinal);
    public HashSet<string> SeenEvents { get; init; } = new(StringComparer.Ordinal);
    public List<string> PendingEvents { get; init; } = new();
    public Dictionary<int, RoomProgress> Rooms { get; init; } = new();

    [JsonConstructor]
    public GameSession(int room = 1, int x = 1, int y = 3, int hp = MaxHp, int kits = 0, int charges = InitialCharges)
    {
        Room = room;
        X = x;
        Y = y;
        Hp = hp;
        Kits = kits;
        Charges = charges;
    }

    [JsonIgnore] public Point Position => new(X, Y);
    [JsonIgnore] public bool TutorialChargeUsed => Flags.Contains("tutorial-charge");
    [JsonIgnore] public bool PhaseComplete => CompletedPhases.Contains(Phase);
    public bool IsUnlocked(int phase) => phase == 0 || (CompletedPhases.Contains(phase - 1) && (phase != 2 || ChipCollected));
    [JsonIgnore] public bool HasKit => Room is 1 or 5 or 9 or 12 or 14 or 16;
    [JsonIgnore] public bool HasTerminal => Room is 1 or >= 4 and <= 18;
    [JsonIgnore] public bool HasCircuit => Room is 3 or 6 or 10 or 14;
    [JsonIgnore] public Point CircuitPosition => Room == 6 ? new(5, 2) : Device;
    public static readonly Point Chip = new(5, 3);
    [JsonIgnore] public bool KitCollected => HasObjective(Room, "kit");
    [JsonIgnore] public bool CircuitRestored => HasObjective(Room, "circuit");
    [JsonIgnore] public bool ChipCollected => HasObjective(7, "chip");
    [JsonIgnore] public bool RequiresTutorialCharge => Room == 2;
    [JsonIgnore] public bool CanExit => CanLeave(Room);
    public const int LastPlayablePhase = 3;
    public static int FirstRoom(int phase) => phase switch { 0 => 1, 1 => 4, 2 => 8, 3 => 14, _ => 19 };
    private int LastRoom => FirstRoom(Phase + 1) - 1;

    private static readonly RoomEnemy[] SecurityEnemies = [new("security-08-1", Device), new("security-08-2", new(5, 5))];
    private static readonly RoomEnemy[] LaboratoryEnemies =
        [new("security-11-1", Device), new("security-11-2", new(5, 5)), new("security-11-3", new(5, 1))];
    private static readonly RoomEnemy[] GeneratorEnemies = [new("security-15", new(5, 5))];
    private static readonly RoomEnemy[] GuardianEnemies = [new("guardian-01", Device, MaxHp: 125, Damage: 20)];

    private static IReadOnlyList<RoomEnemy> Enemies(int room) => room switch
    {
        2 or 4 or 6 => [new($"security-{room:00}", Device)],
        8 => SecurityEnemies,
        11 => LaboratoryEnemies,
        15 => GeneratorEnemies,
        17 => GuardianEnemies,
        _ => []
    };

    public bool IsEnemyDefeated(int room, string id) => Rooms.TryGetValue(room, out var progress)
        && progress.DefeatedEnemies.Contains(id);
    public IEnumerable<RoomEnemy> ActiveEnemies() => Enemies(Room).Where(enemy => !IsEnemyDefeated(Room, enemy.Id));
    public RoomEnemy? AdjacentEnemy() => ActiveEnemies().FirstOrDefault(enemy => IsAdjacent(enemy.Position));
    private bool EnemiesDefeated(int room) => Enemies(room).All(enemy => IsEnemyDefeated(room, enemy.Id));
    [JsonIgnore] public bool InvestigationComplete => HasObjective(13, "investigation");
    [JsonIgnore] public bool HasGenerator => Room is 15 or 16;
    [JsonIgnore] public bool GeneratorDisabled => HasObjective(Room, "generator");
    [JsonIgnore] public bool MissionComplete => CompletedPhases.Contains(3);

    private bool CanLeave(int room) => room switch
    {
        1 or 5 or 9 or 12 => HasObjective(room, "kit"),
        2 or 4 or 11 => EnemiesDefeated(room),
        3 or 10 or 14 => HasObjective(room, "circuit"),
        6 => EnemiesDefeated(room) && HasObjective(room, "circuit"),
        7 => ChipCollected,
        8 => ChipCollected && EnemiesDefeated(room),
        13 => InvestigationComplete,
        15 => HasObjective(15, "generator"),
        16 => HasObjective(15, "generator") && HasObjective(16, "generator"),
        17 => EnemiesDefeated(17),
        _ => false
    };

    private RoomProgress CurrentProgress()
    {
        if (!Rooms.TryGetValue(Room, out var progress))
        {
            progress = new RoomProgress();
            Rooms.Add(Room, progress);
        }

        return progress;
    }

    public bool HasObjective(int room, string objective) =>
        Rooms.TryGetValue(room, out var progress) && progress.Objectives.Contains(objective);

    public static bool IsWall(Point point) => point.X < 1 || point.X > 7 || point.Y < 1 || point.Y > 5;
    public bool IsFixture(Point point) => Room switch
    {
        4 => point is { X: 2 or 6, Y: 1 or 5 },
        5 => point is { X: 2, Y: 1 or 2 } or { X: 6, Y: 1 },
        6 => point is { X: 2, Y: 5 } or { X: 6, Y: 1 or 5 },
        7 => point is { X: 2 or 6, Y: 1 or 5 },
        8 or 11 => point is { X: 2 or 6, Y: 1 or 5 },
        9 => point is { X: 2 or 6, Y: 1 or 2 or 5 },
        10 or 13 => point is { X: 2 or 6, Y: 1 or 5 },
        12 => point is { X: 2 or 6, Y: 1 or 2 or 5 },
        >= 14 and <= 18 => point is { X: 2 or 6, Y: 1 or 5 },
        _ => false
    };

    public bool CanStandAt(Point point) => !IsWall(point)
        && !IsFixture(point)
        && !ActiveEnemies().Any(enemy => point == enemy.Position)
        && !(HasTerminal && point == Terminal)
        && !(HasCircuit && point == CircuitPosition)
        && !(Room is 13 or 18 && point == Device)
        && !(HasGenerator && point == Generator);

    public bool IsAdjacent(Point point) => DistanceTo(point) == 1;
    public bool InReach(Point point) => DistanceTo(point) <= 1;
    private int DistanceTo(Point point) => Math.Abs(X - point.X) + Math.Abs(Y - point.Y);

    public void Move(int dx, int dy)
    {
        if (Math.Abs(dx) + Math.Abs(dy) != 1 || PhaseComplete) return;
        var destination = new Point(X + dx, Y + dy);
        if (!CanStandAt(destination)) return;
        X = destination.X;
        Y = destination.Y;
    }

    public void CollectKit()
    {
        if (!HasKit || KitCollected || !InReach(Kit) || Kits >= MaxKits) return;
        CurrentProgress().Objectives.Add("kit");
        Kits++;
    }

    public void ReadDocument(string id) => CurrentProgress().ReadDocuments.Add(id);
    public void RestoreCircuit()
    {
        if (HasCircuit && IsAdjacent(CircuitPosition)) CurrentProgress().Objectives.Add("circuit");
    }

    public void CollectChip()
    {
        if (Room == 7 && InReach(Chip)) CurrentProgress().Objectives.Add("chip");
    }

    public bool Investigate()
    {
        return Room == 13 && IsAdjacent(Device) && CurrentProgress().Objectives.Add("investigation");
    }

    public bool DisableGenerator() => HasGenerator && IsAdjacent(Generator)
        && HasObjective(14, "circuit") && (Room == 15 || HasObjective(15, "generator"))
        && CurrentProgress().Objectives.Add("generator");

    public bool CompleteMission()
    {
        if (Room != 18 || Phase != 3 || MissionComplete || !IsAdjacent(Device)
            || !Enumerable.Range(14, 4).All(CanLeave)) return false;
        CompletedPhases.Add(3);
        return true;
    }

    public void ApplyCombat(CombatTurn combat, RoomEnemy enemy)
    {
        Hp = combat.Hp;
        Kits = combat.Kits;
        Charges = combat.Charges;
        if (RequiresTutorialCharge && combat.UsedCharge) Flags.Add("tutorial-charge");
        if (Hp > 0 && combat.EnemyHp == 0)
        {
            CurrentProgress().Objectives.Add("encounter");
            CurrentProgress().DefeatedEnemies.Add(enemy.Id);
        }
    }

    public bool Advance()
    {
        if (!CanExit || !InReach(Exit) || PhaseComplete) return false;
        if (Room == LastRoom) CompletedPhases.Add(Phase);
        else
        {
            Room++;
            X = Spawn.X;
            Y = Spawn.Y;
        }
        return true;
    }

    public bool StartNextPhase(int phase)
    {
        if (phase != Phase + 1 || phase > LastPlayablePhase || !PhaseComplete || !IsUnlocked(phase)) return false;
        EntryResources = new PhaseResources(Hp, Kits, Charges);
        Phase = phase;
        Room = FirstRoom(phase);
        X = Spawn.X;
        Y = Spawn.Y;
        return true;
    }

    public GameSession RetryPhase()
    {
        if (Phase == 0) return new GameSession { Id = Id, Name = Name };
        var entry = EntryResources ?? throw new InvalidOperationException();
        int firstRoom = FirstRoom(Phase);
        return new GameSession(room: firstRoom, hp: entry.Hp, kits: entry.Kits, charges: entry.Charges)
        {
            Id = Id, Name = Name, Phase = Phase, EntryResources = entry,
            CompletedPhases = CompletedPhases.Where(phase => phase < Phase).ToHashSet(),
            Flags = new HashSet<string>(Flags, StringComparer.Ordinal),
            SeenEvents = SeenEvents.Where(id => Enumerable.Range(0, Phase).Any(phase => id.StartsWith($"JARVIS_F{phase}_", StringComparison.Ordinal))
                || Enumerable.Range(1, firstRoom - 1).Any(room => id.StartsWith($"JARVIS_S{room:00}_", StringComparison.Ordinal)))
                .ToHashSet(StringComparer.Ordinal),
            Rooms = Rooms.Where(item => item.Key < firstRoom).ToDictionary()
        };
    }

    public bool IsValidSave() =>
        Id != Guid.Empty && !string.IsNullOrWhiteSpace(Name) && Name.Length <= 60
        && Phase is >= 0 and <= LastPlayablePhase && Room >= FirstRoom(Phase) && Room <= LastRoom
        && Hp is > 0 and <= MaxHp && Kits is >= 0 and <= MaxKits && Charges is >= 0 and <= InitialCharges
        && Flags is not null && SeenEvents is not null && PendingEvents is not null
        && Rooms is not null && Rooms.All(entry => entry.Key >= 1 && entry.Key <= Room
            && entry.Value is { Objectives: not null, DefeatedEnemies: not null, ReadDocuments: not null })
        && CompletedPhases is not null && CompletedPhases.All(phase => phase >= 0 && phase <= Phase)
        && (Phase == 0 || (Enumerable.Range(0, Phase).All(CompletedPhases.Contains)
            && EntryResources is { Hp: > 0 and <= MaxHp, Kits: >= 0 and <= MaxKits, Charges: >= 0 and <= InitialCharges }))
        && CanStandAt(Position)
        && Enumerable.Range(1, Room - 1).All(CanLeave)
        && (!IsEnemyDefeated(2, "security-02") || TutorialChargeUsed)
        && (!PhaseComplete || (Room == LastRoom && (Phase == 3 ? IsAdjacent(Device) : CanExit && InReach(Exit))));
}
