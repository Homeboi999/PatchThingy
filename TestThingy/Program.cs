// See https://aka.ms/new-console-template for more information
using TestThingy;

Console.Write("\x1b[?1049h"); // Enable Alternate Buffer
Console.Write("\x1b[?25l"); // Hide Cursor

int index = 0;
TestInterface[] hearts = 
[
    new TestHeart(),
    new TestSquare()
];

while (true)
{
    ConsoleKeyInfo input = Console.ReadKey(true);

    if (input.Key == ConsoleKey.Escape)
    {
        break;
    }

    if (input.Key == ConsoleKey.Z)
    {
        index = (index + 1) % 2;
    }

    hearts[index].OnKeyInput(input.Key);
    hearts[index].Draw();
}

Console.Write("\x1b[?1049l"); // Disable Alternate Buffer
Console.Write("\x1b[?25h"); // Show Cursor