using System;
using System.Threading;

namespace NEXUS
{
    class Program
    {
        // Instancias de los objetos del sistema NEXUS
        static Cadete operadorActual;
        static EnlaceSincronico enlace;
        static ConsolaIris consolaIris;
        static MapaRealidad mapaCartografia;
        static Robot robotCompanero;
        static Enemigo amenazaDetectada;
        static bool sistemaActivo = true;
        static Random rand = new Random(); // Instancia única para evitar duplicaciones

        static void Main(string[] args)
        {
            Console.Title = "NEXUS v1.0 — Control Core (POO Architecture)";

            // Instanciación de subsistemas independientes
            consolaIris = new ConsolaIris();
            mapaCartografia = new MapaRealidad();
            robotCompanero = new Robot("001");
            amenazaDetectada = new Enemigo("Alien-X", "Hostil-Alta", 20);

            MostrarPantallaInicio();
            InicializarOperadorYEnlace();
            BuclePrincipal();
        }

        static void MostrarPantallaInicio()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================================");
            Console.WriteLine("    N E X U S   v 1 . 0   --   S I S T E M A   I N T E G R A D O");
            Console.WriteLine("==================================================================");
            Console.ResetColor();
            Console.WriteLine("\n[SISTEMA]: Inicializando subsistemas POO de navegación y exploración...");
            Thread.Sleep(800);
        }

        static void InicializarOperadorYEnlace()
        {
            Console.Write("\nIngrese el nombre del Cadete: ");
            string nombre = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(nombre))
            {
                Console.Write("⚠️️ El nombre no puede estar vacío. Ingrese el nombre del Cadete: ");
                nombre = Console.ReadLine();
            }

            Console.Write("Ingrese la edad del Cadete: ");
            int edad;
            while (!int.TryParse(Console.ReadLine(), out edad) || edad <= 0)
            {
                Console.Write("⚠️ Edad inválida. Ingrese un número válido mayor a 0: ");
            }

            // Instanciación del Cadete y del Enlace Sincrónico
            operadorActual = new Cadete(nombre, edad);
            enlace = new EnlaceSincronico("ENLACE-CORE-01");

