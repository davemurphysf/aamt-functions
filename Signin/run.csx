#load "SignInTable.csx"
#load "FetchTweetsTable.csx"
#load "AnalyzeTweetsTable.csx"
#load "AzureSignalR.csx"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    var signInTable = new SignInTable(Environment.GetEnvironmentVariable("TableConnectionString"));
    signInTable.Add();
    var signInStats = signInTable.GetStats();

    var fetchTable = new FetchTweetsTable(Environment.GetEnvironmentVariable("TableConnectionString"));
    var fetchStats = fetchTable.GetStats();

    var analyzeTable = new AnalyzeTweetsTable(Environment.GetEnvironmentVariable("TableConnectionString"));
    var analyzeStats = analyzeTable.GetStats();

    var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));
    await signalR.SendAsync("signin", "updateSignInStats", signInStats);

    return req.CreateResponse(HttpStatusCode.OK, new
    {
        authInfo = new
        {
            serviceUrl = signalR.GetClientHubUrl("signin"),
            accessToken = signalR.GenerateAccessToken("signin"),
            signInStats = signInStats,
            fetchStats = fetchStats,
            analyzeStats = analyzeStats
        },
    }, "application/json");
}
