using consumer.Services;
using consumer.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace consumer.Controllers
{
    [ApiController]
    [Route("kafka")]
    public class KafkaController(IConfiguration config, ILastPackageCache cache) : ControllerBase
    {
        /// <summary>
        /// Returns the current run mode of the service.
        /// Defaults to "Kafka" if not configured.
        /// </summary>
        /// <returns>A JSON object with the RunMode property.</returns>
        [HttpGet("mode")] public IActionResult Mode() => Ok(new { RunMode = config["RunMode"] ?? "Kafka" });

        /// <summary>
        /// Returns the Kafka topic name being used.
        /// Defaults to "exchange-packages" if not configured.
        /// </summary>
        /// <returns>A JSON object with the Topic property.</returns>
        [HttpGet("topic")] public IActionResult Topic() => Ok(new { Topic = config.GetSection("Kafka")["Topic"] ?? "exchange-packages" });

        /// <summary>
        /// Retrieves the last consumed Kafka package from the cache.
        /// </summary>
        /// <returns>
        /// 204 No Content if no package is available,
        /// otherwise 200 OK with the last package.
        /// </returns>
        [HttpGet("last")]
        public IActionResult Last()
        {
            var last = cache.GetLastPackage();
            return last is null ? NoContent() : Ok(last);

            //var last = cache.GetLastPackage();
            //return last is null ? NotFound("No package consumed yet.") : Ok(last);
        }

        /// <summary>
        /// Returns the current Kafka configuration values from dependency injection.
        /// Includes BootstrapServers, Topic, GroupId, and RunMode.
        /// </summary>
        /// <param name="ks">Kafka settings injected via IOptions.</param>
        /// <param name="cfg">Application configuration source.</param>
        /// <returns>A JSON object with Kafka configuration details.</returns>
        [HttpGet("config")]
            public IActionResult Config([FromServices] Microsoft.Extensions.Options.IOptions<KafkaSettings> ks,
                [FromServices] IConfiguration cfg) => Ok(new {
                    ks.Value.BootstrapServers,
                    ks.Value.Topic,
                    ks.Value.GroupId,
                    RunMode = cfg["RunMode"] });

    }
}