# Arquitectura y estructura del repositorio

Rama de trabajo: `desenvolvimento-v2`. La rama `main` no debe modificarse durante esta reorganización.

## Mapa
| Ruta | Responsabilidad |
| --- | --- |
| `Nexus.slnx` | Solución que contiene el proyecto de consola y Windows Forms |
| `Nexus.csproj` | Proyecto original de consola |
| `src/` | Código del proyecto original |
| `WindowsForms/WindowsForms.csproj` | Juego gráfico en .NET 10 para Windows |
| `WindowsForms/GameLogic/GameSession.cs` | Estado de la partida, salas, enemigos, inventario y progresión |
| `WindowsForms/GameLogic/CombatTurn.cs` | Acciones, PA, daño, defensa y curación |
| `WindowsForms/Forms/IrisGameForm*.cs` | Interfaz del juego, exploración, arte e inventario |
| `WindowsForms/Forms/CircuitPuzzle.cs` | Rompecabezas de circuitos |
| `WindowsForms/Services/NarrativeCatalog.cs` | Acceso al contenido narrativo |
| `WindowsForms/Data/SaveRepository.cs` | Guardado, listado, carga y eliminación de partidas en SQL Server |
| `sql/01-inicializar.sql` | Creación de la base `NexusIris` y tabla `dbo.Partida` |
| `tests/Phase0/` | Proyecto de verificaciones automatizadas |
| `pacot_codex/01_Documentacao/` | Documentación y requisitos originales |
| `pacot_codex/02_Narrativa/` | JSON de diálogos, documentos, salas y textos |
| `pacot_codex/03_Visuais_Aprovados/` | Referencias visuales aprobadas |
| `pacot_codex/04_Referencias_Adicionais/` | Referencias adicionales |
| `pacot_codex/05_Integracao/` | Instrucciones y control de calidad de integración |
| `docs/` | Índice, arquitectura y material académico |

## Dependencias importantes
1. `Nexus.slnx` referencia `Nexus.csproj` y `WindowsForms/WindowsForms.csproj`.
2. `WindowsForms/WindowsForms.csproj` incorpora `../pacot_codex/02_Narrativa/*.json` como recursos embebidos bajo `Narrativa.*`. **No mover ni renombrar estos JSON sin actualizar el proyecto y verificar la carga.**
3. `tests/Phase0/Nexus.Phase0.Checks.csproj` referencia `../../WindowsForms/WindowsForms.csproj`.
4. `SaveRepository` utiliza `Microsoft.Data.SqlClient`, `dbo.Partida` y, por defecto, la instancia SQL Server Express local. La conexión se puede cambiar mediante `NEXUS_SQL_CONNECTION`.
5. Los archivos de compilación `bin/` y `obj/`, además de `.vs/` y `*.user`, deben permanecer fuera del control de versiones.

## Operación
```powershell
dotnet build Nexus.slnx
dotnet run --project WindowsForms/WindowsForms.csproj
dotnet run --project tests/Phase0/Nexus.Phase0.Checks.csproj
```

Para la instalación de SQL Server y las condiciones de prueba, consultar el [README principal](../README.md).

## Reglas de mantenimiento
- Trabajar sobre `desenvolvimento-v2`; no fusionar automáticamente con `main`.
- Conservar el código de consola y Windows Forms como proyectos distintos.
- No renombrar clases, rutas, archivos JSON o proyectos por motivos estéticos sin ajustar todas las referencias.
- No eliminar referencias visuales ni documentación original sin una auditoría específica.
- Antes de integrar cambios funcionales, ejecutar compilación y pruebas y comprobar manualmente las pantallas y el guardado.
