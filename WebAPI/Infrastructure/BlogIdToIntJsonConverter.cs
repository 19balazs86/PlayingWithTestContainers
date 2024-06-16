using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebAPI.Data;

namespace WebAPI.Infrastructure;

// You can use this converter in the SystemText JsonSerializerOptions
// Or add it globally: services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new BlogIdToIntJsonConverter()));

// Better to use the following libraries that using source generate to create Strongly-Typed Id
// 👤Andrew Lock - https://github.com/andrewlock/StronglyTypedId
// 👤Steve Dunn - https://github.com/SteveDunn/Vogen
public sealed class BlogIdToIntJsonConverter : JsonConverter<BlogId>
{
    public override BlogId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int id;

        if (reader.TokenType == JsonTokenType.String)
        {
            ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;

            // Try to parse number directly from bytes
            if (Utf8Parser.TryParse(span, out id, out int bytesConsumed) && span.Length == bytesConsumed)
                return BlogId.Create(id);

            // Try to parse from a string if the above failed, this covers cases with other escaped UTF characters
            if (int.TryParse(reader.GetString(), out id))
                return BlogId.Create(id);
        }

        // Fallback to default handling

        // GetInt32 throw exception in case of not Number
        id = reader.GetInt32();

        return BlogId.Create(id);
    }

    public override void Write(Utf8JsonWriter writer, BlogId blogId, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(blogId.Value);
    }
}