// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;


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

Console.WriteLine(Config.current);

ScriptMode? chosenMode = null;
Console.WriteLine("─────────────────────────────────");

// chosenMode = ScriptMode.Apply;

while (chosenMode is null)
{
    Console.WriteLine("Please select an option:");
    Console.WriteLine("G - Generate new patches");
    Console.WriteLine("A - Apply existing patches");
    Console.WriteLine("R - Revert to vanilla");
    Console.WriteLine("─────────────────────────────────");

    switch (PromptUserInput(["g", "a", "r"]))
    {
        case "g":
            Console.WriteLine("─────────────────────────────────");
            Console.WriteLine("You've chosen to generate patches. Are you sure? (Y/N)");

            if (PromptUserInput(["y", "n"]) == "y")
            {
                chosenMode = ScriptMode.Generate;
            }
            
            Console.WriteLine("═════════════════════════════════");

            break;

        case "a":
            Console.WriteLine("─────────────────────────────────");
            Console.WriteLine("You've chosen to apply patches. Are you sure? (Y/N)");

            if (PromptUserInput(["y", "n"]) == "y")
            {
                chosenMode = ScriptMode.Apply;
            }
            
            Console.WriteLine("═════════════════════════════════");
            break;

        case "r":
            Console.WriteLine("─────────────────────────────────");
            Console.WriteLine("You've chosen to revert to vanilla. Are you sure? (Y/N)");


            if (PromptUserInput(["y", "n"]) == "y")
            {
                chosenMode = ScriptMode.Revert;
            }
            
            Console.WriteLine("═════════════════════════════════");
            break;
    }
}

if (chosenMode == ScriptMode.Generate)
{
    DataFile vanilla = new(vanillaPath);
    DataFile modded = new(activePath);

    // mayb in the future, double-check that the versions are the same?

    DataHandler.GeneratePatches(vanilla, modded);
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
            Console.WriteLine("ERROR: Could not find game data.");
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
    DataHandler.ApplyPatches(data);

    // Create Backup Data by copying the new Active Data
    // File.Copy(activePath, backupPath);
}

if (chosenMode == ScriptMode.Revert)
{
    // Copy Vanilla Data to Active Data, reverting to
    // the version of the game used to generate patches.
    File.Delete(activePath);
    File.Copy(vanillaPath, activePath);
}

string PromptUserInput(string[] choices)
{
    string? output = Console.ReadLine()?.ToLower();

    while (output is null || !choices.Contains(output))
    {
        if (output is null)
        {
            Console.WriteLine("oh ok, i see how it is.");
            Environment.Exit(1);
        }

        Console.WriteLine("Please select a valid option.");
        output = Console.ReadLine()?.ToLower();
    }

    return output;
}
enum ScriptMode
{
    Generate,
    Apply,
    Revert
}