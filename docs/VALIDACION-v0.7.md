# Validación v0.7

Unity 6000.3.24f1, URP, Windows 64 bits.

- Unity recompiló los scripts sin errores; consola con cero errores y advertencias al iniciar Play.
- Play Mode avanzó más de 500 frames. Se inspeccionó la captura del juego con HUD y el parque en movimiento.
- El ambiente usa un máximo de 52 hojas activas y no asigna físicas individuales. Polvo y recogida son ráfagas de ParticleSystem con límite por efecto.
- Se mantiene la escena ParquePrincipal y la sesión/guardado global de v0.6. No se agregaron atracciones ni mecánicas.

Las pruebas automáticas amplias del parque están documentadas en [v0.6](VALIDACION-v0.6.md). Esta pasada no incluye perfiles comparativos de FPS ni una medición en hardware distinto. La grabación adjunta se conserva como referencia de estilo y no se reutilizaron sus recursos.
