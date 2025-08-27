// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using ImageMagick.Drawing;


// Load the script configs from the .json file next to the .csproj file
Config.current = JsonSerializer.Deserialize<Config>(File.ReadAllText("./PatchThingy.json"))!;

// if it failed to load for whatever reason, panic
if (Config.current is null)
{
    Console.WriteLine("ERROR: Failed to load script config.");
    Environment.Exit(2);
    return; // for compiler
}

// Get the filepath for all 3 versions of data.win
// Each copy serves a different purpose, making the
// process of updating much easier.

// Active Data: the data.win that Deltarune loads, and that Steam would replace.
string activePath = Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data.win");

// Vanilla Data: the version of data.win that the patches were based on
string vanillaPath = Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data-vanilla.win");

// Backup Data: a second copy of the patched data.win, in case of an update.
string backupPath = Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data-backup.win");

ScriptMode? chosenMode = null;

// force scriptmode for test purposes
// chosenMode = ScriptMode.DecorTest;

// setup for the initial menu
ConsoleMenu menu = new ConsoleMenu(50, 6, 8);

// title bar
menu.lines[0].SetText("╾─╴╴╴  Deltarune Patch Script  ╶╶╶─╼", true);
menu.lines[1].SetType(LineType.Separator);

// mode list
menu.lines[2].SetText("     Generate new patches");
menu.lines[3].SetText("     Apply existing patches");
menu.lines[4].SetText("     Restore vanilla data");

// input location
menu.lines[5].SetType(LineType.Separator);
menu.DrawAllLines();

while (chosenMode is null)
{
    menu.lines[6].SetText(" Please select a mode from the list above.", true);
    menu.DrawLine(6);

    switch (menu.PromptUserInput([2, 3, 4]))
    {
        case 0:

            if (menu.ConfirmUserInput(6))
            {
                chosenMode = ScriptMode.Generate;
            }
            else
            {
                menu.lines[2].SetColor(ConsoleColor.White);
                menu.DrawLine(2);
            }

            break;

        case 1:

            if (menu.ConfirmUserInput(6))
            {
                chosenMode = ScriptMode.Apply;
            }
            else
            {
                menu.lines[3].SetColor(ConsoleColor.White);
                menu.DrawLine(3);
            }
            
            break;

        case 2:

            if (menu.ConfirmUserInput(6))
            {
                chosenMode = ScriptMode.Revert;
            }
            else
            {
                menu.lines[4].SetColor(ConsoleColor.White);
                menu.DrawLine(4);
            }
            
            break;
        default:
            menu.lines[6].SetText(" * i see how it is...");
            menu.DrawLine(6);
            ExitMenu();
            break; // for compiler
    }
}

// set up the menu layout for any popups
// or errors that I need
menu.ClearAll();
menu.lines[1].SetColor(ConsoleColor.DarkGray);
menu.lines[2].SetType(LineType.Separator);
menu.lines[3].SetText("! ERROR !", true); // default to error, successes
menu.lines[3].SetColor(ConsoleColor.Red); // will set their header
menu.lines[5].SetType(LineType.Separator);
menu.lines[6].SetColor(ConsoleColor.DarkGray);

if (chosenMode == ScriptMode.Generate)
{
    // load data files
    DataFile vanilla = new(vanillaPath);
    DataFile modded = new(activePath);

    // mayb in the future, double-check that the versions are the same?

    DataHandler.GeneratePatches(menu, vanilla, modded);
}

if (chosenMode == ScriptMode.Apply)
{
    // check if Vanilla Data exists. If not, assume
    // Active Data is an unmodified version of Deltarune.
    if (!File.Exists(vanillaPath))
    {
        // If Active Data ALSO doesn't exist, then panic.
        if (!File.Exists(activePath))
        {
            menu.lines[4].SetText("Could not find game data.", true);
            menu.DrawAllLines();
            Environment.Exit(2);
            return; // for compiler
        }

        // Rename Active Data to create Vanilla Data.
        // 
        // Vanilla Data is used to apply patches so I
        // don't generate patches for the modified version.
        File.Move(activePath, vanillaPath);
    }

    // Apply patches to Vanilla Data, then save changes to Active Data.
    DataFile data = new(vanillaPath);
    DataHandler.ApplyPatches(menu, data);

    // Create Backup Data by copying the new Active Data
    // File.Copy(activePath, backupPath);
}

if (chosenMode == ScriptMode.Revert)
{
    // Copy Vanilla Data to Active Data, reverting to
    // the version of the game used to generate patches.
    File.Delete(activePath);
    File.Copy(vanillaPath, activePath);

    // success popup
    menu.lines[3].SetText("SUCCESS", true);
    menu.lines[3].SetColor(ConsoleColor.Yellow);
    menu.lines[4].SetText("Successfully restored vanilla data!");
    menu.DrawAllLines();
}

// after whatever the script does,
// move cursor out of the box
ExitMenu();

void ExitMenu()
{
    Console.SetCursorPosition(0, menu.lines.Count);
    Console.CursorVisible = true;
    Environment.Exit(0);
}

enum ScriptMode
{
    Generate,
    Apply,
    Revert,
    DecorTest
}