namespace OtrWeb

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
        Movie(file, "some movie", "somewhen")

    let findEpisode file = 
        Episode(file, "some episode", 3, "some show", 1, "somewhen")

    member this.GetInfo file = 
        let info = if isMovie file then
                        findMovie file
                    else
                        findEpisode file
        info