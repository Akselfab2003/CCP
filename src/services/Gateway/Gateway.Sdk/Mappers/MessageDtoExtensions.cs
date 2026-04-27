using Gateway.Sdk.Models;

namespace Gateway.Sdk.Mappers
{
    public static class MessageDtoExtensions
    {
        internal static MessagingService.Sdk.Dtos.MessageDto? ToMessagingServiceDto(this MessageDto source)
        {
            if (source == null) return null;

            return new MessagingService.Sdk.Dtos.MessageDto
            {
                Id = source.Id ?? 0,
                TicketId = source.TicketId ?? 0,
                UserId = source.UserId,
                OrganizationId = source.OrganizationId ?? Guid.Empty,
                Content = source.Content ?? string.Empty,
                CreatedAtUtc = source.CreatedAtUtc,
                UpdatedAtUtc = source.UpdatedAtUtc,
                IsEdited = source.IsEdited ?? false,
                IsDeleted = source.IsDeleted ?? false,
                IsInternalNote = source.IsInternalNote ?? false,
                DeletedAtUtc = source.DeletedAtUtc,
                AttachmentUrl = source.AttachmentUrl,
                AttachmentFileName = source.AttachmentFileName,
                AttachmentContentType = source.AttachmentContentType
            };
        }


    }
}
