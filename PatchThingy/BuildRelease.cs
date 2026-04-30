using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Compiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using xdelta3.net;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

// One class to manage all changes to the data.win
//
// This file contains the functions used for xdelta
// patches, and potentially making complete releases.
partial class DataHandler
{
    public static void ReleasePatches(ConsoleMenu menu, bool allChapters = false)
    {
        // result info
        string[] message = [$"Successfully created xdelta patch for Chapter {DataFile.chapter}!"];

        // raw bytes for Modded Data and Vanilla Data
        byte[] moddedFile;
        byte[] vanillaFile;

        // set filename based on chapter number
        string fileName = $"CPM_Chapter{DataFile.chapter}.xdelta";
        string filePath = Path.Combine(Config.current.ReleasePath, "./xdeltas");

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        string unstablePath = Path.Combine(filePath, "./Unstable");

        if (Directory.Exists(unstablePath))
        {
            if (File.Exists(Path.Combine(unstablePath, fileName)))
            {
                filePath = unstablePath;
            }
        }

        try
        {
            // load data files
            moddedFile = File.ReadAllBytes(DataFile.active);
            vanillaFile = File.ReadAllBytes(DataFile.vanilla);
        }
        catch (FileNotFoundException missingFile)
        {
            message = [$"Could not find {Path.GetFileName(missingFile.FileName)} for Chapter {DataFile.chapter}."];

            if (!allChapters)
            {
                menu.MessagePopup(PopupType.Error, message);
            }
            else
            {
                menu.AddText(message[0], Alignment.Center, ConsoleColor.Red);
            }

            return;
        }

        // generate xdeltas using both files and save them to ReleasePath
        byte[] xdelta = Xdelta3Lib.Encode(vanillaFile, moddedFile).ToArray();
        File.WriteAllBytes(Path.Combine(filePath, fileName), xdelta);

        // find correct success message

        if (!allChapters)
        {
            menu.MessagePopup(PopupType.Success, message);
        }
        else
        {
            menu.AddText(message[0], Alignment.Center, ConsoleColor.Yellow);
        }
    }
}