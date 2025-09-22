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
using System.Diagnostics;

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

// setup for the initial menu
ConsoleMenu menu = new ConsoleMenu(64, 8);
string versionNum = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "??";
Console.Write("\x1b[?1049h");
Console.CursorVisible = false;

// check for resizing
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    PosixSignalRegistration.Create(PosixSignal.SIGWINCH, (context) => { menu.Draw(); });

// create variables used to select
// a chapter and mode.
ScriptMode? chosenMode = null;
string[] chapters = ["Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4"];
string[] scriptModes = ["Generate new patches", "Apply existing patches", "Manage data files"];

// debugger crashes on readkey, so just bypass it as much as i can
#if DEBUG
if (Debugger.IsAttached)
{
    chosenMode = ScriptMode.Apply;
    DataFile.chapter = 2;
}
#endif

// confirm options
string[] dataOptions = [
    "Revert to Vanilla",
    "Update Vanilla Data",
    "Restore from Backup",
    "Create new Backup",
    "Convert Patch to Source"
    ];

// exit menu when crashing
try
{
    // title bar
    menu.AddText($"╾─╴╴╴  PatchThingy v{versionNum}  ╶╶╶─╼", Alignment.Center);
    menu.AddSeparator(false);
    menu.AddText("", Alignment.Center);
    menu.AddSeparator(false); // spacing
    menu.AddSeparator();

    // chapter/mode choicers
    menu.AddSeparator(false); // spacing
    int chapterChoicer = menu.AddChoicer(ChoicerType.Grid, chapters); // 6
    int modeChoicer = menu.AddChoicer(ChoicerType.List, scriptModes); // 7
    int dataModeChoicer = menu.AddChoicer(ChoicerType.Grid, dataOptions); // 8
    menu.AddSeparator(false);

    // script mode confirm messages

    string[] exitMessage = ["Are you sure you want to exit PatchThingy?"];
    string[] generateMessage = ["This will overwrite local patches. Continue?"];
    string[] applyMessage = ["This will discard all unsaved modifications. Continue?"];

    string[] revertMessage = [];
    revertMessage.Append("Copy Vanilla Data to Active Data, reverting to");
    revertMessage.Append("the version of the game used to generate patches.");

    string[] updateMessage = [];
    updateMessage.Append("Copy Active Data to update Vanilla Data.");
    updateMessage.Append("Only use this after verifying files in Steam!");

    string[] loadBackupMessage = [];
    loadBackupMessage.Append("Copy Backup Data to Active Data, restoring");
    loadBackupMessage.Append("to previous version in case something broke.");

    string[] newBackupMessage = [];
    newBackupMessage.Append("Copy Active Data to Backup Data, creating");
    newBackupMessage.Append("a backup without generating new patches.");

    string[] convertPatchesMessage = [];
    convertPatchesMessage.Append("Converts .patch files placed the Source/Code folder");
    convertPatchesMessage.Append("into the full GML code. (Directly from data.win)");

    int curChoicer = chapterChoicer;
    menu.Draw();

    // so i can loop back to chap.select menu
    while (chosenMode is null || DataFile.chapter < 1)
    {
        if (curChoicer == chapterChoicer)
        {
            // reset text
            menu.SetText(2, "Select a Deltarune chapter to patch.");

            switch (menu.PromptChoicer(chapterChoicer))
            {
                case 0:
                    DataFile.chapter = 1;
                    curChoicer = modeChoicer;
                    break;
                case 1:
                    DataFile.chapter = 2;
                    curChoicer = modeChoicer;
                    break;
                case 2:
                    DataFile.chapter = 3;
                    curChoicer = modeChoicer;
                    break;
                case 3:
                    DataFile.chapter = 4;
                    curChoicer = modeChoicer;
                    break;

                default:
                    if (menu.ConfirmChoicer(exitMessage))
                    {
                        ExitMenu();
                    }

                    break; // for compiler
            }
        }

        if (curChoicer == modeChoicer)
        {
            // mode list
            menu.SetText(2, $"Please select an action for Deltarune Chapter {DataFile.chapter}.");

            switch (menu.PromptChoicer(modeChoicer))
            {
                case 0:
                    // don't confirm choice if there isnt already patches
                    if (Directory.Exists(Config.current.OutputPath))
                    {
                        if (menu.ConfirmChoicer(generateMessage))
                        {
                            chosenMode = ScriptMode.Generate;
                        }
                    }
                    else
                    {
                        chosenMode = ScriptMode.Generate;
                    }
                    break;

                case 1:
                    // dont prompt if theres no vanilla
                    // (assumes this is an unmodified game)
                    if (File.Exists(DataFile.vanilla))
                    {
                        if (menu.ConfirmChoicer(applyMessage))
                        {
                            chosenMode = ScriptMode.Apply;
                        }
                    }
                    else
                    {
                        chosenMode = ScriptMode.Apply;
                    }
                    break;

                case 2:
                    curChoicer = dataModeChoicer;
                    break;

                default:
                    DataFile.chapter = 0;
                    curChoicer = chapterChoicer;
                    continue;
            }
        }

        if (curChoicer == dataModeChoicer)
        {
            // new choicer with more options
            switch (menu.PromptChoicer(dataModeChoicer))
            {
                case 0:

                    if (menu.ConfirmChoicer(revertMessage))
                    {
                        chosenMode = ScriptMode.Revert;
                    }

                    break;

                case 1:

                    if (menu.ConfirmChoicer(updateMessage))
                    {
                        chosenMode = ScriptMode.Update;
                    }

                    break;

                case 2:

                    if (menu.ConfirmChoicer(loadBackupMessage))
                    {
                        chosenMode = ScriptMode.LoadBackup;
                    }

                    break;

                case 3:

                    if (menu.ConfirmChoicer(newBackupMessage))
                    {
                        chosenMode = ScriptMode.NewBackup;
                    }

                    break;

                case 4:

                    if (menu.ConfirmChoicer(convertPatchesMessage))
                    {
                        chosenMode = ScriptMode.ConvertPatches;
                    }

                    break;

                default:
                    curChoicer = modeChoicer;
                    continue;
            }
        }
    }

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
            vanilla = new(DataFile.vanilla);
            modded = new(DataFile.active);
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
        File.Copy(DataFile.active, DataFile.backup, true);

        // generate patches
        DataHandler.GeneratePatches(menu, vanilla, modded);
    }

    if (chosenMode == ScriptMode.Apply)
    {
        // check if Vanilla Data exists. If not, assume
        // Active Data is an unmodified version of Deltarune.
        if (!File.Exists(DataFile.vanilla))
        {
            // If Active Data ALSO doesn't exist, then panic.
            if (!File.Exists(DataFile.active))
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
            File.Move(DataFile.active, DataFile.vanilla);
        }

        // Apply patches to Vanilla Data, then save changes to Active Data.
        DataFile data = new(DataFile.vanilla);
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
        string message = "";

        // get the start + end points depending on
        // which mode is selected
        switch (chosenMode)
        {
            case ScriptMode.Revert:
                sourceData = DataFile.vanilla;
                destData = DataFile.active;
                message = $"Successfully loaded Chapter {DataFile.chapter} from {Path.GetFileName(sourceData)}!";
                break;

            case ScriptMode.Update:
                sourceData = DataFile.active;
                destData = DataFile.vanilla;
                message = $"Successfully updated {Path.GetFileName(destData)} for Chapter {DataFile.chapter}!";
                break;
                
            case ScriptMode.LoadBackup:
                sourceData = DataFile.backup;
                destData = DataFile.active;
                message = $"Successfully loaded Chapter {DataFile.chapter} from {Path.GetFileName(sourceData)}!";
                break;
                
            case ScriptMode.NewBackup:
                sourceData = DataFile.active;
                destData = DataFile.backup;
                message = $"Successfully updated {Path.GetFileName(destData)} for Chapter {DataFile.chapter}!";
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
        menu.AddText(message, Alignment.Center);
        menu.AddSeparator(false);
        menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
        menu.Draw();
        menu.PromptChoicer(5);
    }

    if (chosenMode == ScriptMode.ConvertPatches)
    {
        DataFile modded;
        int chapterCount = 0;
        int globalCount = 0;

        try
        {
            modded = new DataFile(DataFile.active);
            chapterCount = DataHandler.ConvertPatches(modded, DataFile.chapter);
            globalCount = DataHandler.ConvertPatches(modded, 0);
        }
        catch (FileNotFoundException)
        {
            menu.AddSeparator();
            menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
            menu.AddText($"Could not find {Path.GetFileName(DataFile.active)} for Chapter {DataFile.chapter}.", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
            menu.Draw();
            menu.PromptChoicer(5);
            ExitMenu();
            return; // for compiler
        }

        string message;
        string chapterMessage = $"{chapterCount} Chapter {DataFile.chapter} patches";
        string globalMessage = $"{globalCount} Global patches";

        // set output message based on how many
        // patches of each type were converted
        if (chapterCount == 0 && globalCount == 0)
        {
            // print error if nothing happened
            // not an error but idk what else to call it
            menu.AddSeparator();
            menu.AddText("! ERROR !", Alignment.Center, ConsoleColor.Red);
            menu.AddText("No patches detected. (Move desired patches to Source/Code)", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
            menu.Draw();
            menu.PromptChoicer(5);
            ExitMenu();
            return; // for compiler
        }
        else if (chapterCount > 0 && globalCount == 0)
        {
            message = $"Converted {chapterMessage}!";
        }
        else if (chapterCount == 0 && globalCount > 0)
        {
            message = $"Converted {globalMessage}!";
        }
        else
        {
            message = $"Converted {chapterMessage} and {globalMessage}!";
        }

        // success popup
        menu.AddSeparator();
        menu.AddText("SUCCESS", Alignment.Center, ConsoleColor.Yellow);
        menu.AddText(message, Alignment.Center);
        menu.AddSeparator(false);
        menu.AddChoicer(ChoicerType.List, ["Exit PatchThingy"]);
        menu.Draw();
        menu.PromptChoicer(5);
    }
}
catch (Exception error) // show crashes in main terminal output
{
    #if DEBUG
    if (Debugger.IsAttached)
    {
        // just throw so i can use debugger on it
        throw;
    }
    #endif

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