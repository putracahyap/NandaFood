using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NandaFood_Auth.Models.Global;

public class ApiMessage<T>
{
    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [DefaultValue(null)]
    [DataMember(EmitDefaultValue = false)]
    //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public ApiMessage() { }
}