using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Gateway.Core.Models.Cache;
public class CacheInvalidationMessage
{
    public string Key { get; set; } = string.Empty;

    public string ToJson() => JsonSerializer.Serialize(this);
    public static CacheInvalidationMessage? FromJson(string json)
        => JsonSerializer.Deserialize<CacheInvalidationMessage>(json);
}
