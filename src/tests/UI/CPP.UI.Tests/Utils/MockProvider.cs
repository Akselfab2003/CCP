namespace CPP.UI.Tests.Fixtures.Website
{
    public class MockProvider<T> where T : class
    {
        public T Current { get; set; } = default!;
    }
}
