using MessagingService.Application.Interfaces;
using MessagingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentStorageService _storageService;
    private readonly MessagingDbContext _dbContext;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        IAttachmentStorageService storageService,
        MessagingDbContext dbContext,
        ILogger<AttachmentsController> logger)
    {
        _storageService = storageService;
        _dbContext = dbContext;
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

        var url = $"/api/attachments/{storedFileName}";

        return Ok(new
        {
            url,
            fileName = file.FileName,
            contentType = file.ContentType
        });
    }

    [HttpGet("{filename}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAttachment(string filename, CancellationToken cancellationToken)
    {
        var filePath = _storageService.GetFilePath(filename);
        if (filePath is null)
            return NotFound();

        var message = await _dbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.AttachmentUrl != null && m.AttachmentUrl.Contains(filename), cancellationToken);

        var contentType = message?.AttachmentContentType ?? "application/octet-stream";
        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, contentType, enableRangeProcessing: true);
    }
}
