program ShadowWizardEconomy;

{$mode objfpc}

type
  TWizard = record
    Name: string;
    StolenMoney: Integer;
    IsWicked: Boolean;
  end;

var
  Glonk: TWizard;

begin
  Glonk.Name := 'Glonk the Unforgivable';
  Glonk.StolenMoney := 999999;
  Glonk.IsWicked := True;
  WriteLn('Shadow Wizard Money Gang stole $', Glonk.StolenMoney);
  WriteLn('Time to get it back with WICKED SPELLS');
end.
