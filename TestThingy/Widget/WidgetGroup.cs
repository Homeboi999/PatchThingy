namespace TestThingy.Widget;

public class WidgetGroup : IWidget
{
    List<IWidget> widgets = [];
    public bool visible;

    public WidgetGroup(bool visible = false)
    {
        this.visible = visible;
    }

    public void Draw(DrawContext box, int line)
    {
        if (visible)
        {
            int curLine = line;

            foreach (IWidget widget in widgets)
            {
                widget.Draw(box, curLine);
                curLine += widget.LineCount;
            }   
        }
    }

    public int LineCount
    {
        get
        {
            int lineTotal = 0;
            
            if (visible)
            {
                foreach (IWidget widget in widgets)
                {
                    lineTotal += widget.LineCount;
                }   
            }

            return lineTotal;
        }
    }

    // Add a widget to the group
    public void AddWidget (IWidget newWidget)
    {
        widgets.Add(newWidget);
    }
    public void InsertWidget (int index, IWidget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets.Insert(index, newWidget);
    }
    public void ReplaceWidget(int index, IWidget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets[index] = newWidget;
    }
}