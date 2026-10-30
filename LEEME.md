# Después de la feria · Versión 0.3

La documentación principal está en [README.md](README.md), y la arquitectura completa en [docs/ARQUITECTURA.md](docs/ARQUITECTURA.md).

## Jugar

La versión para Windows está en la carpeta hermana `Feria-Windows-v0.3`. Abre `DespuesDeLaFeria.exe` y conserva las carpetas que lo acompañan.

En Unity Hub, abre este proyecto con 6000.3.24f1 y la escena `Assets/Feria/Scenes/PrimeraManana.unity`.

WASD: caminar. Ratón: mirar. Clic mantenido: usar herramienta. 1/2: escoba/paño. E: interactuar. Tab: ayuda. Esc: pausa. Enter: reanudar. F5: guardar.

## Repetir sin perder progreso

En el menú elige **Limpieza libre** para empezar la zona de nuevo. Tu partida anterior se conserva. Desde la pausa puedes usar **Volver a partida**. La limpieza libre no escribe sobre el guardado de campaña.

El [protocolo de sesión](docs/PRUEBA-DE-SESION.md) permite observar la comodidad y diversión durante un máximo orientativo de diez minutos.

## Guardado y alcance

Progreso local en JSON v1, con respaldo y recuperación; compatible con la primera prueba. Volumen y sensibilidad en PlayerPrefs. No incluye campaña completa, economía, mejoras, multijugador ni integración de Steam todavía.

Código y arte editable se versionan en el repositorio público del proyecto. Las partidas, cachés y registros locales no forman parte del repositorio.
