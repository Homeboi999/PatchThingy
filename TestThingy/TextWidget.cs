namespace TestThingy;

class TextWidget : IWidget
{
    public string content;
    public ConsoleColor? color = null;
    public Alignment align;

    public TextWidget (string content, Alignment align = Alignment.Left)
    {
        this.content = content;
        this.align = align;
    }

    public int LineCount => 1;

    public void Draw(DrawContext menu, int line)
    {
        int x = 0;
        int y = menu.y + line;

        switch (align)
        {
            case Alignment.Left:
                x = menu.AlignPosition(align);
                break;

            case Alignment.Center:
                x = menu.AlignPosition(align) - (content.Length / 2);
                break;

            case Alignment.Right:
                x = menu.AlignPosition(align) - content.Length;
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