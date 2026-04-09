using System;
using System.Collections.Generic;
using System.Text;

namespace Email.Api.Integration.Tests.Fixtures
{

    [CollectionDefinition("Email")]
    [Trait("Category", "Integration")]
    public class EmailServiceCollection : ICollectionFixture<EmailServiceFixture>
    {
    }
}
