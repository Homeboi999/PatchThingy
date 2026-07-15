using System.Diagnostics;

namespace TestThingy.Widget;

class LogWidget : Widget
{
    List<(string text, MessageType type)> content = [];
    public int logLines => content.Count;
    int scrollAmount = 0;
    public int height;

    public LogWidget (int height)
    {
        this.height = height;
    }

    public override int LineCount => height;

    public override void Draw(DrawContext box, int line)
    {
        int x = box.AlignPosition(Alignment.Left, 6);
        int y = box.y + line;
        int scrollPos = y + LineCount - 1;

        // Draw content line-by-line
        // from the bottom up, so that
        // the most recent entry is index 0
        for (int curLine = 0; curLine < height; curLine++)
        {
            // Read contents of the line
            int index = curLine + scrollAmount;
            string lineText = GetLineText(index);

            // If there's no text, end here.
            if (lineText.Length == 0)
            {
                continue;
            }

            DrawContext.MoveCursor(x - 3, scrollPos - curLine);

            // Set color and Add symbols based on type
            switch (GetLineType(index))
            {
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("!");
                    break;

                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("X");
                    break;

                case MessageType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("✓");
                    break;
            }

            DrawContext.MoveCursor(x, scrollPos - curLine);
            Console.Write(lineText);
            Console.ResetColor();
        }

        // Draw arrows if there are entries past what's shown
        if (GetLineText(scrollAmount - 1).Length > 0)
        {
            DrawContext.MoveCursor(x - 3, scrollPos);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("▼");
            Console.ResetColor();
        }

        if (GetLineText(scrollAmount + height).Length > 0)
        {
            DrawContext.MoveCursor(x - 3, y);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("▲");
            Console.ResetColor();
        }
    }

    // Convenience
    string GetLineText(int index)
    {
        try
        {
            // Try to read content at index
            return content[index].text;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Return an empty string
            return "";
        }
    }

    // Convenience
    MessageType GetLineType(int index)
    {
        try
        {
            // Try to read type at index
            return content[index].type;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Use default type if OoR
            return MessageType.None;
        }
    }

    // TODO: once i have FocusableWidget parent with
    // OnKeyInput, use that instead of hardcoding it
    // in every page with a LogWidget and a ChoicerWidget
    //
    // Scrolling
    public void Scroll(int offset)
    {
        int newScroll = scrollAmount + offset;
        scrollAmount = Math.Clamp(newScroll, 0, Math.Max(content.Count - height, 0));
    }

    // Text Management
    public void Add(string text, MessageType type = MessageType.None)
    {
        content.Insert(0, (text, type));

        // If scrolled up, maintain position
        if (scrollAmount != 0)
        {
            scrollAmount++;
        }
    }
    public void Clear()
    {
        scrollAmount = 0;
        content.Clear();
    }
}