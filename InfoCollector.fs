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
        Movie(parsedName, "somewhen")

    let findEpisode file = 
        let parsedShow = parseShowName file
        let apiShows = (tvApi.FindShow parsedShow) |> List.ofSeq
        let apiShow = match apiShows |> List.tryFind(fun (name, id) -> 
                            (name |> canonizeFileName) = (parsedShow |> canonizeFileName)) with
                      |Some(name, id) -> (name, id)
                      |_ -> ("", -1)
        let parsedEpisode = parseEpisodeName file

        let showEpisodes = tvApi.GetEpisodes(snd apiShow, fst apiShow) |> List.ofSeq
        let apiEpisode = showEpisodes |> Seq.tryFind(fun ep ->
            match ep with
            |Episode(epName, epNumber, epShow, epSeason, epAired) -> (epName |> canonizeFileName) = (parsedEpisode |> canonizeFileName)
            |_ -> false)

        match apiEpisode with
        |None -> Unknown
        |_ -> apiEpisode.Value

    member this.GetInfo file = 
        let fileName = Path.GetFileName file
        let info = if isMovie fileName then
                        findMovie fileName
                    else
                        findEpisode fileName
        { file = file; info = info }