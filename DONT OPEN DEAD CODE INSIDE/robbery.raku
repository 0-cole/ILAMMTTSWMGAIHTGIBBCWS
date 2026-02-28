class ShadowWizardMoneyGang {
    has Str $.name;
    has Int $.stolen-amount;
    has Bool $.is-wicked = True;
    
    method rob($target) {
        say "$.name robs $target for $$.stolen-amount gold";
        say "This is why we can't have nice things";
    }
}

my $glonk = ShadowWizardMoneyGang.new(
    name => "Glonk Prime",
    stolen-amount => 1337,
);

$glonk.rob("the player");
say "REVENGE: FIRST COVENANT";
