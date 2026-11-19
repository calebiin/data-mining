# Minería de datos — Titanic

API en C# que clasifica pasajeros del Titanic con un árbol de decisión corto. El árbol está escrito en `ArbolDecision.cs`: cada pregunta es una función.

## Cómo correrla

Requiere el SDK de .NET 7 (el que usa Visual Studio en esta máquina).

```bash
cd data-mining
dotnet run
```

Abre [http://localhost:5217/swagger](http://localhost:5217/swagger).

## Endpoints

| Método | Ruta | Qué devuelve |
|---|---|---|
| `GET` | `/arbol` | El árbol en texto, el mismo que se lee en el código. |
| `POST` | `/clasificar` | Clasifica a un pasajero y devuelve el camino de funciones que siguió. |

Ejemplo de clasificación:

```http
POST /clasificar
Content-Type: application/json

{ "sexo": "mujer", "clase": 3, "edad": 8 }
```

`sexo` acepta `mujer` / `hombre` o `female` / `male`. `clase` es 1, 2 o 3. Si omites `edad`, el grupo es `desconocido`.

Con los mismos sexo y clase, cambiar la edad de 8 a 30 cambia la predicción: el árbol hace otra pregunta.

## El árbol

Está en `ArbolDecision.cs`, unas treinta líneas.

1. `EsMujer` parte el árbol.
2. Si es mujer, `ClasificarMujer` mira `EsClaseAlta`, `EsNino` y `EdadDesconocida`.
3. Si es hombre, `ClasificarHombre` solo deja vivir a un niño de clase 1 o 2.

Niño es edad menor de 16. Si la edad viene vacía en el CSV, el grupo es `desconocido`.

## Dataset

`data-mining/Data/train.csv` es el `train.csv` público del Titanic (Kaggle). Se leen `Survived`, `Pclass`, `Sex` y `Age`. El resto de columnas se ignora.

## Código

| Archivo | Rol |
|---|---|
| `ArbolDecision.cs` | El árbol: las preguntas y la clasificación. |
| `Pasajero.cs` | Una fila del Titanic. |
| `LectorCsv.cs` | Lee el CSV. |
| `Program.cs` | Carga los datos al arrancar y publica los endpoints. |
