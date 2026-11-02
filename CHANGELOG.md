# Cambios

## 0.9.0 — Tareas claras y respuesta de herramientas

- Objetivos de suelo, mostrador y cartel con marcas de tarea y porcentajes visibles en HUD.
- Panel de metas ampliado y textos principales más legibles, manteniendo la paleta crema y verde feria.
- Roce procedural de escoba sobre hojas y paño sobre madera con timbres diferentes.
- El filtro de madera queda aislado en la fuente de audio de herramienta; recogidas y campanillas conservan su tono.
- Se conserva la estructura del parque, su progreso local y la arquitectura para añadir nuevos puestos.


## 0.8.0 — Luz cálida y paisaje sonoro

- Perfil URP con color cálido, contraste/saturación moderados y viñeta suave.
- Capa de brisa procedural de bajo volumen, filtrada para evitar ruido áspero.
- Roce de la escoba con variación tonal lenta y discreta.
- Ambiente, movimiento y efectos ligeros sin físicas por hoja ni paquetes externos.
- La escena, los minijuegos y el guardado permanecen intactos.


## 0.7.0 — Pulido de movimiento y ambiente

- Efecto de polvo reutilizable en el punto real de contacto de la escoba.
- Recogida de residuos con pequeñas partículas cálidas.
- Hojas otoñales con densidad limitada y sin colisiones individuales.
- Balanceo sutil de copas con fase distinta por árbol.
- Reutiliza los modelos originales, el sistema de barrido y el recorrido del parque.


## 0.6.0 — Parque continuo y base ampliable

- Entrada, plaza con kiosco, plano, señales físicas, circuito posterior, merendero y jardines.
- Canicas y patitos en una escena, conectados a pie; se elimina Visitar del menú del parque.
- Un único jugador, carrito y HUD; detección de áreas con margen y progreso global.
- Simulación y cámaras relativas al origen de cada atracción, incluidas raíces giradas.
- Plano con cuatro parcelas y puntos de conexión para futuros juegos, con tamaños de áreas configurables.
- Guardado v3 de posición global, herramienta y progreso por puesto; migración de v2/v1.
- Limpieza libre sin desplazar al jugador o carrito.
- Pruebas de paseo físico, ciclo completo de 1200 residuos, minijuegos girados y persistencia global.

## 0.5.0 — La poza de los patitos y campaña local

- Segunda zona con 480 residuos, distribución propia y mosaico de ondas.
- Patitos tallados con alas, pico, ojos y gorro; aros de cuerda, canal y cartel original con SVG editable.
- Agua estilizada animada mediante shader propio.
- Tres lanzamientos a blancos móviles; aciertos, reacción del patito, pausa, revancha y récord local.
- Catálogo de puestos, visita desde el menú o cartel y transición entre escenas.
- Campaña JSON v2 con IDs de objetos y progreso independiente; migración de v1 sin modificar el archivo anterior.
- Recuperación del respaldo y protección de campañas incompatibles o posteriores.
- Pruebas de ambos ciclos de limpieza, minijuegos, campaña, viaje de ida/vuelta y continuación de Windows.

## 0.4.0 — Papel, escoba y canicas

- Cinco formas de confeti original con curvatura y aleteo visual separado del guardado.
- Escoba artesanal biselada, mango y remaches; fibras flexibles y pasada más suave.
- Tablero nuevo con carriles, apoyos, dianas numeradas y veta de canica.
- Minijuego de timing con vista cercana, puntuación, notas por curva y revancha sin dinero ni desbloqueos adicionales.
- Récord local en partida normal, protegido durante limpieza libre.
- Servicio MarbleGame separado; vista y recorrido configurables por zona.
- Guardados v1 e índices de residuos conservados.
- Pruebas específicas de minijuego/cámara/representación y ciclo completo de limpieza.

## 0.3.0 — Ambiente, arquitectura y limpieza libre

- Cielo de amanecer propio, vegetación tallada, faroles, casetas lejanas y rótulo arqueado.
- Sistemas separados de residuos, superficies, carrito, restauración, presentación, validación y archivos.
- Reglas y recorrido de canica en una definición de zona.
- Limpieza libre que conserva la campaña; medidas locales de sesión y protocolo de observación.
- Barrido con montones más cohesionados, recogida desde la boca del carro y comprobación de obstáculos.
- Validación de identidad de zona compatible con partidas v1 antiguas.
- Prueba técnica del ciclo completo mediante pasadas y recogida, sin retirar residuos directamente.
- Arquitectura global documentada, con sistemas actuales y ampliaciones pendientes claramente separados.

## 0.2.0 — Primera pasada de identidad y comodidad

- Gráficos originales: cinco ilustraciones con fuentes SVG y PNG.
- Toldo curvo y festoneado, banderines triangulares, carro abierto, escoba, paño y vasos huecos.
- Tres prefabs visuales reutilizables.
- Área de barrido visible y ayuda con Tab para localizar tareas pendientes.
- Volumen y sensibilidad configurables con persistencia local.
- Restauración visual coherente del cartel y bombillas al aplicar una partida.
- Formato de guardado v1 conservado.

## 0.1.0 — Prueba del ciclo principal

- Barrido, carrito, mostrador, cartel, reactivación y canica.
- Guardado JSON con respaldo y recuperación.
- Ejecutable Windows y verificaciones funcionales de la primera escena.
