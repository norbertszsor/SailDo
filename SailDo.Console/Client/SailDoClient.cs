using System.Net.Http.Json;
using SailDo.Api.Transfer;
using System.Text.Json;
using System.Web;
using SailDo.Console.Interfaces;

namespace SailDo.Console.Client
{
    public class SailDoClient(HttpClient client) : ISailDoClient
    {
        public async Task<List<CloudEvent>?> GetEvents(string lastEventId, int? timeOut = null)
        {
            var uriBuilder = new UriBuilder("https://localhost:7077/api/ToDo/Feed");

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (!string.IsNullOrEmpty(lastEventId))
            {
                query["lastEventId"] = lastEventId;
            }

            if (timeOut.HasValue)
            {
                query["timeOut"] = timeOut.Value.ToString();
            }

            uriBuilder.Query = query.ToString();

            var requestUri = uriBuilder.ToString();

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<CloudEvent>>();
        }
    }
}
