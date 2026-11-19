using System.Text.Json.Serialization;

namespace data_mining;

[JsonSerializable(typeof(ArbolResponse))]
[JsonSerializable(typeof(ClasificacionRequest))]
[JsonSerializable(typeof(ClasificacionResponse))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
