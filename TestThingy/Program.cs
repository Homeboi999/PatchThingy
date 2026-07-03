// See https://aka.ms/new-console-template for more information
using TestThingy;
using TestThingy.Page;

Console.Write("\x1b[?1049h"); // Enable Alternate Buffer
Console.Write("\x1b[?25l"); // Hide Cursor

PageManager pageManager = new PageManager();
pageManager.AddPage(new ChaptersPage(pageManager));

while (true)
{
    if (pageManager.IsEmpty)
    {
        break;
    }

    pageManager.DrawPage();

    ConsoleKeyInfo input = Console.ReadKey(true);    
    pageManager.OnKeyInput(input.Key);
}

Console.Write("\x1b[?1049l"); // Disable Alternate Buffer
Console.Write("\x1b[?25h"); // Show Cursor