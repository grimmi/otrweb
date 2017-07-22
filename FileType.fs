namespace OtrWeb

type FileType=
|Episode of name : string * 
            number : int * 
            show : string * 
            season : int * 
            aired : string
|Movie of name : string * 
          released : string
|Unknown 