using System;

namespace NEXUS
{
    // Cadete hereda de Personaje (recibe Salud, Energia y AtaqueBase)
    public class Cadete : Personaje
    {
        private int _edad;

        // Propiedad de lectura pública para la edad
        public int Edad => _edad;

        // Inventario propio del Cadete
        public Inventario InventarioJugador { get; private set; }

        // Construtor: envía el nombre, salud (100), energía (100) y ataque (15) a la clase Personaje
        public Cadete(string nombre, int edad) : base(string.IsNullOrWhiteSpace(nombre) ? "Operador-01" : nombre, 100, 100, 15)
        {
            _edad = edad > 0 ? edad : 18;
            InventarioJugador = new Inventario();
        }

        // Método de comportamiento propio
        public string ObtenerFirmaBiometrica()
        {
            return $"{Nombre.ToUpper()} (EDAD: {_edad})";
        }

        public void ModificarEnergia(int cantidad)
        {
            Energia = Math.Clamp(Energia + cantidad, 0, 100);
            Console.WriteLine($"[ENERGÍA] Nivel actual del traje: {Energia}%");
        }

        // Obligatorio por heredar de Personaje
        public override void Reaccionar()
        {
            Console.WriteLine($"[CADETE] {Nombre} analiza el entorno y ajusta los sistemas del traje.");
        }
    }
}