# Survival Story

Juego 2D de supervivencia hecho en C# con MonoGame y conectado a una base de datos en MySQL.

## ¿De qué se trata?

Este proyecto es nuestro Trabajo Práctico para la materia **Base de Datos** (4°1 - 2026). La idea fue armar un juego de supervivencia donde todo el progreso de cada jugador quede guardado en MySQL, para que si cerrás el juego y volvés a entrar con tu nombre de usuario, arranques en el mismo lugar del mapa y en el día en el que te habías quedado.

## Cómo funciona el juego

Cuando entrás al menú ponés tu nombre de usuario y arrancás en un mapa que tiene zonas de pasto, playa y montaña. En cada partida se generan de forma aleatoria piedras, árboles y bloques de arena que tenés que ir juntando con la herramienta que corresponde para sobrevivir la mayor cantidad de días.

### Lo que tiene el juego:
* **Recursos y herramientas:** Hay 10 piedras (en montaña), 30 árboles (en el pasto) y 15 bloques de arena (en la playa). Tenés que equipar el pico, el hacha o la pala para romperlos, agarrar el drop del suelo y guardarlo en el inventario (tiene 10 lugares).
* **Construcción:** Podés poner una casa adelante del personaje.
* **Barras de estado y día/noche:** Tenés Salud, Hambre, Energía y Moral. Con el tiempo van bajando el hambre y la energía; si el hambre llega a 0 te empieza a bajar la vida. Además, cada ciertos segundos se hace de noche, se oscurece la pantalla y vas perdiendo salud si no aguantás hasta el día siguiente.
* **Animales:** Hay una oveja pacífica que cuando te ve sale corriendo para el otro lado, y un toro que cuando te detecta te persigue.
* **Historial y avisos:** Arriba en la pantalla te muestra la última acción que hiciste y si mantenés la `H` podés ver el historial guardado en una pila. También te tira mensajes de error si usás mal una herramienta o si tenés el inventario lleno.

## Controles

* **W, A, S, D:** Moverse
* **R:** Equipar pico
* **T:** Equipar hacha
* **Y:** Equipar pala
* **U:** Desequipar herramienta
* **E:** Romper recurso (piedra, árbol o arena)
* **Q:** Agarrar drop del suelo
* **Tab:** Abrir / cerrar inventario
* **C:** Construir casa
* **H (mantener):** Ver historial de acciones
* **Escape:** Guardar partida en la base de datos y volver al menú
* **Enter:** Reiniciar cuando morís

## Base de datos (MySQL)

Para conectar C# con MySQL usamos la librería `MySqlConnector` en la clase `ConnectDB.cs`. 
* Cuando ponés tu nombre y apretás **Jugar**, el programa busca en la base con un `SELECT` si ya existís. Si estás guardado, carga tus coordenadas (`axisX`, `axisY`) y tus días sobrevividos (`survived_days`, `time_day`).
* Cuando apretás **Escape**, guarda tu posición y tu progreso haciendo un `UPDATE` (o un `INSERT` si es la primera vez que jugás).

## Herramientas usadas

* C# (.NET) y MonoGame
* MySQL
* MySqlConnector
* Jira (para organizarnos las tareas entre los 4)

## Integrantes (4°1)

* Isaias Britez
* Leonardo Saenz
* Nicolas Rambaldi
* Demian Garcia

# Trabajo aun no terminado
