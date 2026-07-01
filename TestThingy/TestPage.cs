
namespace TestThingy;

class TestPage : Page
{
    override public int MaxWidth => 60;
    (int X, int Y) pos = (0, 0);

    public TestPage(PageManager manager) : base(manager)
    {
        AddText("Test Page!!", Alignment.Center);

        AddSeparator(visible: false);
        AddSeparator(visible: true);
        AddSeparator(visible: false);
        AddText("wowie!", Alignment.Center);

    }

    override public void OnKeyInput(ConsoleKey inputKey)
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
}