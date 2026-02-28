package main

import "fmt"

type ShadowWizard struct {
	Name       string
	HP         int
	MoneyStolen int
	IsAlive    bool
}

func (w *ShadowWizard) GetHit(damage int) string {
	w.HP -= damage
	if w.HP <= 0 {
		w.IsAlive = false
		return fmt.Sprintf("%s has been defeated! Recovered $%d", w.Name, w.MoneyStolen)
	}
	return fmt.Sprintf("%s takes %d damage. HP: %d", w.Name, damage, w.HP)
}

func main() {
	gang := []ShadowWizard{
		{"Glonk", 100, 5000, true},
		{"Billboard Barry", 200, 3000, true},
		{"The Lime Wizard", 150, 7000, true},
	}

	fmt.Println("=== WICKED SPELL COMBAT LOG ===")
	for i := range gang {
		fmt.Println(gang[i].GetHit(9999)) // wicked spells are OP
	}
}
