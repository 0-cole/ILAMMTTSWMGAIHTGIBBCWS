-module(spell_server).
-export([start/0, cast/1]).

start() ->
    io:format("Shadow Wizard Spell Server v1.0~n"),
    io:format("WARNING: unauthorized spell casting will be prosecuted~n"),
    loop().

loop() ->
    receive
        {cast, Spell} ->
            io:format("Casting ~s... BOOM~n", [Spell]),
            loop();
        stop ->
            io:format("Server shutdown. The shadow wizards win.~n")
    end.

cast(Spell) ->
    self() ! {cast, Spell}.
