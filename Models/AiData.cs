using FuncMessageProcessor.Models.DecomposedMessage;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Models
{

        public enum Sentiment
        {
            Positive,
            Neutral,
            Negative
        }

    public class AiData
    {
        public string? Cleaned { get; set; }

        [JsonPropertyName("summary")]
        public String? Summary { get; set; }

        [JsonPropertyName("salutation")]
        public String? Salutation { get; set; }

        [JsonPropertyName("signature")]
        public String? Signature { get; set; }

        [JsonPropertyName("sentiments")]
        public List<String>? Sentiments { get; set; }
        public List<String>? Categories { get; set; }
        public String? Purpose { get; set; }
        public Tags? Tags { get; set; }

        [JsonPropertyName("questions")]
        public List<Question> Questions { get; set; } = null;

        //public List<Dictionary<string, object>>? Tags { get; set; }

    }

    //public class AiData
    //{
    //    [JsonPropertyName("topics")]
    //    public List<Topic>? Topics { get; set; }

    //    [JsonPropertyName("entities")]
    //    public List<String>? Entities { get; set; }

    //    [JsonPropertyName("sentiments")]
    //    public List<String>? Sentiments { get; set; }
    //    public List<String>? Categories { get; set; }
    //    public String? Purpose { get; set; }

    //    [JsonPropertyName("summary")]
    //    public String? Summary { get; set; }

    //    [JsonPropertyName("intent")]
    //    public String? Intent { get; set; }

    //    [JsonPropertyName("signature")]
    //    public String? Signature { get; set; }

    //    [JsonPropertyName("salutation")]
    //    public String? Salutation { get; set; }
    //    public string? Cleaned { get; set; }

    //    public Tags? Tags { get; set; }
    //}

    public class Topic
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
}
