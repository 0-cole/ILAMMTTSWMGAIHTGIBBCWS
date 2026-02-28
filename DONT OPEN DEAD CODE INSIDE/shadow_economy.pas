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
  Glonk.StolenMoney := 30;
  Glonk.IsWicked := True;
  WriteLn('Shadow Wizard Money Gang stole $', Glonk.StolenMoney);
  WriteLn('We committed genocide over it. No regrets.');
end.
