namespace data_mining;

/// <summary>
/// Nodo del árbol. Una hoja ya tiene clase; un nodo interno hace una pregunta
/// y tiene un hijo por cada respuesta observada.
/// </summary>
public sealed class Nodo
{
    public bool EsHoja { get; init; }

    public string? Atributo { get; init; }

    public double Entropia { get; init; }

    public double? Ganancia { get; init; }

    public int Total { get; init; }

    public int Sobrevivieron { get; init; }

    public int ClaseMayoritaria { get; init; }

    public Dictionary<string, Nodo> Hijos { get; init; } = new();
}
