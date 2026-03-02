data class ShadowWizard(
    val name: String,
    val stolenMoney: Int,
    val catchphrase: String,
    var isDefeated: Boolean = false
)

fun main() {
    val moneyGang = listOf(
        ShadowWizard("Glonk", 420, "your money? OUR money."),
        ShadowWizard("Billboard Bob", 1337, "you can't hit what you can't reach"),
        ShadowWizard("Red Wizard Rick", 69, "I'm literally on fire"),
        ShadowWizard("The Gold One", 9001, "its over 9000"),
    )

    println("THE SHADOW WIZARD MONEY GANG INTRODUCES THEMSELVES:")
    moneyGang.forEach { wizard ->
        println("  ${wizard.name}: \"${wizard.catchphrase}\"")
        println("  (holding $${wizard.stolenMoney} of YOUR money)")
    }
    
    val totalStolen = moneyGang.sumOf { it.stolenMoney }
    println("\nTotal debt: $$totalStolen")
    println("Payment method: WICKED SPELLS")
}
