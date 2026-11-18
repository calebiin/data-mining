using System.Globalization;
using System.Text;

namespace data_mining;

/// <summary>
/// Árbol de decisión ID3. En cada nodo elige el atributo con mayor ganancia
/// de información (la mayor baja de entropía) y parte ahí las filas.
/// </summary>
public sealed class ArbolDecision
{
    public const int ProfundidadMaximaDefault = 3;

    public static readonly string[] Atributos = ["Sexo", "Clase", "Edad"];

    private static readonly string[] OrdenSexo = ["mujer", "hombre"];
    private static readonly string[] OrdenClase = ["1", "2", "3"];
    private static readonly string[] OrdenEdad = ["niño", "adulto", "desconocido"];

    private Nodo? _raiz;
    private int _profundidadMaxima = ProfundidadMaximaDefault;

    public Nodo Raiz => _raiz ?? throw new InvalidOperationException("El árbol todavía no está entrenado.");

    public int ProfundidadMaxima => _profundidadMaxima;

    public void Entrenar(IReadOnlyList<Pasajero> filas, int profundidadMaxima = ProfundidadMaximaDefault)
    {
        if (filas.Count == 0)
        {
            throw new ArgumentException("No hay filas para entrenar.", nameof(filas));
        }

        _profundidadMaxima = profundidadMaxima;
        _raiz = Construir(filas, Atributos, profundidad: 0, profundidadMaxima);
    }

    public ResultadoClasificacion Predecir(Pasajero pasajero)
    {
        var recorrido = new List<string>();
        var nodo = Raiz;

        while (!nodo.EsHoja)
        {
            var valor = pasajero.ValorDe(nodo.Atributo!);
            recorrido.Add($"{nodo.Atributo} = {valor}");

            if (!nodo.Hijos.TryGetValue(valor, out var hijo))
            {
                recorrido.Add("sin rama para ese valor; se usa la clase mayoritaria del nodo");
                return new ResultadoClasificacion(nodo.ClaseMayoritaria, recorrido);
            }

            nodo = hijo;
        }

        var etiqueta = Etiqueta(nodo.ClaseMayoritaria);
        recorrido.Add($"hoja: {etiqueta} ({nodo.Sobrevivieron}/{nodo.Total})");
        return new ResultadoClasificacion(nodo.ClaseMayoritaria, recorrido);
    }

    public string ATexto()
    {
        var texto = new StringBuilder();
        Escribir(Raiz, texto, sangria: "");
        return texto.ToString().TrimEnd();
    }

    public NodoDto ADto() => ADto(Raiz);

    /// <summary>
    /// Mezcla con semilla fija, entrena con el 80 % y cuenta aciertos en el 20 % que el árbol no vio.
    /// </summary>
    public static EvaluacionResponse Evaluar(
        IReadOnlyList<Pasajero> pasajeros,
        int profundidadMaxima = ProfundidadMaximaDefault,
        int semilla = 42)
    {
        var baraja = pasajeros.ToList();
        var azar = new Random(semilla);
        for (var i = baraja.Count - 1; i > 0; i--)
        {
            var j = azar.Next(i + 1);
            (baraja[i], baraja[j]) = (baraja[j], baraja[i]);
        }

        var corte = (int)Math.Round(baraja.Count * 0.8);
        var entrenamiento = baraja.GetRange(0, corte);
        var prueba = baraja.GetRange(corte, baraja.Count - corte);

        var arbol = new ArbolDecision();
        arbol.Entrenar(entrenamiento, profundidadMaxima);

        var aciertos = 0;
        foreach (var pasajero in prueba)
        {
            if (arbol.Predecir(pasajero).Clase == pasajero.Sobrevivio)
            {
                aciertos++;
            }
        }

        return new EvaluacionResponse(
            semilla,
            0.8,
            entrenamiento.Count,
            prueba.Count,
            aciertos,
            Math.Round(100.0 * aciertos / prueba.Count, 1),
            profundidadMaxima);
    }

    /// <summary>Mezcla de las clases. 0 = todas iguales; 1 = mitad y mitad.</summary>
    public static double Entropia(IReadOnlyList<Pasajero> filas)
    {
        if (filas.Count == 0)
        {
            return 0;
        }

        var sobrevivieron = 0;
        foreach (var fila in filas)
        {
            if (fila.Sobrevivio == 1)
            {
                sobrevivieron++;
            }
        }

        return EntropiaBinaria(sobrevivieron, filas.Count - sobrevivieron);
    }

    /// <summary>Cuánto baja la entropía si partimos por este atributo.</summary>
    public static double GananciaInformacion(IReadOnlyList<Pasajero> filas, string atributo)
    {
        var total = filas.Count;
        var ganancia = Entropia(filas);

        foreach (var grupo in filas.GroupBy(fila => fila.ValorDe(atributo)))
        {
            var parte = grupo.ToList();
            ganancia -= (double)parte.Count / total * Entropia(parte);
        }

        return ganancia;
    }

