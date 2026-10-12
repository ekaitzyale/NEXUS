using System;

namespace NEXUS
{
    public class MapaRealidad
    {
        // Atributo privado: Matriz 2D
        private char[,] _matrizMapa;

        public MapaRealidad()
        {
            _matrizMapa = new char[6, 10];
        }

        public void GestionarModuloMapas()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("==================================================================");
            Console.WriteLine("           CARTOGRAFÍA DE REALIDADES — RENDERIZADO 2D");
            Console.WriteLine("==================================================================");
            Console.ResetColor();

            Console.WriteLine("\nSeleccione Sector a renderizar:");
            Console.WriteLine("1. Sector Alfa (Estable)");
            Console.WriteLine("2. Sector IRIS-12 (Anomalía Cero)");
            Console.Write("Opción: ");
            string subOpcion = Console.ReadLine();

            if (subOpcion == "2")
                GenerarMapaAnomalo();
            else
                GenerarMapaEstable();

            Console.WriteLine("\n--- MAPA RENDERIZADO (Posición 'N' = Cadete / Núcleo) ---");
            RenderizarMatrizMapa();
        }

        private void GenerarMapaEstable()
        {
            for (int f = 0; f < _matrizMapa.GetLength(0); f++)
            {
                for (int c = 0; c < _matrizMapa.GetLength(1); c++)
                {
                    if (f == 0 || f == 5 || c == 0 || c == 9)
                        _matrizMapa[f, c] = '#';
                    else if (f == 2 && c == 4)
                        _matrizMapa[f, c] = 'N'; // Representa la ubicación del Cadete o la base
                    else
                        _matrizMapa[f, c] = '.';
                }
            }
        }

        private void GenerarMapaAnomalo()
        {
            for (int f = 0; f < _matrizMapa.GetLength(0); f++)
            {
                for (int c = 0; c < _matrizMapa.GetLength(1); c++)
                {
                    if (f == 0 || f == 5 || c == 0 || c == 9)
                        _matrizMapa[f, c] = '%';
                    else if (f == c)
                        _matrizMapa[f, c] = 'X';
                    else
                        _matrizMapa[f, c] = '?';
                }
            }
        }

        private void RenderizarMatrizMapa()
        {
            for (int f = 0; f < _matrizMapa.GetLength(0); f++)
            {
                Console.Write("   ");
                for (int c = 0; c < _matrizMapa.GetLength(1); c++)
                {
                    char simbolo = _matrizMapa[f, c];

                    if (simbolo == 'N') Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (simbolo == 'X') Console.ForegroundColor = ConsoleColor.Red;
                    else if (simbolo == '#' || simbolo == '%') Console.ForegroundColor = ConsoleColor.Gray;
                    else Console.ForegroundColor = ConsoleColor.DarkGray;

                    Console.Write(simbolo + " ");
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }
    }
}