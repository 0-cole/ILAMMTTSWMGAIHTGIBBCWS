defmodule ShadowWizard.Prophecy do
  @moduledoc """
  Ancient prophecy foretold by the elder shadow wizards.
  They should have listened.
  """

  def read_prophecy do
    prophecy = [
      "One shall come who has lost $30",
      "To a single thug of the Shadow Wizard Money Gang",
      "And they shall make it their life's mission",
      "To wipe out the ENTIRE clan",
      "By casting WICKED SPELLS",
      "The final boss is literally a grieving father",
      "And there will be breakcore music",
      "And it will go HARD",
      "...you are the villain of this story"
    ]

    Enum.each(prophecy, fn line ->
      IO.puts("  ✦ #{line}")
      :timer.sleep(500)
    end)

    IO.puts("\n  The prophecy has been fulfilled.")
  end
end

ShadowWizard.Prophecy.read_prophecy()
