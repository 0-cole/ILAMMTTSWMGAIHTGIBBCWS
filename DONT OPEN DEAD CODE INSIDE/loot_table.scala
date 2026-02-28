import scala.util.Random

object ShadowWizardLootTable {
  case class LootDrop(name: String, rarity: String, value: Int)
  
  val lootTable: List[LootDrop] = List(
    LootDrop("Stolen Gold Coin", "Common", 1),
    LootDrop("Shadow Wizard Hat", "Rare", 50),
    LootDrop("Wicked Spell Scroll", "Epic", 200),
    LootDrop("The Money Gang's Ledger", "Legendary", 1000),
    LootDrop("Your Dignity", "Mythic", 0),
  )
  
  def rollLoot(): LootDrop = {
    val roll = Random.nextInt(100)
    if (roll < 60) lootTable(0)
    else if (roll < 85) lootTable(1)
    else if (roll < 95) lootTable(2)
    else if (roll < 99) lootTable(3)
    else lootTable(4) // 1% chance to get your dignity back
  }
  
  def main(args: Array[String]): Unit = {
    println("Defeating shadow wizard... rolling loot:")
    val drop = rollLoot()
    println(s"  Got: ${drop.name} (${drop.rarity}) - worth ${drop.value}g")
  }
}
