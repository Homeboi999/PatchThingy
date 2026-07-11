namespace TestThingy.Widget;

class TextWidget : Widget
{
    List<string> content;
    public ConsoleColor? color;
    public Alignment align;

    public TextWidget (IEnumerable<string> content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        this.content = content.ToList();
        this.color = color;
        this.align = align;
    }

    public override int LineCount => content.Count;

    public override void Draw(DrawContext box, int line)
    {
        int x = 0;
        int y = box.y + line;

        for (int curLine = 0; curLine < content.Count; curLine++)
        {
            switch (align)
            {
                case Alignment.Left:
                    x = box.AlignPosition(align);
                    break;

                case Alignment.Center:
                    x = box.AlignPosition(align) - (content[curLine].Length / 2);
                    break;

                case Alignment.Right:
                    x = box.AlignPosition(align) - content[curLine].Length;
                    break;
            }

            DrawContext.MoveCursor(x, y + curLine);

            if (color is not null)
            {
                Console.ForegroundColor = color ?? ConsoleColor.White;
            }

            Console.Write(content[curLine]);
            Console.ResetColor();
        }
    }

    public void AddLine(string text)
    {
        content.Add(text);
    }
    public void RemoveLine(int index)
    {
        if (index >= 0 && index < content.Count)
        {
            content.RemoveAt(index);
        }
    }
    public void Clear()
    {
        content.Clear();
    }
}