using consumer.Models;
using consumer.Models.DTOs;
using consumer.Services;
using Microsoft.AspNetCore.Mvc;

namespace consumer.Controllers
{
    [ApiController]
    [Route("api")]
    public class ConsumerController : ControllerBase
    {
        private readonly IConsumerService service;

        public ConsumerController(IConsumerService _service)
        {
            service = _service;
        }

        /// <summary>
        /// Returns the last currency rates package: ExchangePackage with ExchangeRates list.
        /// GET: /api/last_package
        /// Requires no body
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("last_package")]
        public async Task<IActionResult> ReadLastPackage(CancellationToken token = default)
        {
            try
            {
                var result = await service.GetLastPackage();

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest("Error");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Returns the last exchange rate for a given currency pair.
        /// GET: /api/pair_last
        /// Body: { "from" : <from>, "to" : <to> }
        /// Returns last ExchangePackage of the two currencies
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("pair_last")]
        public async Task<IActionResult> GetLastFromPair([FromBody] PairRequest request, CancellationToken token = default)
        {
            if (request == null || string.IsNullOrEmpty(request.from) || string.IsNullOrEmpty(request.to))
            {
                return BadRequest("Input data of pairs is required");
            }

            try
            {
                var result = await service.GetLastPairRate(request);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest();
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new {});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}