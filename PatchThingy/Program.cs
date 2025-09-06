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
bool reselectChapter = false;

// confirm options
string[] confirmChoices = ["Confirm", "Cancel"];
string[] dataOptions = [
    "Revert to Vanilla",
    "Update Vanilla from Active",
    "Restore from Backup",
    "Create new Backup",
    "Convert Patch to Source"
    ];

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
        reselectChapter = false;

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

        while (chosenMode is null && !reselectChapter)
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
                    menu.AddChoicer(ChoicerType.Grid, dataOptions);
                    menu.Draw();

                    switch (menu.PromptChoicer(7))
                    {
                        case 0:
                            menu.AddSeparator();
                            menu.AddText("Copy Vanilla Data to Active Data, reverting to", Alignment.Center);
                            menu.AddText("the version of the game used to generate patches.", Alignment.Center);
                            menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                            menu.Draw();

                            if (menu.PromptChoicer(11) == 0)
                            {
                                chosenMode = ScriptMode.Revert;
                            }

                            menu.Remove(8, 11);
                            break;

                        case 1:
                            menu.AddSeparator();
                            menu.AddText("Copy Active Data to update Vanilla Data.", Alignment.Center);
                            menu.AddText("Only use this after verifying files in Steam!", Alignment.Center, ConsoleColor.Yellow);
                            menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                            menu.Draw();

                            if (menu.PromptChoicer(11) == 0)
                            {
                                chosenMode = ScriptMode.Update;
                            }

                            menu.Remove(8, 11);
                            break;

                        case 2:
                            menu.AddSeparator();
                            menu.AddText("Copy Backup Data to Active Data, restoring", Alignment.Center);
                            menu.AddText("to previous version in case something broke.", Alignment.Center);
                            menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                            menu.Draw();

                            if (menu.PromptChoicer(11) == 0)
                            {
                                chosenMode = ScriptMode.LoadBackup;
                            }

                            menu.Remove(8, 11);
                            break;

                        case 3:
                            menu.AddSeparator();
                            menu.AddText("Copy Active Data to Backup Data, creating", Alignment.Center);
                            menu.AddText("a backup without generating new patches.", Alignment.Center);
                            menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                            menu.Draw();

                            if (menu.PromptChoicer(11) == 0)
                            {
                                chosenMode = ScriptMode.NewBackup;
                            }

                            menu.Remove(8, 11);
                            break;

                        case 4:
                            menu.AddSeparator();
                            menu.AddText("Converts .patch files placed the Source/Code folder", Alignment.Center);
                            menu.AddText("into the full GML code. (Directly from data.win)", Alignment.Center);
                            menu.AddChoicer(ChoicerType.Grid, confirmChoices);
                            menu.Draw();

                            if (menu.PromptChoicer(11) == 0)
                            {
                                chosenMode = ScriptMode.ConvertPatches;
                            }

                            menu.Remove(8, 11);
                            break;
                        default:
                            menu.Remove(6, 7);
                            break;
                    }
                    break;

                default:
                    menu.Remove(4, 5);
                    DataFile.chapter = 0;
                    reselectChapter = true;
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
        DataFile vanilla;
        DataFile modded;

        try
        {
            // load data files
            vanilla = new(vanillaPath);
            modded = new(activePath);
        }
        catch (FileNotFoundException)
        {
            menu.AddSeparator();
            menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
            menu.AddText("Unable to locate required data files.", Alignment.Center);
            menu.AddText("(Does data-vanilla.win exist?)", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
            menu.Draw();
            menu.PromptChoicer(6);
            ExitMenu();
            return; // for compiler
        }

        // create backup of previous version
        File.Copy(activePath, backupPath, true);

        // generate patches
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
                menu.AddSeparator();
                menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
                menu.AddText("Could not find game data.", Alignment.Center);
                menu.AddSeparator(false);
                menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
                menu.Draw();
                menu.PromptChoicer(5);
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
    }

    // Manage Data options
    //
    // these are mostly convenience things
    // like switching between different
    // versions of data.win

    if (chosenMode == ScriptMode.Revert || chosenMode == ScriptMode.Update || chosenMode == ScriptMode.LoadBackup || chosenMode == ScriptMode.NewBackup)
    {
        string sourceData = "";
        string destData = "";

        // get the start + end points depending on
        // which mode is selected
        switch (chosenMode)
        {
            case ScriptMode.Revert:
                sourceData = vanillaPath;
                destData = activePath;
                break;

            case ScriptMode.Update:
                sourceData = activePath;
                destData = vanillaPath;
                break;
                
            case ScriptMode.LoadBackup:
                sourceData = backupPath;
                destData = activePath;
                break;
                
            case ScriptMode.NewBackup:
                sourceData = activePath;
                destData = backupPath;
                break;
        }

        // try to copy, otherwise make an error for the menu
        try
        {
            File.Copy(sourceData, destData, true);
        }
        catch (FileNotFoundException)
        {
            menu.AddSeparator();
            menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
            menu.AddText($"Could not find {Path.GetFileName(sourceData)} for Chapter {DataFile.chapter}.", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
            menu.Draw();
            menu.PromptChoicer(5);
            ExitMenu();
            return; // for compiler
        }

        // success popup
        menu.AddSeparator();
        menu.AddText("SUCCESS", Alignment.Center, ConsoleColor.Yellow);
        menu.AddText($"Successfully reverted Chapter {DataFile.chapter} to vanilla!", Alignment.Center);
        menu.AddSeparator(false);
        menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
        menu.Draw();
        menu.PromptChoicer(5);
    }

    if (chosenMode == ScriptMode.ConvertPatches)
    {
        DataFile modded;

        try
        {
            modded = new DataFile(activePath);
        }
        catch (FileNotFoundException)
        {
            menu.AddSeparator();
            menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
            menu.AddText($"Could not find {Path.GetFileName(activePath)} for Chapter {DataFile.chapter}.", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
            menu.Draw();
            menu.PromptChoicer(5);
            ExitMenu();
            return; // for compiler
        }

        DataHandler.ConvertPatches(modded);
    }
}
catch (Exception error) // show crashes in main terminal output
{
    Console.Write("\x1b[?1049l"); // main screen
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine($"   PatchThingy v{versionNum}");
    
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
    Update,
    LoadBackup,
    NewBackup,
    ConvertPatches,
}