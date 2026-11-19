namespace data_mining;

public sealed record ClasificacionRequest(string? Sexo, int Clase, double? Edad);

public sealed record ClasificacionResponse(
    string Prediccion,
    int ClasePredicha,
    string Sexo,
    int Clase,
    string GrupoEdad,
    IReadOnlyList<string> Recorrido);

public sealed record ArbolResponse(
    int Pasajeros,
    string ReglaEdad,
    string ArbolTexto);

public sealed record ErrorResponse(string Mensaje);

public sealed record ResultadoClasificacion(int Clase, IReadOnlyList<string> Recorrido);
