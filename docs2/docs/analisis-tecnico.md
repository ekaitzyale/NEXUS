# Análisis Técnico y Justificación

## 1. Evaluación de Tecnologías para Interfaz Gráfica
- **WPF (Windows Presentation Foundation):** Ofrece gran flexibilidad gráfica mediante XAML, pero presenta una curva de aprendizaje elevada y mayor sobrecarga para los requisitos actuales del proyecto académico.
- **Windows Forms (Seleccionado):** Destaca por su integración nativa y directa con .NET, facilidad en la manipulación de controles visuales estándar (`DataGridView`, `PictureBox`) y rapidez de implementación sobre la lógica orientada a objetos ya existente.

## 2. Estrategia de Persistencia
- Se descarta el almacenamiento en archivos planos (JSON/XML) por falta de integridad transaccional y seguridad ante concurrencia.
- Se selecciona **SQL Server mediante ADO.NET directo** para garantizar un control absoluto sobre las consultas T-SQL ejecutadas, optimizando el rendimiento y cumpliendo estrictamente con los requerimientos del proyecto.
