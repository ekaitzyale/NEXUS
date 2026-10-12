using System;
using System.Threading;

namespace NEXUS
{
    public class ConsolaIris
    {
        private string[] _logsIRIS;

        public ConsolaIris()
        {
            _logsIRIS = new string[4] {
                "[LOG 001]: Transmisión cifrada recibida desde Sector Alfa.",
                "[LOG 002]: Frecuencia de enlace ajustada a 1420.405 MHz.",
                "[LOG 003]: Anomalía vectorial detectada en coordenadas IRIS-12.",
                "[LOG 004]: Advertencia de colapso de fase en matriz de convergencia."
            };
        }

        public void MostrarLogs()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================================");
            Console.WriteLine("         CONSOLA DE COMUNICACIÓN IRIS — LOGS RECEPCIONADOS");
            Console.WriteLine("==================================================================");
            Console.ResetColor();

            Console.WriteLine("\nRegistros decodificados en memoria temporal:\n");
            for (int i = 0; i < _logsIRIS.Length; i++)
            {
                Console.WriteLine($"  > {_logsIRIS[i]}");
                Thread.Sleep(300);
            }

            Console.WriteLine("\nEnviando ping de retorno a IRIS Core...");
            Thread.Sleep(500);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[ESTADO]: Conexión Inter-Consola Estable.");
            Console.ResetColor();
        }

        // Método dinámico para transmitir eventos del juego en tiempo real
        public void EmitirAlerta(string origen, string mensaje, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{origen.ToUpper()} — ALERTA IRIS]: {mensaje}");
            Console.ResetColor();
        }
    }
}