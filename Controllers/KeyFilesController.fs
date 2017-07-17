namespace OtrWeb.Controllers

open OtrWeb
open OtrWeb.Options
open Newtonsoft.Json.Linq
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options

[<Route("api/keyfiles")>]
type KeyFilesController(otrConfig : IOptions<OtrOptions>, tvApi : TvDbApi) =
    inherit Controller()

    let options = otrConfig.Value

    [<HttpGet>]
    member this.Get() =
        let response = JObject()
        response.Add("tvdburl", JToken.FromObject(tvApi.Options.ApiUrl))

        response