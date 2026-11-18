namespace data_mining;

/// <summary>
/// Un pasajero del Titanic ya preparado para el árbol.
/// La edad entra como categoría porque ID3 parte por valores discretos.
/// </summary>
public sealed class Pasajero
{
    public const double CorteEdadNino = 16;

    public Pasajero(string sexo, int clase, string grupoEdad, int sobrevivio)
    {
        Sexo = sexo;
        Clase = clase;
        GrupoEdad = grupoEdad;
        Sobrevivio = sobrevivio;
    }

    public string Sexo { get; }

    public int Clase { get; }

    public string GrupoEdad { get; }

    /// <summary>1 sobrevive, 0 no sobrevive. Es la clase que el árbol aprende.</summary>
    public int Sobrevivio { get; }

    public string ValorDe(string atributo) => atributo switch
    {
        "Sexo" => Sexo,
        "Clase" => Clase.ToString(),
        "Edad" => GrupoEdad,
        _ => throw new ArgumentOutOfRangeException(nameof(atributo), atributo, "Atributo desconocido.")
    };

    public static string GrupoPorEdad(double? edad)
    {
        if (edad is null)
        {
            return "desconocido";
        }

        return edad.Value < CorteEdadNino ? "niño" : "adulto";
    }
}
