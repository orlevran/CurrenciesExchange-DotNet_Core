using Microsoft.AspNetCore.Mvc;
using producer.Services;
using producer.Models.DTOs;

namespace producer.Controllers
{
    [ApiController]
    [Route("kafka")]
    public class KafkaController(IConfiguration config, IKafkaStreamManager sm) : ControllerBase
    {
        private readonly IConfiguration config = config;
        private readonly IKafkaStreamManager streamManager = sm;

        /*
        - POST /kafka/start
        - Start a new Kafka stream with given pairs and interval
        - Body: { "pairs": [ {"from": "USD", "to": "EUR"}, ... ], "intervalInSeconds": 60 }
        - Returns 400 Bad Request if input is invalid or service not in Kafka mode
        - Returns 200 OK with { "streamId": "generated-id", "intervalSeconds": 60, "pairs": [...] }
        - Example: POST /kafka/start
        - Body: { "pairs": [ {"from": "USD", "to": "EUR"}, {"from": "GBP", "to": "JPY"} ], "intervalInSeconds": 120 }
        - Returns: { "streamId": "streamid1", "intervalSeconds": 120, "pairs": [ {"from": "USD", "to": "EUR"}, {"from": "GBP", "to": "JPY"} ] }
        - Note: interval is minimum 60 seconds, higher values accepted
        */
        [HttpPost("start")]
        public IActionResult Start([FromBody] StartKafkaStreamRequest req, CancellationToken ct)
        {
            var mode = config["RunMode"] ?? "Kafka";
            if (!string.Equals(mode, "Kafka", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Service is not in Kafka mode.");

            if (req.Pairs is null || req.Pairs.Count == 0)
                return BadRequest("At least one pair is required.");

            var id = streamManager.Start(req, ct);
            return Ok(new { streamId = id, intervalSeconds = Math.Max(60, req.IntervalInSeconds), pairs = req.Pairs });
        }

        /*
        - POST /kafka/stop/{id}
        - Stop a running stream by its ID
        - Returns 200 OK if stopped
        - Returns 404 Not Found if no such stream
        - Requires no body
        - Example: POST /kafka/stop/streamid1
        - Returns: { "streamId": "streamid1", "status": "stopped" }
        - or: { "streamId": "streamid1", "status": "not-found" }
        - Note: stopping a non-existing stream is not an error, as it may have already stopped or never existed
        - This is idempotent and safe to call multiple times
        - CancellationToken is not used here
        */
        [HttpPost("stop/{id}")]
        public IActionResult Stop(string id)
        {
            id = (id ?? "").Trim();
            var ok = streamManager.Stop(id);
            return ok ? Ok(new { streamId = id, status = "stopped" })
                    : NotFound(new { streamId = id, status = "not-found", active = sm.List() });
        }

        /*
        - GET /kafka/streams
        - List active streams
        - Returns array of stream IDs
        - Example: [ "streamid1", "streamid2" ]
        - If empty, returns []
        - Requires no body
        - Returns 200 OK
        */
        [HttpGet("streams")]
        public IActionResult Streams() => Ok(sm.List());

        /*
        - GET /kafka/instance
        - Get instance ID and process ID
        - Returns { "instanceId": "id", "pid": 12345 }
        - Example: { "instanceId": "abc123", "pid": 5678 }
        - Requires no body
        - Returns 200 OK
        */
        [HttpGet("instance")]
        public IActionResult Instance([FromServices] InstanceStamp s)
            => Ok(new { instanceId = s.Id, pid = s.Pid });
    }
}