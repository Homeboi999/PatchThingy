// See https://aka.ms/new-console-template for more information
using TestThingy;
using TestThingy.Page;

Console.Write("\x1b[?1049h"); // Enable Alternate Buffer
Console.Write("\x1b[?25l"); // Hide Cursor

ChaptersPage startPage = new ChaptersPage();
startPage.RunLoop();

Console.Write("\x1b[?1049l"); // Disable Alternate Buffer
Console.Write("\x1b[?25h"); // Show Cursor