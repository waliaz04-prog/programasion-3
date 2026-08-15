# Guía para el video de entrega

Duración recomendada: 4 a 6 minutos.

## 1. Presentación

"Este es mi proyecto Ticket Rush para Programación 3. El objetivo es recolectar objetos durante una partida de 60 segundos y superar el mejor puntaje guardado en PlayFab."

## 2. Menú y PlayFab

- Mostrar el panel inicial.
- Entrar a crear cuenta y enseñar los campos de correo, usuario y contraseña.
- Iniciar sesión con una cuenta de prueba.
- Explicar que el SDK de PlayFab realiza el registro, inicio de sesión, guardado del récord y consulta del leaderboard.
- Mostrar brevemente el modo invitado y explicar que no guarda resultados ni permite consultar puestos.

## 3. Partida

- Presionar `Jugar 60 segundos`.
- Mostrar el temporizador y el texto de puntos.
- Recoger varios objetos.
- Enseñar que los distintos objetos entregan puntuaciones diferentes.
- Esperar a que el clima cambie y mostrar el nombre de la ciudad consultada.

Explicación sugerida:

"WeatherManager elige aleatoriamente una de diez ciudades y consulta OpenWeatherMap. Según el código climático modifica el fondo, la luz ambiental, la luz direccional y la niebla. Hace una consulta al comenzar y otra después de 30 segundos."

## 4. Final y leaderboard

- Mostrar el mensaje de final de partida.
- Esperar el regreso automático al menú.
- Abrir `Ver puntuaciones`.
- Mostrar diez puestos y la fila separada del jugador conectado.
- Señalar el avatar seguro, el nombre, la posición y el puntaje.

Explicación sugerida:

"Antes de actualizar PlayFab, el juego consulta las estadísticas del jugador. Solamente envía el resultado cuando supera el récord anterior. Después solicita los diez mejores puestos y la posición del jugador actual."

## 5. Flujo de scripts

- `MenuInspectorController`: controla paneles, registro, login y leaderboard.
- `GameManager`: controla los 60 segundos, puntos y final de partida.
- `CollectablesSpawn`: utiliza object pooling para reutilizar coleccionables.
- `PlayFabLeaderboardManager`: conserva sesión, récord y puntuaciones.
- `WeatherManager`: consulta la API y aplica los cambios visuales.
- `ExamServicesBootstrap`: mantiene los servicios entre escenas.

## 6. Cierre

"El proyecto incluye una build de Windows, el video de funcionamiento y el repositorio con el código. La clave privada del clima no se publica en GitHub."

