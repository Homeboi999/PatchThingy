namespace TestThingy.Widget;

class ChoicerWidget : IWidget
{
    ChoicerType type;
    IReadOnlyList<string> choices = [];

    public int curSelection = 0;
    public bool chosen = false;
    public bool focused = true;
    public bool visible = true;

    public ChoicerWidget (IReadOnlyList<string> choices, ChoicerType type = ChoicerType.Grid)
    {
        this.choices = choices;
        this.type = type;
    }

    // LineCount changes based on the
    // # of choices and the ChoicerType
    public int LineCount
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

    public void Draw(DrawContext box, int line)
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

    public ChoicerResult OnKeyInput(ConsoleKey inputKey)
    {
        switch (inputKey)
        {
            // Confirm
            case ConsoleKey.Z:
            case ConsoleKey.Enter:
                return ChoicerResult.Confirm;

            // Cancel
            case ConsoleKey.X:
            case ConsoleKey.Escape:
                return ChoicerResult.Cancel;

            // Move cursor
            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
                ChangeSelection(inputKey);
                return ChoicerResult.Waiting;
            
            // Do nothing
            default:
                return ChoicerResult.Waiting;
        }
    }

    void WriteChoice (string text, int x, int y, bool selected)
    {
        if (focused && !chosen && selected)
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