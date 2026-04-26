using CCP.Shared.AuthContext;
using MessagingService.Api.IntegrationTests.Fixtures;
using MessagingService.Sdk.Dtos;
using MessagingService.Sdk.Services;
using TicketService.Sdk.Services.Ticket;

namespace MessagingService.Api.IntegrationTests.Tests
{
    [Collection("MessagingService")]
    public class MessagesEndpointTests
    {
        private readonly MessagingServiceFixture _fixture;

        public MessagesEndpointTests(MessagingServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateMessage_ShouldCreateMessageSuccessfully()
        {
            // Arrange
            Guid Org_Id = Guid.NewGuid();
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            using var ticketScope = _fixture.TicketSDK.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            ITicketService ticketService = ticketScope.ServiceProvider.GetRequiredService<ITicketService>();
            ticketScope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);
            SDK_Scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);

            Guid User_Id = Guid.NewGuid();
            var ticketResult = await ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
            {

                Title = "Test",
                AssignedUserId = User_Id,
                CustomerId = Guid.NewGuid(),
                Description = "Test",
                OrganizationId = Org_Id
            }, CCP.Shared.ValueObjects.TicketOrigin.Manual, TestContext.Current.CancellationToken);

            Assert.NotNull(ticketResult);
            Assert.True(ticketResult.IsSuccess);


            var ticketId = ticketResult.Value;



            // Act
            Result<MessageDto> result = await messageSdkService.CreateMessageAsync(ticketId: ticketId,
                                                                    organizationId: Org_Id,
                                                                    userId: User_Id,
                                                                    content: "Hello, this is a test message!",
                                                                    cancellationToken: TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(ticketId, result.Value.TicketId);
            Assert.Equal(Org_Id, result.Value.OrganizationId);
            Assert.Equal(User_Id, result.Value.UserId);
            Assert.Equal("Hello, this is a test message!", result.Value.Content);
            Assert.False(result.Value.IsDeleted);
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnBadRequestForInvalidInput()
        {
            // Arrange
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            Guid Org_Id = Guid.NewGuid();
            Guid User_Id = Guid.NewGuid();
            int InvalidTicketId = -1;

            // Act
            Result<MessageDto> result = await messageSdkService.CreateMessageAsync(ticketId: InvalidTicketId,
                                                                    organizationId: Org_Id,
                                                                    userId: User_Id,
                                                                    content: "This message should fail",
                                                                    cancellationToken: TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task GetMessageById_ShouldReturnMessageForValidId()
        {
            // Arrange
            Guid Org_Id = Guid.NewGuid();
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            using var ticketScope = _fixture.TicketSDK.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            ITicketService ticketService = ticketScope.ServiceProvider.GetRequiredService<ITicketService>();
            ticketScope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);
            SDK_Scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);

            Guid User_Id = Guid.NewGuid();
            var ticketResult = await ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
            {

                Title = "Test",
                AssignedUserId = User_Id,
                CustomerId = Guid.NewGuid(),
                Description = "Test",
                OrganizationId = Org_Id
            }, CCP.Shared.ValueObjects.TicketOrigin.Manual, TestContext.Current.CancellationToken);
            var TicketId = ticketResult.Value;
            Result<MessageDto> createResult = await messageSdkService.CreateMessageAsync(ticketId: TicketId,
                                                                          organizationId: Org_Id,
                                                                          userId: User_Id,
                                                                          content: "Hello, this is a test message!",
                                                                          cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(createResult);
            Assert.True(createResult.IsSuccess);
            int MessageId = createResult.Value.Id;

            // Act
            Result<MessageDto> getResult = await messageSdkService.GetMessageByIdAsync(MessageId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(getResult);
            Assert.True(getResult.IsSuccess);
            Assert.Equal(MessageId, getResult.Value.Id);
            Assert.Equal(TicketId, getResult.Value.TicketId);
            Assert.Equal(Org_Id, getResult.Value.OrganizationId);
            Assert.Equal(User_Id, getResult.Value.UserId);
            Assert.Equal("Hello, this is a test message!", getResult.Value.Content);
        }

        [Fact]
        public async Task GetMessageById_ShouldReturnNotFoundForInvalidId()
        {
            // Arrange
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            int NonExistingMessageId = -1;

            // Act
            Result<MessageDto> result = await messageSdkService.GetMessageByIdAsync(NonExistingMessageId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }


        [Fact]
        public async Task GetMessagesByTicketId_ShouldReturnMessagesForGivenTicket()
        {
            // Arrange
            Guid Org_Id = Guid.NewGuid();
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            using var ticketScope = _fixture.TicketSDK.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            ITicketService ticketService = ticketScope.ServiceProvider.GetRequiredService<ITicketService>();
            ticketScope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);
            SDK_Scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);

            Guid User_Id = Guid.NewGuid();
            var ticketResult = await ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
            {

                Title = "Test",
                AssignedUserId = User_Id,
                CustomerId = Guid.NewGuid(),
                Description = "Test",
                OrganizationId = Org_Id
            }, CCP.Shared.ValueObjects.TicketOrigin.Manual, TestContext.Current.CancellationToken);
            var TicketId = ticketResult.Value;
            await messageSdkService.CreateMessageAsync(ticketId: TicketId,
                                                        organizationId: Org_Id,
                                                        userId: User_Id,
                                                        content: "Hello, this is a test message!",
                                                        cancellationToken: TestContext.Current.CancellationToken);

            // Act
            Result<PagedMessagesDto> result = await messageSdkService.GetMessagesByTicketIdAsync(TicketId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Items);
            Assert.All(result.Value.Items, msg => Assert.Equal(TicketId, msg.TicketId));
        }

