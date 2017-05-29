module TiredHandsParserManager

    open FSharp.Data

    open BeerInfo
    open RepositoryUtilities
    open Twilio

    [<Literal>]
    let tiredHandsUrl = "http://www.tiredhands.com/fermentaria/beers/"

    [<Literal>]
    let classNameOfBeers = "menu-item-title"

    [<Literal>]
    let tiredHandsJsonFileName = "TiredHands.json"

    let emptySet = Set.empty 

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

    let beerNamesChangedHandler( difference : Set<string> ) ( newBeerInfo : BeerInfo ) : unit =
        printfn "Difference is: %A" difference

        // Write new information to the JSON file.
        serializeBeerInfo ( newBeerInfo )
        |> writeToFile tiredHandsJsonFileName 

    let beerNamesNotChangedHandler() : unit =
        printfn "Nothing changed!" 

    let getLatestBeerInfo = 
        getBeerNamesFromTiredHands >> createBeerInfoFromBeerNames

    let getBeerInfoFromTiredHands() =
        let newBeerInfo = getLatestBeerInfo()
        let oldBeerInfo = readFromBeerInfoFileAndReturn( tiredHandsJsonFileName )

        if oldBeerInfo.IsNone then 
            writeToFile ( serializeBeerInfo newBeerInfo ) tiredHandsJsonFileName
            ()

        else
            let difference  = BeerInfo.GetBeerDifferences newBeerInfo oldBeerInfo.Value
            if difference = emptySet then 
                beerNamesNotChangedHandler() 
            else
                beerNamesChangedHandler difference newBeerInfo