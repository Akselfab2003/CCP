namespace ChatService.Api.Endpoints
{
    public static class SessionEndpoints
    {
        public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
        {
            var sessionRoute = app.MapGroup("/session")
                                  .WithTags("Sessions");


            sessionRoute.MapGet("/", GetSessions)
                        .WithName("GetSessions")
                        .WithTags("Sessions")
                        .RequireAuthorization();

            sessionRoute.MapPost("/", CreateSession)
                        .WithName("CreateSession")
                        .WithTags("Sessions");
            return app;
        }

        private static async Task<IResult> CreateSession()
        {
            return Results.Ok();
        }

        private static async Task<IResult> GetSessions()
        {
            return Results.Ok();
        }
    }
}
