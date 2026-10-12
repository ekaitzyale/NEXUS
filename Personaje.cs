using System;

namespace NEXUS
{
    public abstract class Personaje
    {
        public string Nombre { get; protected set; }
        public int Salud { get; protected set; }
        public int Energia { get; protected set; }
        public int AtaqueBase { get; protected set; }

        public Personaje(string nombre, int salud, int energia, int ataqueBase)
        {
            Nombre = nombre;
            Salud = Math.Clamp(salud, 0, 100);
            Energia = Math.Clamp(energia, 0, 100);
            AtaqueBase = ataqueBase;
        }

        public bool EstaVivo => Salud > 0;

        // Método para reducir salud sin bajar de 0
        public virtual void RecibirDanio(int cantidad)
        {
            Salud = Math.Clamp(Salud - cantidad, 0, 100);
            Console.WriteLine($"[ALERT] {Nombre} recibió {cantidad} de daño. Salud restante: {Salud}%");
        }

        // Cada subclase (Cadete, Robot, Enemigo) definirá cómo reacciona
        public abstract void Reaccionar();
    }
}