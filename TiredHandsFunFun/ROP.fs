module ROP

// https://fsharpforfunandprofit.com/posts/recipe-part2/ - thanks as always, Uncle Scott. 
type Result<'TSuccess, 'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure

let either successTrack failureTrack twoTrackInput = 
    match twoTrackInput with
    | Success s -> successTrack s
    | Failure f -> failureTrack f  

let fail input =
    Failure input

let success input = 
    Success input 

let bind switchFunction = 
    either fail switchFunction 

let (>>=) switchFunction twoTrackInput = 
    bind switchFunction twoTrackInput