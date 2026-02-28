let shadowWizardGang = [
    ("Glonk", 10, true);
    ("Billboard Bob", 12, true);
    ("The Purple One", 8, true);
]

let totalStolen = 
    shadowWizardGang 
    |> List.map (fun (_, gold, _) -> gold) 
    |> List.fold (+) 0

printfn "Total money stolen by shadow wizard money gang: $%d" totalStolen
printfn "Spells required to get it back: ALL OF THEM"
printfn "Is this a proportionate response? NO"
printfn "Do we care? ALSO NO"
