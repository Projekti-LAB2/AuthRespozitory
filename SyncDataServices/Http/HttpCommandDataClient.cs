using System.Net.Http;
using System.Threading.Tasks;

namespace AuthenticationAPI.SyncDataServices.Http {
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;

        public HttpCommandDataClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public Task SendAuthenticationToCommand()
        {
            throw new System.NotImplementedException();
        }
    }
}