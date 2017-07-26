module JsonHandling

open OtrWeb
open Newtonsoft.Json.Linq

let makeJson (fileinfo : FileInfo) = 
    let file, info = (fileinfo.file, fileinfo.info)
    let response = match info with
                   |Episode(name, number, show, candidates, season, aired) ->
                        let episode = JObject()
                        episode.Add("type", JToken.FromObject("episode"))
                        episode.Add("file", JToken.FromObject(file))
                        episode.Add("name", JToken.FromObject(name))
                        episode.Add("number", JToken.FromObject(number))
                        episode.Add("show", JToken.FromObject(show))
                        episode.Add("candidates", JToken.FromObject(candidates))
                        episode.Add("season", JToken.FromObject(season))
                        episode.Add("aired", JToken.FromObject(aired))
                        episode
                   |Movie(name, released, candidates) ->
                        let movie = JObject()
                        movie.Add("type", JToken.FromObject("movie"))
                        movie.Add("file", JToken.FromObject(file))
                        movie.Add("name", JToken.FromObject(name))
                        movie.Add("released", JToken.FromObject(released))
                        movie.Add("candidates", JToken.FromObject(candidates))
                        movie
                   |Unknown ->
                        let unknown = JObject()
                        unknown.Add("type", JToken.FromObject("unknown"))
                        unknown.Add("file", JToken.FromObject(file))
                        unknown
    response