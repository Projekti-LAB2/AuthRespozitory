using System.Threading.Tasks;

namespace AuthenticationAPI.SyncDataServices.Http {
    public interface ICommandDataClient {
        Task SendAuthenticationToCommand(); 
    }
}