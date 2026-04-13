namespace CPP.UI.Tests
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMockedScoped<TService>(this IServiceCollection services) where TService : class
        {
            var provider = new MockProvider<TService>();

            services.AddSingleton(provider);

            services.AddScoped(sp =>
            {
                var mock = sp.GetRequiredService<MockProvider<TService>>().Current;

                if (mock is null)
                {
                    throw new InvalidOperationException($"Mock for {typeof(TService).FullName} is not set.");
                }

                return mock;
            });
        }

        public static void RemoveService<T>(this IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        }
    }
}
