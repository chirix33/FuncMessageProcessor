using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Models
{
    public class Tags
    {
        [JsonPropertyName("message_tags")]
        public List<String>? MessageTags { get; set; }
    }
}
