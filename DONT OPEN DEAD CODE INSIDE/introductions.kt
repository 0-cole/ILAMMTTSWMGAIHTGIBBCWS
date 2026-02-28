data class ShadowWizard(
    val name: String,
    val stolenMoney: Int,
    val catchphrase: String,
    var isDefeated: Boolean = false
)

fun main() {
    val moneyGang = listOf(
        ShadowWizard("Glonk", 10, "your money? OUR money."),
        ShadowWizard("Billboard Bob", 8, "I didn't even take your money dude"),
        ShadowWizard("Red Wizard Rick", 7, "IT WAS $30 WHY ARE YOU DOING THIS"),
        ShadowWizard("The King", 5, "YOU KILLED MY FAMILY! I WONT LET YOU LEAVE HERE ALIVE!"),
    )

    println("THE SHADOW WIZARD MONEY GANG INTRODUCES THEMSELVES:")
    moneyGang.forEach { wizard ->
        println("  ${wizard.name}: \"${wizard.catchphrase}\"")
        println("  (holding $${wizard.stolenMoney} of YOUR $30)")
    }
    
    val totalStolen = moneyGang.sumOf { it.stolenMoney }
    println("\nTotal debt: $$totalStolen")
    println("Bodies so far: countless")
    println("Was it worth it: absolutely not")
    println("Will we stop: absolutely not")
}
