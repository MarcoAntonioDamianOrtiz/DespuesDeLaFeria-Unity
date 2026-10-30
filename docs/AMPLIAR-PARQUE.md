# Ampliar el parque continuo

La base v0.6 está preparada para ampliar el mismo lugar, conservando los dos puestos actuales y el recorrido físico. No hay un sistema de transporte entre atracciones.

## Plano y capacidad actual

ParquePrincipal.asset define el ID del parque, límites globales, entrada, inicio del carrito, catálogo y parcelas futuras. La envolvente inicial es de 200 × 200 m. El recinto construido tiene unos 104 × 98 m. El suelo y la composición exterior se prolongan más allá de las puertas, con jardines y vegetación.

| Parcela | Centro aproximado X/Z | Acceso actual X/Z | Reserva |
|---|---|---|---|
| Oeste | -76 / 45 | -52 / 45 | 36 × 32 m |
| Este | 76 / 45 | 52 / 45 | 36 × 32 m |
| Noroeste | -28 / 118 | -28 / 88 | 36 × 32 m |
| Noreste | 28 / 118 | 28 / 88 | 36 × 32 m |

Son áreas reservadas. Sus puertas físicas indican En restauración. No se representan como cuatro minijuegos terminados ni contienen tareas inventadas para llenar espacio. Se puede continuar extendiendo el plano al norte y ajustar sus límites.

## Añadir una atracción

1. Elegir una parcela y diseñar el camino y sus vistas antes de añadir la actividad.
2. Crear una raíz de atracción con escala 1, posición y giro propios. Las coordenadas de tareas y juego son locales a esa raíz.
3. Crear una FairZoneDefinition con ID estable, tipo de actividad, cantidad de residuos/superficies, límites locales, rectángulo de activación, entrada, anclas locales compatibles y cámara de actividad. El área no tiene que medir lo mismo que las canicas.
4. Crear los objetos y sus identidades. Cada ID debe permanecer asociado a su objeto aunque se reordene una lista. El ID de zona separa los espacios de nombres.
5. Añadir la definición al catálogo y el contexto FairGame bajo ParkSession. La sesión descubre sus contextos al iniciar, comparte jugador/carrito/HUD y calcula el progreso total sin un número fijo de dos puestos.
6. Implementar la lógica y presentación del nuevo juego. Los tipos actuales son canicas y patitos; un juego diferente requiere su servicio y controles/presentación correspondientes. El plano no finge un sistema universal de minijuegos ya terminado.
7. Abrir físicamente su acceso y conectar el camino, bordes, señales y ambiente. No añadir un botón de carga de escena.
8. Probar llegada a pie, salida, recorrido con carrito, guardado, recarga y datos de versiones anteriores.

Un puesto nuevo puede empezar con estado inicial cuando no haya un registro suyo en la partida. Cambiar la cantidad o las identidades de un puesto publicado necesita migración de contenido; no se reasigna progreso silenciosamente.

## Mantener continuidad

El jugador, la cámara, el HUD y el carrito pertenecen a ParkSession. Ningún nuevo puesto debe traer copias de esos objetos. Los límites del jugador y carrito son los globales; los límites del confeti siguen perteneciendo a su zona. La entrada/salida de un área solo cambia objetivos y ayuda, con margen para evitar parpadeo en el borde.

Las cámaras de actividad enfocan la atracción y recuperan la vista anterior; el cuerpo permanece donde estaba. Los carteles de paseo orientan con flechas y nombres.

## Crecimiento y rendimiento

La versión actual mantiene todo el circuito en una escena. Los registros y orígenes de atracción dejan una separación útil para futuros sectores, pero no se ha implementado streaming. Antes de hacerlo se debe medir memoria, CPU, renderizado y la cantidad real de residuos/props.

Si resulta necesario cargar sectores, conservar la sesión global, registrar los contextos al cargarlos y mantener en memoria el progreso de sectores descargados. Nunca guardar solo los sectores presentes ni descargar el suelo bajo jugador o carrito. Precargar antes del acceso y mantener las vistas y la iluminación coherentes.

## Generadores y edición

ContinuousParkBuilder.Build reconstruye ParquePrincipal usando las escenas anteriores como kits. Sustituye la escena generada; se usa deliberadamente y no es necesario para jugar. Después de ampliar manualmente el mapa, no ejecutarlo esperando que conserve esas ediciones.

PolishPaths actúa sobre los caminos y la arboleda del circuito original. Para una ampliación real, ampliar también la fuente del trazado o editar sus prefabs/mallas conscientemente. Los assets del plano, las definiciones y el catálogo son editables; la geometría, cercas y accesos deben acompañar esos cambios.
