namespace data_mining;

public sealed record ClasificacionRequest(string? Sexo, int Clase, double? Edad);

public sealed record ClasificacionResponse(
    string Prediccion,
    int ClasePredicha,
    string Sexo,
    int Clase,
    string GrupoEdad,
    IReadOnlyList<string> Recorrido);

public sealed record EvaluacionResponse(
    int Semilla,
    double ProporcionEntrenamiento,
    int FilasEntrenamiento,
    int FilasPrueba,
    int Aciertos,
    double PorcentajeAcierto,
    int ProfundidadMaxima);

public sealed record ArbolResponse(
    string Algoritmo,
    string Objetivo,
    string ReglaEdad,
    int Pasajeros,
    int ProfundidadMaxima,
    IReadOnlyList<string> Atributos,
    string ArbolTexto,
    NodoDto Raiz);

public sealed record NodoDto(
    bool EsHoja,
    string? Atributo,
    double Entropia,
    double? Ganancia,
    int Total,
    int Sobrevivieron,
    int NoSobrevivieron,
    int ClaseMayoritaria,
    string Etiqueta,
    IReadOnlyList<RamaDto>? Ramas);

public sealed record RamaDto(string Valor, NodoDto Nodo);

public sealed record ErrorResponse(string Mensaje);

public sealed record ResultadoClasificacion(int Clase, IReadOnlyList<string> Recorrido);
