# ACTIVIDAD 6 — PRUEBAS Y DEPURACIÓN

**Proyecto:** NEXUS — PROTOCOLO IRIS  
**Rama de trabajo:** `testing-debugging`  
**Tecnologías:** C#, Windows Forms y SQL Server  
**Fecha:** 23/11/2026

## 1. Objetivo

Comprobar el funcionamiento del sistema de guardado y recuperación de partidas mediante herramientas de depuración de Visual Studio.

## 2. Procedimiento realizado

Se ejecutó el videojuego desde Visual Studio en modo depuración.

En el archivo `SaveRepository.cs`, dentro del método `SaveAsync()`, se colocó un punto de interrupción (*breakpoint*) en la instrucción:

```csharp
await connection.OpenAsync();
```

Posteriormente, se guardó una partida desde el menú de pausa.

Al llegar al punto de interrupción, se utilizó la tecla **F10** para ejecutar el código paso a paso.

## 3. Resultados obtenidos

- La conexión con SQL Server se abrió correctamente.
- La transacción SQL se inició sin errores.
- El videojuego continuó funcionando después de la depuración.
- Se guardó una partida en la fase 1, sala 04.
- Se cerró y volvió a abrir el videojuego.
- Al cargar la partida, se comprobó que el enemigo derrotado continuaba eliminado.

## 4. Conclusión

Las pruebas realizadas permitieron verificar el funcionamiento de la conexión a SQL