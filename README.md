# NEXUS — PROTOCOLO IRIS

Juego de escritorio en C# con .NET 10, Windows Forms y SQL Server. La campaña consta de cuatro fases y 18 salas.

## Instalación en Windows

1. Instala el SDK de .NET 10 y SQL Server. Para trabajar en Visual Studio, habilita la carga de trabajo **Desarrollo de escritorio de .NET** en una versión compatible con .NET 10.
2. Ejecuta [el script de inicialización](sql/01-inicializar.sql) con una cuenta que pueda crear la base de datos. En SSMS se puede abrir y ejecutar como script. Con SQL Server Express local y autenticación de Windows también puedes usar:

   ```powershell
   sqlcmd -S 'lpc:.\SQLEXPRESS' -E -C -b -i sql/01-inicializar.sql
   ```

3. Abre `Nexus.slnx` y selecciona **WindowsForms** como proyecto de inicio, o ejecuta:

   ```powershell
   dotnet run --project WindowsForms/WindowsForms.csproj
   ```

La conexión predeterminada utiliza la instancia local `SQLEXPRESS` por memoria compartida y la autenticación de Windows. Para otra instancia, configura la variable de entorno `NEXUS_SQL_CONNECTION` antes de iniciar el juego. La cuenta del juego necesita permisos de lectura, inserción, actualización y eliminación sobre `dbo.Partida`. Mantén las credenciales fuera del repositorio.

El proyecto de consola anterior sigue disponible en `Nexus.csproj`. Los cuatro JSON del paquete se incorporan al ejecutable durante la compilación.

## Fase 0

- Muévete con WASD o las flechas e interactúa con E. Para leer la orden de mantenimiento, acércate al terminal de la Sala 01.
- En Entrenamiento, acércate al robot y pulsa **ENFRENTAR**. Confirma al menos una carga de JARVIS antes de finalizar un turno. Revisa el resultado y pulsa **CONTINUAR**.
- Restablece el circuito de la Sala 03 y sal por la puerta para completar la fase.
- Usa **GUARDAR PARTIDA** en la pausa. **CONTINUAR** en el menú abre los guardados. También se guarda al completar la fase.

## Inventario y puntos de acción

- Durante la exploración y el combate, pulsa **I** o **INVENTARIO** para consultar botiquines, objetos de misión y documentos encontrados. Pulsa **I** de nuevo o selecciona **CERRAR** para regresar.
- La primera lectura de cada documento ocurre en su terminal. Después puedes volver a leerlo desde el inventario, sin narración.
- El inventario es de consulta. Puedes llevar hasta cinco botiquines y usarlos en combate con **USAR KIT**. Cada uso cuesta 1 PA y recupera hasta 30 HP.
- El Cadete comienza cada turno con 3 PA básicos. Cada acción **RESERVAR** aporta +1 PA al siguiente turno y cada carga de JARVIS aporta +1 PA al actual. El total del turno está limitado a 10 PA. Los PA asignados a acciones todavía cuentan para el límite hasta que se resuelva el turno.

## Fase 1 — Señal Perdida

- Desde la pantalla de fase completada del Tutorial, selecciona **CONTINUAR** e inicia **SEÑAL PERDIDA**. Se conservan la salud, los botiquines y las cargas de JARVIS.
- Recorre Corredor Central, Enfermería, Sala Eléctrica y Comunicaciones. Derriba las unidades de las salas 04 y 06, recoge el botiquín de la sala 05 y restablece el circuito de la sala 06.
- Lee los documentos opcionales en los terminales de sus salas. En Comunicaciones, recoge el chip de información antes de salir. La fase se guarda al terminar y desbloquea la Fase 2.
- **REINTENTAR** restaura los recursos con los que el Cadete entró en Señal Perdida y conserva el Tutorial completado.

## Fase 2 — Protocolo IRIS

- Completa Señal Perdida y selecciona **CONTINUAR**. La entrada en Seguridad exige el chip de Comunicaciones. Se conserva el inventario y el estado del Cadete.
- Recorre Seguridad, Depósito, Laboratorio A, Laboratorio B, Archivo Científico e Investigación del NÚCLEO ZERO. Pulsa **ENFRENTAR** junto a cada robot. En Laboratorio A, restablece el circuito con **E**.
- Consulta los documentos en sus terminales y vuelve a leerlos desde **INVENTARIO**. En la sala 13, usa **E** junto al equipo de investigación antes de salir.
- El juego guarda al completar Protocolo IRIS y desbloquea Última Transmisión en la selección.

## Fase 3 — Última Transmisión

- En Centro de Control, restablece el circuito con **E**. Desactiva el Generador A y después el Generador B antes de entrar en la cámara de seguridad.
- Hay un botiquín en Centro de Control y otro en Generador B. El robot común de Generador A está fuera del paso principal. En la sala 17, vence a **GUARDIÁN-01** para acceder al Núcleo Central.
- Interactúa con el equipo del Núcleo Central para completar la misión. El juego guarda el desenlace y lo vuelve a mostrar al cargar esa partida.

La voz, la música, los efectos y el pulido visual están pendientes.

## Verificación de la campaña

```powershell
dotnet run --project tests/Phase0/Nexus.Phase0.Checks.csproj
```

La ejecución básica verifica las reglas de combate, los recursos narrativos y la solvencia de circuitos generados. Con `NEXUS_TEST_SQL_CONNECTION` configurada para una base de pruebas inicializada con el mismo esquema, el ejecutor verifica el repositorio SQL y recorre las salas 01–18 mediante los controles WinForms. Crea y elimina únicamente sus propias partidas de prueba. Configura `NEXUS_TEST_CAPTURES` con una carpeta de salida si quieres guardar capturas del recorrido.

## Organización y material académico

- [Índice de documentación](docs/README.md)
- [Arquitectura y estructura del repositorio](docs/ARQUITECTURA.md)
- [Guía de estudio para el examen de Programación](docs/examen/GUIA_ESTUDIO.md)

La rama `desenvolvimento-v2` conserva el proyecto original de consola y el juego Windows Forms en una misma solución. Las rutas de código, JSON, SQL y pruebas se mantienen sin cambios para preservar sus dependencias.
