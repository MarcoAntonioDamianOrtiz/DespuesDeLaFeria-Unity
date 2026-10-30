# Después de la feria · Versión 0.2

La documentación actual está en [README.md](README.md). Esta versión añade arte original, ayuda con Tab y ajustes de volumen y sensibilidad. La compilación actual para Windows está en la carpeta hermana `Feria-Windows-v0.2`.

Proyecto de Unity 6.3 LTS (6000.3.24f1), Universal 3D / URP.

## Abrir y jugar

Abre este directorio desde Unity Hub. Abre `Assets/Feria/Scenes/PrimeraManana.unity`, pulsa Play y elige **Entrar a la feria**.

También puedes pulsar Enter para entrar o reanudar. La versión independiente para Windows está en la carpeta hermana `Feria-Windows`: abre `DespuesDeLaFeria.exe` y conserva junto al ejecutable las demás carpetas de esa versión.

- WASD: caminar. Ratón: mirar.
- Mantener clic izquierdo: usar la herramienta.
- 1: escoba. 2: paño. Rueda: cambiar herramienta.
- E: colocar cartel, mover/estacionar carrito, encender puesto o soltar canica, según lo que estés mirando.
- Esc: pausa. F5: guardar.

Acerca el carrito a un montón. Su recogedor amarillo recibe los residuos: barre hacia él. Limpia las manchas del mostrador con el paño, coloca el cartel caído y acciona el interruptor del lado derecho del puesto. Cuando vuelva la luz, acércate a la canica para probar el mecanismo.

## Alcance

Una zona pequeña y arte provisional modelado con formas simples. Incluye movimiento en primera persona, barrido, recogida con carrito, limpieza de mostrador, cartel, reactivación de luces y una animación de canica. Audio generado localmente; no requiere descargas durante la partida.

Esta versión permite evaluar los controles y el ciclo de trabajo. No está validado que sostenga diez minutos de diversión. No incluye todavía campaña, economía, mejoras, separación de residuos, reasignación de controles ni sincronización con Steam.

## Guardado

Se guarda automáticamente cada 25 segundos de juego, al estacionar el carrito y completar ciertas interacciones, y al salir. Usa `Application.persistentDataPath` bajo la empresa **FeriaIndie** y producto **Despues de la feria**. Archivo `feria-slot-1.json`, respaldo `.bak`, versión 1. El archivo temporal se reemplaza de forma atómica. Si el principal no puede leerse, se intenta el respaldo.

## Organización

- `Assets/Feria/Scripts/FairPlayer.cs`: movimiento, vista y controles.
- `Assets/Feria/Scripts/FairGame.cs`: residuos, tareas, audio y guardado.
- `Assets/Feria/Scripts/FairHud.cs`: interfaz y pausa.
- `Assets/Feria/Editor/FairSceneBuilder.cs`: construcción reproducible de la escena. El menú **Feria > Crear primera zona** recrea la escena; úsalo únicamente para regenerarla, pues sustituye su contenido actual.
- `Assets/Feria/Art`: materiales, fuente y ambiente.

No se ha publicado en servicios externos ni creado un repositorio remoto.
