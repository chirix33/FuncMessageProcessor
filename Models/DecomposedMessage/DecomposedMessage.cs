using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Models.DecomposedMessage
{
    public class DecomposedMessage
    {
        [JsonPropertyName("questions")]
        public List<Question> Questions { get; set; } = null;
    }
}
