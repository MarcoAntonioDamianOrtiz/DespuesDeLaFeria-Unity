# Verificación de la versión 0.2

Pruebas realizadas en Unity 6000.3.24f1 mediante `FairVerification.Run`, disponible en el menú **Feria > Verificar zona en Play**.

- PASS: barrido con contacto y desplazamiento.
- PASS: recogida mediante el carrito.
- PASS: la ayuda señala un residuo pendiente.
- PASS: tareas obligatorias, luces y movimiento de la canica.
- PASS: restauración del cartel y de las bombillas a su estado inicial.
- PASS: aplicación de volumen y sensibilidad y sus preferencias locales.
- PASS: referencias a iconos y paño; seis mallas con normales válidas.
- PASS: guardado, respaldo, corrupción y recuperación en una carpeta aislada; rechazo de una versión de archivo desconocida.

Se comprobó además el avance real de fotogramas en Play y se inspeccionaron capturas del menú y de la escena. La consola quedó sin errores ni advertencias tras la ejecución final.

La compilación final para Windows terminó con cero errores y cero advertencias. Se abrió el ejecutable, se comprobó el menú, se movió el control de volumen y se verificó el cambio al paño con su modelo e icono. El registro del ejecutable corregido no mostró excepciones. El material del área de barrido se referencia como asset para conservar su shader en la compilación.

La verificación restaura el estado del juego en memoria y no escribe en la carpeta de partidas del usuario. No equivale a un estudio de diversión, rendimiento en diferentes equipos o accesibilidad completa. Las teclas aún no son reasignables.
