// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.IO

open FSharp.Data

open Chiron
open Chiron.Operators

open Twilio

(*
    getBeerNamesFromTiredHands
    |> createBeerInfoFromBeerNames 
    |> checkIfBeerNamesChanged [ Parse JSON file -> Create old beer info -> check if things have changed ]
    |> trySendAlert
*)

// https://stackoverflow.com/questions/29114973/can-i-use-system-timers-timer-in-an-f-pcl-library
type AsyncTimer(interval, callback) = 
    let mb = new MailboxProcessor<bool>(fun inbox ->
            async { 
                let stop = ref false
                while not !stop do
                    // Sleep for our interval time
                    do! Async.Sleep interval

                    // Timers raise on threadpool threads - mimic that behavior here
                    do! Async.SwitchToThreadPool()
                    callback()

                    // Check for our stop message
                    let! msg = inbox.TryReceive(1)
                    stop := defaultArg msg false
            })

    member __.Start() = mb.Start()
    member __.Stop() = mb.Post true

[<Literal>]
let tiredHandsUrl = "http://www.tiredhands.com/fermentaria/beers/"

[<Literal>]
let classNameOfBeers = "menu-item-title"

[<Literal>]
let beerInfoPath = "BeerInfo.json" 

let emptySet = Set.empty

type BeerInfo = { TimeOfLookup : DateTime; Beers : string list } with
    static member ToJson( beerInfo : BeerInfo ) =
        Json.write "TimeOfLookup" beerInfo.TimeOfLookup
        *> Json.write "Beers" beerInfo.Beers

    static member FromJson( _ : BeerInfo ) =
        fun timeOfLookup beers -> { TimeOfLookup = timeOfLookup; Beers = beers }
        <!> Json.read "TimeOfLookup" 
        <*> Json.read "Beers"

    static member GetBeerDifferences( beerInfo1 : BeerInfo ) ( beerInfo2 : BeerInfo ) =
        let beerSet1 = Set< string > ( beerInfo1.Beers ) 
        let beerSet2 = Set< string > ( beerInfo2.Beers ) 
        beerSet1 - beerSet2 

let tiredHandsParser = HtmlDocument.Load( tiredHandsUrl ) 

let cleanupDataFromTheWebsite ( input : string ) = 
    input.Trim().Replace(":", " ")

let beerFindingPredicate ( a : string, innerText : string ) : bool =
    a = classNameOfBeers && not ( innerText.Contains("*")) && not ( innerText.Contains( "Military" ))
    
let getBeerNamesFromTiredHands() : string list =
    tiredHandsParser.Descendants ["div"]
    |> Seq.choose( fun x -> 
        x.TryGetAttribute("class")
        |> Option.map(fun a -> a.Value(), x.InnerText() ))
    |>  Seq.filter(fun ( a,  innerText ) -> beerFindingPredicate ( a, innerText ) ) 
    |> Seq.map(fun ( a, innerText ) -> cleanupDataFromTheWebsite innerText ) 
    |> Seq.toList

let createBeerInfoFromBeerNames ( beers : string list ) : BeerInfo = 
    { TimeOfLookup = DateTime.Now; Beers = beers } 

let serializeBeerInfo ( beerInfo : BeerInfo ) : string = 
    beerInfo 
    |> Json.serialize
    |> Json.formatWith JsonFormattingOptions.Pretty
    
let deserializeBeerInfo ( jsonizedBeerInfo : string ): BeerInfo = 
    jsonizedBeerInfo 
    |> Json.parse
    |> Json.deserialize

let writeToFile ( jsonizedBeerInfo : string ) : unit =
    File.WriteAllText( beerInfoPath, jsonizedBeerInfo )

let readFromBeerInfoFileAndReturn() : BeerInfo option =
    if File.Exists ( beerInfoPath ) then
        File.ReadAllText( beerInfoPath )
        |> deserializeBeerInfo 
        |> Some
    else
        None

let getLatestBeerNames = 
    getBeerNamesFromTiredHands >> createBeerInfoFromBeerNames

let beerNamesChangedHandler( difference : Set<string> ) : unit =
    printfn "Difference is: %A" difference

let beerNamesNotChangedHandler() : unit =
    printfn "Nothing changed!" 

let getResults() : unit =
    let newBeerInfo = getLatestBeerNames()
    let oldBeerInfo = readFromBeerInfoFileAndReturn().Value 
    let difference  = BeerInfo.GetBeerDifferences newBeerInfo oldBeerInfo 

    match difference with
    | emptySet ->  beerNamesNotChangedHandler() 
    | _        ->  beerNamesChangedHandler( difference )

[<EntryPoint>]
let main argv = 
    (*
    printTiredHandsBeers
    let d = serializeBeerInfo { TimeOfLookup = DateTime.Now; Beers = [] }
    writeToFile ( d )
    printfn "%A" ( d )

    let p = readFromBeerInfoFileAndReturn() 
    printfn "%A" p
    *)

    let asyncTimer = AsyncTimer(5000, getResults)

    printfn "Starting the Timer."
    asyncTimer.Start()
    System.Console.ReadKey() |> ignore

    asyncTimer.Stop()
    printfn "Stopped the Timer."
    System.Console.ReadKey() |> ignore

    0 
