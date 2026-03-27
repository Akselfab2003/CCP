using System.Reflection;
using Xunit.v3;

namespace CPP.UI.Tests.Attributes
{
    public class WithTestNameAttribute : BeforeAfterTestAttribute
    {
        public static string CurrentTestName { get; private set; } = string.Empty;
        public static string CurrentTestClassName { get; private set; } = string.Empty;

        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            CurrentTestName = methodUnderTest.Name;
            CurrentTestClassName = methodUnderTest.DeclaringType!.Name;
        }

        public override void After(MethodInfo methodUnderTest, IXunitTest test) => base.After(methodUnderTest, test);
    }
}
