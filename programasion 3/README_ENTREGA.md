# Ticket Rush - Examen de Programación 3

Minijuego de recolección desarrollado en Unity. Cada partida dura 60 segundos y el jugador debe recoger objetos para mejorar su puntuación.

## Funciones principales

- Modo invitado: permite jugar sin guardar resultados ni consultar el leaderboard.
- Registro e inicio de sesión mediante PlayFab.
- Conservación del puntaje más alto del jugador.
- Leaderboard con 10 posiciones y una fila separada para el jugador conectado.
- Avatar seguro generado mediante color e inicial del usuario.
- Consulta de OpenWeatherMap para una de 10 ciudades aleatorias.
- Cambio de fondo, iluminación, ambiente y niebla según las condiciones climáticas.
- Segundo cambio climático a los 30 segundos de la partida.

## Configuración local

1. Abrir el proyecto con Unity 6000.4.0f1.
2. Crear `Assets/Resources/OpenWeatherApiKey.txt`.
3. Escribir únicamente una API key activa de OpenWeatherMap dentro del archivo.
4. En PlayFab, usar el Title ID `2F0C7` y activar `Allow client to post player statistics`.
5. Abrir la escena `Assets/Scenes/Menu.unity`.

La API key real está excluida del repositorio y no debe publicarse.

## Flujo de prueba

1. Crear una cuenta o iniciar sesión desde `Menu`.
2. Presionar `Jugar 60 segundos`.
3. Recoger objetos y observar el cambio del clima.
4. Esperar el final de la partida y el regreso automático al menú.
5. Abrir `Ver puntuaciones` para comprobar el récord y el leaderboard.

## Controles

- Movimiento: WASD.
- Correr: control configurado en el Input System del proyecto.
- Cámara: ratón.

## Escenas incluidas

- `Menu`: autenticación, modo invitado, menú principal y leaderboard.
- `SampleScene`: minijuego, temporizador, puntuación y cambios climáticos.

