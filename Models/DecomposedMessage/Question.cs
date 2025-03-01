using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Models.DecomposedMessage
{
    public class Question
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; }

        [JsonPropertyName("topics")]
        public List<Topic> Topics { get; set; }

        [JsonPropertyName("intents")]
        public List<String> Intents { get; set; }
    }
}
