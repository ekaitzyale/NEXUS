# Reglas de selección visual

- Las 20 imágenes suministradas por el usuario se consideran **referencias seleccionadas**, no sprites finales ni una interfaz ya funcional. Las correcciones de textos y valores deben aplicarse al construir controles de Windows Forms, sin pintar texto estático sobre PNG.
- `03_Visuais_Aprovados/` contiene las imágenes que el usuario volvió a adjuntar; conservar la composición y no usar las 64 alternativas antiguas.
- JARVIS: **referencia oficial** `JARVIS_MODELO_OFICIAL.png` (esfera no humanoide); descartar imágenes con cara humana.
- Documento: `DOCUMENTO_MODELO_APROVADO.png`; plantilla única, datos de `documentos_encontrables.json`, lectura sin voz y solo en terminal.
- `04_Referencias_Adicionais/PRANCHA_CONCEPTUAL_NO_USAR_COMO_ATLAS.png` es una referencia de conjunto generada, NO un conjunto de 20 pantallas listas para recortar; no se debe usar como recurso de producción.
- Menú principal: NUEVA PARTIDA, CONTINUAR, CONFIGURACIÓN, SALIR; quitar CRÉDITOS. CONTINUAR abre CARGAR PARTIDA.
- Pausa: botón visible en exploración, REANUDAR / CONFIGURACIÓN / GUARDAR PARTIDA / SALIR AL MENÚ. El diseño específico aún necesita validación del usuario; usar el estilo aprobado, sin declararlo aprobado.
- No asumir que un texto erróneo de un mockup cambia reglas: kit +30 HP, robot común 40 HP, Fase 2 salas 08–13, circuito 5×5 flechas verde/naranja.
- La voz aprobada del JARVIS es Vector – Tec Voice (ID NoiYxL9g25M4orC8Q0ls); archivos de voz se integrarán más tarde.
