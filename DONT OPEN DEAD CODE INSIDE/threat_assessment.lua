#!/usr/bin/env lua
-- Shadow Wizard Money Gang Threat Assessment

local threats = {
    { name = "Glonk", danger = 3, weakness = "fireball to the face" },
    { name = "Billboard Shooter", danger = 7, weakness = "punch their projectiles back" },
    { name = "Purple Wizard", danger = 5, weakness = "chain lightning go brrr" },
    { name = "The Money King", danger = 10, weakness = "unknown (good luck)" },
}

print("=== SHADOW WIZARD THREAT ASSESSMENT ===")
print("")
for _, t in ipairs(threats) do
    local status = t.danger >= 7 and "EXTREME DANGER" or "manageable (probably)"
    print(string.format("%-20s | Danger: %d/10 | Status: %s", t.name, t.danger, status))
    print(string.format("%-20s | Weakness: %s", "", t.weakness))
    print("")
end
print("CONCLUSION: cast wicked spells and pray")
