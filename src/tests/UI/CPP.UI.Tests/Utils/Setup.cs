using CPP.UI.Tests.Fixtures.Website;

namespace CPP.UI.Tests.Utils
{
    public class Setup
    {
        private void AddMockedScoped<T>(IServiceCollection services)
          where T : class
        {
            RemoveService<T>(services);

            var provider = new MockProvider<T>();
            _mockproviders[typeof(T)] = provider;

            services.AddSingleton(provider);

            services.AddScoped(sp =>
            {
                var mock = provider.Current;

                if (mock == null)
                    throw new InvalidOperationException(
                        $"Mock for {typeof(T).Name} not configured.");

                return mock;
            });
        }

        private void RemoveService<T>(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(T));

            if (descriptor != null)
                services.Remove(descriptor);
        }

        public T SetMock<T>() where T : class
        {
            var mock = NSubstitute.Substitute.For<T>();
            ((MockProvider<T>)_mockproviders[typeof(T)]).Current = mock;
            return mock;
        }

        public void ResetMocks()
        {
            foreach (var provider in _mockproviders.Values)
            {
                ((dynamic)provider).ClearReceivedCalls();
            }
        }

    }
}
