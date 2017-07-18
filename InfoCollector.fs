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
        Movie(file, parsedName, "somewhen")

    let findEpisode file = 
        let parsedShow = parseShowName file
        let apiShows = (tvApi.FindShow parsedShow) |> List.ofSeq
        let apiShow = match apiShows |> List.tryFind(fun (name, id) -> 
                            (name |> canonizeFileName) = (parsedShow |> canonizeFileName)) with
                      |Some(name, id) -> name
                      |_ -> ""
        let parsedEpisode = parseEpisodeName file
        Episode(file, parsedEpisode, -1, parsedShow, -1, "somewhen")

    member this.GetInfo file = 
        let fileName = Path.GetFileName file
        let info = if isMovie fileName then
                        findMovie fileName
                    else
                        findEpisode fileName
        info