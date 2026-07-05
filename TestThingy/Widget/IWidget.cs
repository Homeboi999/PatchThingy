namespace TestThingy.Widget;

public interface IWidget
{
    public void Draw(DrawContext box, int line);
    public int LineCount { get; }
}