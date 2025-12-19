# PatchThingy

PatchThingy is a mod development tool for Deltarune that was created alongside my [Custom Party Menu](https://gamebanana.com/wips/95949). It uses decompiled code to generate and apply patches, as well as a few features to manage the versions of `data.win` used during development.

## Data Files
Patchthingy makes use of multiple copies of Deltarune's game files, particularly `data.win`. There are three copies of the game data for each chapter, each with a specific purpose for development:
- Active Data (`data.win`)
  - The version of Deltarune that gets loaded into the game.
  - This is usually the modded data, but is also used to update Deltarune via Steam.
- Vanilla Data (`data-vanilla.win`)
  - The *exact version* of Deltarune used to make the mod.
  - This version is used to generate patches so updates to Deltarune don't affect mod development too much.
- Backup Data (`data-backup.win`)
  - A user-created backup of the modded version, created by copying `data.win` to a different file.
  - This is most useful when trying to make the standard `.xdelta` files for the release version of the mod.

## Releases?
There are currently **no plans** to commit to release versions of PatchThingy. As of right now, I am developing my own tool for my own use, meaning I have put *zero* thought into user experience or compatibility with other platforms.

###### I'm not going to pretend I know what I'm doing.

Feel free to compile your own version, but (at least for now,) I'm not going to worry about making "official" releases. PatchThingy was developed on Linux, so there's no guarantee Windows even works.