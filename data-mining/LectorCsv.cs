using System.Globalization;
using System.Text;

namespace data_mining;

public static class LectorCsv
{
    public static List<Pasajero> Leer(string ruta)
    {
        var lineas = File.ReadAllLines(ruta);
        var pasajeros = new List<Pasajero>(lineas.Length);

        for (var i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i]))
            {
                continue;
            }

            var campos = Partir(lineas[i]);
            if (campos.Count < 6)
            {
                continue;
            }

            var sobrevivio = int.Parse(campos[1], CultureInfo.InvariantCulture);
            var clase = int.Parse(campos[2], CultureInfo.InvariantCulture);
            var sexo = campos[4] == "female" ? "mujer" : "hombre";
            double? edad = null;
            if (!string.IsNullOrWhiteSpace(campos[5])
                && double.TryParse(campos[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var edadLeida))
            {
                edad = edadLeida;
            }

            pasajeros.Add(new Pasajero(sexo, clase, Pasajero.GrupoPorEdad(edad), sobrevivio));
        }

        return pasajeros;
    }

    /// <summary>Separa una línea CSV respetando comas dentro de comillas (el nombre del pasajero).</summary>
    public static List<string> Partir(string linea)
    {
        var campos = new List<string>();
        var actual = new StringBuilder();
        var entreComillas = false;

        for (var i = 0; i < linea.Length; i++)
        {
            var caracter = linea[i];
            if (caracter == '"')
            {
                if (entreComillas && i + 1 < linea.Length && linea[i + 1] == '"')
                {
                    actual.Append('"');
                    i++;
                }
                else
                {
                    entreComillas = !entreComillas;
                }
            }
            else if (caracter == ',' && !entreComillas)
            {
                campos.Add(actual.ToString());
                actual.Clear();
            }
            else
            {
                actual.Append(caracter);
            }
        }

        campos.Add(actual.ToString());
        return campos;
    }
}
