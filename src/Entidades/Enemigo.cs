using System;

namespace NEXUS
{
    // Enemigo hereda de Personaje
    public class Enemigo : Personaje
    {
        public string TipoHostilidad { get; private set; }

        public Enemigo(string nombre, string tipoHostilidad, int ataque)
            : base(nombre, 70, 50, ataque)
        {
            TipoHostilidad = tipoHostilidad;
        }

        public override void Reaccionar()
        {
            Console.WriteLine($"[👾 ENEMIGO] {Nombre} ({TipoHostilidad}) entra en modo de ataque.");
        }
    }
}