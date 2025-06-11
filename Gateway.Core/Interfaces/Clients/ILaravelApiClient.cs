using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Clients
{
    public interface ILaravelApiClient
    {
        Task<bool> IsHealthyAsync();
    }
}
