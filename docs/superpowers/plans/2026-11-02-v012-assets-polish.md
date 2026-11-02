# Pulido v0.12 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans for native execution after review, or superpowers:subagent-driven-development if the user chooses that method. Steps use checkbox syntax for tracking.

**Goal:** Pulir el contenido existente con manchas variadas, herramientas de Blender, recursos descargados seleccionados y validación real en Unity y Windows.

**Architecture:** Adaptar los servicios y el HUD de Assets/Feria; mantener referencias, IDs y guardado v3. Crear variantes y prefabs nuevos sobre los modelos elegidos, sin sobrescribir sus fuentes ni rehacer ParquePrincipal. Incorporar un catálogo visual de manchas independiente de la persistencia.

**Tech Stack:** Unity 6000.3.24f1, URP 17.3.0, C#, uGUI/TextMeshPro, AudioMixer, Blender 5.1.2 y Python local. Sin paquetes ni servicios de pago nuevos.

**Spec:** [Solicitud completa](../specs/2026-11-02-pulido-v012.md). [Inspección comprobada](../inspections/2026-11-02-v012.md).

## Global Constraints

- Un solo jugador y datos guardados localmente.
- No reconstruir el videojuego, eliminar minijuegos, añadir niveles o introducir teletransportes.
- Conservar el parque grande y continuo y sus parcelas para crecimiento.
- No sobrescribir assets originales sin respaldo.
- No publicar cambios ni sobrescribir la rama estable sin autorización.
- No compras, créditos ni dependencias adicionales sin revisar su necesidad y obtener autorización.
- No usar recursos sin verificar su licencia; no confundir permiso de uso en el juego con redistribución de archivos fuente.
- No mover paquetes del usuario ni sustituir sus `.meta` y GUID de forma masiva.
- No afirmar pruebas, importaciones, renderizado o compilaciones que no se hayan ejecutado.

## Review Focus

1. Una partida v3 con manchas parcialmente limpias debe conservar progreso y contadores al asignar otro aspecto visual.
2. Un borde tenue de líquido, visto oblicuamente, debe responder dentro del rango, sin permitir limpiar detrás del puesto.
3. Cambiar escoba/paño, pausar o dejar de mantener clic debe detener sonido y contacto sin dejar animaciones activas.
4. Prefabs externos con ejes o dimensiones distintos no deben aplastarse, invadir los caminos ni ocupar áreas de futuras atracciones.
5. La entrega pública futura no debe incluir modelos cuyo permiso solo cubre uso en el juego; los originales licenciados deben poder recuperarse localmente.

## Decisiones de alcance para revisar

- Enfoque híbrido siguiendo la solicitud: usar primero los paquetes existentes donde mejoren la escena; conservar los modelos originales v0.11 que ya funcionen bien y crear en Blender la escoba y el paño que faltan.
- Elegir seis o más siluetas de árboles de EmbersStorm, varios arbustos/flores y una distribución determinista. Cambiar variantes en las ubicaciones actuales, no ampliar el mapa.
- Selección inicial de Playground: `flower_bed`, `trash_can`, `park_sign`, `picnic_table`, `hedge` y `hedge_round`; evaluar `park_bench`/`park_lamp` como variantes de mobiliario o LOD, sin sustituir por sistema los modelos propios más detallados.
- Selección de Amusement: señales, barandillas y detalles de quioscos sobre los puestos actuales. Excluir atracciones nuevas y el modelo `spinning_teacups`, que presenta advertencias y no es necesario para el alcance.
- Siete familias de manchas: cola, naranja, fresa, jugo, uva, grasa y polvo. Mantener 16 IDs por mostrador; no añadir una categoría persistente obligatoria al JSON.
- Propuesta de alcance de escoba: máximo de contacto de 2,8 m desde la cámara, sujeto a la comprobación de la pose del modelo. Dar una indicación de acercamiento cuando el suelo apuntado quede fuera de alcance.
- Trabajo local en `polish/v012-assets-tools`. La versión 0.12 de Windows se entrega para revisar antes de cualquier publicación o integración en main.

## Task 1 — Fijar la base y admitir recursos seleccionados

**Files:** Create `docs/receipts/v012-assets.json`, `docs/THIRD-PARTY-ASSETS-v012.md`; preserve existing folders under `Assets/_Game/Art/ExternalAssets`, `Assets/EmbersStorm -  Free Nature Pack` and `Assets/_Game/audio/KenneyUI`.

**Interfaces:** Consumes rutas y licencias del inventario. Produces un catálogo con fuente, hash, licencia, escala, triángulos, materiales y destino para cada recurso elegido.

