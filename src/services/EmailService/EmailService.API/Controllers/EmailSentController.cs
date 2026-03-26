using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    public class EmailSentController : ControllerBase
    {
        private readonly IEmailSent _emailSentLogic;

        public EmailSentController(IEmailSent emailSentLogic)
        {
            _emailSentLogic = emailSentLogic;
        }

        [HttpPost("CreateSentEmail")]
        public async Task<ActionResult> CreateSentEmail(EmailSent emailSent)
        {
            var result = await _emailSentLogic.CreateAsync(emailSent);
            if (result.IsSuccess)
            {
                return Ok("Email sent created successfully.");
            }
            else
            {
                return BadRequest(result.Error.ToString());
            }
        }
        [HttpDelete("DeleteSentEmail/{id}")]
        public async Task<ActionResult> DeleteSentEmail(int id)
        {
            var result = await _emailSentLogic.DeleteAsync(id);
            if (result.IsSuccess)
            {
                return Ok("Email sent deleted successfully.");
            }
            else
            {
                return NotFound(result.Error.ToString());
            }
        }
        [HttpGet("GetSentEmailById/{id}")]
        public async Task<ActionResult<EmailSent>> GetSentEmailById(int id)
        {
            var result = await _emailSentLogic.GetByIdAsync(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("There is no sent email with this id");
            }
        }
        [HttpGet("GetSentEmailByOrganizationId/{organizationId}")]
        public async Task<ActionResult<EmailSent>> GetSentEmailByOrganizationId(Guid organizationId)
        {
            var result = await _emailSentLogic.GetByOrganizationIdAsync(organizationId);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("There is no sent email with this organization id");
            }
        }
    }
}
