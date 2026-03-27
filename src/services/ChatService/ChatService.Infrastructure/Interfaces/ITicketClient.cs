using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Interfaces
{
    public interface ITicketClient
    {
        Task<int?> CreateTicketAsync(
            string title,
            Guid? customerId,
            string note,
            string bearerToken,
            CancellationToken ct = default);
    }
}
