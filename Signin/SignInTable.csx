#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

class SignInTable
{
    public class SignInStats
    {
        [JsonProperty(PropertyName = "totalNumber")]
        public int TotalNumber { get; set; }

        public SignInStats() { }
    }

    class SignInInfo : TableEntity
    {
        public DateTime Timestamp { get; set; }

        public SignInInfo(DateTime timestamp)
        {
            PartitionKey = "SignIn";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = timestamp;
        }

        public SignInInfo()
        {
        }
    }

    private CloudTable table;

    public SignInTable(string connectionString)
    {
        var account = CloudStorageAccount.Parse(connectionString);
        var client = account.CreateCloudTableClient();
        table = client.GetTableReference("SignInInfo");
    }

    public void Add()
    {
        var newInfo = new SignInInfo(DateTime.Now);
        var insert = TableOperation.Insert(newInfo);
        table.Execute(insert);
    }

    public SignInStats GetStats()
    {
        var query = new TableQuery<SignInInfo>();
        var stats = new SignInStats();
        stats.TotalNumber = table.ExecuteQuery(query).Count();

        return stats;
    }
}