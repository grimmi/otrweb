namespace OtrWeb

open OtrWeb.Options
open Newtonsoft.Json
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
    let cache = ShowCache()

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

    member this.CacheShow parsed real =
        cache.SaveMapping parsed real

    member this.FindShow showName =
        let cachedShow = cache.GetMappingFor showName
        match cachedShow with
        |Some(parsed, real, id) -> [(real, id)] |> Seq.ofList
        |_ -> let query = sprintf "/search/series?name=%s" (Uri.EscapeDataString(showName))
              let response = (this.Get query) |> Async.RunSynchronously
              response.["data"]
              |> Seq.map(fun d -> (d.Value<string>("seriesName"), d.Value<int>("id")))


    member private this.LoadEpisodesFromApi(showId, showName) = async {

        let getEpisodePage showId page : Async<JObject>= async{
            let! response = this.Get(sprintf "/series/%d/episodes?page=%d" showId page)
            return response
        }

        let! firstPage = getEpisodePage showId 1
        let lastPageNo = if isNull firstPage.["links"] then 
                            0
                         else
                            firstPage.["links"].Value<int>("last")

        let pages = if lastPageNo > 1 then
                        [ 2 .. lastPageNo ]
                        |> Seq.map(fun p -> async { let! pageResult = getEpisodePage showId p
                                                    return pageResult } |> Async.RunSynchronously)
                        |> List.ofSeq
                        |> List.append [firstPage]  
                    else if lastPageNo = 0 then
                        []
                    else
                        [firstPage]

        return pages
        |> Seq.collect(fun p -> p.["data"] 
                                |> Seq.choose(fun e -> 
                                    try
                                        let name = e.Value<string>("episodeName")
                                        let number = e.Value<int>("airedEpisodeNumber")
                                        let season = e.Value<int>("airedSeason")
                                        let aired = e.Value<string>("firstAired")
                                        Some(Episode(name, number, showName, [||], season, aired))
                                    with
                                        | :? Exception as ex -> None))
                                                            
        |> Seq.sortBy(fun ep -> match ep with
                                |Episode(_,num,_,_,season,_) -> (num, season)
                                |_ -> (0,0))
    }

    member this.GetEpisodes (showId:int, showName:string) = 
        let cachedEpisodes = cache.GetEpisodes showId
        let episodes = match cachedEpisodes with
                       |[] -> let apiEpisodes = (async {
                                        let! apiEpisodes = this.LoadEpisodesFromApi(showId, showName)            
                                        return apiEpisodes
                                    } |> Async.RunSynchronously)
                              cache.SaveEpisodes (showId, showName) apiEpisodes
                              apiEpisodes
                       |_ -> cachedEpisodes |> Seq.ofList

        episodes