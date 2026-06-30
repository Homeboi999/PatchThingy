// See https://aka.ms/new-console-template for more information
using TestThingy;

Console.Write("\x1b[?1049h"); // Enable Alternate Buffer
Console.Write("\x1b[?25l"); // Hide Cursor

while (true)
{
    ConsoleKeyInfo input = Console.ReadKey(true);

    if (input.Key == ConsoleKey.Escape)
    {
        break;
    }

    TestHeart.OnKeyInput(input.Key);
    TestHeart.Draw();
}

Console.Write("\x1b[?1049l"); // Disable Alternate Buffer
Console.Write("\x1b[?25h"); // Show Cursor