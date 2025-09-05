// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Reflection;
using ImageMagick.Drawing;
using System.Runtime.InteropServices;

Console.CancelKeyPress += (sender, eventArgs) => ExitMenu();

// Load the script configs from the .json file next to the .csproj file
string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#if DEBUG
Config.current = JsonSerializer.Deserialize<Config>(File.ReadAllText(Path.Combine(configPath, "PatchThingy/config-debug.json")))!;
#else
Config.current = JsonSerializer.Deserialize<Config>(File.ReadAllText(Path.Combine(configPath, "PatchThingy/config.json")))!;
#endif
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
#if DEBUG
// Console.ReadKey doesnt work in debug console.
// if attached to the debugger, a breakpoint here
// should let me change modes manually?
Console.WriteLine(chosenMode);
#endif

// setup for the initial menu
ConsoleMenu menu = new ConsoleMenu(64, 5);
string versionNum = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "??";
Console.Write("\x1b[?1049h");
Console.CursorVisible = false;

// check for resizing
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    PosixSignalRegistration.Create(PosixSignal.SIGWINCH, (context) => { menu.Draw(); });

// exit menu when crashing
try
{
    // title bar
    menu.AddText($"╾─╴╴╴  PatchThingy v{versionNum}  ╶╶╶─╼", Alignment.Center);
    menu.AddSeparator();

    // mode list
    string[] scriptModes = ["Generate new patches", "Apply existing patches", "Manage data files"];
    menu.AddChoicer(ChoicerType.List, scriptModes);

    // input location
    menu.AddSeparator();
    menu.AddText("Please select a mode from the list above.", Alignment.Center);
    menu.Draw();

    // confirm options
    string[] confirmChoices = ["Confirm", "Cancel"];

    while (chosenMode is null)
    {
        menu.SetText(4, "Please select a mode from the list above.");
        menu.Draw();

        switch (menu.PromptChoicer(2))
        {
            case 0:
                menu.SetText(4, "This will overwrite local patches. Continue?");
                menu.AddSeparator();
                menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                menu.Draw();

                if (menu.PromptChoicer(6) == 0)
                {
                    chosenMode = ScriptMode.Generate;
                }

                menu.Remove(5, 6);
                break;

            case 1:
                menu.SetText(4, "This will discard ALL unsaved changes. Continue?");
                menu.AddSeparator();
                menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                menu.Draw();

                if (menu.PromptChoicer(6) == 0)
                {
                    chosenMode = ScriptMode.Apply;
                }

                menu.Remove(5, 6);
                break;

            case 2:
                menu.SetText(4, "Currently this just reverts to vanilla. Continue?");
                menu.AddSeparator();
                menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                menu.Draw();

                if (menu.PromptChoicer(6) == 0)
                {
                    chosenMode = ScriptMode.Revert;
                }

                menu.Remove(5, 6);
                break;

            default:
                menu.SetText(4, "Are you sure you want to exit PatchThingy?");
                menu.AddSeparator();
                menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                menu.Draw();

                if (menu.PromptChoicer(6) == 0)
                {
                    ExitMenu();
                }

                menu.Remove(5, 6);
                break; // for compiler
        }
    }

    // clear the menu and re-add the header
    menu.RemoveAll();
    menu.AddText($"╾─╴╴╴  PatchThingy v{versionNum}  ╶╶╶─╼", Alignment.Center);

    if (chosenMode == ScriptMode.Generate)
    {
        // load data files
        DataFile vanilla = new(vanillaPath);
        DataFile modded = new(activePath);

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
                menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText("Could not find game data.", Alignment.Center);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(3);
                ExitMenu();
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
        menu.AddSeparator();
        menu.AddText("SUCCESS", Alignment.Center, ConsoleColor.Yellow);
        menu.AddText("Successfully applied patches!", Alignment.Center);
        menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
        menu.Draw();
        menu.PromptChoicer(10);
    }
}
catch (Exception error) // show crashes in main terminal output
{
    Console.Write("\x1b[?1049l"); // main screen
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"   PatchThingy v{versionNum}");
    Console.ForegroundColor = ConsoleColor.Red;
    foreach (string line in error.ToString().Split("\n"))
    {
        Console.WriteLine(line);
        Console.ForegroundColor = ConsoleColor.DarkGray;
    }
    Console.WriteLine();
    Console.ResetColor();
    Console.CursorVisible = true;
    Environment.Exit(2);
}

// after whatever the script does,
// move cursor out of the box
ExitMenu();

void ExitMenu()
{
    Console.Write("\x1b[?1049l"); // main screen
    // Console.SetCursorPosition(0, Console.BufferHeight);
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