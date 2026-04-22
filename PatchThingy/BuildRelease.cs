using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using xdelta3.net;

// One class to manage all changes to the data.win
//
// This file contains the functions used for xdelta
// patches, and potentially making complete releases.
partial class DataHandler
{
    public static void ReleasePatches(ConsoleMenu menu, bool lastChapter = true, bool allChapters = false)
    {
        byte[] moddedFile;
        byte[] vanillaFile;

        string fileName = $"CPM_Chapter{DataFile.chapter}.xdelta";

        try
        {
            moddedFile = File.ReadAllBytes(DataFile.active);
            vanillaFile = File.ReadAllBytes(DataFile.vanilla);
        }
        catch (FileNotFoundException missingFile)
        {
            menu.MessagePopup(PopupType.Error, [$"Could not find {Path.GetFileName(missingFile.FileName)} for Chapter {DataFile.chapter}."]);
            return;
        }

        byte[] xdelta = Xdelta3Lib.Encode(vanillaFile, moddedFile).ToArray();
        File.WriteAllBytes(Path.Combine(Config.current.ReleasePath, fileName), xdelta);

        string[] message = [];

        if (allChapters)
        {
            // success popup
            message = ["Successfully created xdelta patches for all chapters!"];
        }
        else
        {
            message = [$"Successfully created xdelta patch for Chapter {DataFile.chapter}!"];
        }

        if (lastChapter)
        {
            // success popup
            menu.MessagePopup(PopupType.Success, message);
        }
    }
}