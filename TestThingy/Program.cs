// See https://aka.ms/new-console-template for more information
using TestThingy;

Console.Write("\x1b[?1049h"); // Enable Alternate Buffer
Console.Write("\x1b[?25l"); // Hide Cursor

PageManager pageManager = new PageManager();
pageManager.AddPage(new TestHeart(pageManager));

while (true)
{
    if (pageManager.IsEmpty)
    {
        break;
    }

    ConsoleKeyInfo input = Console.ReadKey(true);
    
    pageManager.OnKeyInput(input.Key);
    pageManager.DrawPage();
}

Console.Write("\x1b[?1049l"); // Disable Alternate Buffer
Console.Write("\x1b[?25h"); // Show Cursor