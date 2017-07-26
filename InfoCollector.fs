namespace OtrWeb

open FileNameParser
open System.IO
open System.Text.RegularExpressions

type InfoCollector(tvApi : TvDbApi, movieApi : MovieDbApi) =
    let durationPattern = @"(\d)*_TVOON_DE"    
    let beforeDatePattern = @".+?(?=_\d\d.\d\d.\d\d)"
    let datePattern = @"(\d\d.\d\d.\d\d)"

    let episodeWithDate show date = 
        Episode("unknown", -1, show, [||], -1, date)

    let episodeWithName show name =
        Episode(name, -1, show, [||], -1, "unknown")

    let isMovie file = 
        let timeMatch = Regex.Match(file, durationPattern)
        let durationString = timeMatch.Value.Split('_').[0]

        match durationString |> int with
        |duration when duration > 90 -> true
        |_ -> false

    let matchMovie parsed m =
        match m with
        |Movie(name, _, _) -> (name |> canonizeFileName) = (parsed |> canonizeFileName)
        |_ -> false

    let findMovie file =
        let parsedName = parseMovieName file
        let movies = (movieApi.SearchMovie parsedName) |> Async.RunSynchronously

        let found = movies |> Seq.tryFind(fun m -> m |> matchMovie parsedName)

        match found with
        |Some(movie) -> movie
        |_ -> Unknown

    let listAlternatives shows =
        let showNames = shows |> Seq.map(fun (name, id) -> name) |> Array.ofSeq
        Episode("unknown", -1, "unknown", showNames, -1, "unknown")

    let dateCompare parsed name aired  =
        aired = parsed

    let nameCompare parsed name aired =
        (name |> canonizeFileName) = (parsed |> canonizeFileName)

    let matchEpisode compare value e =
        match e with
        |Episode(name, _, _, _, _, aired) -> compare value name aired 
        |_ -> false

    let find compare value = Seq.tryFind(fun e -> e |> matchEpisode compare value)

    let find parse compare file episodes =
        let parsedValue = parse file
        let apiEpisode = episodes |> find compare parsedValue
        (apiEpisode, parsedValue)

    let findByDate = find parseEpisodeDate dateCompare
    let findByName = find parseEpisodeName nameCompare

    let makeEpisode showName maker (episode, value) =
        match episode with
        |None -> maker showName value
        |Some(e) -> e

    let containsEpisodeName (file:string) = file.Contains("__")

    let findEpisodeOfShow file (showName, showId) =
        let showEpisodes = tvApi.GetEpisodes(showId, showName)
        match containsEpisodeName file with
        |true -> findByName file showEpisodes |> makeEpisode showName episodeWithName
        |false -> findByDate file showEpisodes |> makeEpisode showName episodeWithDate

    let findEpisode file = 
        let parsedShow = parseShowName file
        let apiShows = (tvApi.FindShow parsedShow) |> List.ofSeq

        match apiShows with
        |[] -> Unknown
        |[found] -> 
            tvApi.CacheShow parsedShow found
            findEpisodeOfShow file found
        |_ -> listAlternatives apiShows

    member this.GetInfo file = 
        let fileName = Path.GetFileName file
        match isMovie fileName with
        |true -> { file = file; info = findMovie fileName }
        |false -> { file = file; info = findEpisode fileName }