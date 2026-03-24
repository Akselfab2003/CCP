using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace CCP.Sdk.utils.Abstractions
{
    public class KiotaApiClientAbstraction<T> : IKiotaApiClient<T>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiclientName;
        private readonly Func<IRequestAdapter, T> _clientFactory;

        public T Client { get; private set; }

        public KiotaApiClientAbstraction(IHttpClientFactory httpClientFactory, string apiclientName, Func<IRequestAdapter, T> clientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _apiclientName = apiclientName;
            _clientFactory = clientFactory;
            Client = CreateApiClient();
        }

        public T CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient(_apiclientName);
            var requestAdapter = new HttpClientRequestAdapter(authenticationProvider: new AnonymousAuthenticationProvider(),
                                                              httpClient: client);
            return _clientFactory(requestAdapter);
        }
    }
}
