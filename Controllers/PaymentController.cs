using CyberSourceIntegration.Services;
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

        [HttpPost("create-session")]
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
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var transactionId = await _cyberSourceService.ProcessPaymentAsync(request.TransientToken);
                return Ok(new { transactionId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class ProcessPaymentRequest
    {
        public string TransientToken { get; set; } = string.Empty;
    }
}
