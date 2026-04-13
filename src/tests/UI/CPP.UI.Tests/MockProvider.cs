namespace CPP.UI.Tests
{
    public class MockProvider<T> where T : class
    {
        public T Current { get; set; } = default!;
    }
}
