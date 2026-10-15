using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameManagement.Application.Common.Converters
{
    public class NullableBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Trường hợp truyền true/false chuẩn dạng boolean
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;
            if (reader.TokenType == JsonTokenType.Null) return null;

            // Trường hợp truyền chuỗi "true" hoặc "false"
            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString();
                if (bool.TryParse(value, out bool result))
                {
                    return result;
                }
            }

            // Nếu người dùng nhập sai định dạng (ví dụ: "abc", 123...), trả về null để [Required] bắt lỗi
            throw new JsonException("Có Server không hợp lệ. Chỉ chấp nhận true hoặc false.");
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteBooleanValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}