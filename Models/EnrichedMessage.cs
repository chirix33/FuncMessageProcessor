using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FuncMessageProcessor.Models
{
    public class EnrichedMessage : Message
    {
        public MessageMetadata Metadata { get; set; } = null!;
        public AiData AiData { get; set; } = null!;
    }
}
