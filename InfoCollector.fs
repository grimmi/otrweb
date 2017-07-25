namespace OtrWeb

open FileNameParser
open System.IO
open System.Text.RegularExpressions

type InfoCollector(tvApi : TvDbApi, movieApi : MovieDbApi) =
    let durationPattern = @"(\d)*_TVOON_DE"    
    let beforeDatePattern = @".+?(?=_\d\d.\d\d.\d\d)"
    let datePattern = @"(\d\d.\d\d.\d\d)"

    let isMovie file = 
        let timeMatch = Regex.Match(file, durationPattern)
        let durationString = timeMatch.Value.Split('_').[0]

        match durationString |> int with
        |duration when duration > 90 -> true
        |_ -> false

    let findMovie file =
        let parsedName = parseMovieName file
        let movies = (movieApi.SearchMovie parsedName) |> Async.RunSynchronously

        let found = movies |> Seq.tryFind(fun m -> match m with
                                                   |Movie(name, released) -> (name |> canonizeFileName) = (parsedName |> canonizeFileName)
                                                   |_ -> false)

        match found with
        |Some(movie) -> movie
        |_ -> Unknown

    let findEpisode file = 
        let parsedShow = parseShowName file
        let apiShows = (tvApi.FindShow parsedShow) |> List.ofSeq
        let (apiShowName, apiShowId) = if (apiShows |> Seq.length = 1) then
                                            apiShows |> Seq.head
                                       else
                                           match apiShows |> List.tryFind(fun (name, id) -> 
                                                (name |> canonizeFileName) = (parsedShow |> canonizeFileName)) with
                                           |Some(name, id) -> 
                                             tvApi.CacheShow parsedShow (name, id)
                                             (name, id)
                                           |_ -> (parsedShow, -1)

        if apiShowId = -1 then
            Unknown
        else
            let showEpisodes = tvApi.GetEpisodes(apiShowId, apiShowName) |> List.ofSeq
            let episode = if file.Contains("__") then
                                let parsedEpisode = parseEpisodeName file
                                let apiEpisode = showEpisodes |> Seq.tryFind(fun ep ->
                                    match ep with
                                    |Episode(epName, epNumber, epShow, epSeason, epAired) -> (epName |> canonizeFileName) = (parsedEpisode |> canonizeFileName)
                                    |_ -> false)

                                match apiEpisode with
                                |None -> Episode(parsedEpisode, -1, apiShowName, -1, "unknown")
                                |_ -> apiEpisode.Value
                          else
                                let episodeDate = parseEpisodeDate file
                                let apiEpisode = showEpisodes |> Seq.tryFind(fun ep ->
                                    match ep with
                                    |Episode(epName, epNumber, epShow, epSeason, epAired) -> epAired = episodeDate
                                    |_ -> false)

                                match apiEpisode with
                                |None -> Episode("unknown", -1, apiShowName, -1, episodeDate)
                                |_ -> apiEpisode.Value
            episode

    member this.GetInfo file = 
        let fileName = Path.GetFileName file
        let info = if isMovie fileName then
                        findMovie fileName
                    else
                        findEpisode fileName
        { file = file; info = info }