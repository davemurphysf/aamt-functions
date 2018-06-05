#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

class AnalyzeTweetsTable
{
    public enum AnalyzeType
    {
        Picture = 0,
        Text = 1
    }
    public class AnalyzeTweetsStats
    {
        [JsonProperty(PropertyName = "totalText")]
        public int TotalText { get; set; }

        [JsonProperty(PropertyName = "totalPicture")]
        public int TotalPicture { get; set; }

        [JsonProperty(PropertyName = "text")]
        public Dictionary<string, int> Text { get; private set; }

        [JsonProperty(PropertyName = "picture")]
        public Dictionary<string, int> Picture { get; private set; }

        public AnalyzeTweetsStats()
        {
            Text = new Dictionary<string, int>();
            Picture = new Dictionary<string, int>();
        }
    }

    class AnalyzeTweets : TableEntity
    {
        public string Type { get; set; }
        public string Text { get; set; }

        public AnalyzeTweets(string type, string text)
        {
            PartitionKey = "AnalyzeTweets";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Type = type.ToLowerInvariant();
            Text = text;
        }

        public AnalyzeTweets()
        {
        }
    }

    private CloudTable table;

    public AnalyzeTweetsTable(string connectionString)
    {
        var account = CloudStorageAccount.Parse(connectionString);
        var client = account.CreateCloudTableClient();
        table = client.GetTableReference("AnalyzeTweets");
    }

    public void Add(string type, string text)
    {
        if (!type.Equals("text", StringComparison.OrdinalIgnoreCase) && !type.Equals("image", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var newInfo = new AnalyzeTweets(type, text);
        var insert = TableOperation.Insert(newInfo);
        table.Execute(insert);
    }

    public AnalyzeTweetsStats GetStats()
    {
        var query = new TableQuery<AnalyzeTweets>();
        var stats = new AnalyzeTweetsStats();
        foreach (var info in table.ExecuteQuery(query))
        {
            if (info.Type.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                stats.TotalText++;
                if (!stats.Text.ContainsKey(info.Text)) stats.Text[info.Text] = 0;
                stats.Text[info.Text]++;
            }
            else
            {
                stats.TotalPicture++;
                if (!stats.Picture.ContainsKey(info.Text)) stats.Picture[info.Text] = 0;
                stats.Picture[info.Text]++;
            }
        }
        return stats;
    }
}