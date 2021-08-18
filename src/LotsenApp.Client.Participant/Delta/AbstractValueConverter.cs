#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LotsenApp.Client.Participant.Delta
{
    [ExcludeFromCodeCoverage]
    public class AbstractValueConverter<T, TS>: JsonConverter<TS[]> where T: TS
    {
        public override TS[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T[]>(ref reader, options)?.Cast<TS>().ToArray();
        }

        public override void Write(Utf8JsonWriter writer, TS[] value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            JsonSerializer.Serialize(writer, value.Cast<T>(), options);
        }
    }
}