namespace OtrWeb

type FileType=
|Episode of file : string * name : string * number : int * show : string * season : int * aired : string
|Movie of file : string * name : string * released : string
|Unknown of file : string 