        [Fact]
        public async Task GetMessagesByTicketId_ShouldReturnBadRequestForInvalidTicketId()
        {
            // Arrange
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            int NonExistingTicketId = -1;

            // Act
            Result<PagedMessagesDto> result = await messageSdkService.GetMessagesByTicketIdAsync(NonExistingTicketId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }


        [Fact]
        public async Task UpdateMessage_ShouldUpdateMessageSuccessfully()
        {
            // Arrange
            Guid Org_Id = Guid.NewGuid();
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            using var ticketScope = _fixture.TicketSDK.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            ITicketService ticketService = ticketScope.ServiceProvider.GetRequiredService<ITicketService>();
            ticketScope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);
            SDK_Scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);

            Guid User_Id = Guid.NewGuid();
            var ticketResult = await ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
            {

                Title = "Test",
                AssignedUserId = User_Id,
                CustomerId = Guid.NewGuid(),
                Description = "Test",
                OrganizationId = Org_Id
            }, CCP.Shared.ValueObjects.TicketOrigin.Manual, TestContext.Current.CancellationToken);
            var TicketId = ticketResult.Value;
            Result<MessageDto> createResult = await messageSdkService.CreateMessageAsync(ticketId: TicketId,
                                                                          organizationId: Org_Id,
                                                                          userId: User_Id,
                                                                          content: "Hello, this is a test message!",
                                                                          cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(createResult);
            Assert.True(createResult.IsSuccess);
            int MessageId = createResult.Value.Id;

            // Act
            Result<MessageDto> updateResult = await messageSdkService.UpdateMessageAsync(MessageId, "Updated content", cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(updateResult);
            Assert.True(updateResult.IsSuccess);
            Assert.Equal(MessageId, updateResult.Value.Id);
            Assert.Equal("Updated content", updateResult.Value.Content);
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnNotFoundForInvalidMessageId()
        {
            // Arrange
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            int NonExistingMessageId = -1;
            // Act
            Result<MessageDto> result = await messageSdkService.UpdateMessageAsync(NonExistingMessageId, "Updated content", cancellationToken: TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }


        [Fact]
        public async Task DeleteMessage_ShouldSoftDeleteMessageSuccessfully()
        {
            // Arrange
            Guid Org_Id = Guid.NewGuid();
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            using var ticketScope = _fixture.TicketSDK.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            ITicketService ticketService = ticketScope.ServiceProvider.GetRequiredService<ITicketService>();
            ticketScope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);
            SDK_Scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>().SetOrganizationId(Org_Id);

            Guid User_Id = Guid.NewGuid();
            var ticketResult = await ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
            {

                Title = "Test",
                AssignedUserId = User_Id,
                CustomerId = Guid.NewGuid(),
                Description = "Test",
                OrganizationId = Org_Id
            }, CCP.Shared.ValueObjects.TicketOrigin.Manual, TestContext.Current.CancellationToken);
            var TicketId = ticketResult.Value;
            Result<MessageDto> createResult = await messageSdkService.CreateMessageAsync(ticketId: TicketId,
                                                                          organizationId: Org_Id,
                                                                          userId: User_Id,
                                                                          content: "Hello, this is a test message!",
                                                                          cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(createResult);
            Assert.True(createResult.IsSuccess);
            int MessageId = createResult.Value.Id;
            // Act
            var deleteResult = await messageSdkService.DeleteMessageAsync(MessageId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(deleteResult);
            Assert.True(deleteResult.IsSuccess);

            // Verify the message is marked as deleted
            Result<MessageDto> getResult = await messageSdkService.GetMessageByIdAsync(MessageId, cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(getResult);
            Assert.True(getResult.IsSuccess);
            Assert.True(getResult.Value.IsDeleted);
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturnNotFoundForInvalidMessageId()
        {
            // Arrange
            using var SDK_Scope = _fixture.SDK.CreateScope();
            using var DB_Scope = _fixture.DB.CreateScope();
            IMessageSdkService messageSdkService = SDK_Scope.ServiceProvider.GetRequiredService<IMessageSdkService>();
            int NonExistingMessageId = -1;

            // Act
            var result = await messageSdkService.DeleteMessageAsync(NonExistingMessageId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }
    }
}
