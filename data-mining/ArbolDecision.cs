namespace data_mining;

/// <summary>
/// Árbol de decisión del Titanic, escrito para leerse en pantalla.
/// Las preguntas están en funciones; el recorrido es el árbol.
/// </summary>
public sealed class ArbolDecision
{
    public const string Texto =
        "¿EsMujer?\n" +
        "  sí → ClasificarMujer\n" +
        "    ¿EsClaseAlta? (clase 1 o 2)\n" +
        "      sí → sobrevive\n" +
        "      no → ¿EsNino o EdadDesconocida?\n" +
        "        sí → sobrevive\n" +
        "        no → no sobrevive\n" +
        "  no → ClasificarHombre\n" +
        "    ¿EsNino y EsClaseAlta?\n" +
        "      sí → sobrevive\n" +
        "      no → no sobrevive";

    public ResultadoClasificacion Clasificar(Pasajero pasajero)
    {
        var recorrido = new List<string>();
        var prediccion = EsMujer(pasajero)
            ? ClasificarMujer(pasajero, recorrido)
            : ClasificarHombre(pasajero, recorrido);

        return new ResultadoClasificacion(prediccion == "sobrevive" ? 1 : 0, recorrido);
    }

    private static string ClasificarMujer(Pasajero pasajero, List<string> recorrido)
    {
        recorrido.Add("EsMujer → sí");

        if (EsClaseAlta(pasajero))
        {
            recorrido.Add("EsClaseAlta → sí → sobrevive");
            return "sobrevive";
        }

        recorrido.Add("EsClaseAlta → no");

        if (EsNino(pasajero) || EdadDesconocida(pasajero))
        {
            recorrido.Add(EsNino(pasajero)
                ? "EsNino → sí → sobrevive"
                : "EdadDesconocida → sí → sobrevive");
            return "sobrevive";
        }

        recorrido.Add("adulta de tercera → no sobrevive");
        return "no sobrevive";
    }

    private static string ClasificarHombre(Pasajero pasajero, List<string> recorrido)
    {
        recorrido.Add("EsMujer → no");

        if (EsNino(pasajero) && EsClaseAlta(pasajero))
        {
            recorrido.Add("EsNino y EsClaseAlta → sí → sobrevive");
            return "sobrevive";
        }

        recorrido.Add("EsNino y EsClaseAlta → no → no sobrevive");
        return "no sobrevive";
    }

    private static bool EsMujer(Pasajero pasajero) => pasajero.Sexo == "mujer";

    private static bool EsNino(Pasajero pasajero) => pasajero.GrupoEdad == "niño";

    private static bool EsClaseAlta(Pasajero pasajero) => pasajero.Clase < 3;

    private static bool EdadDesconocida(Pasajero pasajero) => pasajero.GrupoEdad == "desconocido";
}
