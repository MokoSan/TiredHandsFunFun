module ROP

// https://fsharpforfunandprofit.com/posts/recipe-part2/ - thanks as always, Uncle Scott. 
type Result<'TSuccess, 'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure