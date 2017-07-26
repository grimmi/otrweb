module FileNameParser

open System
open System.Text.RegularExpressions

let beforeDatePattern = @".+?(?=_\d\d.\d\d.\d\d)"
let datePattern = @"(\d\d.\d\d.\d\d)"
    
let canonizeFileName (name:string) =
    name.ToLower()
    |> String.filter(fun c -> Char.IsLetter c || Char.IsDigit c)

let parseEpisodeName (file:string) =
        let fileParts = file.Split([|"__"|], StringSplitOptions.RemoveEmptyEntries)
        let episodePart = String.Concat(fileParts |> Array.skip 1)
        let episodeNameMatch = Regex.Match(episodePart, beforeDatePattern)
        match episodeNameMatch.Value with
        |"" -> episodePart
        |_ -> episodeNameMatch.Value

let parseEpisodeDate (file:string) = 
    match Regex.Match(file, datePattern).Value.Split('.') with
    |[|yy;mm;dd|] -> sprintf "20%s-%s-%s" yy mm dd
    |_ -> ""

let parseShowName file =
    let nameMatch = Regex.Match(file, beforeDatePattern)
    nameMatch.Value.Split([|"__"|], StringSplitOptions.RemoveEmptyEntries)
    |> Seq.head
    |> String.map(fun c -> match c with
                           |'_' -> ' '
                           |_ -> c)

let parseMovieName file = 
    let beforeDate = Regex.Match(file, beforeDatePattern).Value
    String.map(fun c -> match c with
                        |'_' -> ' '
                        |_ -> c) <| beforeDate                                       