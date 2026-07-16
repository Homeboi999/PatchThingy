namespace PatchThingy.Widgets;

public delegate void ChoicerEventHandler(ChoicerWidget sender, ChoicerEventArgs e);

public class ChoicerEventArgs : EventArgs
{
    public int choice { get; set; }
}