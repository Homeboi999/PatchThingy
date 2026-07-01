namespace TestThingy;

class SeparatorWidget : IWidget
{
    bool Visible;

    public SeparatorWidget (bool Visible)
    {
        this.Visible = Visible;
    }

    public int LineCount => 1;

    public void Draw(DrawContext box, int line)
    {
        if (Visible)
        {
            DrawContext.MoveCursor(0, box.y + line);

            string sepString = box.AssembleRow('┠', '─', '┨');

            Console.Write(sepString);
        }
    }
}