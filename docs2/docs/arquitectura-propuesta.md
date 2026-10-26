# Arquitectura Propuesta (NEXUS v3.0)

## 1. Patrón de Arquitectura por Capas
Para evolucionar hacia una aplicación de escritorio robusta, se estructurará la solución en tres capas claramente desacopladas:
1. **Capa de Presentación (UI - Windows Forms):** Formularios interactivos (`FormMenu`, `FormMapa`, `FormInventario`, `FormMisiones`) encargados de capturar acciones del usuario y renderizar estados.
2. **Capa de Lógica de Negocio (Core / Domain):** Reutilización íntegra de las clases POO de H3 (`Personaje`, `Cadete`, `Robot`, `Inventario`, etc.) para mantener intactas las reglas del dominio.
3. **Capa de Acceso a Datos (DAL):** Módulos de conexión mediante ADO.NET (`SqlConnection`, `SqlCommand`) para la comunicación directa con SQL Server.

## 2. Diagrama de Arquitectura
[ Windows Forms (UI) ] ──> [ Lógica de Negocio (POO / H3) ] ──> [ ADO.NET (DAL) ] ──> [ SQL Server ]
