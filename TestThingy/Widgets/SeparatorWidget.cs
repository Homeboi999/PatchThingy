namespace TestThingy.Widget;

class SeparatorWidget : IWidget
{
    bool visible;

    public SeparatorWidget (bool visible)
    {
        this.visible = visible;
    }

    public int LineCount => 1;

    public void Draw(DrawContext box, int line)
    {
        if (visible)
        {
            DrawContext.MoveCursor(0, box.y + line);

            string sepString = box.AssembleRow('┠', '─', '┨');

            Console.Write(sepString);
        }
    }
}