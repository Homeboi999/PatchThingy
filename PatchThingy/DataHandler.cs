using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

// One class to manage all changes to the data.win
//
// This file contains the misc. script modes, most
// of which are copying data files to/from active.
partial class DataHandler
{
    public static void ManageDataFiles(ConsoleMenu menu, ScriptMode? chosenMode, bool multiChapter = false)
    {
        // necessary for functionality
        string sourceData = "";
        string destData = "";

        // nice looking console output
        string message = "";
        string[] modeText = ["", "", ""]; // array bc grammar
        bool success = true;

        // get the start + end points depending on
        // which mode is selected
        switch (chosenMode)
        {
            case ScriptMode.LoadVanilla:
                sourceData = DataFile.vanilla;
                destData = DataFile.active;
                message = $"Successfully loaded Chapter {DataFile.chapter} from {Path.GetFileName(sourceData)}!";
                modeText = ["load", "ed", "Vanilla Data"];
                break;

            case ScriptMode.UpdateVanilla:
                sourceData = DataFile.active;
                destData = DataFile.vanilla;
                message = $"Successfully updated {Path.GetFileName(destData)} for Chapter {DataFile.chapter}!";
                modeText = ["update", "d", "Vanilla Data"];
                break;
                
            case ScriptMode.LoadBackup:
                sourceData = DataFile.backup;
                destData = DataFile.active;
                message = $"Successfully loaded Chapter {DataFile.chapter} from {Path.GetFileName(sourceData)}!";
                modeText = ["load", "ed", "Backup Data"];
                break;
                
            case ScriptMode.UpdateBackup:
                sourceData = DataFile.active;
                destData = DataFile.backup;
                message = $"Successfully updated {Path.GetFileName(destData)} for Chapter {DataFile.chapter}!";
                modeText = ["update", "d", "Backup Data"];
                break;
        }

        // try to copy, otherwise make an error for the menu
        try
        {
            File.Copy(sourceData, destData, true);
        }
        catch (FileNotFoundException)
        {
            message = $"Could not find {Path.GetFileName(sourceData)} for Chapter {DataFile.chapter}.";
            success = false;
        }

        if (multiChapter)
        {
            if (success)
            {
                message = $"Chapter {DataFile.chapter}: Successfully {modeText[0] + modeText[1] + " " + modeText[2]}!";
                menu.AddText(message, Alignment.Left, ConsoleColor.Yellow);
            }
            else
            {
                message = $"Chapter {DataFile.chapter}: Unable to {modeText[0] + " " + modeText[2]}.";
                menu.AddText(message, Alignment.Left, ConsoleColor.Red);
            }
        }
        else
        {
            if (success)
            {
                menu.MessagePopup(PopupType.Success, [message]);
            }
            else
            {
                menu.MessagePopup(PopupType.Error, [message]);
            }
        }
    }
}