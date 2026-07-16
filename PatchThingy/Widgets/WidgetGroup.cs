namespace PatchThingy.Widgets;

public class WidgetGroup : Widget
{
    List<Widget> widgets = [];
    public bool visible;

    public WidgetGroup(bool visible = false)
    {
        this.visible = visible;
    }

    public override void Draw(DrawContext box, int line)
    {
        if (visible)
        {
            int curLine = line;

            foreach (Widget widget in widgets)
            {
                widget.Draw(box, curLine);
                curLine += widget.LineCount;
            }   
        }
    }

    public override int LineCount
    {
        get
        {
            int lineTotal = 0;
            
            if (visible)
            {
                foreach (Widget widget in widgets)
                {
                    lineTotal += widget.LineCount;
                }   
            }

            return lineTotal;
        }
    }

    // Add a widget to the group
    public void AddWidget (Widget newWidget)
    {
        widgets.Add(newWidget);
    }
    public void InsertWidget (int index, Widget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets.Insert(index, newWidget);
    }
    public void ReplaceWidget(int index, Widget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets[index] = newWidget;
    }
}