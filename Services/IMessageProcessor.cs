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
        void ProcessMessage(EnrichedMessage inputMessage);
    }
}
