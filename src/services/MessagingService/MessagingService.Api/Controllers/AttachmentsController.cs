using MessagingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentStorageService _storageService;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        IAttachmentStorageService storageService,
        ILogger<AttachmentsController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadAttachment(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        await using var stream = file.OpenReadStream();
        var storedFileName = await _storageService.SaveFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        var url = $"/attachments/{storedFileName}";

        return Ok(new
        {
            url,
            fileName = file.FileName,
            contentType = file.ContentType
        });
    }
}
