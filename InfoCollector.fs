namespace OtrWeb

type InfoCollector(tvApi : TvDbApi, movieApi : MovieDbApi) =

    member this.GetInfo file = 
        file