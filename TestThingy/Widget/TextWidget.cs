namespace TestThingy.Widget;

class TextWidget : IWidget
{
    public string content;
    public ConsoleColor? color;
    public Alignment align;

    public TextWidget (string content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        this.content = content;
        this.color = color;
        this.align = align;
    }

    public int LineCount => 1;

    public void Draw(DrawContext box, int line)
    {
        int x = 0;
        int y = box.y + line;

        switch (align)
        {
            case Alignment.Left:
                x = box.AlignPosition(align);
                break;

            case Alignment.Center:
                x = box.AlignPosition(align) - (content.Length / 2);
                break;

            case Alignment.Right:
                x = box.AlignPosition(align) - content.Length;
                break;
        }

        DrawContext.MoveCursor(x, y);

        if (color is not null)
        {
            Console.ForegroundColor = color ?? ConsoleColor.White;
        }

        Console.Write(content);
        Console.ResetColor();
    }
}