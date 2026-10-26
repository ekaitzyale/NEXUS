# Diseño del Juego — NEXUS

## 1. Visión del Juego
NEXUS es un simulador táctico por turnos donde el operador gestiona unidades biológicas (`Cadete`) y mecánicas (`Robot`) frente a amenazas (`Enemigo`). El objetivo es completar misiones de exploración en una cuadrícula, optimizar recursos de energía y gestionar un inventario limitado.

## 2. Ciclo Principal de Juego
1. **Despliegue:** Selección de exploradores y carga de objetos desde el inventario.
2. **Exploración:** Movimiento en la matriz del `MapaRealidad` con consumo de recursos.
3. **Encuentro/Combate:** Resolución de eventos por turnos mediante el método polimórfico `Reaccionar()`.
4. **Progresión:** Actualización de experiencia del cadete y guardado de estado en la base de datos.

## 3. Mecánicas y Reglas
- **Salud y Límites:** La salud se rige estrictamente por `Math.Clamp(valor, 0, 100)`. Los daños negativos son rechazados para evitar curaciones indeseadas.
- **Inventario:** Estructura encapsulada con `CapacidadMaxima` fija. Utiliza consultas LINQ (`Where`, `OrderByDescending`) para filtrar consumibles por `ValorEfecto`.
- **Interfaces:** Objetos interactivos implementan `IActivable` para activar mecanismos (ej. escudos del robot o consolas).

## 4. Diseño de Pantallas (Wireframes)
1. **Menú Principal (`FormMenu`):** Botones de "Nueva Partida", "Cargar Partida", "Opciones" y "Salir".
2. **Mapa de Exploración (`FormMapa`):** Cuadrícula 2D con iconos de entidades, barra de salud y estado de energía.
3. **Gestión de Misiones (`FormMisiones`):** Listado de objetivos activos y estado de sincronización.
4. **Inventario (`FormInventario`):** Visualización en `DataGridView` de objetos con filtros y estadísticas de los personajes.
