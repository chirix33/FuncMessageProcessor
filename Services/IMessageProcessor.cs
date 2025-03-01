using FuncMessageProcessor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    public interface IMessageProcessor
    {
        Task ProcessMessage(EnrichedMessage inputMessage);
        Task<AiData> ExtractEntitiesAndTopics(String content);
        Task<Tags> AssignTags(String content);
        Task<String> DecomposeMessage(String content);
    }
}
