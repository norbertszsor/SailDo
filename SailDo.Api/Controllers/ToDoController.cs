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

        private static int _itemSequence;
        private static int _eventSequence;

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
            var events = Events.Where(e => string.Compare(e.Id, lastEventId, StringComparison.Ordinal) > 0)
                .ToList();

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

            Items.Add(newItem);

            Events.Add(new CloudEvent
            {
                Id = GenerateKey(true),
                Type = "ToDoItemCreated",
                Source = "api/todo",
                Time = DateTime.UtcNow.ToString("o"),
                Data = newItem,
                Method = "PUT"
            });

            return Created($"/api/todo/{key}", newItem);
        }

        [HttpPut]
        public IActionResult Update([FromBody] ToDoItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Key == item.Key);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Value = item.Value;
            existingItem.IsDone = item.IsDone;

            Events.Add(new CloudEvent
            {
                Id = GenerateKey(true),
                Type = "ToDoItemUpdated",
                Source = "api/todo",
                Time = DateTime.UtcNow.ToString("o"),
                Data = existingItem,
                Method = "PUT"
            });

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
                Id = GenerateKey(true),
                Type = "ToDoItemDeleted",
                Source = "api/todo",
                Time = DateTime.UtcNow.ToString("o"),
                Method = "DELETE",
                Subject = key
            });

            return NoContent();
        }

        public static string GenerateKey(bool isEvent = false)
        {
            return isEvent
                ? Interlocked.Increment(ref _eventSequence).ToString()
                : Interlocked.Increment(ref _itemSequence).ToString();
        }
    }
}
