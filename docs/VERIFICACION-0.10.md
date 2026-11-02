# Verificación de la fase 0.10

Revisión en Unity 6000.3.24f1, escena `ParquePrincipal`, 1 de noviembre de 2026 (hora de México).

## Resultados en Play

- `ContinuousParkVerification.Run()`: pasó. Una escena, jugador/carrito/HUD/oyente únicos, cuatro parcelas para ampliación y recorrido físico de 5.495 pasos con CharacterController sin saltos entre zonas.
- Ciclo de tareas: pasaron 1.200 residuos, barrido y recogida, dos mostradores, carteles, reactivación y luces; canica y tres aros en puestos desplazados y girados.
- Persistencia: pasó el guardado global v3, migración v2, recuperación desde respaldo, rechazo de posiciones inválidas y protección de versiones posteriores. Las pruebas usan archivos aislados y recuperan el estado anterior del parque.
- `FairCampaignVerification.Run()`: pasó. Migración v1 a v2, progreso independiente, identidades estables al reordenar residuos, respaldo y rechazo de duplicados.
- `FairPolishVerification.Run()`: pasó. Dieciséis manchas orgánicas separadas por mostrador, reducción proporcional al limpiar y recuperación de su escala, relieve presente, filtros separados para brisa y pájaros, muestras de audio finitas y sin saturación, silencio inmediato de herramientas al pausar.
- El contador de frames avanzó durante la sesión. Cero componentes de script perdidos; tras las pruebas corregidas, consola sin errores ni advertencias.
- Compilación Windows Standalone64 regenerada: `Succeeded`, cero errores, cero advertencias; 115.083.531 bytes según el informe de Unity. El ejecutable queda en `outputs/Feria-Windows-v0.10/DespuesDeLaFeria.exe` junto a sus datos.

## Ajustes derivados de la revisión

El filtro de la brisa ya no atenúa los trinos. Las fuentes se configuran antes de activarlas para evitar el aviso de audio sin clip. Los pasos usan una fuente propia, por lo que su variación de tono no altera campanillas o notas. La distancia de pasos se mide después de aplicar el límite del mapa.

Se redujeron y redistribuyeron las manchas para separar mejor sus siluetas, conservando sus dieciséis identificadores. El verificador del parque recupera la opción de guardado que tenía al comenzar.

La caminata se revisó en el código y mediante recorrido físico programado. La captura cercana del mostrador se inspeccionó visualmente. Estas comprobaciones no equivalen a una sesión humana de diez minutos ni juzgan la comodidad de cámara o la calidad percibida del sonido.
