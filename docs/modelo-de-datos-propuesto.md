# Modelo de Datos Propuesto (SQL Server)

## 1. Esquema Relacional
- **`Jugador`**: `IdJugador` (PK, INT), `Nombre` (VARCHAR), `FechaCreacion` (DATETIME).
- **`Personaje`**: `IdPersonaje` (PK, INT), `IdJugador` (FK), `Nombre` (VARCHAR), `Salud` (INT), `TipoPersonaje` (VARCHAR: 'Cadete', 'Robot', 'Enemigo'), `PosicionX` (INT), `PosicionY` (INT).
- **`Inventario`**: `IdInventario` (PK, INT), `IdPersonaje` (FK), `CapacidadMaxima` (INT).
- **`Objeto`**: `IdObjeto` (PK, INT), `IdInventario` (FK), `Nombre` (VARCHAR), `TipoObjeto` (VARCHAR), `ValorEfecto` (INT).

## 2. Estrategia de Persistencia
Se emplearán **procedimientos almacenados en T-SQL** para ejecutar transacciones atómicas que aseguren el guardado y carga síncrona del estado completo del mapa, los personajes y sus inventarios asociados.