- [ ] Registrar los cambios previos del usuario y ejecutar las suites existentes sobre una partida de prueba aislada.
- [ ] Escribir y ejecutar `FairV012AssetVerification.Run()` para comprobar los modelos seleccionados, materiales URP, referencias y licencias registradas; registrar cualquier fallo antes de corregirlo.
- [ ] Conservar LICENSE/README junto a la documentación y separar recursos originales de variantes adaptadas. No duplicar FBX ya importados.
- [ ] Confirmar si los avisos de importación siguen siendo reproducibles; no borrar cachés ni metadatos por un mensaje histórico.
- [ ] Crear respaldo del material propio que vaya a reemplazarse y un commit local solo de archivos autorizados del bloque; excluir recursos sin permiso de redistribución pública.

## Task 2 — Reproducir y corregir contacto, alcance y oclusión

**Files:** Modify `Assets/Feria/Scripts/FairGame.cs`, `Assets/Feria/Scripts/Simulation/SurfaceCleaning.cs`, `Assets/Feria/Editor/FairInteractionVerification.cs`; create `Assets/Feria/Editor/FairV012ContactVerification.cs`.

**Interfaces:** Preserve `SurfaceCleaning.Raycast(Ray,float,out int,out Vector3)`, `FairGame.UseTool(bool,float)` and `TrySweepPoint(out Vector3)`. Expose a single configured sweep range and a contextual out-of-range state.

- [ ] Escribir pruebas para que una mancha parcialmente limpia responda en centro y bordes; observar el resultado con el código actual antes de modificarlo.
- [ ] Escribir la prueba de escoba que rechaza un contacto a más de 2,8 m, ver su fallo actual por el límite de cinco metros y corregirlo.
- [ ] Reproducir objetos delante de una mancha, ángulo rasante, cámara móvil, UI visible, pausa y herramienta incorrecta. Corregir solo causas demostradas.
- [ ] Comprobar que el radio de barrido y la animación representan la misma zona de contacto; si el modelo exige otro valor, documentar la decisión antes de cambiarlo.
- [ ] Ejecutar `FairV012ContactVerification.Run()` y `FairInteractionVerification.Run()`; guardar resultados y commit del bloque.

## Task 3 — Catálogo visual y desaparición progresiva de manchas

**Files:** Create `Assets/Feria/Scripts/Configuration/FairStainCatalog.cs`, `Assets/Feria/Definitions/ManchasV012.asset`, `Assets/Feria/Art/Original/Shaders/FairSpill.shader`, `Assets/Feria/Editor/FairV012StainBuilder.cs`, `Assets/Feria/Editor/FairV012StainVerification.cs`; modify `SurfaceCleaning.cs` and `FairGame.cs`.

**Interfaces:** `FairStainProfile FairStainCatalog.Get(int stableIndex)` devuelve `displayName` (string), `color` (Color), `opacity` y `roughness` (float) y `seed` (int). `string FairGame.SurfaceLabel` expone el nombre contextual. Se conservan `float[] SurfaceCleaning.Capture()` y `void SurfaceCleaning.Restore(float[] data)`.

- [ ] Escribir una comprobación de siete perfiles distintos y de persistencia de progreso 0, 0,45 y 1; observar el fallo de la nueva capacidad antes de implementarla.
- [ ] Crear perfiles y contornos de derrame con semillas deterministas, sin convertirlos en siete círculos recoloreados.
- [ ] Crear shader/materiales URP que reflejen brillo de bebida y grasa, polvo mate y opacidad de líquido. Mantener los límites de interacción coherentes con la malla.
- [ ] Conectar progreso a escala/revelado/alpha mediante propiedades compartidas o MaterialPropertyBlock, sin crear un material por frame.
- [ ] Verificar que los IDs de 32 manchas siguen siendo los mismos, cargar partidas v3 antiguas y repetir centro/borde/oclusiones. Renderizar cada familia sobre el mostrador y registrar capturas antes del commit.

## Task 4 — Escoba y paño propios de Blender

**Files:** Create `ArtSources/Blender/v012_tools.py`, nuevos `.blend` y FBX bajo `ArtSources/Blender/v012/` y `Assets/Feria/Art/Original/ToolsV012/`; create `Assets/Feria/Editor/FairV012ToolsBuilder.cs`; modify `FairPlayer.cs` y variantes de herramienta en `Assets/Feria/Prefabs`.

**Interfaces:** Preserve `FairPlayer.broom`, `cloth`, `AnimateTool(bool,float,float)` y los puntos de reposo/agarre. Los reemplazos son hijos visuales nuevos, con padres a escala unitaria y sin colliders que intercepten clics.

