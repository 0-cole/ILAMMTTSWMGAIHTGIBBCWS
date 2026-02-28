% the shadow wizards use prolog to plan their heists
steal_money(wizard, target) :- 
    is_shadow(wizard),
    has_money(target),
    write('Your money is gone. Sorry.'), nl.

is_shadow(glonk).
is_shadow(billboard).
has_money(player).

:- steal_money(glonk, player).
