// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Text.Json;
using TestThingy.Data;
using TestThingy.Pages;

// ────────────────────────────────────────────────────────────
// Misc. Setup
// ────────────────────────────────────────────────────────────

// Bind Ctrl+C to ExitMenu so it
// doesn't fuck up the console.
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

// ────────────────────────────────────────────────────────────
// Main Functionality
// ────────────────────────────────────────────────────────────

Console.Write("\x1b[?1049h"); // Enable Alternate Buffer
Console.CursorVisible = false;

try
{
    MainChapterPage startPage = new MainChapterPage();
    startPage.RunLoop();
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

    WriteException(error);
    ExitMenu(2);
}

// after exiting, exit (lmao)
ExitMenu();

// ────────────────────────────────────────────────────────────
// Exit Functions
// ────────────────────────────────────────────────────────────

void ExitMenu(int exitCode = 0)
{
    Console.Write("\x1b[?1049l"); // main screen
    Console.CursorVisible = true;
    Environment.Exit(exitCode);
}

void WriteException(Exception error)
{
    Console.Write("\x1b[?1049l"); // main screen
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine($"   PatchThingy v{Page.versionNum}");
    
    foreach (string line in error.ToString().Split("\n"))
    {
        Console.WriteLine(line);
        Console.ForegroundColor = ConsoleColor.DarkGray;
    }

    Console.ResetColor();
}