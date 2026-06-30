
namespace TestThingy;

class TestSquare : TestInterface
{
    (int X, int Y) pos = (0, 0);

    public void OnKeyInput(ConsoleKey inputKey)
    {
        switch (inputKey)
        {
            case ConsoleKey.LeftArrow:
                pos.X = Math.Max(pos.X - 2, 0);
                break;
            
            case ConsoleKey.RightArrow:
                pos.X = Math.Min(pos.X + 2, Console.BufferWidth - 1);
                break;
            
            case ConsoleKey.UpArrow:
                pos.Y = Math.Max(pos.Y - 1, 0);
                break;
            
            case ConsoleKey.DownArrow:
                pos.Y = Math.Min(pos.Y + 1, Console.BufferHeight - 1);
                break;
        }
    }

    public void Draw()
    {
        Console.Clear();
        Console.SetCursorPosition(pos.X, pos.Y);
        Console.Write("🟥");
    }
}