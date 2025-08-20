// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;


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

return;
DataFile vanilla = new("data-vanilla.win");
DataFile modded = new("data.win");

DataHandler.GeneratePatches(vanilla, modded);

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