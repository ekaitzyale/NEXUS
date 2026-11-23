# INSTRUCCIÓN MAESTRA — CODEX / NEXUS PROTOCOLO IRIS

Trabaja **sobre el repositorio local NEXUS existente**, rama `desenvolvimento-v2`. Antes de tocar archivos, inspecciona solución, `WindowsForms/WindowsForms.csproj`, `Program.cs`, `IrisGameForm.cs`, `CircuitPuzzle.cs`, README y cambios sin confirmar. **No reinicies el proyecto, no sobrescribas trabajo existente y no hagas push sin autorización**. Los `obj/` y `bin/` son artefactos generados; no incluirlos en commits.

## Fuentes de verdad y precedencia
1. Este paquete `01_Documentacao/REQUISITOS_Y_NARRATIVA.md` y los JSON de `02_Narrativa/` para reglas y textos; 2. Documento técnico `01_Documentacao/NEXUS_Documento_Tecnico_v1.docx` para arquitectura; 3. Imágenes en `03_Visuais_Aprovados/` como referencia de estilo/composición; 4. Código actual para reutilización. Si hay contradicción entre mockup y reglas escritas, gana la regla escrita. Si una decisión no está especificada, informar y elegir solución mínima reversible sin cambiar historia.

## Implementar en esta entrega
C# .NET 10 Windows Forms con SQL Server; cuatro fases, 18 salas y recorrido completo; menús, nombre de Cadete, continuar→cargar, configuración, pausa visible, guardado/carga, mapas top-down, movimiento ortogonal, interacción E, encuentros solo por ENFRENTAR, combate con PA/defensa/reserva/kit/cargas JARVIS, minijuego existente, jefatura GUARDIÁN-01, resultados, game over, fase completa y final. Implementar TODOS los textos de `02_Narrativa/` con disparadores y progresión narrativa, documentos solo en su terminal y sin narración; no escribir textos en imágenes estáticas. No reemplazar los textos por placeholders ni inventar diálogos. Al mostrar texto de JARVIS, utilizar la plantilla visual aprobada con esfera y Cadete; el primer evento «Conexión establecida.» es exclusivamente de voz y no debe generar panel o subtítulo.

## Posponer SOLO recursos de audio y polimento
No generar ni integrar aún MP3/WAV de JARVIS, música ni efectos sonoros; dejar `AudioManager` con interfaz preparada y claves de audio basadas en los IDs de JARVIS. Sin sonido de pasos. Posponer transiciones y efectos no esenciales. No posponer textos, documentos, progresión ni guardado.

## Entrega y verificación
Entregar proyecto compilable en Windows/Visual Studio, scripts SQL, configuración de conexión sin secretos en Git, README, pruebas de reglas de combate, circuito y persistencia, recorrido funcional de 18 salas, y lista honesta de lo que está listo o pendiente. Hacer cambios incrementales y verificables, sin afirmar que está compilado o probado si no se ejecutó. Pedir permiso antes de operaciones destructivas o commits/push.
