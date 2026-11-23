using System.Reflection;
using System.Diagnostics;
using WindowsForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.AutoFlush = true;
        CheckCombat();
        CheckPaLimit();
        CheckRoomDistribution();
        CheckNarrative();
        for (int i = 0; i < 20; i++)
        {
            using var puzzle = new CircuitPuzzle();
            Solve(puzzle);
        }

        Console.WriteLine("OK: combate, límite de 10 PA, distribución de salas, narrativa y 20 circuitos generados.");
        string? connection = Environment.GetEnvironmentVariable("NEXUS_TEST_SQL_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connection))
        {
            Task.Run(() => CheckSave(connection)).GetAwaiter().GetResult();
            Console.WriteLine("OK: crear, listar, guardar, cargar y eliminar en SQL Server.");
            CheckUi(connection);
            Console.WriteLine("OK: recorrido WinForms 01–18, generadores, GUARDIÁN-01, inventario, guardado y final.");
        }
        else Console.WriteLine("Pruebas SQL y WinForms omitidas. Configura NEXUS_TEST_SQL_CONNECTION.");
    }

    private static void CheckCombat()
    {
        var encounter = new CombatTurn(83, 1, 6);
        Require(encounter.RemainingPa == 3, "PA básicos");
        Require(!encounter.AllocateCharges(0) && !encounter.AllocateCharges(7), "Validación de cargas");
        Require(encounter.AllocateCharges(1) && encounter.RemainingPa == 4 && encounter.Charges == 5, "Carga de JARVIS");
        Require(encounter.Choose(CombatAction.Defend) && encounter.Choose(CombatAction.Reserve)
            && encounter.Choose(CombatAction.UseKit) && encounter.Choose(CombatAction.Reserve), "Plan de turno");
        Require(encounter.Hp == 83 && encounter.Kits == 1, "Acciones aún en cola");
        var result = encounter.Resolve(new FixedRoll(51));
        Require(result.PlayerBlocks == 1 && result.DamageTaken == 20 && result.Healing == 17, "Defensa y curación");
        Require(encounter.Hp == 80 && encounter.Kits == 0 && result.NextPlayerPa == 5, "Consumo y reserva");
        encounter.Next();
        Require(encounter.RemainingPa == 5 && encounter.Actions.Count == 0, "Inicio del siguiente turno");

        var defeated = new CombatTurn(10, 0, 6);
        for (int i = 0; i < 3; i++) Require(defeated.Choose(CombatAction.Reserve), "Reserva");
        defeated.Resolve(new FixedRoll(51));
        Require(defeated.Hp == 0, "Derrota");
    }

    private static void CheckNarrative()
    {
        var catalog = new NarrativeCatalog();
        Require(Enumerable.Range(1, 18).Sum(room => catalog.Documents(room).Count()) == 18, "Documentos del paquete");
        Require(Enumerable.Range(1, 18).All(room => catalog.Room(room).Number == room), "Salas del paquete");
        Require(catalog.Line(-1, 0, "primer_inicio_tras_carga_100").Text == "Conexión establecida.", "Evento solo de voz");
        Require(catalog.Documents(1).Single().Id == "DOC_001" && !catalog.Documents(2).Any(), "Terminales locales");
    }

    private static void CheckPaLimit()
    {
        var encounter = new CombatTurn(100, 0, 6);
        var random = new FixedRoll(70);
        for (int turn = 0; turn < 2; turn++)
        {
            while (encounter.RemainingPa > 0) Require(encounter.Choose(CombatAction.Reserve), "Reservar PA");
            Require(encounter.Resolve(random).NextPlayerPa == 6 + turn * 3, "PA base más reserva");
            encounter.Next();
        }

        Require(encounter.RemainingPa == 9 && encounter.SelectableCharges == 1, "Espacio hasta el límite");
        encounter.Choose(CombatAction.Reserve);
        Require(!encounter.AllocateCharges(2) && encounter.Charges == 6, "Rechazo sin consumo por encima de 10");
        Require(encounter.AllocateCharges(1) && encounter.Charges == 5, "Consumo exacto hasta 10");
        encounter.Undo();
        Require(encounter.RemainingPa == CombatTurn.MaxPa && encounter.SelectableCharges == 0, "Deshacer respeta el límite");
        while (encounter.RemainingPa > 0) encounter.Choose(CombatAction.Reserve);
        Require(!encounter.AllocateCharges(1) && encounter.Charges == 5, "La cola no libera espacio para cargas");
        var result = encounter.Resolve(random);
        Require(result.NextPlayerPa == 10, "La reserva se limita a 10");
        encounter.Next();
        Require(encounter.RemainingPa == 10 && !encounter.AllocateCharges(1), "Nuevo turno al máximo");
    }

    private static void CheckRoomDistribution()
    {
        var rooms = Enumerable.Range(1, 18)
            .Select(number => new GameSession(room: number) { Id = Guid.NewGuid(), Name = "Cadete" }).ToArray();
        Require(rooms.Where(room => room.HasKit).Select(room => room.Room).SequenceEqual([1, 5, 9, 12, 14, 16]),
            "Dos botiquines en Fase 3 y distribución previa conservada");
        Require(rooms.Take(13).Select(room => room.ActiveEnemies().Count()).SequenceEqual([0, 1, 0, 1, 0, 1, 0, 2, 0, 0, 3, 0, 0]),
            "Enemigos de las fases 0–2 conservados");
        var finalEnemies = rooms.Skip(13)
            .SelectMany(room => room.ActiveEnemies().Select(enemy => (room.Room, Enemy: enemy))).ToArray();
        Require(finalEnemies.Length == 2 && finalEnemies[0] is (15, { Id: "security-15", MaxHp: 40, Damage: 10 })
            && finalEnemies[0].Enemy.Position.Y != GameSession.Exit.Y
            && finalEnemies[1] is (17, { Id: "guardian-01", MaxHp: 125, Damage: 20 }),
            "Un robot común fuera del paso principal y un GUARDIÁN-01 con atributos oficiales");
    }

    private static void Solve(CircuitPuzzle puzzle)
    {
        var tiles = (Button[,])typeof(CircuitPuzzle).GetField("tiles", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(puzzle)!;
        (int Row, int Column, string Symbol)[] route =
        [
            (2, 0, "─"), (2, 1, "┘"), (1, 1, "┌"), (1, 2, "─"), (1, 3, "┐"), (2, 3, "└"), (2, 4, "─")
        ];
        var onClick = typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var (row, column, target) in route)
        {
            if (puzzle.Solved) break;
            var tile = tiles[row, column];
            for (int rotation = 0; rotation < 4 && !puzzle.Solved && tile.Text != target; rotation++)
                onClick.Invoke(tile, [EventArgs.Empty]);
        }

        Require(puzzle.Solved, "Circuito 5×5 resoluble");
    }

    private static async Task CheckSave(string connection)
    {
        var repository = new SaveRepository(connection);
        var session = new GameSession { Id = Guid.NewGuid(), Name = "Cadete Ñ ' Prueba" };
        await repository.SaveAsync(session, create: true);
        try
        {
            Require((await repository.ListAsync()).Any(save => save.Id == session.Id), "Partida en la lista");
            session.Move(-1, 0);
            session.Move(1, 1);
            Require(session.Position == GameSession.Spawn && !session.Advance(), "Muros, diagonales y salida bloqueada");
            for (int i = 0; i < 2; i++) session.Move(1, 0);
            session.CollectKit();
            session.CollectKit();
            Require(session.Kits == 1, "Botiquín único");
            session.ReadDocument("DOC_001");
            session.SeenEvents.Add("JARVIS_F0_INICIO");
            session.PendingEvents.Add("JARVIS_S01_KIT");
            for (int i = 0; i < 3; i++) session.Move(1, 0);
            Require(session.Advance() && session.Room == 2, "Salida de Mantenimiento");

            var combat = new CombatTurn(session.Hp, session.Kits, session.Charges);
            combat.AllocateCharges(1);
            for (int i = 0; i < 4; i++) combat.Choose(CombatAction.Attack);
            combat.Resolve(new FixedRoll(51));
            session.ApplyCombat(combat, session.AdjacentEnemy() ?? session.ActiveEnemies().Single());
            for (int i = 0; i < 5; i++) session.Move(1, 0);
            Require(session.Advance() && session.Room == 3, "Salida de Entrenamiento");
            for (int i = 0; i < 3; i++) session.Move(1, 0);
            session.RestoreCircuit();
            session.Move(0, -1);
            session.Move(1, 0);
            session.Move(1, 0);
            session.Move(0, 1);
            Require(session.Advance() && session.IsUnlocked(1) && !session.IsUnlocked(2), "Desbloqueo de la fase siguiente");
            await repository.SaveAsync(session);

            var resumed = await repository.LoadAsync(session.Id);
            Require(resumed.Name == session.Name && resumed.Position == session.Position && resumed.Room == 3, "Identidad y posición");
            Require(resumed.Hp == 70 && resumed.Kits == 1 && resumed.Charges == 5, "Recursos persistentes");
            Require(resumed.Rooms[2].DefeatedEnemies.Contains("security-02") && resumed.CircuitRestored
                && resumed.PhaseComplete && resumed.TutorialChargeUsed, "Objetivos y flags");
            Require(resumed.SeenEvents.Contains("JARVIS_F0_INICIO") && resumed.PendingEvents.Contains("JARVIS_S01_KIT")
                && resumed.Rooms[1].ReadDocuments.Contains("DOC_001"), "Eventos y documento persistentes");
            var retry = resumed.RetryPhase();
            Require(retry.Id == resumed.Id && retry.Room == 1 && retry.Hp == 100 && retry.Charges == 6
                && retry.Kits == 0 && !retry.PhaseComplete, "Punto de reinicio");
            Require(resumed.StartNextPhase(1) && resumed.Room == 4 && resumed.Hp == 70
                && resumed.Charges == 5 && !resumed.CanExit, "Transición desde un guardado de Tutorial");
            await repository.SaveAsync(resumed);
            var signal = await repository.LoadAsync(session.Id);
            Require(signal.Phase == 1 && signal.EntryResources == new PhaseResources(70, 1, 5), "Punto de reinicio persistente");
            var secondRetry = signal.RetryPhase();
            Require(secondRetry.Room == 4 && secondRetry.Hp == 70 && secondRetry.Charges == 5
                && secondRetry.CompletedPhases.SetEquals([0]) && secondRetry.Rooms[1].ReadDocuments.Contains("DOC_001"),
                "Reintento de Señal Perdida conserva el Tutorial");
        }
        finally
        {
            await repository.DeleteAsync(session.Id);
        }
        Require(!(await repository.ListAsync()).Any(save => save.Id == session.Id), "Eliminación");
    }

    private static void CheckUi(string connection)
    {
        Environment.SetEnvironmentVariable("NEXUS_SQL_CONNECTION", connection);
        var random = new FixedRoll(51);
        using var form = new IrisGameForm(random);
        Exception? failure = null;
        form.Shown += (_, _) => form.BeginInvoke(() =>
        {
            try { PlayCampaign(form, new SaveRepository(connection), random); }
            catch (Exception error) { failure = error; }
            finally { Application.ExitThread(); }
        });
        Application.Run(form);
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(failure);
    }

    private static void PlayCampaign(Form form, SaveRepository repository, FixedRoll random)
    {
        string name = $"Prueba UI {Guid.NewGuid():N}";
        try
        {
            WaitFor(() => FindButton(form, "NUEVA PARTIDA") != null, "Menú principal");
            Click(form, "NUEVA PARTIDA");
            ControlsOf<TextBox>(form).Single().Text = name;
            Capture(form, "identificacion");
            Click(form, "CONFIRMAR");
            WaitFor(() => FindButton(form, "INICIAR") != null, "Selección de fase");
            Require(ControlsOf<Button>(form).Count(button => button.Text == "FASE BLOQUEADA" && !button.Enabled) == 3, "Fases bloqueadas");
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () => Click(form, "INICIAR"));
            WaitFor(() => FindButton(form, "PAUSA") != null, "Sala 01");
            WithModal(form, "INVENTARIO", dialog =>
            {
                Require(ControlsOf<ListBox>(dialog).Single().Items.Count == 1, "No se muestran documentos por descubrir");
                Require(ControlsOf<TextBox>(dialog).Single().Text.Contains("CANTIDAD: 0"), "Inventario inicial");
                Key(dialog, Keys.I);
            }, () => Key(form, Keys.I));
            Key(form, Keys.Up, 2);
            Key(form, Keys.Right);
            WithModal(form, "ORDEN DE MANTENIMIENTO", dialog =>
            {
                Require(ControlsOf<TextBox>(dialog).Single().Text.Contains("suministro médico"), "Texto definitivo del terminal");
                Click(dialog, "CERRAR");
            }, () => Key(form, Keys.E));
            Key(form, Keys.Down, 2);
            Key(form, Keys.Right);
            Key(form, Keys.E);
            Click(form, "PAUSA");
            Require(IsExposed(FindButton(form, "GUARDAR PARTIDA")!), "Guardado visible en pausa");
            Capture(form, "pausa");
            Click(form, "GUARDAR PARTIDA");
            WaitFor(() => HasText(form, "PARTIDA GUARDADA"), "Guardado manual");
            WithModal(form, "¿SALIR AL MENÚ?", dialog => Click(dialog, "SALIR AL MENÚ"), () => Click(form, "SALIR AL MENÚ"));
            Click(form, "CONTINUAR");
            WaitFor(() => ControlsOf<ListBox>(form).Any(), "Listado de partidas");
            var list = ControlsOf<ListBox>(form).Single();
            list.SelectedItem = list.Items.Cast<SavedGame>().Single(save => save.Name == name);
            Click(form, "CARGAR");
            WaitFor(() => FindButton(form, "PAUSA") != null, "Retomar exploración");
            Require(HasText(form, "BOTIQUINES 1/5"), "Inventario cargado");
            WithModal(form, "INVENTARIO", dialog =>
            {
                var inventory = ControlsOf<ListBox>(dialog).Single();
                Require(inventory.Items[0].ToString()!.Contains("×1"), "Botiquín desde SQL");
                inventory.SelectedIndex = 1;
                WithModal(dialog, "ORDEN DE MANTENIMIENTO", doc =>
                {
                    Require(ControlsOf<TextBox>(doc).Single().Text.Contains("suministro médico"), "Relectura de texto oficial desde SQL");
                    Click(doc, "CERRAR");
                }, () => Click(dialog, "LEER DOCUMENTO"));
                Click(dialog, "CERRAR (I)");
            }, () => Click(form, "INVENTARIO"));
            Capture(form, "mantenimiento");
            Key(form, Keys.Right, 3);
            Key(form, Keys.E);

            Require(FindButton(form, "ENFRENTAR") == null, "No hay encuentro a distancia");
            Key(form, Keys.Right, 3);
            Key(form, Keys.E);
            Require(FindButton(form, "ENFRENTAR") != null, "E no inicia el combate");
            Click(form, "ENFRENTAR");
            Click(form, "ATACAR");
            WithModal(form, "INVENTARIO", dialog =>
            {
                Require(ControlsOf<ListBox>(dialog).Single().Items.Count == 2 && FindButton(dialog, "USAR KIT") == null,
                    "Inventario de combate solo para consulta");
                Key(dialog, Keys.I);
            }, () => Key(form, Keys.I));
            Require(HasText(form, "PA DISPONIBLES 2") && HasText(form, "MÁXIMO 10 POR TURNO") && HasText(form, "ATACAR"),
                "El inventario conserva PA y la cola, y el combate muestra el límite");
            for (int i = 0; i < 2; i++) Click(form, "ATACAR");
            Require(FindButton(form, "FINALIZAR TURNO")?.Enabled == false, "Carga obligatoria en tutorial");
            WithModal(form, "HABILIDAD JARVIS", dialog =>
            {
                Require(HasText(dialog, "MÁXIMO 10 PA POR TURNO"), "El selector muestra el límite");
                Click(dialog, "+");
                Click(dialog, "CANCELAR");
            }, () => Click(form, "HABILIDAD JARVIS (6)"));
            WithModal(form, "HABILIDAD JARVIS", dialog =>
            {
                Click(dialog, "+");
                Click(dialog, "CONFIRMAR");
            }, () => Click(form, "HABILIDAD JARVIS (6)"));
            Click(form, "ATACAR");
            Capture(form, "combate");
            Click(form, "FINALIZAR TURNO");
            Require(IsExposed(FindButton(form, "CONTINUAR")!), "Continuar visible en el resultado");
            Capture(form, "resultado");
            ClickLast(form, "CONTINUAR");
            for (int turn = 0; turn < 6 && FindButton(form, "PAUSA") == null; turn++)
            {
                if (FindButton(form, "REINTENTAR") != null)
                    throw new InvalidOperationException("El Cadete cayó en el recorrido de prueba.");
                while (FindButton(form, "ATACAR") is { Enabled: true }) Click(form, "ATACAR");
                Click(form, "FINALIZAR TURNO");
                ClickLast(form, "CONTINUAR");
            }

            Require(FindButton(form, "PAUSA") != null, "Victoria en Entrenamiento");
            Key(form, Keys.Right, 2);
            Key(form, Keys.E);
            Key(form, Keys.Right, 3);
            WithModal(form, "NEXUS · PANEL DE ENERGÍA", dialog => Solve((CircuitPuzzle)dialog), () => Key(form, Keys.E));
            Key(form, Keys.Up);
            Key(form, Keys.Right, 2);
            Key(form, Keys.Down);
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () => Key(form, Keys.E));
            WaitFor(() => ControlsOf<Button>(form).Any(button => button.Text == "CONTINUAR" && button.Enabled)
                && Task.Run(repository.ListAsync).GetAwaiter().GetResult().Any(save => save.Name == name && save.Completed),
                "Guardado automático de fase");
            Capture(form, "fase-completada");
            Click(form, "CONTINUAR");
            Require(ControlsOf<Button>(form).Count(button => button.Text == "FASE BLOQUEADA") == 2, "Fase 1 desbloqueada");
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () =>
                Press(ControlsOf<Button>(form).Single(button => button.Text == "INICIAR" && button.Enabled), "INICIAR FASE 1"));
            WaitFor(() => HasText(form, "Corredor Central"), "Sala 04");
            Require(HasText(form, "SALUD 70/100") && HasText(form, "JARVIS 5/6"), "Recursos conservados desde el Tutorial");
            ReadTerminal("REGISTRO DE TURNO 031");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            Fight();
            ExitRoom();

            Require(HasText(form, "Enfermería"), "Sala 05");
            ReadTerminal("INFORME MÉDICO 014", "NOTA DE SUMINISTROS");
            Key(form, Keys.Down);
            Key(form, Keys.E);
            Require(HasText(form, "BOTIQUINES 2/5"), "Botiquín de Enfermería");
            Key(form, Keys.Right);
            ExitRoom();

            Require(HasText(form, "Sala Eléctrica"), "Sala 06");
            ReadTerminal("AVISO DE RED ELÉCTRICA");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            Fight();
            Key(form, Keys.Up);
            WithModal(form, "NEXUS · PANEL DE ENERGÍA", dialog => Solve((CircuitPuzzle)dialog), () => Key(form, Keys.E));
            Key(form, Keys.Down);
            ExitRoom();

            Require(HasText(form, "Comunicaciones"), "Sala 07");
            ReadTerminal("ÚLTIMA COMUNICACIÓN PARCIAL");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            Key(form, Keys.E);
            Require(HasText(form, "CHIP DE INFORMACIÓN"), "Chip en inventario");
            WithModal(form, "INVENTARIO", dialog =>
            {
                var inventory = ControlsOf<ListBox>(dialog).Single();
                Require(inventory.Items.Count == 8, "Botiquines, chip y seis documentos de dos fases");
                inventory.SelectedIndex = 1;
                Require(ControlsOf<TextBox>(dialog).Single().Text.Contains("código de acceso"), "Descripción del chip");
                Click(dialog, "CERRAR (I)");
            }, () => Click(form, "INVENTARIO"));
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), ExitRoom);
            WaitFor(() => FindButton(form, "CONTINUAR") is { Enabled: true }, "Guardado automático de Señal Perdida");
            var entry = Task.Run(repository.ListAsync).GetAwaiter().GetResult().Single(save => save.Name == name);
            Require(entry.Phase == 1 && entry.Completed, "La fase completada figura en SQL");
            var final = Task.Run(() => repository.LoadAsync(entry.Id)).GetAwaiter().GetResult();
            Require(final.ChipCollected && final.IsUnlocked(2) && final.Rooms.Values.Sum(room => room.ReadDocuments.Count) == 6
                && final.Charges == 5 && final.Rooms[6].Objectives.Contains("circuit"), "Estado final de la fase en SQL");
            final.Rooms[7].Objectives.Remove("chip");
            Require(!final.IsUnlocked(2) && !final.StartNextPhase(2) && final.Room == 7 && !final.IsValidSave(),
                "Seguridad bloqueada sin chip, incluso con fase 1 completada");
            Click(form, "CONTINUAR");
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () =>
                Press(ControlsOf<Button>(form).Single(button => button.Text == "INICIAR" && button.Enabled), "INICIAR FASE 2"));
            WaitFor(() => HasText(form, "Seguridad"), "Sala 08");
            Require(HasText(form, "CHIP DE INFORMACIÓN") && HasText(form, "SALUD 30/100")
                && HasText(form, "BOTIQUINES 2/5") && HasText(form, "JARVIS 5/6"), "Acceso y recursos persistentes");
            ReadTerminal("ORDEN DE SEGURIDAD 044");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            FightWithCharge(useKit: true);
            Click(form, "PAUSA");
            Click(form, "GUARDAR PARTIDA");
            WaitFor(() => HasText(form, "PARTIDA GUARDADA"), "Guardado parcial de Seguridad");
            var security = Task.Run(() => repository.LoadAsync(entry.Id)).GetAwaiter().GetResult();
            Require(security.Room == 8 && security.ActiveEnemies().Count() == 1 && !security.CanExit
                && security.Kits == 1 && security.Charges == 4 && security.Hp == 40, "Primer robot y recursos persistentes");
            var phaseRetry = security.RetryPhase();
            Require(phaseRetry.ActiveEnemies().Count() == 2 && phaseRetry.Hp == 30
                && phaseRetry.Kits == 2 && phaseRetry.Charges == 5 && phaseRetry.ChipCollected,
                "Reintento de Protocolo IRIS conserva recursos de entrada y chip");
            WithModal(form, "¿SALIR AL MENÚ?", dialog => Click(dialog, "SALIR AL MENÚ"), () => Click(form, "SALIR AL MENÚ"));
            Click(form, "CONTINUAR");
            WaitFor(() => ControlsOf<ListBox>(form).Any(), "Lista de partidas de Fase 2");
            var phaseSaves = ControlsOf<ListBox>(form).Single();
            phaseSaves.SelectedItem = phaseSaves.Items.Cast<SavedGame>().Single(save => save.Id == entry.Id);
            Click(form, "CARGAR");
            WaitFor(() => HasText(form, "Seguridad"), "Reanudar Seguridad");
            Require(FindButton(form, "ENFRENTAR") is null, "Primer robot no reaparece");
            WithModal(form, "INVENTARIO", dialog =>
            {
                var list = ControlsOf<ListBox>(dialog).Single();
                list.SelectedItem = list.Items.Cast<object>().Single(item => item.ToString() == "ORDEN DE SEGURIDAD 044");
                WithModal(dialog, "ORDEN DE SEGURIDAD 044", doc =>
                {
                    Require(ControlsOf<TextBox>(doc).Single().Text.Contains("personal acreditado"), "Documento original cargado desde SQL");
                    Click(doc, "CERRAR");
                }, () => Click(dialog, "LEER DOCUMENTO"));
                Key(dialog, Keys.I);
            }, () => Key(form, Keys.I));
            Key(form, Keys.Down, 2);
            FightWithCharge(useKit: false);
            Key(form, Keys.Up, 2);
            ExitRoom();

            Require(HasText(form, "Depósito"), "Sala 09");
            ReadTerminal("INVENTARIO DE CONTINGENCIA");
            Key(form, Keys.Down);
            Key(form, Keys.E);
            Require(HasText(form, "BOTIQUINES 2/5"), "Botiquín de Depósito");
            Key(form, Keys.Right);
            ExitRoom();

            Require(HasText(form, "Laboratorio A"), "Sala 10");
            ReadTerminal("BITÁCORA DE LABORATORIO A");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            WithModal(form, "NEXUS · PANEL DE ENERGÍA", dialog => Solve((CircuitPuzzle)dialog), () => Key(form, Keys.E));
            Key(form, Keys.Down);
            Key(form, Keys.Right, 2);
            Key(form, Keys.Up);
            Key(form, Keys.E);

            Require(HasText(form, "Laboratorio B"), "Sala 11");
            ReadTerminal("REPORTE DE INCIDENTE 019");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            FightWithCharge(useKit: true);
            Key(form, Keys.Down, 2);
            FightWithCharge(useKit: false);
            Key(form, Keys.Up, 4);
            FightWithCharge(useKit: true);
            Key(form, Keys.Down, 2);
            ExitRoom();

            Require(HasText(form, "Archivo Científico"), "Sala 12");
            ReadTerminal("ACTA CIENTÍFICA RESERVADA", "CORRESPONDENCIA INTERNA");
            Key(form, Keys.Down);
            Key(form, Keys.E);
            Require(HasText(form, "BOTIQUINES 1/5"), "Botiquín del Archivo Científico");
            Key(form, Keys.Right);
            ExitRoom();

            Require(HasText(form, "Investigación del NÚCLEO ZERO"), "Sala 13");
            ReadTerminal("PROTOCOLO IRIS — CONTENCIÓN");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () => Key(form, Keys.E));
            Key(form, Keys.Down);
            Key(form, Keys.Right, 2);
            Key(form, Keys.Up);
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () => Key(form, Keys.E));
            WaitFor(() => FindButton(form, "CONTINUAR") is { Enabled: true }, "Guardado automático de Protocolo IRIS");
            var iris = Task.Run(() => repository.LoadAsync(entry.Id)).GetAwaiter().GetResult();
            Require(iris.PhaseComplete && iris.IsUnlocked(3) && iris.InvestigationComplete,
                "Desbloqueo de fase 3");
            Require(iris.ChipCollected && iris.Kits == 1 && iris.Charges == 0 && iris.Rooms[8].DefeatedEnemies.Count == 2
                && iris.Rooms[11].DefeatedEnemies.Count == 3 && iris.Rooms[10].Objectives.Contains("circuit")
                && iris.Rooms.Values.Sum(room => room.ReadDocuments.Count) == 13, "Estado final de Protocolo IRIS en SQL");
            Require(new NarrativeCatalog().Documents(13).All(doc => iris.Rooms[13].ReadDocuments.Contains(doc.Id)),
                "Registro de contención conservado");
            Click(form, "CONTINUAR");
            WithModal(form, "JARVIS", dialog => Click(dialog, "CONTINUAR"), () =>
                Press(ControlsOf<Button>(form).Single(button => button.Text == "INICIAR" && button.Enabled), "INICIAR FASE 3"));
            WaitFor(() => HasText(form, "Centro de Control"), "Sala 14");
            Require(HasText(form, $"SALUD {iris.Hp}/100") && HasText(form, "BOTIQUINES 1/5")
                && HasText(form, "JARVIS 0/6") && HasText(form, "CHIP DE INFORMACIÓN"), "Recursos llegan a Última Transmisión");
            ReadTerminal("ORDEN DE CONTROL CENTRAL");
            Key(form, Keys.Down);
            Key(form, Keys.E);
            Require(HasText(form, "BOTIQUINES 2/5"), "Botiquín de Centro de Control");
            Key(form, Keys.Right);
            WithModal(form, "NEXUS · PANEL DE ENERGÍA", dialog => Solve((CircuitPuzzle)dialog), () => Key(form, Keys.E));
            WalkAroundDevice();
            Key(form, Keys.E);

            Require(HasText(form, "Generador A"), "Sala 15");
            ReadTerminal("REGISTRO GENERADOR A");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            WalkAroundDevice();
            WithModal(form, "ACCESO BLOQUEADO", dialog => Click(dialog, "CERRAR"), () => Key(form, Keys.E));
            Key(form, Keys.Down);
            Key(form, Keys.Left, 2);
            Key(form, Keys.Down);
            Fight();
            Key(form, Keys.Up, 3);
            Key(form, Keys.E);
            Key(form, Keys.Down);
            WalkAroundDevice();
            Key(form, Keys.E);

            Require(HasText(form, "Generador B"), "Sala 16");
            ReadTerminal("REGISTRO GENERADOR B");
            Key(form, Keys.Down);
            Key(form, Keys.E);
            Require(HasText(form, "BOTIQUINES 3/5"), "Botiquín de Generador B");
            Key(form, Keys.Right);
            Key(form, Keys.Up);
            Key(form, Keys.E);
            Key(form, Keys.Down);
            WalkAroundDevice();
            Key(form, Keys.E);

            Require(HasText(form, "Cámara del GUARDIÁN-01"), "Sala 17");
            ReadTerminal("DIRECTIVA GUARDIÁN-01");
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            SaveAndResume("Cámara del GUARDIÁN-01");
            var checkpoint = Task.Run(() => repository.LoadAsync(entry.Id)).GetAwaiter().GetResult();
            Require(checkpoint.Phase == 3 && checkpoint.Room == 17 && checkpoint.Kits == 3
                && checkpoint.Rooms[14].Objectives.SetEquals(["kit", "circuit"])
                && checkpoint.Rooms[15].Objectives.Contains("generator") && checkpoint.Rooms[15].DefeatedEnemies.Count == 1
                && checkpoint.Rooms[16].Objectives.SetEquals(["kit", "generator"]) && !checkpoint.CanExit,
                "Circuito, generadores, robot, botiquines y puerta persistidos");
            Key(form, Keys.E);
            Click(form, "ENFRENTAR");
            Require(HasText(form, "GUARDIÁN-01") && HasText(form, "125/125 HP") && HasText(form, "DAÑO 20"), "Atributos oficiales del guardián");
            for (int action = 0; action < 3; action++) Click(form, "DEFENDER");
            Click(form, "FINALIZAR TURNO");
            Require(HasText(form, "Bloqueos: Cadete 3"), "Defensa contra ataques del guardián");
            Click(form, "CONTINUAR");
            random.Roll = 90;
            for (int turn = 0; turn < 3; turn++)
            {
                if (turn == 0) Click(form, "USAR KIT");
                else Click(form, "ATACAR");
                Click(form, "ATACAR");
                Click(form, "ATACAR");
                Click(form, "FINALIZAR TURNO");
                Click(form, "CONTINUAR");
            }
            Require(ControlsOf<TextBox>(form).Any(text => text.Text.Contains("GUARDIÁN-01 neutralizado")), "Confirmación de victoria");
            ExitRoom();

            Require(HasText(form, "Núcleo Central"), "Sala 18");
            ReadTerminal("TRANSMISIÓN FINAL — IRIS");
            WithModal(form, "INVENTARIO", dialog =>
            {
                Require(ControlsOf<ListBox>(dialog).Single().Items.Count == 20, "18 documentos, chip y botiquines");
                Click(dialog, "CERRAR (I)");
            }, () => Key(form, Keys.I));
            Key(form, Keys.Down);
            Key(form, Keys.Right);
            WithModal(form, "JARVIS", dialog =>
            {
                Require(ControlsOf<TextBox>(dialog).Single().Text == "No estamos solos. Y nunca lo estuvimos.", "Desenlace oficial");
                Click(dialog, "CONTINUAR");
            }, () => Key(form, Keys.E));
            WaitFor(() => FindButton(form, "VOLVER AL MENÚ") is { Enabled: true }, "Misión completada y guardada");
            var ending = Task.Run(() => repository.LoadAsync(entry.Id)).GetAwaiter().GetResult();
            Require(ending.MissionComplete && ending.Room == 18 && ending.Kits == 2
                && ending.Rooms[17].DefeatedEnemies.SetEquals(["guardian-01"])
                && ending.Rooms.Values.Sum(room => room.ReadDocuments.Count) == 18
                && ending.SeenEvents.Contains("JARVIS_S18_FINAL"), "Final, inventario y narración guardados en SQL");
            Click(form, "VOLVER AL MENÚ");
            LoadSavedGame("MISIÓN COMPLETADA");
            Require(HasText(form, "No estamos solos. Y nunca lo estuvimos."), "El guardado final restaura el desenlace");

            void WalkAroundDevice()
            {
                Key(form, Keys.Down);
                Key(form, Keys.Right, 2);
                Key(form, Keys.Up);
            }

            void SaveAndResume(string title)
            {
                Click(form, "PAUSA");
                Click(form, "GUARDAR PARTIDA");
                WaitFor(() => HasText(form, "PARTIDA GUARDADA"), "Guardado manual de Última Transmisión");
                WithModal(form, "¿SALIR AL MENÚ?", dialog => Click(dialog, "SALIR AL MENÚ"), () => Click(form, "SALIR AL MENÚ"));
                LoadSavedGame(title);
            }

            void LoadSavedGame(string title)
            {
                Click(form, "CONTINUAR");
                WaitFor(() => ControlsOf<ListBox>(form).Any(), "Lista de partidas");
                var list = ControlsOf<ListBox>(form).Single();
                list.SelectedItem = list.Items.Cast<SavedGame>().Single(save => save.Id == entry.Id);
                Click(form, "CARGAR");
                WaitFor(() => HasText(form, title), title);
            }

            void ReadTerminal(params string[] titles)
            {
                Key(form, Keys.Right, 2);
                Key(form, Keys.Up);
                WithModals(form, titles, dialog => Click(dialog, "CERRAR"), () => Key(form, Keys.E));
            }

            void ExitRoom()
            {
                Key(form, Keys.Right, 2);
                Key(form, Keys.E);
            }

            void Fight()
            {
                Click(form, "ENFRENTAR");
                Click(form, "ATACAR");
                Click(form, "ATACAR");
                Click(form, "DEFENDER");
                Click(form, "FINALIZAR TURNO");
                Click(form, "CONTINUAR");
            }

            void FightWithCharge(bool useKit)
            {
                Click(form, "ENFRENTAR");
                var jarvis = ControlsOf<Button>(form).Single(button => button.Text.StartsWith("HABILIDAD JARVIS"));
                WithModal(form, "HABILIDAD JARVIS", dialog =>
                {
                    Click(dialog, "+");
                    Click(dialog, "CONFIRMAR");
                }, () => Press(jarvis, jarvis.Text));
                Click(form, "ATACAR");
                Click(form, "ATACAR");
                Click(form, "DEFENDER");
                Click(form, useKit ? "USAR KIT" : "DEFENDER");
                Click(form, "FINALIZAR TURNO");
                Click(form, "CONTINUAR");
            }
        }
        finally
        {
            foreach (var entry in Task.Run(repository.ListAsync).GetAwaiter().GetResult().Where(save => save.Name == name))
                Task.Run(() => repository.DeleteAsync(entry.Id)).GetAwaiter().GetResult();
        }
    }

    private static void Capture(Form form, string name)
    {
        var directory = Environment.GetEnvironmentVariable("NEXUS_TEST_CAPTURES");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, form.Bounds with { X = 0, Y = 0 });
        bitmap.Save(Path.Combine(directory, name + ".png"));
    }

    private static IEnumerable<T> ControlsOf<T>(Control root) where T : Control
    {
        foreach (Control control in root.Controls)
        {
            if (control is T match && match.Visible) yield return match;
            foreach (var descendant in ControlsOf<T>(control)) yield return descendant;
        }
    }

    private static Button? FindButton(Control page, string text) =>
        ControlsOf<Button>(page).FirstOrDefault(button => button.Text == text);

    private static bool HasText(Control page, string text) =>
        ControlsOf<Label>(page).Any(label => label.Text.Contains(text, StringComparison.Ordinal));

    private static bool IsExposed(Control control)
    {
        var middle = control.PointToScreen(new Point(control.Width / 2, control.Height / 2));
        for (var child = control; child.Parent is { } parent; child = parent)
        {
            var point = parent.PointToClient(middle);
            if (!parent.ClientRectangle.Contains(point) || parent.GetChildAtPoint(point, GetChildAtPointSkip.Invisible) != child)
                return false;
        }
        return true;
    }

    private static void Click(Control page, string text) => Press(FindButton(page, text), text);

    private static void ClickLast(Control page, string text) =>
        Press(ControlsOf<Button>(page).LastOrDefault(button => button.Text == text), text);

    private static void Press(Button? button, string name)
    {
        Require(button is { Enabled: true }, $"Botón activo: {name}");
        button!.PerformClick();
        Application.DoEvents();
    }

    private static void Key(Form form, Keys key, int count = 1)
    {
        var handler = form.GetType().GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int i = 0; i < count; i++) handler.Invoke(form, [default(Message), key]);
        Application.DoEvents();
    }

    private static void WithModal(Form owner, string title, Action<Form> handle, Action open) =>
        WithModals(owner, [title], handle, open);

    private static void WithModals(Form owner, string[] titles, Action<Form> handle, Action open)
    {
        Exception? failure = null;
        int handled = 0;
        var deadline = DateTime.UtcNow.AddSeconds(15);
        using var timer = new System.Windows.Forms.Timer { Interval = 30 };
        timer.Tick += (_, _) =>
        {
            var dialog = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form != owner && form.Text == titles[handled]);
            if (dialog?.Text != titles[handled] && DateTime.UtcNow < deadline) return;
            timer.Stop();
            try
            {
                Require(dialog?.Text == titles[handled], $"Ventana: {titles[handled]}");
                handle(dialog!);
                handled++;
                if (handled < titles.Length) timer.Start();
            }
            catch (Exception error)
            {
                failure = error;
                dialog?.Close();
            }
        };
        timer.Start();
        open();
        WaitFor(() => handled == titles.Length || failure != null, string.Join(", ", titles));
        if (failure != null) throw failure;
    }

    private static void WaitFor(Func<bool> ready, string description)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (!ready() && DateTime.UtcNow < deadline)
        {
            Application.DoEvents();
            Thread.Sleep(15);
        }
        Require(ready(), description);
    }

    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
    }

    private sealed class FixedRoll(int roll) : Random
    {
        public int Roll { get; set; } = roll;
        public override int Next(int maxValue) => Roll;
    }
}
