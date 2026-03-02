let shadowWizardGang = [
    ("Glonk", 100, true);
    ("Billboard Bob", 200, true);
    ("The Purple One", 150, true);
]

let totalStolen = 
    shadowWizardGang 
    |> List.map (fun (_, gold, _) -> gold) 
    |> List.fold (+) 0

printfn "Total money stolen by shadow wizard money gang: %d" totalStolen
printfn "Spells required to get it back: %d" (totalStolen / 10)
printfn "Will we get it back? ABSOLUTELY"
