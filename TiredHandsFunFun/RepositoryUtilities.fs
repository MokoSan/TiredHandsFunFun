module RepositoryUtilities 

    open System.IO
    open BeerInfo 

    let writeToFile ( jsonizedBeerInfo : string ) ( beerInfoPath : string ) : unit =
        File.WriteAllText( beerInfoPath, jsonizedBeerInfo )

    let readFromBeerInfoFileAndReturn( beerInfoPath ) : BeerInfo option =
        if File.Exists ( beerInfoPath ) then
            File.ReadAllText( beerInfoPath )
            |> deserializeBeerInfo 
            |> Some
        else
            None