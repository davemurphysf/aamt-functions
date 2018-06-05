#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

class FetchTweetsTable
{
    public class FetchTweetsStats
    {
        [JsonProperty(PropertyName = "totalNumber")]
        public int TotalNumber { get; set; }

        [JsonProperty(PropertyName = "username")]
        public Dictionary<string, int> Username { get; private set; }

        public FetchTweetsStats()
        {
            Username = new Dictionary<string, int>();
        }
    }

    class FetchTweets : TableEntity
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public int Count { get; set; }

        public FetchTweets(string username, int count)
        {
            PartitionKey = "FetchTweets";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Username = username;
            Count = count;
        }

        public FetchTweets()
        {
        }
    }

    private CloudTable table;

    public FetchTweetsTable(string connectionString)
    {
        var account = CloudStorageAccount.Parse(connectionString);
        var client = account.CreateCloudTableClient();
        table = client.GetTableReference("FetchTweets");
    }

    public void Add(string username, int count)
    {
        var newInfo = new FetchTweets(username, count);
        var insert = TableOperation.Insert(newInfo);
        table.Execute(insert);
    }

    public FetchTweetsStats GetStats()
    {
        var query = new TableQuery<FetchTweets>();
        var stats = new FetchTweetsStats();
        foreach (var info in table.ExecuteQuery(query))
        {
            stats.TotalNumber++;
            if (!String.IsNullOrWhiteSpace(info.Username))
            {
                if (!stats.Username.ContainsKey(info.Username)) stats.Username[info.Username] = 0;
                stats.Username[info.Username] += info.Count;
            }
        }
        return stats;
    }
}