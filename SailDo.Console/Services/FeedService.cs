using SailDo.Api.Transfer;
using SailDo.Console.Helpers;
using SailDo.Console.Interfaces;

namespace SailDo.Console.Services
{
    public class FeedService(ISailDoClient client) : IFeedServices
    {
        private const string ToDoSnapshotFileName = "ToDoItemsCurrentSnapshot.json";

        public async Task ProcessFeed()
        {
            CreateSnapshotFile();

            var lastEventId = string.Empty;

            while (true)
            {
                try
                {
                    var events = await client.GetEvents(lastEventId);

                    if (events == null || events.Count == 0)
                    {
                        System.Console.WriteLine($"No events received");

                        await Task.Delay(10000);

                        continue;
                    }

                    foreach (var cloudEvent in events)
                    {
                        ProcessEvent(cloudEvent);

                        lastEventId = cloudEvent.Id;
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                    throw;
                }

                await Task.Delay(10000);
            }
        }

        private static void ProcessEvent(CloudEvent cloudEvent)
        {
            switch (cloudEvent.Method)
            {
                case "PUT":
                    CreateOrUpdateSnapshotItem(cloudEvent);
                    break;
                case "DELETE":
                    DeleteSnapshotItem(cloudEvent);
                    break;
            }
        }

        private static void DeleteSnapshotItem(CloudEvent cloudEvent)
        {
            var items = JsonHelper.Deserialize<List<ToDoItem>>(File.ReadAllText(ToDoSnapshotFileName));

            var item = items.FirstOrDefault(i => i.Key == ((ToDoItem)cloudEvent.Data!).Key);

            if (item is not null)
            {
                items.Remove(item);
            }

            JsonHelper.Serialize(items, ToDoSnapshotFileName);
        }

        private static void CreateOrUpdateSnapshotItem(CloudEvent cloudEvent)
        {
            var items = JsonHelper.Deserialize<List<ToDoItem>>(File.ReadAllText(ToDoSnapshotFileName));

            var eventItem = cloudEvent.Data!;

            var existingItem = items.FirstOrDefault(i => i.Key == eventItem.Key);

            if (cloudEvent.Type == "ToDoItemUpdated" && existingItem != null)
            {
                existingItem.Value = eventItem.Value;
                existingItem.IsDone = eventItem.IsDone;
            }
            else
            {
                items.Add(eventItem);
            }

            JsonHelper.Serialize(items, ToDoSnapshotFileName);
        }

        private static void CreateSnapshotFile()
        {
            File.WriteAllText(ToDoSnapshotFileName, "[]");
        }
    }
}
