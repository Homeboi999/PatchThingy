using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

// One class to manage all changes to the data.win
//
// This file contains functions that help format
// the console output from the apply/generate functions.
partial class DataHandler
{
    static int fileCount = 0; // count of completed files

    static void WriteProgress(string fileType)
    {
        fileCount++;
        Console.Write($"{fileType} - {fileCount}\r");
        // Code - Code Patches: 24
    }

    static void ResetProgress(bool retainCount = false)
    {
        if (!retainCount)
        {
            fileCount = 0;
        }
        // Advance to the next line between sections.
        Console.WriteLine();
    }
}