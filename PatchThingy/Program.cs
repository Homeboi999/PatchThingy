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
string[] chapterChoices = ["All Chapters", "Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4"];
string[] scriptModes = ["Generate new patches", "Apply existing patches", "File Management"];

// if true, loop through each chapter for any mode
bool allChapters = false;
int globalChapter = 0;

// debugger crashes on readkey, so just bypass it as much as i can
#if DEBUG
if (Debugger.IsAttached)
{
    chosenMode = ScriptMode.Apply;
    DataFile.chapter = 1;
    allChapters = false;
}
#endif

// confirm options
string[] fileOptions = [
    "Vanilla Data",
    "Mod Backup",
    "Convert Patch to Source",
    "Update Source Code"
    ];

string[] dataOptions = ["Restore", "Update"];

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
    int globalPatchChoicer = menu.AddChoicer(ChoicerType.Grid, chapters); // 9
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
    int choice = -1;
    menu.Draw();

    // so i can loop back to chap.select menu
    while (chosenMode is null || (DataFile.chapter < 1 && !allChapters))
    {
        if (curChoicer == chapterChoicer)
        {
            // reset text
            menu.SetText(2, "Select a Deltarune chapter to patch.");
            choice = menu.PromptChoicer(chapterChoicer);

            switch (choice)
            {
                // All Chapters
                case 0:
                    allChapters = true;
                    curChoicer = modeChoicer;
                    break;

                // Cancel pressed
                case -1:
                    if (menu.ConfirmChoicer(exitMessage) == 0)
                    {
                        ExitMenu();
                    }
                    break; // for compiler

                // Individual Chapters
                default:
                    DataFile.chapter = choice;
                    curChoicer = modeChoicer;
                    break;
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
                    if (!allChapters && Directory.Exists(Config.current.OutputPath))
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
                    if (!allChapters && File.Exists(DataFile.vanilla))
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
                            chosenMode = ScriptMode.LoadVanilla;
                            break;
                        case 1:
                            chosenMode = ScriptMode.UpdateVanilla;
                            break;
                        default:
                            // cancel
                            break;
                    }

                    break;

                case 1:

                    switch (menu.ConfirmChoicer(backupEditMessage, dataOptions))
                    {
                        case 0:
                            chosenMode = ScriptMode.LoadBackup;
                            break;
                        case 1:
                            chosenMode = ScriptMode.UpdateBackup;
                            break;
                        default:
                            // cancel
                            break;
                    }

                    break;

                case 2:

                    if (menu.ConfirmChoicer(convertPatchesMessage) == 0)
                    {
                        chosenMode = ScriptMode.ConvertPatches;
                    }

                    break;

                case 3:

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

        // choose a chapter to use for global patches
        if (allChapters && (chosenMode == ScriptMode.Generate || chosenMode == ScriptMode.ConvertPatches))
        {
            menu.SetText(2, "Which chapter should Global Patches be generated from?");

            // make sure a chapter gets selected,
            // unless the user backs out
            while (globalChapter < 1 && chosenMode is not null)
            {
                // select a preferred chapter
                choice = menu.PromptChoicer(globalPatchChoicer) + 1;

                switch (choice)
                {
                    // cancel pressed
                    case 0:
                        chosenMode = null;
                        break;
                    
                    // chapter selected
                    default:
                        // update the globalChapter variable
                        if (chosenMode == ScriptMode.Generate && Directory.Exists(Config.current.OutputPath))
                        {
                            // move generate message here so it flows nicer
                            if (menu.ConfirmChoicer(generateMessage) == 0)
                            {
                                globalChapter = choice;
                            }
                        }
                        else
                        {
                            globalChapter = choice;
                        }
                        // if not confirmed, loop again to prompt the og choicer
                        break;
                }
            }
        }
    }

    // clear menu
    menu.RemoveAll();
    menu.ResizeBox(80);

    if (chosenMode == ScriptMode.Generate)
    {
        // generate patches
        if (allChapters)
        {
            // clear menu again
            menu.RemoveAll();

            // set up the menu for console output
            // only do this once at the very start
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

    if (chosenMode == ScriptMode.Apply || chosenMode == ScriptMode.ImportSource)
    {
        // set up the menu for console output
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

        if (allChapters)
        {
            // loop through & apply
            for (int i = 1; i <= chapters.Length; i++)
            {
                DataFile.chapter = i;
                DataHandler.ApplyPatches(menu, (chosenMode == ScriptMode.ImportSource),(i == chapters.Count()));
            }
        }
        else
        {
            // apply
            DataHandler.ApplyPatches(menu, (chosenMode == ScriptMode.ImportSource));
        }
        
        ExitMenu();
    }

    // Manage Data options
    //
    // these are mostly convenience things
    // like switching between different
    // versions of data.win
    if (chosenMode == ScriptMode.LoadVanilla || chosenMode == ScriptMode.UpdateVanilla || chosenMode == ScriptMode.LoadBackup || chosenMode == ScriptMode.UpdateBackup)
    {
        if (allChapters)
        {
            // set up the menu for console output
            menu.ResizeBox(80);
            menu.AddSeparator(false);

            // loop through chapters
            for (int i = 1; i <= chapters.Length; i++)
            {
                DataFile.chapter = i;
                DataHandler.ManageDataFiles(menu, chosenMode, true);
            }

            // exit patchthingy prompt
            menu.AddSeparator(false);
            menu.MessagePopup(PopupType.Message, []);
            
        }
        else
        {
            DataHandler.ManageDataFiles(menu, chosenMode);
        }

        ExitMenu();
    }

    if (chosenMode == ScriptMode.ConvertPatches)
    {
        DataFile modded;
        int globalCount = 0;
        int chapterCount = 0;

        string message;
        string chapterMessage = "";
        string globalMessage = "";

        if (allChapters)
        {
            // menu setup
            menu.AddSeparator(false);

            // keep track of chapter counts separately
            int chapterTotal = 0;

            for (int i = 1; i <= chapters.Length; i++)
            {
                DataFile.chapter = i;

                try
                {
                    modded = new DataFile(DataFile.active);
                }
                catch (FileNotFoundException)
                {
                    menu.MessagePopup(PopupType.Error, [$"Could not find {Path.GetFileName(DataFile.active)} for Chapter {DataFile.chapter}."]);
                    return;
                }

                chapterCount = DataHandler.PatchesToCode(menu, modded, DataFile.chapter);
                chapterTotal += chapterCount;

                if (globalChapter == DataFile.chapter)
                {
                    globalCount = DataHandler.PatchesToCode(menu, modded, 0);
                }

                if (chapterCount > 0)
                {
                    menu.AddText($"Chapter {DataFile.chapter}: Converted {chapterCount} Patches");
                }
            }

            chapterMessage = $"{chapterTotal} Chapter-Specific patches";
            globalMessage = $"{globalCount} Global patches";

            // we're done with the per-chapter
            // count, and that string is used
            // in the success message.
            chapterCount = chapterTotal;
        }
        else
        {
            try
            {
                modded = new DataFile(DataFile.active);
            }
            catch (FileNotFoundException)
            {
                menu.MessagePopup(PopupType.Error, [$"Could not find {Path.GetFileName(DataFile.active)} for Chapter {DataFile.chapter}."]);
                return;
            }

            chapterCount = DataHandler.PatchesToCode(menu, modded, DataFile.chapter);
            globalCount = DataHandler.PatchesToCode(menu, modded, 0);

            chapterMessage = $"{chapterCount} Chapter {DataFile.chapter} patches";
            globalMessage = $"{globalCount} Global patches";
        }

        if (chapterCount + globalCount > 0)
        {
            menu.AddSeparator(false);
        }

        // set output message based on how many
        // patches of each type were converted
        if (chapterCount == 0 && globalCount == 0)
        {
            // make different popup if nothing happened
            message = "No patches detected. (Move desired patches to the Code folder)";
            menu.MessagePopup(PopupType.Message, [message]);
            
            ExitMenu();
            return;
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

        ExitMenu();
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