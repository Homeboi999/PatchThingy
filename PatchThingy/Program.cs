// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

Config.current = JsonSerializer.Deserialize<Config>(File.ReadAllText("./PatchThingy.json"))!;

if (Config.current is null)
{
    Environment.Exit(2);
    return;
}

Console.WriteLine(Config.current);

ScriptMode? chosenMode = null;
Console.WriteLine("─────────────────────────────────");

while (chosenMode is null)
{
    Console.WriteLine("Please select an option:");
    Console.WriteLine("G - Generate new patches");
    Console.WriteLine("A - Apply existing patches");
    Console.WriteLine("─────────────────────────────────");

    switch (PromptUserInput(["g", "a"]))
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
    }
}

if (chosenMode == ScriptMode.Generate)
{
    DataFile vanilla = new(Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data-vanilla.win"));
    DataFile modded = new(Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data.win"));

    DataHandler.GeneratePatches(vanilla, modded);
}

if (chosenMode == ScriptMode.Apply)
{
    string dataPath = Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data-vanilla.win");

    if (!File.Exists(dataPath)) // check if data-vanilla.win exists
    {
        string fallbackPath = Path.Combine(Config.current.GamePath, DataFile.chapterFolder, "data.win");

        if (!File.Exists(fallbackPath)) // double-check that the normal data.win exists too
        {
            Console.WriteLine("ERROR: Could not find game data.");
            Environment.Exit(2); // panic
            return;
        }

        File.Move(fallbackPath, dataPath); // rename data.win to data-vanilla.win
    }

    DataFile data = new(dataPath);
    DataHandler.ApplyPatches(data);
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
    Apply
}