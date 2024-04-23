namespace SailDo.Api.Transfer;

public class CloudEvent
{
    public string SpecVersion { get; private set; } = "1.0";

    public required string Id { get; set; }

    public string? Subject { get;  set; }

    public required string Type { get; set; }

    public required string Source { get; set; }

    public required string Time { get; set; }

    public required string Method { get; set; }

    public ToDoItem? Data { get; set; }
}