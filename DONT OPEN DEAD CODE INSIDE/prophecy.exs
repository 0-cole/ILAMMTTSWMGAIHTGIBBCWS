defmodule ShadowWizard.Prophecy do
  @moduledoc """
  Ancient prophecy foretold by the elder shadow wizards.
  They should have listened.
  """

  def read_prophecy do
    prophecy = [
      "One shall come who has lost all their money",
      "To the Shadow Wizard Money Gang",
      "And they shall have to get it back",
      "By casting WICKED SPELLS",
      "And there will be breakcore music",
      "And it will go HARD"
    ]

    Enum.each(prophecy, fn line ->
      IO.puts("  ✦ #{line}")
      :timer.sleep(500)
    end)

    IO.puts("\n  The prophecy has been fulfilled.")
  end
end

ShadowWizard.Prophecy.read_prophecy()