            // Adicionar ítem inicial al inventario del Cadete
            Objeto kitInicial = new Objeto("Kit Médico Básico", "Consumible", 1);
            operadorActual.InventarioJugador.AgregarObjeto(kitInicial);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[ÉXITO]: Vinculación biométrica completada. Bienvenida/o, {operadorActual.ObtenerFirmaBiometrica()}.");
            Console.ResetColor();
            Console.WriteLine("Presione cualquier tecla para acceder al Kernel de NEXUS...");
            Console.ReadKey();
        }

        static void BuclePrincipal()
        {
            while (sistemaActivo)
            {
                if (enlace.EvaluarFalloCritico())
                {
                    NotificarFalloCritico();
                    break;
                }

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("==================================================================");
                Console.WriteLine($"  NEXUS v1.0 | Cadete: {operadorActual.Nombre} | Salud: {operadorActual.Salud}% | Energía: {operadorActual.Energia}%");
                Console.WriteLine("==================================================================");
                Console.ResetColor();

                MostrarEstadoEnlace();

                Console.WriteLine("\n--- MENÚ DE ACCIONES DISPONIBLES ---");
                Console.WriteLine("1. Explorar Terreno (Consumo de Batería y Energía)");
                Console.WriteLine("2. Sondear Espectro IRIS (Escanear Amenazas)");
                Console.WriteLine("3. Consola IRIS: Transmisión y Logs (Arreglos)");
                Console.WriteLine("4. Cartografía: Mapas de Realidades (Matrices)");
                Console.WriteLine("5. Ver Inventario del Cadete");
                Console.WriteLine("6. Interactuar con Unidades (Robot / Enemigo)");
                Console.WriteLine("7. Recargar Batería y Estabilizar Sincronía");
                Console.WriteLine("8. Salida Controlada del Sistema");
                Console.Write("\nSeleccione una opción (1-8): ");

                string opcion = Console.ReadLine();
                EjecutarOpcion(opcion);
            }
        }

        static void MostrarEstadoEnlace()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" Sincronía: {enlace.NivelSincronia:F1}% | Carga: {enlace.CargaBateria}% | Amenazas: {enlace.AmenazasEntorno:F1}% | Marca IRIS: {(enlace.MarcaIrisDetectada ? "SÍ" : "NO")}");
            Console.ResetColor();
        }

        static void EjecutarOpcion(string opcion)
        {
            switch (opcion)
            {
                case "1":
                    ExplorarTerreno();
                    break;
                case "2":
                    SondearEspectro();
                    break;
                case "3":
                    consolaIris.MostrarLogs();
                    Pausar();
                    break;
                case "4":
                    mapaCartografia.GestionarModuloMapas();
                    Pausar();
                    break;
                case "5":
                    Console.Clear();
                    operadorActual.InventarioJugador.MostrarInventario();
                    Pausar();
                    break;
                case "6":
                    InteractuarConUnidades();
                    break;
                case "7":
                    EstabilizarSistema();
                    break;
                case "8":
                    ConfirmarSalida();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[ERROR]: Opción inválida. Intente de nuevo.");
                    Console.ResetColor();
                    Thread.Sleep(1000);
                    break;
            }
        }

        static void ExplorarTerreno()
        {
            Console.Clear();
            Console.WriteLine("=== EXPLORANDO EL SECTOR EXTERNO ===");

            bool exito = enlace.RegistrarExploracion(operadorActual);

            if (!exito)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ADVERTENCIA]: Carga de batería o energía críticamente baja. No es seguro explorar.");
                Console.ResetColor();
                Pausar();
                return;
            }

            Console.WriteLine("\n[TELEMETRÍA]: Avanzando por el sector...");
            Thread.Sleep(600);

            int evento = rand.Next(1, 4);

            switch (evento)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[HALLAZGO]: ¡Has encontrado un contenedor de suministros!");
                    Objeto nuevoItem = new Objeto("Célula de Energía", "Consumible", 1);
                    operadorActual.InventarioJugador.AgregarObjeto(nuevoItem);
                    Console.ResetColor();
                    break;

                case 2:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[¡EMBOSCADA!]: Un enemigo ha aparecido en el radar.");
                    Console.ResetColor();
                    amenazaDetectada.Reaccionar();
                    robotCompanero.Reaccionar();
                    operadorActual.RecibirDanio(10);
                    break;

                case 3:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n[EXPLORACIÓN LIMPIA]: Sector despejado. Datos de telemetría registrados correctamente.");
                    Console.ResetColor();
                    break;
            }

            Pausar();
        }

        static void SondearEspectro()
        {
            Console.Clear();
            Console.WriteLine("=== SONDEO DE ESPECTRO EM ===");
            enlace.ActivarSondeoIris();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[¡ALERTA!]: Firma electromagnética IRIS detectada.");
            Console.WriteLine("Nivel de amenaza incrementado en +35%.");
            Console.ResetColor();
            Pausar();
        }

        static void InteractuarConUnidades()
        {
            Console.Clear();
            Console.WriteLine("=== INTERACCIÓN CON ENTIDADES DEL SISTEMA ===");
            Console.WriteLine("1. Activar protocolo del Robot de soporte");
            Console.WriteLine("2. Registrar escaneo del Enemigo detectado");
            Console.Write("\nOpción: ");
            string sub = Console.ReadLine();

            if (sub == "1")
            {
                robotCompanero.Reaccionar();
            }
            else if (sub == "2")
            {
                amenazaDetectada.Reaccionar();
                operadorActual.RecibirDanio(5);
            }
            Pausar();
        }

        static void EstabilizarSistema()
        {
            Console.Clear();
            Console.WriteLine("=== RECARGA Y ESTABILIZACIÓN DE KERNEL ===");
            enlace.EstabilizarEnlace();
            operadorActual.ModificarEnergia(35);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[ÉXITO]: Batería recargada, traje energizado y sincronía re-estabilizada.");
            Console.ResetColor();
            Pausar();
        }

        static void NotificarFalloCritico()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("==================================================================");
            Console.WriteLine("!!! CRITICAL SYSTEM FAILURE: TRIPLEX IMPACT DETECTADO !!!");
            Console.WriteLine("==================================================================");
            Console.WriteLine("\nSincronía por debajo del 20%, Amenazas superiores al 60% y Marca IRIS activa.");
            Console.WriteLine("NEXUS v1.0 ha entrado en modo de emergencia y cierre defensivo.");
            Console.ResetColor();
            sistemaActivo = false;
            Pausar();
        }

        static void ConfirmarSalida()
        {
            Console.Clear();
            Console.Write("¿Está seguro que desea desconectar NEXUS v1.0? (S/N): ");
            string res = Console.ReadLine().Trim().ToUpper();
            if (res == "S")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[SISTEMA]: Guardando sesión de {operadorActual.Nombre}...");
                Console.WriteLine("[SISTEMA]: Desconexión segura completada. ¡Hasta pronto!");
                Console.ResetColor();
                sistemaActivo = false;
                Thread.Sleep(1200);
            }
        }


        static void Pausar()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}