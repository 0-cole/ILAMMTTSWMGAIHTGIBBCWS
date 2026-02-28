-- shadow wizards dont want you to know this
module ShadowWizardMoneyGang where

castSpell :: String -> String
castSpell spell = "You cast " ++ spell ++ " but it was not very effective"

main :: IO ()
main = putStrLn (castSpell "Wicked Fireball")
