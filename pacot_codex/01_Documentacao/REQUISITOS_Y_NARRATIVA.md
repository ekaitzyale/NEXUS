# NEXUS — PROTOCOLO IRIS | Especificación funcional y narrativa

**Estado:** documento de entrega para implementación, basado en acuerdos de diseño. Los textos narrativos de este paquete son una redacción de implementación que sigue el arco acordado; si una línea exacta de un borrador anterior no consta en los materiales accesibles, no se presenta como cita histórica.

## Alcance innegociable
C# .NET 10, Windows Forms, SQL Server, cuatro fases, 18 salas, año 2187. La nave IRIS lleva un mes sin responder. El Cadete investiga con JARVIS, orbe cian no humanoide. La inteligencia de la nave no es el villano. El NÚCLEO ZERO y la última transmisión se revelan gradualmente. Final: «No estamos solos. Y nunca lo estuvimos».

## Inicio, medio y fin
**Inicio:** Tutorial y Señal Perdida. Se introducen los controles, el primer combate, el circuito, la desaparición de las comunicaciones y el chip de acceso. **Medio:** Protocolo IRIS. Seguridad, laboratorios y archivos aportan pruebas crecientes de una anomalía y de medidas de contención, sin anticipar la conclusión. **Final:** Última Transmisión. Centro de Control → Generador A → Generador B → GUARDIÁN-01 → Núcleo Central; conclusión solo al final.

## Fases y salas
- **Fase 0 — Tutorial / Sala 01 — Mantenimiento:** Recuperar el kit inicial y reconocer los controles.
- **Fase 0 — Tutorial / Sala 02 — Entrenamiento:** Superar el primer encuentro y aprender el uso de JARVIS.
- **Fase 0 — Tutorial / Sala 03 — Acceso Principal:** Restablecer la conexión del circuito de acceso.
- **Fase 1 — Señal Perdida / Sala 04 — Corredor Central:** Examinar el primer sector sin comunicación.
- **Fase 1 — Señal Perdida / Sala 05 — Enfermería:** Recuperar un kit y consultar los registros médicos.
- **Fase 1 — Señal Perdida / Sala 06 — Sala Eléctrica:** Restablecer la ruta energética mediante el circuito.
- **Fase 1 — Señal Perdida / Sala 07 — Comunicaciones:** Recuperar el chip de información que permite avanzar.
- **Fase 2 — Protocolo IRIS / Sala 08 — Seguridad:** Acceder con el chip y superar dos robots de vigilancia.
- **Fase 2 — Protocolo IRIS / Sala 09 — Depósito:** Recuperar suministros y consultar el inventario de emergencia.
- **Fase 2 — Protocolo IRIS / Sala 10 — Laboratorio A:** Resolver el circuito de acceso al laboratorio.
- **Fase 2 — Protocolo IRIS / Sala 11 — Laboratorio B:** Superar tres robots y examinar los experimentos.
- **Fase 2 — Protocolo IRIS / Sala 12 — Archivo Científico:** Recuperar un kit y leer los registros de investigación.
- **Fase 2 — Protocolo IRIS / Sala 13 — Investigación del NÚCLEO ZERO:** Descubrir el vínculo entre la señal y el protocolo.
- **Fase 3 — Última Transmisión / Sala 14 — Centro de Control:** Resolver el circuito que habilita la secuencia final.
- **Fase 3 — Última Transmisión / Sala 15 — Generador A:** Desactivar el primer generador.
- **Fase 3 — Última Transmisión / Sala 16 — Generador B:** Desactivar el segundo generador.
- **Fase 3 — Última Transmisión / Sala 17 — Cámara del GUARDIÁN-01:** Derrotar al guardián.
- **Fase 3 — Última Transmisión / Sala 18 — Núcleo Central:** Completar la misión y conocer la última transmisión.

## Combate — valores exactos
Cadete: 100 HP máximo, daño 20. Robot común: 40 HP, daño 10. GUARDIÁN-01: 125 HP, daño 20. Tres PA por turno. ATACAR, DEFENDER, RESERVAR y USAR KIT cuestan un PA. DEFENDER bloquea un ataque entrante; RESERVAR suma +1 PA al turno siguiente. Kit: cura 30 HP, máximo 5 unidades. JARVIS: 6 cargas para toda la partida, +1 PA por carga, selección individual en modal con +/−, confirmar/cancelar; uso de al menos una carga en tutorial. No gastar cargas automáticamente. Resultado de turno con CONTINUAR manual. HP y recursos persisten.

## Exploración
Vista superior 2D, WASD/flechas una casilla ortogonal, E interactuar; no diagonal, no minimapa, no sonidos de pasos. ENFRENTAR solo al estar adyacente, sin combate automático. Pausa por botón visible, no por ESC. Cadete traje blanco/gris y visor cian; robots rojos arácnidos.

## Circuitos
Reutilizar **sin alterar la lógica** el circuito giratorio 5×5 existente; flecha verde de entrada y naranja de salida. Puzzles en salas 03, 06, 10 y 14. Solo se permite adaptar apariencia sin modificar solvencia.

## Historia y ventanas de lectura
Los diálogos completos están en `jarvis_dialogos.json`, con ID, fase, sala y disparador. JARVIS usa el modelo visual aprobado (orbe cian, Cadete a la izquierda, panel holográfico, CONTINUAR). Inicio de aplicación «Conexión establecida.» solo voz después del 100% de carga; mientras el audio no esté producido, registrar evento sin mostrar subtítulo o caja. Introducción de Fase 0 con texto después de carga; aperturas de las demás fases con diálogo contextual. Los documentos están en `documentos_encontrables.json`: lectura local por E en el terminal, no los narra JARVIS, no hay biblioteca ARCHIVOS. El contenido es dinámico sobre el mismo panel visual, no texto horneado en PNG.

## Persistencia
SQL Server con esquema y scripts reproducibles. Guardar nombre del Cadete, fase/sala, posición, HP, kits, cargas, flags, enemigos derrotados, objetivos y estado de documentos/terminales; guardar al completar fase y manualmente en pausa; CONTINUAR abre CARGAR PARTIDA, no es un segundo sistema. No desbloquear fases por defecto. La Fase 3 es estrictamente lineal.

## Entregable y criterio de aceptación
Solución Windows Forms compilable, esquema SQL Server, README de instalación, pruebas básicas y recorrido íntegro 01→18, sin audio real en esta primera entrega. No sustituir interfaces por imágenes estáticas; utilizar controles y contenido dinámico. No inventar reglas, diálogos ni rutas nuevas; reportar incertidumbres antes de alterar decisiones.
