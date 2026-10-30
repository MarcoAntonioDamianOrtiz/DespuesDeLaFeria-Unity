# Después de la feria

Juego independiente para un jugador, desarrollado con **Unity 6.3 LTS / C# / URP**.

Recupera una pequeña feria al amanecer: barre confeti, lleva los residuos al carrito, limpia el mostrador y devuelve un cartel a su sitio. Cuando terminas, el puesto recupera sus luces y puedes jugar a encestar una canica eligiendo el momento de lanzamiento.

![Primera zona con ambiente de amanecer](docs/images/feria-v04.png)

## Estado: primera zona jugable, versión 0.4

- Confeti original de cinco formas, curvatura y aleteo al barrer; posiciones de guardado estables.
- Escoba con madera biselada, mango, remaches y fibras flexibles.
- Canicas con vista cercana, medidor, cinco dianas, puntuaciones 10/50/100, notas por curva y revancha.
- Récord de canicas local; la limpieza libre no lo modifica.
- Movimiento en primera persona y dos herramientas.
- Barrido con área visible y recogida mediante un carrito movible.
- Ayuda contextual con Tab para encontrar trabajo pendiente.
- Ajustes de sensibilidad y volumen guardados localmente.
- Modelos y gráficos originales: toldo festoneado, banderines, carrito, escoba, paño, carteles e iconos.
- Ambiente de amanecer con cielo propio, faroles, vegetación, casetas lejanas y fachada arqueada.
- Limpieza libre para repetir la zona sin sustituir el progreso guardado.
- Sistemas de residuos, carrito, restauración, presentación y persistencia separados; reglas en una definición de zona.
- Guardado JSON local con respaldo, recuperación y formato versionado.
- Sin servidores, cuentas de jugador, multijugador ni conexión obligatoria.

Es una zona de prueba; todavía no incluye campaña completa, economía, mejoras, Steamworks ni Steam Cloud. La duración y el equilibrio siguen en evaluación.

## Abrir el proyecto

1. Añade esta carpeta en Unity Hub.
2. Usa **6000.3.24f1**, la versión de `ProjectSettings/ProjectVersion.txt`.
3. Abre `Assets/Feria/Scenes/PrimeraManana.unity`.
4. Pulsa Play y entra desde el menú o con Enter.

Los paquetes se restauran desde `Packages/manifest.json` y `Packages/packages-lock.json`. El repositorio no contiene `Library`, ejecutables, registros ni partidas personales.

## Controles

| Acción | Control |
|---|---|
| Caminar / mirar | WASD / ratón |
| Barrer o limpiar | Mantener clic izquierdo |
| Escoba / paño | 1 / 2, o rueda |
| Mover carrito, colocar cartel, encender, jugar a canicas | E |
| Lanzar / otra ronda de canicas | E / E o R |
| Volver de las canicas a la feria | Esc |
| Localizar tarea pendiente | Tab |
| Pausa / reanudar | Esc / Enter |
| Guardar | F5 |

Coloca el recogedor amarillo del carrito delante de los montones. Limpia el mostrador con el paño. El puesto se enciende al completar las tres tareas.

## Arte propio

Las fuentes vectoriales editables están en `ArtSources/`. Los PNG, materiales y mallas están en `Assets/Feria/Art/Original/`, y los prefabs visuales en `Assets/Feria/Prefabs/`.

Los generadores `FairArtPass.cs`, `FairVisualPolish.cs` y `FairDetailPolish.cs` documentan la construcción de objetos y ambiente. No se utilizaron paquetes de modelos ni ilustraciones descargados para el arte de esta versión. Consulta [ART_PROVENANCE.md](ART_PROVENANCE.md) para distinguir nuestro contenido de las dependencias de Unity y las fuentes tipográficas.

## Documentación

- [Diseño de la primera zona](docs/DISENO.md).
- [Arquitectura y guardado](docs/ARQUITECTURA.md).
- [Prueba de sesión de hasta diez minutos](docs/PRUEBA-DE-SESION.md).
- [Cambios](CHANGELOG.md) y [validación de v0.4](docs/VALIDACION-v0.4.md).

## Derechos

Repositorio público para mostrar y versionar el desarrollo. El código y el arte propios no se ofrecen bajo una licencia de reutilización abierta. Se reservan los derechos del titular del proyecto. Las dependencias y fuentes de terceros conservan sus licencias respectivas.
