module JsonHandling

open OtrWeb
open Newtonsoft.Json.Linq

let makeJson info = 
    let response = match info with
                   |Episode(file, name, number, show, season, aired) ->
                        let episode = JObject()
                        episode.Add("type", JToken.FromObject("episode"))
                        episode.Add("file", JToken.FromObject(file))
                        episode.Add("name", JToken.FromObject(name))
                        episode.Add("number", JToken.FromObject(number))
                        episode.Add("show", JToken.FromObject(show))
                        episode.Add("season", JToken.FromObject(season))
                        episode.Add("aired", JToken.FromObject(aired))
                        episode
                   |Movie(file, name, released) ->
                        let movie = JObject()
                        movie.Add("type", JToken.FromObject("movie"))
                        movie.Add("file", JToken.FromObject(file))
                        movie.Add("name", JToken.FromObject(name))
                        movie.Add("released", JToken.FromObject(released))
                        movie
                   |Unknown(file) ->
                        let unknown = JObject()
                        unknown.Add("type", JToken.FromObject("unknown"))
                        unknown.Add("file", JToken.FromObject(file))
                        unknown
    response