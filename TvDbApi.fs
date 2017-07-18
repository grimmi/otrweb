namespace OtrWeb

open OtrWeb.Options
open Newtonsoft.Json.Linq
open Microsoft.Extensions.Options
open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text

type TvDbApi(options: IOptions<TvDbOptions>) =

    let mutable apiClient : HttpClient = null
    let mutable loggedIn : bool = false

    member this.Options = options.Value

    member private this.Login =
        let token = async{
            use loginClient = new HttpClient()
            let body = this.GetCredentialsBody
            let request = new HttpRequestMessage(HttpMethod.Post, (this.Options.ApiUrl + "/login"))
            request.Content <- new StringContent(body, Encoding.UTF8, "application/json")
            let! response = (loginClient.SendAsync(request) |> Async.AwaitTask)
            let! responseString = (response.Content.ReadAsStringAsync() |> Async.AwaitTask)
            let jsonToken = JObject.Parse(responseString)
            return jsonToken.["token"] |> string } |> Async.RunSynchronously
        apiClient <- new HttpClient()
        apiClient.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)
        apiClient.DefaultRequestHeaders.Add("Accept-Language", "en") |> ignore
        loggedIn <- true

    member private this.GetCredentialsBody =
        let body = JObject()
        body.Add("apikey", JToken.FromObject(this.Options.ApiKey))
        body.Add("username", JToken.FromObject(this.Options.UserName))
        body.Add("userkey", JToken.FromObject(this.Options.UserKey))
        body.ToString()

    member private this.Get url =
        let rec get retry = async{
            if not loggedIn then
                this.Login
            let request = new HttpRequestMessage(HttpMethod.Get, Uri(this.Options.ApiUrl + url))
            let! response = (apiClient.SendAsync(request) |> Async.AwaitTask)
            match (response.StatusCode, retry) with
            |(HttpStatusCode.Unauthorized, true) -> this.Login
                                                    let! result = get false
                                                    return result
            |(HttpStatusCode.OK, _) -> let! responseString = (response.Content.ReadAsStringAsync() |> Async.AwaitTask)
                                       return JObject.Parse(responseString)
            |_ -> let errResponse = JObject()
                  errResponse.Add("code", JToken.FromObject(response.StatusCode))
                  errResponse.Add("data", JToken.FromObject([||]))
                  return errResponse
        }

        get true

    member this.FindShow showName =
        let query = sprintf "/search/series?name=%s" (Uri.EscapeDataString(showName))
        let response = (this.Get query) |> Async.RunSynchronously
        response.["data"]
        |> Seq.map(fun d -> (d.Value<string>("seriesName"), d.Value<int>("id")))
