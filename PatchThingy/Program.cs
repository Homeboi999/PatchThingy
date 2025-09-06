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

// create variables used to select
// a chapter and mode.
ScriptMode? chosenMode = null;

// setup for the initial menu
ConsoleMenu menu = new ConsoleMenu(64, 8);
string versionNum = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "??";
Console.Write("\x1b[?1049h");
Console.CursorVisible = false;

// check for resizing
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    PosixSignalRegistration.Create(PosixSignal.SIGWINCH, (context) => { menu.Draw(); });

// variables that get used during the loop
string[] chapters = ["Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4"];
string[] scriptModes = ["Generate new patches", "Apply existing patches", "Manage data files"];
bool backToStart = false;

// confirm options
string[] confirmChoices = ["Confirm", "Cancel"];

// exit menu when crashing
try
{
    // title bar
    menu.AddText($"╾─╴╴╴  PatchThingy v{versionNum}  ╶╶╶─╼", Alignment.Center);
    menu.AddText("Select a Deltarune chapter to patch.", Alignment.Center);
    menu.AddSeparator();

    // chapter choicer
    menu.AddChoicer(ChoicerType.Grid, chapters);

    // so i can loop back to chap.select menu
    while (chosenMode is null && DataFile.chapter < 1)
    {
        DataFile.chapter = 0;
        backToStart = false;

        // reset text
        menu.SetText(1, "Select a Deltarune chapter to patch.");

        while (DataFile.chapter < 1)
        {
            menu.Draw();

            switch (menu.PromptChoicer(3))
            {
                case 0:
                    DataFile.chapter = 1;
                    break;
                case 1:
                    DataFile.chapter = 2;
                    break;
                case 2:
                    DataFile.chapter = 3;
                    break;
                case 3:
                    DataFile.chapter = 4;
                    break;

                default:
                    menu.AddSeparator();
                    menu.AddText("Are you sure you want to exit PatchThingy?", Alignment.Center);
                    menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                    menu.Draw();

                    if (menu.PromptChoicer(6) == 0)
                    {
                        ExitMenu();
                    }

                    menu.Remove(4, 6);
                    break; // for compiler
            }
        }

        // mode list
        menu.SetText(1, $"Please select an action for Deltarune Chapter {DataFile.chapter}.");
        menu.AddSeparator();
        menu.AddChoicer(ChoicerType.List, scriptModes);

        while (chosenMode is null && !backToStart)
        {
            menu.Draw();

            switch (menu.PromptChoicer(5))
            {
                case 0:
                    if (Directory.Exists(Config.current.OutputPath))
                    {
                        menu.AddSeparator();
                        menu.AddText("This will overwrite local patches. Continue?", Alignment.Center);
                        menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                        menu.Draw();

                        if (menu.PromptChoicer(8) == 0)
                        {
                            chosenMode = ScriptMode.Generate;
                        }

                        menu.Remove(6, 8);
                    }
                    else
                    {
                        chosenMode = ScriptMode.Generate;
                    }
                    break;

                case 1:
                    menu.AddSeparator();
                    menu.AddText("This will discard all unsaved modifications. Continue?", Alignment.Center);
                    menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                    menu.Draw();

                    if (menu.PromptChoicer(8) == 0)
                    {
                        chosenMode = ScriptMode.Apply;
                    }

                    menu.Remove(6, 8);
                    break;

                case 2:
                    menu.AddSeparator();
                    menu.AddText("Currently this just reverts to vanilla. Continue?", Alignment.Center);
                    menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                    menu.Draw();

                    if (menu.PromptChoicer(8) == 0)
                    {
                        chosenMode = ScriptMode.Revert;
                    }

                    menu.Remove(6, 8);
                    break;

                default:
                    menu.Remove(4, 5);
                    DataFile.chapter = 0;
                    backToStart = true;
                    break;
            }
        }
    }

    // Get the filepath for all 3 versions of data.win
    // Each copy serves a different purpose, making the
    // process of updating much easier.

    // Active Data: the data.win that Deltarune loads, and that Steam would replace.
    string activePath = Path.Combine(Config.current.GamePath, DataFile.GetPath(), "data.win");

    // Vanilla Data: the version of data.win that the patches were based on
    string vanillaPath = Path.Combine(Config.current.GamePath, DataFile.GetPath(), "data-vanilla.win");

    // Backup Data: a second copy of the patched data.win, in case of an update.
    string backupPath = Path.Combine(Config.current.GamePath, DataFile.GetPath(), "data-backup.win");

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
                menu.AddSeparator(false);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(4);
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
        menu.AddText("Successfully reverted to vanilla!", Alignment.Center);
        menu.AddSeparator(false);
        menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
        menu.Draw();
        menu.PromptChoicer(5);
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