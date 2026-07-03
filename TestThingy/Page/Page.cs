using TestThingy.Widget;

namespace TestThingy;

abstract class Page
{
    protected readonly PageManager manager;
    List<IWidget> widgets = [];

    public Page(PageManager manager)
    {
        this.manager = manager;
    }

    abstract public int MaxWidth { get; }

    public abstract void OnKeyInput(ConsoleKey inputKey);
    
    public void Draw()
    {
        Console.Write("\x1b[?2026h"); // stop display
        Console.Clear();
        var boxWidth = Math.Min(MaxWidth, Console.BufferWidth - 2);
        var boxHeight = Math.Max(0, widgets.Select(widget => widget.LineCount).Sum() - 1);
        var boxX = Math.Clamp((Console.BufferWidth - boxWidth) / 2 - 1, 0, Math.Max(Console.BufferWidth - 1, 0));
        var boxY = Math.Max((Console.BufferHeight - boxHeight) / 2 - 1, 0);

        // update box position, without going OoB
        DrawContext box = new DrawContext(boxX, boxY, boxWidth, boxHeight);

        // assemble box as single string
        // so it writes instantly
        DrawContext.MoveCursor(0, box.y);
        
        string boxString = "";
        boxString += box.AssembleRow('┏', '━', '┓');
        for (int i = 0; i < box.height; i++)
        {
            boxString += box.AssembleRow('┃', ' ', '┃');
        }
        boxString += box.AssembleRow('┗', '━', '┛');

        // draw box to screen
        Console.Write(boxString);

        // Potential future version
        int line = 0;
        foreach (IWidget widget in widgets)
        {
            widget.Draw(box, line);
            line += widget.LineCount;
        }

        Console.Write("\x1b[?2026l"); // start display
    }

    // Add a text widget to the menu
    public void AddWidget (IWidget newWidget)
    {
        widgets.Add(newWidget);
    }
    public void InsertWidget (int index, IWidget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets.Insert(index, newWidget);
    }
    public void ReplaceWidget(int index, IWidget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets[index] = newWidget;
    }
}