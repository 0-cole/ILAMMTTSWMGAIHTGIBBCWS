use std::fmt;

#[derive(Debug)]
enum WickedSpell {
    Fireball { damage: f32, radius: f32 },
    ChainLightning { damage: f32, bounces: u32 },
    Punch { damage: f32, can_boost_projectiles: bool },
}

impl fmt::Display for WickedSpell {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        match self {
            WickedSpell::Fireball { damage, radius } =>
                write!(f, "🔥 FIREBALL ({}dmg, {}m radius)", damage, radius),
            WickedSpell::ChainLightning { damage, bounces } =>
                write!(f, "⚡ CHAIN LIGHTNING ({}dmg, {} bounces)", damage, bounces),
            WickedSpell::Punch { damage, can_boost_projectiles } =>
                write!(f, "👊 PUNCH ({}dmg, boost: {})", damage, can_boost_projectiles),
        }
    }
}

fn main() {
    let loadout = vec![
        WickedSpell::Fireball { damage: 50.0, radius: 5.0 },
        WickedSpell::ChainLightning { damage: 30.0, bounces: 5 },
        WickedSpell::Punch { damage: 25.0, can_boost_projectiles: true },
    ];

    println!("=== WICKED SPELL LOADOUT ===");
    for spell in &loadout {
        println!("  {}", spell);
    }
    println!("\nShadow Wizard Money Gang: \"oh no\"");
}
