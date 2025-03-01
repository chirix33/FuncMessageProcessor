using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    public interface IKernelService
    {
        public ServiceProvider BuildServiceProvider(string? model = null);
    }
}
