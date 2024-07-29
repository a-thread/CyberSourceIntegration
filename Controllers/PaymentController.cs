using CyberSourceIntegration.Services;
using CyberSourceIntegration.Models;
using Microsoft.AspNetCore.Mvc;

namespace CyberSourceIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ICyberSourceService _cyberSourceService;

        public PaymentController(ICyberSourceService cyberSourceService)
        {
            _cyberSourceService = cyberSourceService;
        }

        [HttpGet("create-session")]
        public async Task<IActionResult> CreatePaymentSession()
        {
            try
            {
                var captureContext = await _cyberSourceService.CreatePaymentSessionAsync();
                return Ok(new { captureContext });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.TransientToken))
            {
                return BadRequest("Invalid payment request.");
            }

            try
            {
                var result = await _cyberSourceService.ProcessPaymentAsync(request.TransientToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
