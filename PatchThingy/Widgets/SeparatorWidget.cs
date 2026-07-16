namespace PatchThingy.Widgets;

class SeparatorWidget : Widget
{
    bool visible;

    public SeparatorWidget (bool visible)
    {
        this.visible = visible;
    }

    public override int LineCount => 1;

    public override void Draw(DrawContext box, int line)
    {
        if (visible)
        {
            DrawContext.MoveCursor(0, box.y + line);

            string sepString = box.AssembleRow('┠', '─', '┨');

            Console.Write(sepString);
        }
    }
}