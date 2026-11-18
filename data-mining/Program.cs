using data_mining;
using Microsoft.AspNetCore.Http.HttpResults;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

var app = builder.Build();

var rutaCsv = ResolverRutaCsv(app);
var pasajeros = LectorCsv.Leer(rutaCsv);
if (pasajeros.Count == 0)
{
    throw new InvalidOperationException($"El CSV no tiene pasajeros: {rutaCsv}");
}

var arbol = new ArbolDecision();
arbol.Entrenar(pasajeros);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Mineria v1");
        opciones.DocumentTitle = "Minería de datos — Titanic";
    });
}

app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
    .ExcludeFromDescription();

app.MapGet("/arbol", () => new ArbolResponse(
        "ID3 (ganancia de información)",
        "Sobrevivio",
        "niño si edad < 16, adulto si edad >= 16, desconocido si viene vacía",
        pasajeros.Count,
        arbol.ProfundidadMaxima,
        ArbolDecision.Atributos,
        arbol.ATexto(),
        arbol.ADto()))
    .WithName("ObtenerArbol")
    .WithTags("Arbol")
    .WithSummary("Árbol ID3 entrenado con todos los pasajeros.")
    .WithDescription("Devuelve el árbol en texto, para leerlo en clase, y la estructura con la entropía y la ganancia de cada pregunta.");

app.MapPost("/clasificar", Results<Ok<ClasificacionResponse>, BadRequest<ErrorResponse>> (ClasificacionRequest solicitud) =>
    {
        if (!TryCrearPasajero(solicitud, out var pasajero, out var error))
        {
            return TypedResults.BadRequest(new ErrorResponse(error));
        }

        var resultado = arbol.Predecir(pasajero);
        return TypedResults.Ok(new ClasificacionResponse(
            resultado.Clase == 1 ? "sobrevive" : "no sobrevive",
            resultado.Clase,
            pasajero.Sexo,
            pasajero.Clase,
            pasajero.GrupoEdad,
            resultado.Recorrido));
    })
    .WithName("ClasificarPasajero")
    .WithTags("Arbol")
    .WithSummary("Predice si un pasajero nuevo sobrevive.")
    .WithDescription("Recorre el árbol con sexo, clase y edad. Acepta mujer/hombre o female/male.");

app.MapGet("/evaluacion", () => ArbolDecision.Evaluar(pasajeros))
    .WithName("EvaluarArbol")
    .WithTags("Arbol")
    .WithSummary("Aciertos en un 20 % que el árbol no vio al entrenar.")
    .WithDescription("Baraja los pasajeros con una semilla fija, entrena con el 80 % y mide el porcentaje de acierto en el resto.");

app.Run();

static string ResolverRutaCsv(WebApplication app)
{
    var candidatos = new[]
    {
        Path.Combine(app.Environment.ContentRootPath, "Data", "train.csv"),
        Path.Combine(AppContext.BaseDirectory, "Data", "train.csv")
    };

    foreach (var ruta in candidatos)
    {
        if (File.Exists(ruta))
        {
            return ruta;
        }
    }

    throw new FileNotFoundException(
        "No se encontró Data/train.csv junto al proyecto ni en la carpeta de salida.");
}

static bool TryCrearPasajero(ClasificacionRequest solicitud, out Pasajero pasajero, out string error)
{
    pasajero = null!;
    var sexo = solicitud.Sexo?.Trim().ToLowerInvariant() switch
    {
        "female" or "mujer" or "f" => "mujer",
        "male" or "hombre" or "m" => "hombre",
        _ => null
    };

    if (sexo is null)
    {
        error = "Sexo debe ser mujer/hombre o female/male.";
        return false;
    }

    if (solicitud.Clase is < 1 or > 3)
    {
        error = "Clase debe ser 1, 2 o 3.";
        return false;
    }

    if (solicitud.Edad is < 0 or > 120)
    {
        error = "Edad debe estar entre 0 y 120, o omitirse si se desconoce.";
        return false;
    }

    error = "";
    pasajero = new Pasajero(sexo, solicitud.Clase, Pasajero.GrupoPorEdad(solicitud.Edad), sobrevivio: 0);
    return true;
}
