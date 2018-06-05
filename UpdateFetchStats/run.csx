#load "FetchTweetsTable.csx"
#load "AzureSignalR.csx"

#r "Newtonsoft.Json"

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FetchTweetsQueueMessage
{
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("count")]
    public int Count { get; set; }
}

public static async Task Run(string myQueueItem, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");

    var table = new FetchTweetsTable(Environment.GetEnvironmentVariable("TableConnectionString"));
    var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

    if (!String.IsNullOrWhiteSpace(myQueueItem))
    {
        var msg = JsonConvert.DeserializeObject<FetchTweetsQueueMessage>(myQueueItem);
        table.Add(msg.Username, msg.Count);
    }

    var stats = table.GetStats();
    await signalR.SendAsync("signin", "updateFetchTweetsStats", stats.TotalNumber, stats.Username);
}
