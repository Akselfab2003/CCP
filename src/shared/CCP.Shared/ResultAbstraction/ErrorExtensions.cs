namespace CCP.Shared.ResultAbstraction
{
    public static class ErrorExtensions
    {
        public static string ToLogString(this Error error)
        {
            return $"Error Code: {error.Code}, Type: {error.Type}, Description: {error.Description}";
        }
    }
}
