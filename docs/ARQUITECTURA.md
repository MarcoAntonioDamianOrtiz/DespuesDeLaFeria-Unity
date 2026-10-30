# Arquitectura actual

## Componentes

| Componente | Responsabilidad |
|---|---|
| `FairPlayer` | Movimiento, cámara, controles y representación de herramientas |
| `FairGame` | Simulación ligera de residuos, tareas, feedback, audio y persistencia |
| `FairHud` | Indicadores, ayuda, pausa y ajustes |
| `LocalSaveStore` | Escritura de archivos y recuperación desde el respaldo, verificable en una carpeta aislada |
| `FairSceneBuilder` | Construcción de la escena inicial de prueba |
| `FairArtPass` | Generación de arte original y aplicación sobre la escena |

La escena preparada es el punto de entrada. Las referencias están serializadas, y los assets visuales se guardan en el proyecto. La autoría se ejecuta en el editor; el jugador no necesita generar assets ni descargar contenido.

## Residuos

720 posiciones estables y velocidades ligeras, sin un Rigidbody por confeti. El barrido aplica fuerza en un radio visible; el rozamiento frena el movimiento. El recogedor retira los residuos que alcanzan su entrada. Al superar el 97%, se retiran los fragmentos finales para evitar una búsqueda minuciosa.

## Persistencia

Se mantiene el formato de partida **v1** y los índices de los 720 residuos y 16 manchas. Esta pasada artística no altera esa correspondencia, por lo que las partidas de la primera prueba siguen siendo utilizables.

`Application.persistentDataPath/feria-slot-1.json` almacena posiciones, residuos retirados, limpieza, objetivos, carrito, posición del jugador y tiempo. Se valida antes de aplicar sus datos. La escritura temporal sustituye al archivo principal conservando `.bak`; si el principal no puede leerse, se intenta recuperar el respaldo.

Las preferencias `feria.volume` y `feria.sensitivity` usan PlayerPrefs. Se guardan al pausar y salir. No contienen progreso de campaña.

## Alcance pendiente

La escritura y lectura están separadas en `LocalSaveStore`. La simulación sigue agrupada en `FairGame`; su división en más servicios se hará cuando exista una segunda zona que justifique compartirlos. Steam Cloud y Steamworks no están integrados. El repositorio remoto es una herramienta de desarrollo, no una dependencia de la partida.

## Regeneración

El menú **Feria > Crear primera zona** sustituye la escena por su versión básica; usarlo únicamente para regenerarla deliberadamente. Después se aplica **Feria > Aplicar arte original v0.2**. Para cambios normales, editar la escena y prefabs existentes.