- [ ] Definir comprobaciones de dimensiones, ejes, número de materiales, mallas válidas y relación del cabezal con el contacto; detectar fallos en cualquier reemplazo antes de incorporarlo.
- [ ] Ejecutar Blender en un proceso de fondo nuevo, inspeccionar el exportador disponible y producir mango proporcionado, cabezal, haces de cerdas y tela irregular con pliegues y dobladillo. No tocar el documento abierto.
- [ ] Presupuesto inicial: escoba y paño por debajo de 3.000 triángulos cada uno, materiales compartidos y sin Rigidbody por cerda o vértice.
- [ ] Conservar fuentes `.blend`, script y exportaciones. Importar FBX mediante Unity y preparar prefabs con escala correcta, no únicamente imágenes de referencia.
- [ ] Integrar reposo, caminar, entrada/salida del contacto, barrido y frotado con fase de audio y transiciones suaves. Mantener balanceo de cámara pequeño.
- [ ] Probar cambio de herramienta, contacto, pausa, fin de limpieza y vuelta de minijuegos. Registrar render en primera persona y commit local.

## Task 5 — Efectos y audio del contacto

**Files:** Modify `Assets/Feria/Scripts/Presentation/FairVfx.cs`, `Assets/Feria/Scripts/Presentation/FairAudio.cs`, `Assets/Feria/Scripts/FairGame.cs`; create `Assets/Feria/Resources/AudioV012.asset` y `Assets/Feria/Editor/FairV012AudioVerification.cs` si se necesita configuración de clips externos.

**Interfaces:** Conservar grupos del mixer Herramientas, Pasos, Interacciones y Ambiente. Los clips Kenney se resuelven desde una configuración de recursos; el foley de pasos y herramientas permanece independiente.

- [ ] Probar que el sonido cesa sin contacto, al soltar y en pausa, y que no se duplica al cambiar de zona.
- [ ] Seleccionar clic/confirmación Kenney verificados por hash para menú, herramientas y objetivos; mantener su licencia y procedencia.
- [ ] Inspeccionar la licencia de los paquetes de pasos antes de seleccionar grabaciones. Si no se acredita, producir variantes originales locales y registrar esa decisión.
- [ ] Ajustar fricción de escoba y frotado de tela al ritmo real de la herramienta; comprobar muestras finitas, ausencia de saturación y respeto al volumen maestro.
- [ ] Reutilizar partículas de polvo, hojas y confirmación, sin emisores por residuo. Validar antes/después con capturas y clips de escucha para revisión humana; commit.

## Task 6 — Mobiliario, vegetación y luz

**Files:** Create `Assets/Feria/Editor/FairV012EnvironmentBuilder.cs`, variantes bajo `Assets/_Game/Prefabs/Environment/V012/`, materiales adaptados separados de originales y, si procede, metadatos de viento sin Update propio. Modify `Assets/Feria/Scripts/Presentation/FairVfx.cs` y `Assets/Feria/Scenes/ParquePrincipal.unity` mediante el Editor.

**Interfaces:** Consumir catálogo de Task 1; conservar los orígenes de puestos, caminos, tareas, referencias de minijuegos y parcelas. Una función de aplicación idempotente actualiza solo los elementos seleccionados.

- [ ] Comprobar escala y colliders de cada variante, ausencia de caminos bloqueados y unicidad de jugador/carrito/HUD; observar fallos reales antes de corregirlos.
- [ ] Integrar las variantes elegidas de mobiliario y señales y mezclar vegetación de EmbersStorm con la dirección de color existente.
- [ ] Usar copas y arbustos de distintas formas, alturas y fases de brisa. Animar visuales, no troncos con colliders, y mantener LOD y materiales compartidos.
- [ ] Añadir jardineras y pequeños detalles en bordes existentes, manteniendo despejados recorrido, recogedor y reservas futuras.
- [ ] Comparar luz principal, ambiente, sombras, SSAO, bloom y niebla desde cámaras idénticas. Aplicar solo ajustes beneficiosos; no sumar luces dinámicas indiscriminadamente.
- [ ] Ejecutar recorrido físico, limpieza completa y minijuegos tras la integración; registrar capturas, métricas y commit local.

## Task 7 — HUD, documentación visual y cierre

**Files:** Modify `Assets/Feria/Scripts/FairHud.cs`, `ProjectSettings/ProjectSettings.asset` (solo versión, sin mezclar cambios previos), documentación y versión; create diseños originales en `ArtSources/UI/v012/`, `Assets/Feria/Editor/FairV012PresentationVerification.cs`, `docs/receipts/v012-*.json`, `docs/PULIDO-0.12.md`.

**Interfaces:** Consumir `FairGame.SurfaceLabel`, progreso individual y contadores reales. Conservar un Canvas/EventSystem, safe area y ajustes existentes.

