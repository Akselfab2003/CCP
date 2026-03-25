using CCP.Shared.ResultAbstraction;
using Xunit;

namespace TestUtils.Assertions
{
    public static class ResultAssertions
    {
        private static string ErrorMessageFormatted(this Result result) => $"Status: {(result.IsSuccess ? "Success" : "Failure")}, Error-Code: {result.Error.Code}, Error-Description: {result.Error.Description}";

        public static void ShouldBeSuccess(Result result)
        {
            Assert.True(result.IsSuccess, result.ErrorMessageFormatted());
        }

        public static void ShouldBeFailure(Result result)
        {
            Assert.False(result.IsSuccess, result.ErrorMessageFormatted());
        }
    }
}
