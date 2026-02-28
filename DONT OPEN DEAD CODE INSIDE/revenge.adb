with Ada.Text_IO; use Ada.Text_IO;

procedure Revenge is
   Money_Lost : constant Integer := 30;
   Spells_Cast : Integer := 0;
begin
   Put_Line("I LOST ALL MY MONEY TO THE SHADOW WIZARD MONEY GANG");
   Put_Line("Money lost: $" & Integer'Image(Money_Lost));
   Put_Line("Current plan: cast wicked spells until problem resolved");
   Put_Line("Status: REVENGE IN PROGRESS");
end Revenge;