- [ ] Comprobar nombres largos en español, porcentaje y contadores tras cargar; evitar que el panel tape el paño/mancha o intercepte clics.
- [ ] Diseñar componentes compactos de nombre/tipo/progreso y mejorar los iconos. Usar Figma solo si sus herramientas están disponibles; de lo contrario conservar SVG y especificación de tokens sin afirmar una entrega en Figma.
- [ ] Verificar 1280×720, 1920×1080, 2560×1080 y 720×1280, restaurando la vista del Editor al terminar.
- [ ] Ejecutar todas las suites existentes y nuevas. Comparar rendimiento bajo la misma cámara, resolución y carga; no traducir draw calls a promesas de FPS.
- [ ] Compilar `outputs/Feria-Windows-v0.12`, ejecutar el diagnóstico en práctica sin escribir campaña y comprobar por hashes que los archivos de progreso no cambiaron.
- [ ] Realizar revisión final independiente según el método aprobado, resolver problemas importantes con pruebas y dejar un informe de límites y valoración humana pendiente.
- [ ] Entregar ejecutable, capturas, inventario de reutilizados/creados, archivos cambiados, pruebas y problemas pendientes. Conservar la rama local; no hacer push, merge en main ni publicar sin autorización específica posterior.

## Criterios de entrega

La fase debe producir una versión funcional y comprobada sobre el contenido actual. Su aprobación técnica exige pruebas de interacción y persistencia, importación y render de assets elegidos, audio/animación sincronizados, recorrido sin bloqueos, medición comparable y compilación Windows real. La comodidad durante diez minutos y la calidad percibida del foley requieren revisión humana; no se sustituirán por afirmaciones de una prueba programada.

## Ejecución y contratos de verificación

Se seguirá el patrón actual de verificaciones Editor: `public static string[] Run()`, excepción con `FAIL` cuando falle una condición y mensajes `PASS` únicamente después de comprobarla. Las pruebas que cambian el mundo se ejecutan en Play, con `ParkSession.suppressSave=true`, snapshot previo y restauración en `finally`. Una excepción o fallo de compilación es un fallo de bloque, no una aprobación.

Ejemplo de invocación real del CLI instalado, desde la carpeta del proyecto y con el Editor conectado:

```powershell
& 'C:/Users/mo27m/AppData/Local/Unity/bin/unity.exe' command eval --caller plugin --skill unity-cli --project-path 'C:/Users/mo27m/Documents/Codex/2026-10-29/qu/outputs/DespuesDeLaFeria' --format json --code 'return FairInteractionVerification.Run();'
```

Contratos que deben quedar fijados en las nuevas suites:

- Contacto: rechazar suelo a 2,81 m con límite 2,8 m, aceptar 2,79 m sin obstáculos; comprobar centro y cuatro bordes visibles de cada mancha con progresos 0 y 0,45; rechazar geometría sólida interpuesta y distancia de paño superior a 3,5 m.
- Manchas: exactamente siete perfiles distinguibles; `Get(i)` determinista; completar y recargar conserva los 32 IDs; `Restore(Capture())` conserva 0, 0,45 y 1 dentro de tolerancia 0,001.
- Recursos: cada ruta seleccionada existe en AssetDatabase, sus MeshFilter tienen malla y materiales compatibles, y la procedencia/licencia figura en el inventario. Antes de cada commit, contrastar `git diff --cached --name-only` con la lista de fuentes excluidas de redistribución.
- Herramientas/audio: ninguna malla vacía; escala raíz (1,1,1); menos de 3.000 triángulos por herramienta; soltar, pausar y cambiar de herramienta detiene el contacto y audio activo; muestras de audio finitas y valor absoluto máximo menor o igual a 1.
- Presentación/entorno: un jugador, Canvas y EventSystem; paneles dentro de pantalla en las cuatro resoluciones indicadas; colliders simples en mobiliario; recorrido previo y vuelta de minijuegos funcionales.

Las seis suites existentes que deben repetirse al cierre son `ContinuousParkVerification`, `FairInteractionVerification`, `FairMovementVerification`, `FairPresentationVerification`, `FairPolishVerification` y `FairCampaignVerification`. Se conservarán sus resultados junto a los de las nuevas suites y al registro real de compilación Windows.

Autorrevisión realizada: requisitos de la solicitud cubiertos por los siete bloques, interfaces contrastadas con el código actual, pruebas de persistencia y licencia vinculadas a sus bloques, y restricciones de publicación conservadas. Las decisiones de alcance visual y método de ejecución quedan para revisión del usuario.

## Estado

Inspección completada; implementación pendiente de revisión del plan y elección del método de ejecución que exige el skill writing-plans de Superpowers.
