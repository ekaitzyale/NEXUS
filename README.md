# NEXUS

Aplicación de consola desarrollada en C# y .NET 10. NEXUS simula un sistema de navegación y exploración con un cadete, unidades de apoyo, amenazas y distintos subsistemas.

## Funcionalidades

- **Exploración del terreno:** consume batería y energía, y puede generar encuentros o hallazgos.
- **Sondeo IRIS:** registra amenazas electromagnéticas y activa una marca de alerta.
- **Consola IRIS:** muestra registros de comunicación.
- **Cartografía:** genera mapas 2D de sectores estables y anómalos mediante matrices.
- **Inventario:** permite registrar y consultar objetos encontrados.
- **Interacción con unidades:** incluye un robot de apoyo y una entidad enemiga.
- **Estabilización del sistema:** recupera recursos y ajusta las métricas del enlace.
- **Salida controlada:** permite cerrar la simulación desde el menú.

## Tecnologías y conceptos

- C#
- .NET 10
- Programación orientada a objetos (POO)
- Clases abstractas, herencia, encapsulamiento y sobrescritura de métodos
- Arreglos, listas y matrices

## Requisitos

- SDK de [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)

Comprueba la instalación con:

```bash
dotnet --version
```

## Cómo ejecutar

Desde la carpeta raíz del repositorio, ejecuta:

```bash
dotnet run --project Nexus.csproj
```

También puedes abrir `Nexus.slnx` con una versión compatible de Visual Studio o del entorno .NET que utilices.

## Estructura del repositorio

```text
NEXUS/
├── src/
│   ├── Program.cs
│   ├── Entidades/
│   │   ├── Cadete.cs
│   │   ├── Enemigo.cs
│   │   ├── Objeto.cs
│   │   ├── Personaje.cs
│   │   └── Robot.cs
│   └── Sistemas/
│       ├── ConsolaIris.cs
│       ├── EnlaceSincronico.cs
│       ├── Inventario.cs
│       └── MapaRealidad.cs
├── docs/
│   └── Manual-Tecnico.pdf
├── Nexus.csproj
├── Nexus.slnx
└── README.md
```

## Documentación

Consulta el [Manual Técnico de NEXUS](docs/Manual-Tecnico.pdf) para acceder al documento incluido en el repositorio.

## Estado del proyecto

Proyecto educativo en desarrollo. Esta organización separa el código por responsabilidad y mantiene el punto de entrada y los archivos de configuración en la raíz.
