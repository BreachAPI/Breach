# Breach: Mod Loader For Axiom Verge 1

## About

Breach is a [MonoMod](https://github.com/MonoMod/MonoMod)-based mod loader for Axiom Verge 1.

Currently crude and a work-in-progress. Once bare minimum functionality is reached, this README will be updated with more detailed compilation/installation instructions as well as a planned features roadmap.

## Installation

Compile `AxiomVerge.Breach.mm` and copy the output (all of it) to the game's folder. Compile `Breach.MiniInstaller` and run the binary (e.g. via `dotnet run`), passing along the game's folder path. TODO: Decide whether we want to keep the MonoMod library versions in sync between the two, in which case it would be convenient to bundle everything together and extract to the game's folder.

The MiniInstaller will backup the original game executable to `orig_AxiomVerge.exe`, as well as generate two modified copies of the game: `strip_AxiomVerge.exe` which is stripped of all code and has every type, field and method publicized, as well as `pub_AxiomVerge.exe` which only has internal types publicized. `pub_AxiomVerge.exe` is generated as an intermediate part of the installation process. Breach publicizes internal types since there's so many of them, in order to make modding easier. The final `AxiomVerge.exe` has publicized types, but retains the original accessors on type members.

## Repository Contents

- `AxiomVerge.Breach.mm`: Class library containing the necessary game patches. This is the assembly that gets injected into the game by `MonoMod.Patcher`.
- `Breach.API`: Class library containing the mod loading logic and APIs. It is referenced and invoked by `AxiomVerge.Breach.mm`.
- `Breach.MiniInstaller`: Cross-platform console mini-utility that handles installation logic. It stays up-to-date with the mod loader this way.

## Community

Join the official [Discord server](https://discord.gg/m2BvJDVqXx) of the game for discussion (channel: #av1-modding).

Shoutouts to (in no particular order): Leemyy, Wartori, Sabera, Aria, 0x0ade, the Celeste modding community, the MonoMod community, the FNA and MonoGame communities.
