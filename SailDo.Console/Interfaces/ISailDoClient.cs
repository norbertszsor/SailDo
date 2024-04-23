using SailDo.Api.Transfer;

namespace SailDo.Console.Interfaces
{
    public interface ISailDoClient
    {
        Task<List<CloudEvent>?> GetEvents(string lastEventId, int? timeOut = null);
    }
}
