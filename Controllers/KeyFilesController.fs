namespace OtrWeb.Controllers

open JsonHandling
open OtrWeb
open OtrWeb.Options
open Newtonsoft.Json.Linq
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options
open System.IO

[<Route("api/keyfiles")>]
type KeyFilesController(otrConfig : IOptions<OtrOptions>, collector : InfoCollector) =
    inherit Controller()

    let options = otrConfig.Value

    [<HttpGet>]
    member this.Get() =
        let infos = Directory.GetFiles(options.KeyFilePath, "*.otrkey")
                    |> Seq.map (collector.GetInfo >> makeJson)

        let response = JObject()
        response.Add("files", JToken.FromObject(infos))      
        response  