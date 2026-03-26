using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    public class EmailReceivedController : ControllerBase
    {
        private readonly IEmailReceived _emailReceivedLogic;

        public EmailReceivedController(IEmailReceived emailReceivedLogic)
        {
            _emailReceivedLogic = emailReceivedLogic;
        }
        [HttpPost("CreateReceivedEmail")]
        public async Task<ActionResult> CreateReceivedEmail(EmailReceived emailReceived)
        {
            var result = await _emailReceivedLogic.CreateAsync(emailReceived);
            if (result.IsSuccess)
            {
                return Ok("Email received created successfully.");
            }
            else
            {
                return BadRequest(result.Error.ToString());
            }
        }
        [HttpDelete("DeleteReceivedEmail/{id}")]
        public async Task<ActionResult> DeleteReceivedEmail(int id)
        {
            var result = await _emailReceivedLogic.DeleteAsync(id);
            if (result.IsSuccess)
            {
                return Ok("Email received deleted successfully.");
            }
            else
            {
                return NotFound(result.Error.ToString());
            }
        }
        [HttpGet("GetReceivedEmailById/{id}")]
        public async Task<ActionResult<EmailReceived>> GetReceivedEmailById(int id)
        {
            var result = await _emailReceivedLogic.GetByIdAsync(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("There is no received email with this id");
            }
        }
        [HttpGet("GetReceivedEmailByOrganizationId/{organizationId}")]
        public async Task<ActionResult<EmailReceived>> GetReceivedEmailByOrganizationId(Guid organizationId)
        {
            var result = await _emailReceivedLogic.GetByOrganizationIdAsync(organizationId);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("There is no received email with this organization id");
            }
        }
    }
}
