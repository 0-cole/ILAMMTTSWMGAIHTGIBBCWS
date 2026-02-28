(defun cast-wicked-spell (spell-name mana-cost)
  "Casts a wicked spell. The shadow wizards hate this one weird trick."
  (if (> mana-cost 100)
      (format t "You don't have enough mana for ~A, skill issue" spell-name)
      (format t "~A was cast! The shadow wizards are SHAKING" spell-name)))

(cast-wicked-spell "CHAIN LIGHTNING" 50)
