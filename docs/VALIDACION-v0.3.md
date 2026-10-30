# Verificación v0.3

## Comprobaciones del núcleo

`FairVerification.Run` pasó los controles de barrido, recogida, ayuda contextual, tareas, luces, canica, restauración visual, preferencias, referencias del arte, normales de mallas y recuperación de archivos corruptos. La consola final del editor quedó sin errores ni advertencias.

## Recorrido completo automatizado

`FairJourneyVerification.Run` pasó las siguientes comprobaciones:

- Completar los 720 residuos mediante impulsos de barrido y recogida; no desactivar piezas para simular limpieza.
- Impedir la reactivación mientras faltan mostrador y cartel.
- Completar las tareas, activar el puesto y recorrer toda la pista de canicas.
- Mantener el archivo de campaña intacto durante limpieza libre, con el autoguardado activo y almacenamiento de prueba aislado.
- Recuperar el progreso previo al salir del ensayo.
- Rechazar un guardado con identidad de otra zona.

El controlador automático utilizó 2.213 pasos de 20 ms. Llama a la API de herramientas con objetivos directos; no representa la navegación de una persona ni una estimación de duración o diversión.

## Inspección visual

Se inspeccionaron capturas de la escena y del menú. Se ajustó la escala del menú para mantenerlo dentro de la altura disponible. Se revisaron las fuentes del arte, el nuevo ambiente y las referencias de materiales.

## Observación humana pendiente

La duración, comodidad durante diez minutos y deseo de repetir siguen pendientes de evaluación humana. El modo de limpieza libre y `PRUEBA-DE-SESION.md` permiten realizarla sin sobrescribir la partida.

## Ejecutable de Windows

La compilación final terminó con cero errores y cero advertencias. Se abrió la versión independiente, se inspeccionaron su cielo y ambiente, y se inició limpieza libre desde el menú: reaparecieron los residuos y tareas pendientes conservando el avance de campaña. El registro consultado durante la comprobación no mostró excepciones ni errores de shaders.
