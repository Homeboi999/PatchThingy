namespace TestThingy.Widget;

public class ChoicerWidget : Widget
{
    ChoicerType type;
    IReadOnlyList<string> choices = [];

    public int curSelection = 0;
    public bool chosen = false;
    public bool visible = true;

    public ChoicerWidget (IReadOnlyList<string> choices, ChoicerType type = ChoicerType.Grid)
    {
        this.choices = choices;
        this.type = type;
    }

    public event ChoicerEventHandler? Confirmed = null;
    public event EventHandler? Cancelled = null;

    // LineCount changes based on the
    // # of choices and the ChoicerType
    public override int LineCount
    {
        get
        {
            if (!visible)
            {
                return 0;
            }

            switch(type)
            {
                case ChoicerType.List:
                    return choices.Count;

                case ChoicerType.Grid:
                    return choices.Count / 2 + (choices.Count % 2);
                    
                default:
                    return 1;
            }
        }
    }

    public override void Draw(DrawContext box, int line)
    {
        if (!visible)
        {
            return;
        }

        int[] columnPos = box.AlignColumns();
        int startLine = box.y + line;

        switch (type)
        {
            case ChoicerType.List:
                for (int i = 0; i < choices.Count; i++)
                {
                    WriteChoice(choices[i], columnPos[0], startLine + i, curSelection == i);
                }
                break;

            case ChoicerType.Grid:
                for (int i = 0; i < choices.Count; i++)
                {
                    WriteChoice(choices[i], columnPos[i % 2], startLine + (i / 2), curSelection == i);
                }
                break;
        }
    }

    public void OnKeyInput(ConsoleKey inputKey)
    {
        switch (inputKey)
        {
            // Confirm
            case ConsoleKey.Z:
            case ConsoleKey.Enter:
                chosen = true;
                Confirmed?.Invoke(this, new() { choice = curSelection });
                break;

            // Cancel
            case ConsoleKey.X:
            case ConsoleKey.Escape:
                Cancelled?.Invoke(this, new());
                break;

            // Move cursor
            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
                ChangeSelection(inputKey);
                break;
            
            // Do nothing
            default:
                break;
        }
    }

    void WriteChoice (string text, int x, int y, bool selected)
    {
        if (focused && selected)
        {
            // move heart
            DrawContext.MoveCursor(x - 3, y);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("♥️");
            Console.ResetColor();
        }
        else
        {
            // clear heart
            DrawContext.MoveCursor(x - 3, y);
            Console.Write("  ");
        }

        if (chosen && selected)
        {
            // set color
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        
        // draw text at position
        DrawContext.MoveCursor(x, y);
        Console.Write(text);

        // reset color
        Console.ResetColor();
    }

    void ChangeSelection(ConsoleKey directionKey)
    {
        switch (directionKey)
        {
            // UP/DOWN: only wrap on List
            // do nothing on InLine
            case ConsoleKey.UpArrow:
                if (type == ChoicerType.List)
                {
                    if (curSelection - 1 < 0)
                    {
                        curSelection = choices.Count - 1;
                    }
                    else
                    {
                        curSelection--;
                    }
                }
                else if (type == ChoicerType.Grid)
                {
                    if (curSelection - 2 >= 0)
                    {
                        curSelection -= 2;
                    }
                }
                break;

            case ConsoleKey.DownArrow:
                if (type == ChoicerType.List)
                {
                    if (curSelection + 1 >= choices.Count)
                    {
                        curSelection = 0;
                    }
                    else
                    {
                        curSelection++;
                    }
                }
                else if (type == ChoicerType.Grid)
                {
                    if (curSelection + 2 < choices.Count)
                    {
                        curSelection += 2;
                    }
                }
                break;

            // LEFT/RIGHT: dont move right from end of list
            // do nothing on List
            case ConsoleKey.LeftArrow:
                if (type != ChoicerType.List)
                {
                    if (curSelection % 2 == 1)
                    {
                        curSelection--;
                    }
                }
                break;

            case ConsoleKey.RightArrow:
                if (type != ChoicerType.List)
                {
                    if (curSelection % 2 == 0 && curSelection + 1 < choices.Count)
                    {
                        curSelection++;
                    }
                }
                break;

            // other keys are ignored
            default:
                break;
        }
    }
}