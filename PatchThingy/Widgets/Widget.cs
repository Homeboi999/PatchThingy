namespace PatchThingy.Widgets;

public abstract class Widget
{
    public bool focused = false;
    public abstract void Draw(DrawContext box, int line);
    public abstract int LineCount { get; }
}