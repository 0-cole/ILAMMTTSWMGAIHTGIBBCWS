import Foundation

enum WickedSpell: String, CaseIterable {
    case fireball = "FIREBALL"
    case chainLightning = "CHAIN LIGHTNING"  
    case punch = "PUNCH (its not a spell but it works)"
}

struct ShadowWizard {
    let name: String
    var stolenCash: Int
    let isEvil: Bool = true
    
    func taunt() -> String {
        return "\(name) says: your money belongs to us now lmao"
    }
}

let gang = [
    ShadowWizard(name: "Glonk", stolenCash: 420),
    ShadowWizard(name: "Billboard Dave", stolenCash: 69),
]

for wizard in gang {
    print(wizard.taunt())
}
print("Time to cast some WICKED SPELLS")
