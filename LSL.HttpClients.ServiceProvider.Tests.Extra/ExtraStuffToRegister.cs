using System.Net.Http;

namespace LSL.HttpClients.ServiceProvider.Tests.Extra
{
    public interface IAnotherClient { HttpClient HttpClient { get; } }

    public class AnotherClient : IAnotherClient
    {
            public AnotherClient(HttpClient httpClient)
            {
                HttpClient = httpClient;
            }

            public HttpClient HttpClient { get; }
    }
}
