namespace SailDo.Api.Transfer;

public class ToDoItem
{
    public required string Key { get; set; }

    public required string Value { get; set; }

    public bool IsDone { get; set; }
}