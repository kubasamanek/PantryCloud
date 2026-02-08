using System.Text.Json;
using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Household;

/// <summary>
/// Converts Role from API: accepts either number (0=Owner, 1=Member) or string ("Owner", "Member").
/// We store as int: 0 = Owner, 1 = Member, so filtering works regardless of API format.
/// </summary>
internal sealed class HouseholdRoleJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var num))
            return num;
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.Equals(s, "Owner", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.Equals(s, "Member", StringComparison.OrdinalIgnoreCase)) return 1;
        }
        if (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.None)
            return 1;
        return 1;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}
