using System;
using System.Collections.Generic;

namespace NEXUS
{
    public class Inventario
    {
        private List<Objeto> objetos;
        private int capacidadMaxima;

        public Inventario(int capacidad = 10)
        {
            objetos = new List<Objeto>();
            capacidadMaxima = capacidad;
        }

        // 1. Agregar objeto al inventario
        public bool AgregarObjeto(Objeto nuevoObjeto)
        {
            if (objetos.Count >= capacidadMaxima)
            {
                Console.WriteLine("[INVENTARIO] Capacidad llena. No puedes cargar más objetos.");
                return false;
            }

            objetos.Add(nuevoObjeto);
            Console.WriteLine($"[🎁 OBJETO AGREGADO] {nuevoObjeto.Nombre} guardado en el inventario.");
            return true;
        }

        // 2. Consultar inventario
        public void MostrarInventario()
        {
            Console.WriteLine("\n=== 🎒 INVENTARIO DEL CADETE ===");
            if (objetos.Count == 0)
            {
                Console.WriteLine(" El inventario está vacío.");
                Console.WriteLine("===============================\n");
                return;
            }

            for (int i = 0; i < objetos.Count; i++)
            {
                Console.WriteLine($" [{i + 1}] {objetos[i]}");
            }
            Console.WriteLine("===============================\n");
        }

        // 3. Utilizar un objeto por índice
        public Objeto UsarObjeto(int numeroCasilla)
        {
            int posicion = numeroCasilla - 1; // Convertimos de 1-10 a 0-9

            // Control de situaciones: validamos que el índice exista para no romper el programa
            if (posicion < 0 || posicion >= objetos.Count)
            {
                Console.WriteLine("[⚠️ ERROR] No posees un objeto en esa casilla del inventario.");
                return null;
            }

            Objeto objetoUsado = objetos[posicion];
            objetos.RemoveAt(posicion); // Se elimina automáticamente al consumirse
            Console.WriteLine($"[USO] Has utilizado: {objetoUsado.Nombre}");
            return objetoUsado;
        }

        public int CantidadObjetos => objetos.Count;
    }
}