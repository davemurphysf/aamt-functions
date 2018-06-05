#load "AnalyzeTweetsTable.csx"
#load "AzureSignalR.csx"

#r "Newtonsoft.Json"

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AnalyzeTweetsQueueMessage
{
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("text")]
    public string Text { get; set; }
}

public static async Task Run(string myQueueItem, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");

    var table = new AnalyzeTweetsTable(Environment.GetEnvironmentVariable("TableConnectionString"));
    var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

    if (!String.IsNullOrWhiteSpace(myQueueItem))
    {
        var msg = JsonConvert.DeserializeObject<AnalyzeTweetsQueueMessage>(myQueueItem);
        table.Add(msg.Type, msg.Text);
    }

    var stats = table.GetStats();
    await signalR.SendAsync("signin", "updateAnalyzeTweetsStats", stats.TotalText, stats.TotalPicture, stats.Text, stats.Picture);
}
