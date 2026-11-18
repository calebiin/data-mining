using System.Text.Json.Serialization;

namespace data_mining;

[JsonSerializable(typeof(ArbolResponse))]
[JsonSerializable(typeof(NodoDto))]
[JsonSerializable(typeof(RamaDto))]
[JsonSerializable(typeof(ClasificacionRequest))]
[JsonSerializable(typeof(ClasificacionResponse))]
[JsonSerializable(typeof(EvaluacionResponse))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
