// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
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
Console.Write("\x1b[?1049h");
Console.CursorVisible = false;

// check for resizing
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    PosixSignalRegistration.Create(PosixSignal.SIGWINCH, (context) => { menu.Draw(); });

// create variables used to select
// a chapter and mode.
ScriptMode? chosenMode = null;
string[] chapters = ["Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4"];
string[] chapterChoices = ["Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4", "All Chapters"];
string[] scriptModes = ["Generate new patches", "Apply existing patches", "File Management"];

// if true, loop through each chapter for any mode
bool allChapters = false;

// debugger crashes on readkey, so just bypass it as much as i can
#if DEBUG
if (Debugger.IsAttached)
{
    chosenMode = ScriptMode.Apply;
    DataFile.chapter = 4;
}
#endif

// confirm options
string[] fileOptions = [
    "Vanilla Data",
    "Mod Backup",
    "Convert Patch to Source",
    "Update Source Code"
    ];

string[] dataOptions = ["Update", "Restore"];

// exit menu when crashing
try
{
    // title bar
    menu.AddText($"╾─╴╴╴  PatchThingy v{ConsoleMenu.versionNum}  ╶╶╶─╼", Alignment.Center);
    menu.AddSeparator(false);
    menu.AddText("", Alignment.Center);
    menu.AddSeparator(false); // spacing
    menu.AddSeparator();

    // chapter/mode choicers
    menu.AddSeparator(false); // spacing
    int chapterChoicer = menu.AddChoicer(ChoicerType.Grid, chapterChoices); // 6
    int modeChoicer = menu.AddChoicer(ChoicerType.List, scriptModes); // 7
    int fileChoicer = menu.AddChoicer(ChoicerType.Grid, fileOptions); // 8
    menu.AddSeparator(false);

    // script mode confirm messages

    string[] exitMessage = ["Are you sure you want to exit PatchThingy?"];
    string[] generateMessage = ["This will overwrite local patches. Continue?"];
    string[] applyMessage = ["This will discard all unsaved modifications. Continue?"];

    string[] vanillaEditMessage = ["", ""];
    vanillaEditMessage[0] = "Copy the Vanilla Data to/from the Active Data.";
    vanillaEditMessage[1] = "Used to update Deltarune or unload current patches.";

    string[] backupEditMessage = ["", ""];
    backupEditMessage[0] = "Copy the Backup Data to/from the Active Data.";
    backupEditMessage[1] = "Used to save/load a patched version of Deltarune.";

    string[] convertPatchesMessage = ["", ""];
    convertPatchesMessage[0] = "Converts .patch files placed the Source/Code folder";
    convertPatchesMessage[1] = "into the full GML code. (Directly from data.win)";

    string[] importSourceMessage = ["", ""];
    importSourceMessage[0] = "Updates the .gml code present in the Active Data";
    importSourceMessage[1] = "without interfering with other parts of the game.";

    int curChoicer = chapterChoicer;
    menu.Draw();

    // so i can loop back to chap.select menu
    while (chosenMode is null || (DataFile.chapter < 1 && !allChapters))
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
                case 4:
                    allChapters = true;
                    curChoicer = modeChoicer;
                    break;

                default:
                    if (menu.ConfirmChoicer(exitMessage) == 0)
                    {
                        ExitMenu();
                    }

                    break; // for compiler
            }
        }

        if (curChoicer == modeChoicer)
        {
            // mode list
            if (allChapters)
            {
                menu.SetText(2, $"Please select an action for all chapters of Deltarune.");
            }
            else
            {
                menu.SetText(2, $"Please select an action for Deltarune Chapter {DataFile.chapter}.");
            }

            switch (menu.PromptChoicer(modeChoicer))
            {
                case 0:
                    // don't confirm choice if there isnt already patches
                    if (Directory.Exists(Config.current.OutputPath))
                    {
                        if (menu.ConfirmChoicer(generateMessage) == 0)
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
                        if (menu.ConfirmChoicer(applyMessage) == 0)
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
                    curChoicer = fileChoicer;
                    break;

                default:
                    DataFile.chapter = 0;
                    curChoicer = chapterChoicer;
                    allChapters = false;
                    continue;
            }
        }

        if (curChoicer == fileChoicer)
        {
            // new choicer with more options
            switch (menu.PromptChoicer(fileChoicer))
            {
                case 0:

                    switch (menu.ConfirmChoicer(vanillaEditMessage, dataOptions))
                    {
                        case 0:
                            chosenMode = ScriptMode.UpdateVanilla;
                            break;
                        case 1:
                            chosenMode = ScriptMode.LoadVanilla;
                            break;
                        default:
                            curChoicer = modeChoicer;
                            break;
                    }

                    break;

                case 2:

                    switch (menu.ConfirmChoicer(backupEditMessage, dataOptions))
                    {
                        case 0:
                            chosenMode = ScriptMode.UpdateBackup;
                            break;
                        case 1:
                            chosenMode = ScriptMode.LoadBackup;
                            break;
                        default:
                            curChoicer = modeChoicer;
                            break;
                    }

                    break;

                case 4:

                    if (menu.ConfirmChoicer(convertPatchesMessage) == 0)
                    {
                        chosenMode = ScriptMode.ConvertPatches;
                    }

                    break;

                case 5:

                    if (menu.ConfirmChoicer(importSourceMessage) == 0)
                    {
                        chosenMode = ScriptMode.ImportSource;
                    }

                    break;

                default:
                    curChoicer = modeChoicer;
                    continue;
            }
        }
    }

    // clear menu
    menu.RemoveAll();

    if (chosenMode == ScriptMode.Generate)
    {
        // generate patches
        if (allChapters)
        {
            // menu setup
            menu.AddSeparator(false);
            menu.AddText("Which chapter should Global Patches be generated from?", Alignment.Center);
            menu.AddSeparator(false);
            menu.AddSeparator();
            menu.AddSeparator(false);
            int globalPatchChoicer = menu.AddChoicer(ChoicerType.Grid, chapters);
            menu.AddSeparator(false);
            int globalChapter = 0;

            // make sure a chapter gets selected.
            while (globalChapter < 1)
            {
                // select a preferred chapter
                int choice = menu.PromptChoicer(globalPatchChoicer) + 1;

                // confirm selection, then update the globalChapter variable
                if (choice > 0 && menu.ConfirmChoicer([$"Use Chapter {choice} to generate Global Patches?"], ["Yes", "No"]) == 0)
                {
                    globalChapter = choice;
                }
            }
            // clear menu again
            menu.RemoveAll();

            // set up the menu for console output
            // only do this once at the very start
            menu.ResizeBox(80);
            menu.AddSeparator();        // 1
            menu.AddSeparator(false);   // 2
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);   // 9
            menu.AddSeparator();        // 10

            // set up last text beforehand so i can
            // use ReplaceText instead so multi-chapters reuse it
            menu.AddText("bepis");

            // loop through each chapter to generate patches.
            // starts at 1 for ch1. chapter length includes
            // "All Chapters" option, so it is ignored for this.
            for (int i = 1; i <= chapters.Count(); i++)
            {
                // set the current chapter to load
                DataFile.chapter = i;

                // only generate global patches if chosen
                DataHandler.GeneratePatches(menu, (DataFile.chapter != globalChapter), (i == chapters.Count()));
            }
        }
        else
        {
            // set up the menu for console output
            menu.ResizeBox(80);
            menu.AddSeparator();        // 1
            menu.AddSeparator(false);   // 2
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);
            menu.AddSeparator(false);   // 9
            menu.AddSeparator();        // 10

            // set up last text beforehand so i can
            // use ReplaceText instead so multi-chapters reuse it
            menu.AddText("bepis");

            // generate
            DataHandler.GeneratePatches(menu);
        }
        ExitMenu();
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
                menu.MessagePopup(PopupType.Error, ["Could not find game data."]);
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

    if (chosenMode == ScriptMode.LoadVanilla || chosenMode == ScriptMode.UpdateVanilla || chosenMode == ScriptMode.LoadBackup || chosenMode == ScriptMode.UpdateBackup)
    {
        string sourceData = "";
        string destData = "";
        string message = "";

        // get the start + end points depending on
        // which mode is selected
        switch (chosenMode)
        {
            case ScriptMode.LoadVanilla:
                sourceData = DataFile.vanilla;
                destData = DataFile.active;
                message = $"Successfully loaded Chapter {DataFile.chapter} from {Path.GetFileName(sourceData)}!";
                break;

            case ScriptMode.UpdateVanilla:
                sourceData = DataFile.active;
                destData = DataFile.vanilla;
                message = $"Successfully updated {Path.GetFileName(destData)} for Chapter {DataFile.chapter}!";
                break;
                
            case ScriptMode.LoadBackup:
                sourceData = DataFile.backup;
                destData = DataFile.active;
                message = $"Successfully loaded Chapter {DataFile.chapter} from {Path.GetFileName(sourceData)}!";
                break;
                
            case ScriptMode.UpdateBackup:
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
            string error = $"Could not find {Path.GetFileName(sourceData)} for Chapter {DataFile.chapter}.";
            menu.MessagePopup(PopupType.Error, [error]);
            ExitMenu();
            return; // for compiler
        }

        // success popup
        menu.MessagePopup(PopupType.Success, [message]);
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
            menu.MessagePopup(PopupType.Error, [$"Could not find {Path.GetFileName(DataFile.active)} for Chapter {DataFile.chapter}."]);
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
            menu.MessagePopup(PopupType.Error, ["No patches detected. (Move desired patches to Source/Code)"]);
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
        menu.MessagePopup(PopupType.Success, [message]);
    }

    if (chosenMode == ScriptMode.ImportSource)
    {
        // make sure data.win exists
        if (!File.Exists(DataFile.active))
        {
            menu.MessagePopup(PopupType.Error, ["Could not find game data."]);
            ExitMenu();
            return; // for compiler
        }

        // set up the menu for console output
        menu.ResizeBox(80);
        menu.AddSeparator();        // 1
        menu.AddSeparator(false);   // 2
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);
        menu.AddSeparator(false);   // 9
        menu.AddSeparator();        // 10
        menu.AddText($"Deltarune Chapter {DataFile.chapter} - Importing Code...", Alignment.Center);
        menu.Draw();

        // load Active Data.
        DataFile data = new(DataFile.active);
        CodeImportGroup importGroup = new(data.Data);

        // chapter code
        string codePath = Path.Combine(DataHandler.GetPath(DataFile.chapter), DataHandler.codeFolder);
        if (Path.Exists(codePath))
        {
            foreach (string filePath in Directory.EnumerateFiles(codePath))
            {
                // read code from file
                var codeFile = File.ReadAllText(filePath);
                string codeName = Path.GetFileNameWithoutExtension(filePath);

                // add file to data
                try
                {
                    importGroup.QueueReplace(codeName, codeFile);
                }
                catch (Exception error) when (error.Message == $"Collision event cannot be automatically resolved; must attach to object manually ({Path.GetFileNameWithoutExtension(filePath)})")
                {
                    // build error message
                    string[] errorMessage = [
                        $"Failed to import code file {Path.GetFileNameWithoutExtension(filePath)}",
                        $"Collision event cannot be automatically resolved; must attach to object manually."
                        ];
                        
                    // option to continue importing.
                    if (menu.MessagePopup(PopupType.Warning, errorMessage))
                    {
                        ExitMenu();
                    }

                    return; // stop trying to import
                }

                // scroll log output in menu
                menu.Remove(2);
                menu.InsertText(9, $"Added code {Path.GetFileName(filePath)}");
                menu.Draw();
            }
        }

        // global code
        codePath = Path.Combine(DataHandler.GetPath(0), DataHandler.codeFolder);
        if (Path.Exists(codePath))
        {
            foreach (string filePath in Directory.EnumerateFiles(codePath))
            {
                // read code from file
                var codeFile = File.ReadAllText(filePath);
                string codeName = Path.GetFileNameWithoutExtension(filePath);

                // add file to data
                try
                {
                    importGroup.QueueReplace(codeName, codeFile);
                }
                catch (Exception error) when (error.Message == $"Collision event cannot be automatically resolved; must attach to object manually ({Path.GetFileNameWithoutExtension(filePath)})")
                {
                    // build error message
                    string[] errorMessage = [$"Failed to import code file {Path.GetFileNameWithoutExtension(filePath)}", "Collision event cannot be automatically resolved; must attach to object manually."];
                    if (menu.MessagePopup(PopupType.Warning, errorMessage))

                    return; // stop trying to import
                }

                // scroll log output in menu
                menu.Remove(2);
                menu.InsertText(9, $"Added code {Path.GetFileName(filePath)}");
                menu.Draw();
            }
        }

        // save file
        importGroup.Import();
        data.SaveChanges(Path.Combine(Config.current.GamePath, DataFile.GetPath(), "data.win"));

        // success popup
        menu.MessagePopup(PopupType.Success, ["Successfully updated code!"]);
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

    Console.WriteLine($"   PatchThingy v{ConsoleMenu.versionNum}");
    
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
    LoadVanilla,
    UpdateVanilla,
    LoadBackup,
    UpdateBackup,
    ConvertPatches,
    ImportSource,
}