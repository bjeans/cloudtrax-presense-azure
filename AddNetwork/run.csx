﻿#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

#load "..\Shared\SharedClasses.csx"
#load "..\Shared\Settings.csx"

#load "..\Shared\ApplicationInsights.csx"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;


private static readonly string FunctionName = "CloudTrax-AddNetwork";

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable outputTable, TraceWriter log)
{
    var telemetryClient = ApplicationInsights.CreateTelemetryClient();
    telemetryClient.TrackStatus(FunctionName, "" , "Function triggered by http request");

    var json = await req.Content.ReadAsStringAsync(); // get the Content into a string
    
    NetworkSecret ns = JsonConvert.DeserializeObject<NetworkSecret>(json); // parse the content as JSON

    if (ns.network_id == null || ns.hashKey ==null)
    {
        telemetryClient.TrackStatus(FunctionName, "" , "Unable to create new network",false);
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a network name and shared key in the request body");
    } else {

        ns.PartitionKey = ns.network_id;
        ns.RowKey = ns.network_id;
        //fire and forget
        TableOperation operation = TableOperation.InsertOrReplace(ns);
        TableResult result = outputTable.Execute(operation);
        //TODO: Need to check the result

        telemetryClient.TrackStatus(FunctionName, "" , $"New network ({ns.network_id}) created",true);   
        return req.CreateResponse(HttpStatusCode.Created);
    }

    
    
}

