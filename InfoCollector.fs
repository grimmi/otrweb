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

    let findMovie file =
        let parsedName = parseMovieName file
        let movies = (movieApi.SearchMovie parsedName) |> Async.RunSynchronously

        let found = movies |> Seq.tryFind(fun m -> match m with
                                                   |Movie(name, released, _) -> (name |> canonizeFileName) = (parsedName |> canonizeFileName)
                                                   |_ -> false)

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

    let episodeFinder compare value =
        Seq.tryFind(fun e -> match e with
                             |Episode(epName, epNumber, epShow, cand, epSeason, epAired) -> compare epName epAired value
                             |_ -> false)

    let findEpisodeByName file showName showId episodes =
        let parsedEpisode = parseEpisodeName file
        let apiEpisode = episodes |> episodeFinder nameCompare parsedEpisode

        match apiEpisode with
        |None -> episodeWithName showName parsedEpisode
        |_ -> apiEpisode.Value

    let findEpisodeByDate (file:string) showName showId episodes =
        let episodeDate = parseEpisodeDate file
        let apiEpisode = episodes |> episodeFinder dateCompare episodeDate

        match apiEpisode with
        |None -> episodeWithDate showName episodeDate
        |Some(episode) -> episode

    let findEpisodeOfShow (file:string) (showName, showId) =
        let showEpisodes = tvApi.GetEpisodes(showId, showName)
        match file.Contains("__") with
        |true -> findEpisodeByName file showName showId showEpisodes
        |false -> findEpisodeByDate file showName showId showEpisodes                      

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
        let info = if isMovie fileName then
                        findMovie fileName
                    else
                        findEpisode fileName
        { file = file; info = info }