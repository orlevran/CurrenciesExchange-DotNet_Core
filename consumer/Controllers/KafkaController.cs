using consumer.Services;
using consumer.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace consumer.Controllers
{
    [ApiController]
    [Route("kafka")]
    public class KafkaController(IConfiguration config, ILastPackageCache cache) : ControllerBase
    {
        [HttpGet("mode")] public IActionResult Mode() => Ok(new { RunMode = config["RunMode"] ?? "Kafka" });
        [HttpGet("topic")] public IActionResult Topic() => Ok(new { Topic = config.GetSection("Kafka")["Topic"] ?? "exchange-packages" });
        [HttpGet("last")]
        public IActionResult Last()
        {
            var last = cache.GetLastPackage();
            return last is null ? NoContent() : Ok(last);

            //var last = cache.GetLastPackage();
            //return last is null ? NotFound("No package consumed yet.") : Ok(last);
        }

        [HttpGet("config")]
            public IActionResult Config([FromServices] Microsoft.Extensions.Options.IOptions<KafkaSettings> ks,
                [FromServices] IConfiguration cfg) => Ok(new {
                    ks.Value.BootstrapServers,
                    ks.Value.Topic,
                    ks.Value.GroupId,
                    RunMode = cfg["RunMode"] });

    }
}