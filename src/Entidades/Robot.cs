using System;

namespace NEXUS
{
    // La sintaxis ": Personaje" transforma Robot en una subclase
    public class Robot : Personaje
    {
        public bool EscudoDefensivoActivo { get; private set; }

        // El : base(...) envía los valores iniciales al constructor de Personaje
        public Robot(string idRobot) : base($"Robot-NEXUS-{idRobot}", 80, 100, 10)
        {
            EscudoDefensivoActivo = false;
        }

        // Si tiene el escudo activo, reduce el daño a la mitad
        public override void RecibirDanio(int cantidad)
        {
            int danioReal = EscudoDefensivoActivo ? cantidad / 2 : cantidad;
            base.RecibirDanio(danioReal);
        }

        // Implementación del método abstracto obligatorio
        public override void Reaccionar()
        {
            EscudoDefensivoActivo = true;
            Console.WriteLine($"[🤖 ROBOT] {Nombre} activó su protocolo defensivo de escudo.");
        }
    }
}