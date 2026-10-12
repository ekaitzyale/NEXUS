using System;

namespace NEXUS
{
    public enum TipoObjeto
    {
        Energia,
        Escudo,
        Ataque,
        Analizador,
        Consumible
    }

    public class Objeto
    {
        public string Nombre { get; private set; }
        public TipoObjeto Tipo { get; private set; }
        public int ValorEfecto { get; private set; }

        public Objeto(string nombre, TipoObjeto tipo, int valorEfecto)
        {
            Nombre = nombre;
            Tipo = tipo;
            ValorEfecto = valorEfecto;
        }

        // Sobrecarga para que acepte tanto texto como enum sin dar error
        public Objeto(string nombre, string tipoTexto, int valorEfecto)
        {
            Nombre = nombre;
            ValorEfecto = valorEfecto;

            if (Enum.TryParse(tipoTexto, true, out TipoObjeto tipoResultado))
            {
                Tipo = tipoResultado;
            }
            else
            {
                Tipo = TipoObjeto.Consumible;
            }
        }

        public override string ToString()
        {
            return $"{Nombre} [{Tipo}] (+{ValorEfecto})";
        }
    }
}