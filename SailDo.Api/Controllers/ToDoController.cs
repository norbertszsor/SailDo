using Microsoft.AspNetCore.Mvc;
using SailDo.Api.Transfer;

namespace SailDo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private static readonly List<ToDoItem> Items = [];
        private static readonly List<CloudEvent> Events = [];

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Items);
        }

        [HttpGet("{key}")]
        public IActionResult GetByKey(string key)
        {
            var item = Items.FirstOrDefault(i => i.Key == key);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpGet("Feed")]
        public IActionResult GetFeed(string? lastEventId, int timeOut = 5000)
        {
            var lastId = string.IsNullOrEmpty(lastEventId) ? Guid.Empty : Guid.Parse(lastEventId);

            var events = Events.Where(e => Guid.Parse(e.Id).CompareTo(lastId) < 0).ToList();

            if (events.Count == 0)
            {
                var waitHandle = new ManualResetEvent(false);

                waitHandle.WaitOne(timeOut);
            }

            return Ok(events);
        }

        [HttpPost]
        public IActionResult Create([FromBody] string value)
        {
            var key = GenerateKey();

            var newItem = new ToDoItem
            {
                Key = key,
                Value = value,
                IsDone = false
            };

            var cloudEvent = new CloudEvent
            {
                Id = key,
                Type = "ToDoItemCreated",
                Source = "api/todo",
                Time = DateTime.UtcNow.ToString("o"),
                Data = newItem,
                Method = "PUT",
                Subject = key
            };

            Items.Add(newItem);

            Events.Add(cloudEvent);

            return Created($"/api/todo/{key}", newItem);
        }

        [HttpPut("{key}")]
        public IActionResult Update(string key, [FromBody] ToDoItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Key == key);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Value = item.Value;
            existingItem.IsDone = item.IsDone;

            var cloudEvent = new CloudEvent
            {
                Id = existingItem.Key,
                Type = "ToDoItemUpdated",
                Source = "api/todo",
                Time = DateTime.UtcNow.ToString("o"),
                Data = existingItem,
                Method = "PUT"
            };

            Events.Add(cloudEvent);

            return Ok(existingItem);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            var item = Items.FirstOrDefault(i => i.Key == key);

            if (item == null)
            {
                return NotFound();
            }

            Items.Remove(item);

            Events.Add(new CloudEvent
            {
                Id = key,
                Type = "ToDoItemDeleted",
                Source = "api/todo",
                Time = DateTime.UtcNow.ToString("o"),
                Method = "DELETE"
            });

            return NoContent();
        }

        private static string GenerateKey() => Guid.NewGuid().ToString();
    }
}
