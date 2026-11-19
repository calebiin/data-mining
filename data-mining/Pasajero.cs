namespace data_mining;

/// <summary>Un pasajero del Titanic. La edad entra como niño, adulto o desconocido.</summary>
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

    /// <summary>1 sobrevive, 0 no sobrevive. Es la etiqueta que trae el CSV.</summary>
    public int Sobrevivio { get; }

    public static string GrupoPorEdad(double? edad)
    {
        if (edad is null)
        {
            return "desconocido";
        }

        return edad.Value < CorteEdadNino ? "niño" : "adulto";
    }
}
