using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Clients
{
    public interface IAiServicesClient
    {
        Task<bool> IsHealthyAsync();
    }
}
