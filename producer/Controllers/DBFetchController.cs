using Microsoft.AspNetCore.Mvc;
using producer.Models.DTOs;
using producer.Services;

namespace producer.Controllers
{
    [ApiController]
    [Route("api")]
    public class DBFetchController : ControllerBase
    {
        private readonly IExchangeRateService service;

        public DBFetchController(IExchangeRateService _service)
        {
            service = _service;    
        }

        [HttpPost]
        [Route("fetch")]
        public async Task<IActionResult> FetchPackage([FromBody] ExchangePackageRequest request, CancellationToken token = default)
        {
            if (request == null || request.currencies == null)
            {
                return BadRequest("Input data of currencies pairs is required");
            }

            try
            {
                var result = await service.DocumentPackage(request, token);

                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Fetch and insert package process failed");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}