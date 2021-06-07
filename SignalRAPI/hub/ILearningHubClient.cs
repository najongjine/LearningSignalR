using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRAPI.hub
{
  public interface ILearningHubClient
  {
    Task ReceiveMessage(string message);
  }
}
