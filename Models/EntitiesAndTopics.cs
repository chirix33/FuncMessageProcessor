using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Models
{
    public class EntitiesAndTopics
    {
        [JsonPropertyName("topics")]
        public List<Topic>? Topics { get; set; }
        
        [JsonPropertyName("entities")]
        public List<String>? Entities { get; set; }

        [JsonPropertyName("sentiments")]
        public List<String>? Sentiments { get; set; }

        [JsonPropertyName("summary")]
        public String? Summary { get; set; }

        [JsonPropertyName("intent")]
        public String? Intent { get; set; }

        [JsonPropertyName("signature")]
        public String? Signature { get; set; }

        [JsonPropertyName("salutation")]
        public String? Salutation { get; set; }

    }
}