    private static Nodo Construir(
        IReadOnlyList<Pasajero> filas,
        IReadOnlyList<string> disponibles,
        int profundidad,
        int profundidadMaxima)
    {
        var hoja = CrearHoja(filas);
        if (hoja.Entropia == 0 || profundidad >= profundidadMaxima || disponibles.Count == 0)
        {
            return hoja;
        }

        string? mejor = null;
        var mejorGanancia = 0d;
        foreach (var atributo in Atributos)
        {
            if (!disponibles.Contains(atributo))
            {
                continue;
            }

            var ganancia = GananciaInformacion(filas, atributo);
            if (ganancia > mejorGanancia + 1e-9)
            {
                mejor = atributo;
                mejorGanancia = ganancia;
            }
        }

        if (mejor is null)
        {
            return hoja;
        }

        var grupos = filas
            .GroupBy(fila => fila.ValorDe(mejor))
            .ToDictionary(grupo => grupo.Key, grupo => (IReadOnlyList<Pasajero>)grupo.ToList());

        var restantes = new List<string>(disponibles.Count - 1);
        foreach (var atributo in disponibles)
        {
            if (atributo != mejor)
            {
                restantes.Add(atributo);
            }
        }

        var hijos = new Dictionary<string, Nodo>();
        foreach (var valor in OrdenarValores(mejor, grupos.Keys))
        {
            hijos[valor] = Construir(grupos[valor], restantes, profundidad + 1, profundidadMaxima);
        }

        return new Nodo
        {
            EsHoja = false,
            Atributo = mejor,
            Entropia = hoja.Entropia,
            Ganancia = mejorGanancia,
            Total = hoja.Total,
            Sobrevivieron = hoja.Sobrevivieron,
            ClaseMayoritaria = hoja.ClaseMayoritaria,
            Hijos = hijos
        };
    }

    private static Nodo CrearHoja(IReadOnlyList<Pasajero> filas)
    {
        var sobrevivieron = 0;
        foreach (var fila in filas)
        {
            if (fila.Sobrevivio == 1)
            {
                sobrevivieron++;
            }
        }

        var total = filas.Count;
        return new Nodo
        {
            EsHoja = true,
            Entropia = EntropiaBinaria(sobrevivieron, total - sobrevivieron),
            Total = total,
            Sobrevivieron = sobrevivieron,
            ClaseMayoritaria = sobrevivieron > total - sobrevivieron ? 1 : 0
        };
    }

    private static double EntropiaBinaria(int positivos, int negativos)
    {
        var total = positivos + negativos;
        if (total == 0 || positivos == 0 || negativos == 0)
        {
            return 0;
        }

        var p = (double)positivos / total;
        var q = (double)negativos / total;
        return -(p * Math.Log2(p) + q * Math.Log2(q));
    }

    private static IEnumerable<string> OrdenarValores(string atributo, IEnumerable<string> presentes)
    {
        var preferido = atributo switch
        {
            "Sexo" => OrdenSexo,
            "Clase" => OrdenClase,
            "Edad" => OrdenEdad,
            _ => Array.Empty<string>()
        };

        var vistos = presentes.ToHashSet(StringComparer.Ordinal);
        foreach (var valor in preferido)
        {
            if (vistos.Contains(valor))
            {
                yield return valor;
            }
        }

        foreach (var valor in vistos.OrderBy(valor => valor, StringComparer.Ordinal))
        {
            if (!preferido.Contains(valor))
            {
                yield return valor;
            }
        }
    }

    private static void Escribir(Nodo nodo, StringBuilder texto, string sangria)
    {
        if (nodo.EsHoja)
        {
            texto.Append(sangria);
            texto.Append(Etiqueta(nodo.ClaseMayoritaria));
            texto.Append(" (");
            texto.Append(nodo.Sobrevivieron);
            texto.Append('/');
            texto.Append(nodo.Total);
            texto.AppendLine(")");
            return;
        }

        texto.Append(sangria);
        texto.Append(nodo.Atributo);
        texto.Append("?  ganancia=");
        texto.Append(nodo.Ganancia?.ToString("0.000", CultureInfo.InvariantCulture));
        texto.Append("  entropia=");
        texto.Append(nodo.Entropia.ToString("0.000", CultureInfo.InvariantCulture));
        texto.Append("  (");
        texto.Append(nodo.Sobrevivieron);
        texto.Append('/');
        texto.Append(nodo.Total);
        texto.AppendLine(")");

        foreach (var (valor, hijo) in nodo.Hijos)
        {
            texto.Append(sangria);
            texto.Append("  ");
            texto.Append(valor);
            texto.AppendLine(":");
            Escribir(hijo, texto, sangria + "    ");
        }
    }

    private static NodoDto ADto(Nodo nodo)
    {
        IReadOnlyList<RamaDto>? ramas = null;
        if (!nodo.EsHoja)
        {
            var lista = new List<RamaDto>(nodo.Hijos.Count);
            foreach (var (valor, hijo) in nodo.Hijos)
            {
                lista.Add(new RamaDto(valor, ADto(hijo)));
            }

            ramas = lista;
        }

        return new NodoDto(
            nodo.EsHoja,
            nodo.Atributo,
            Math.Round(nodo.Entropia, 3),
            nodo.Ganancia is null ? null : Math.Round(nodo.Ganancia.Value, 3),
            nodo.Total,
            nodo.Sobrevivieron,
            nodo.Total - nodo.Sobrevivieron,
            nodo.ClaseMayoritaria,
            Etiqueta(nodo.ClaseMayoritaria),
            ramas);
    }

    private static string Etiqueta(int clase) => clase == 1 ? "sobrevive" : "no sobrevive";
}
