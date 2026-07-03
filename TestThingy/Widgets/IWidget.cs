namespace TestThingy.Widget;

interface IWidget
{
    public void Draw(DrawContext box, int line);
    public int LineCount { get; }
}