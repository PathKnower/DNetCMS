using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNetTool.Hubs
{
    public class ProcessingHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.InvokeAsync("Send", message);
        }
    }
}
