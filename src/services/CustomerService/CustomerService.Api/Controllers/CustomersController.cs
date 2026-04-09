using CustomerService.Api.DB.Models;
using CustomerService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        //Henter alle customers fra databasen
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomers();
            return Ok(customers);
        }

        //Henter en specifik customer via ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var customer = await _customerService.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound(); //HTTP 404
            }

            return Ok(customer); //HTTP 200
        }

        //Opretter en ny customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            var createdCustomer = await _customerService.CreateCustomer(customer);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = createdCustomer.Id },
                createdCustomer
            );
        }

        //Opdaterer en eksisterende customer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Customer customer)
        {
            //Kald servicen for at opdatere kunden
            var updatedCustomer = await _customerService.UpdateCustomer(customer);

            //Hvis kunden ikke blev fundet, retuner 404
            if (updatedCustomer == null)
            {
                return NotFound();
            }

            //Retuner 200 OK med den opdaterede kunde
            return Ok(updatedCustomer);
        }

        //Sletter en customer permanent
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            //Kald servicen for at slette kunden
            var wasDeleted = await _customerService.DeleteCustomer(id);

            if (!wasDeleted)
            {
                return NotFound(); //returner 404
            }

            return NoContent(); //Returner 204
        }
    }
}
