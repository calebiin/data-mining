# Minería de datos — Titanic

API en C# que entrena un árbol de decisión **ID3** con los pasajeros del Titanic y clasifica si una persona nueva habría sobrevivido.

El algoritmo está escrito en el proyecto (entropía y ganancia de información). No usa una librería de machine learning.

## Cómo correrla

Requiere el SDK de .NET 10.

```bash
cd data-mining
dotnet run
```

Abre [http://localhost:5217/swagger](http://localhost:5217/swagger).

## Endpoints

| Método | Ruta | Qué devuelve |
|---|---|---|
| `GET` | `/arbol` | Árbol entrenado con los 891 pasajeros, en texto y en JSON. Cada pregunta incluye su entropía y su ganancia. |
| `POST` | `/clasificar` | Predicción para un pasajero y el camino que recorrió en el árbol. |
| `GET` | `/evaluacion` | Aciertos al entrenar con el 80 % y probar con el 20 % restante. La mezcla usa la semilla `42`, así que el resultado no cambia entre ejecuciones. |

Ejemplo de clasificación:

```http
POST /clasificar
Content-Type: application/json

{ "sexo": "mujer", "clase": 3, "edad": 8 }
```

`sexo` acepta `mujer` / `hombre` o `female` / `male`. `clase` es 1, 2 o 3. Si omites `edad`, el grupo es `desconocido`.

Con los mismos sexo y clase, cambiar la edad de 8 a 30 cambia la predicción: el árbol hace otra pregunta.

## Qué aprende el árbol

Pregunta, en este orden, por el atributo que más ordena los datos:

1. **Sexo:** `mujer` o `hombre`.
2. **Clase:** `1`, `2` o `3`.
3. **Edad:** `niño` si es menor de 16, `adulto` si no, `desconocido` si el CSV la trae vacía.

La profundidad máxima es 3. En una hoja, la clase predicha es la mayoritaria de ese grupo (`sobrevive` o `no sobrevive`). Si hay empate, predice que no sobrevive.

## Dataset

`data-mining/Data/train.csv` es el `train.csv` público del Titanic (Kaggle). Se leen `Survived`, `Pclass`, `Sex` y `Age`. El resto de columnas se ignora.

## Código

| Archivo | Rol |
|---|---|
| `ArbolDecision.cs` | ID3: entropía, ganancia, entrenamiento, predicción y evaluación. |
| `Nodo.cs` | Pregunta o hoja. |
| `Pasajero.cs` | Una fila lista para el árbol. |
| `LectorCsv.cs` | Lee el CSV. |
| `Program.cs` | Carga los datos al arrancar y publica los endpoints. |
