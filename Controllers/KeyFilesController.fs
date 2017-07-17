namespace OtrWeb.Controllers

open OtrWeb
open Newtonsoft.Json.Linq
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options

[<Route("api/keyfiles")>]
type KeyFilesController(otrConfig : IOptions<OtrOptions>) =
    inherit Controller()

    let options = otrConfig.Value

    [<HttpGet>]
    member this.Get() =
        ""