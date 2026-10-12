using System;

namespace NEXUS
{
    /// <summary>
    /// Representa la entidad de Enlace/Telemetría del Cadete en NEXUS v1.0.
    /// Encapsula las métricas vitales y valida las condiciones de estabilidad.
    /// </summary>
    public class EnlaceSincronico
    {
        // ==========================================
        // 1. ATRIBUTOS PRIVADOS (ENCAPSULAMIENTO)
        // ==========================================
        private string _idEnlace;
        private double _nivelSincronia;
        private int _cargaBateria;
        private double _amenazasEntorno;
        private bool _marcaIrisDetectada;

        // ==========================================
        // 2. PROPIEDADES (DESCRIPTORES DE ACCESO)
        // ==========================================
        public string IdEnlace => _idEnlace;

        public double NivelSincronia
        {
            get => _nivelSincronia;
            private set { _nivelSincronia = Math.Clamp(value, 0.0, 100.0); }
        }

        public int CargaBateria
        {
            get => _cargaBateria;
            private set { _cargaBateria = Math.Clamp(value, 0, 100); }
        }

        public double AmenazasEntorno
        {
            get => _amenazasEntorno;
            private set { _amenazasEntorno = Math.Clamp(value, 0.0, 100.0); }
        }

        public bool MarcaIrisDetectada => _marcaIrisDetectada;

        // ==========================================
        // 3. CONSTRUCTORES
        // ==========================================

        public EnlaceSincronico(string idEnlace)
        {
            _idEnlace = idEnlace;
            NivelSincronia = 100.0;
            CargaBateria = 100;
            AmenazasEntorno = 0.0;
            _marcaIrisDetectada = false;
        }

        public EnlaceSincronico(string idEnlace, double sincroniaInicial, int bateriaInicial)
        {
            _idEnlace = idEnlace;
            NivelSincronia = sincroniaInicial;
            CargaBateria = bateriaInicial;
            AmenazasEntorno = 20.0;
            _marcaIrisDetectada = false;
        }

        // ==========================================
        // 4. MÉTODOS Y REGLAS DE NEGOCIO
        // ==========================================

        // Permite registrar la exploración sincronizándola con el traje del Cadete
        public bool RegistrarExploracion(Cadete cadete)
        {
            if (CargaBateria < 15 || cadete.Energia < 15)
            {
                Console.WriteLine("[ALERTA ENLACE]: Carga o Energía del traje insuficiente para continuar explorando.");
                return false;
            }

            CargaBateria -= 15;
            NivelSincronia -= 10.0;
            cadete.ModificarEnergia(-15); // Reduce la energía del traje del Cadete
            return true;
        }

        public void ActivarSondeoIris()
        {
            AmenazasEntorno += 35.0;
            _marcaIrisDetectada = true;
            Console.WriteLine("[ENLACE IRIS]: Sondeo activado. Amenazas detectadas en el entorno.");
        }

        public void EstabilizarEnlace()
        {
            CargaBateria += 35;
            NivelSincronia += 25.0;
            AmenazasEntorno -= 20.0;
            Console.WriteLine("[ENLACE]: Parámetros de sincronía reestabilizados.");
        }

        public bool EvaluarFalloCritico()
        {
            return NivelSincronia <= 20.0 && AmenazasEntorno >= 60.0 && _marcaIrisDetectada;
        }
    }
}