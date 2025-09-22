using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using ImageMagick;

// Class responisble for drawing the PatchThingy menus
public partial class ConsoleMenu
{
    public (int x, int y) Pos;

    // box size, accounting for resizing
    public (int width, int height) Size
    {
        get
        {
            return (Math.Min(maxWidth, Console.BufferWidth - 2), Math.Max(0, WidgetLine(MenuWidgets.Count) - 1));
        }
    }

    int maxWidth;
    int margin;

    // Constructor
    public ConsoleMenu(int boxWidth, int boxMargins = 1)
    {
        margin = boxMargins;
        maxWidth = boxWidth;
    }

    // change the target size of the box
    public void ResizeBox(int width)
    {
        maxWidth = width;
    }
    public void ResizeBox(int width, int margin)
    {
        maxWidth = width;
        this.margin = margin;
    }

    // menu content setup
    interface IWidget
    {
        public void Draw(ConsoleMenu menu, int line);
        public int LineCount();
    }

    List<IWidget> MenuWidgets = [];

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

        public void Draw(ConsoleMenu menu, int line)
        {
            int x = 0;
            int y = menu.Pos.y + line;

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

            MoveCursor(x, y);

            if (color is not null)
            {
                Console.ForegroundColor = color ?? ConsoleColor.White;
            }

            Console.Write(content);
            Console.ResetColor();
        }
        
        public int LineCount()
        {
            return 1;
        }
    }

    class SeparatorWidget : IWidget
    {
        bool Visible;

        public SeparatorWidget (bool Visible)
        {
            this.Visible = Visible;
        }

        public void Draw(ConsoleMenu menu, int line)
        {
            if (Visible)
            {
                MoveCursor(0, menu.Pos.y + line);

                string sepString = menu.AssembleRow('┠', '─', '┨');

                Console.Write(sepString);
            }
        }
        
        public int LineCount()
        {
            return 1;
        }
    }

    // TODO: implement this. making it work again first
    class LogWidget : IWidget
    {
        public int size;
        public int index = 0;
        public List<string> content = [];
        public Alignment align;

        public LogWidget (int size, Alignment align = Alignment.Left)
        {
            this.size = size;
            this.align = align;
        }

        public void Draw(ConsoleMenu menu, int line)
        {
            int x = 0;
            int y = menu.Pos.y + line;

            // assemble array of strings
            // from content starting at index
            //
            // draw each string with alignment code here
            //
            // if indexing OoB, blank string
            switch (align)
            {
                case Alignment.Left:
                    x = menu.AlignPosition(align);
                    break;

                case Alignment.Center:
                    x = menu.AlignPosition(align) - (content[0].Length / 2);
                    break;

                case Alignment.Right:
                    x = menu.AlignPosition(align) - content[0].Length;
                    break;
            }

            MoveCursor(x, y);
            Console.Write(content);
        }
        
        public int LineCount()
        {
            return 1;
        }
    }

    // Add a text widget to the menu
    public void AddText (string content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        TextWidget newText = new(content);
        newText.align = align;
        newText.color = color;
        MenuWidgets.Add(newText);
    }
    public void InsertText (int index, string content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        if (index > MenuWidgets.Count || index < 0)
        {
            return;
        }

        TextWidget newText = new(content);
        newText.align = align;
        newText.color = color;
        MenuWidgets.Insert(index, newText);
    }
    public void ReplaceText(int index, string content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        if (index > MenuWidgets.Count || index < 0)
        {
            return;
        }

        TextWidget newText = new(content);
        newText.align = align;
        newText.color = color;
        MenuWidgets[index] = newText;
    }

    public void SetText (int index, string content)
    {
        TextWidget widget = (TextWidget)MenuWidgets[index];

        if (widget is not null)
        {
            widget.content = content;
        }
    }
    public void SetAlignment(int index, Alignment align)
    {
        TextWidget widget = (TextWidget)MenuWidgets[index];

        if (widget is not null)
        {
            widget.align = align;
        }
    }
    public void SetColor(int index, ConsoleColor? color)
    {
        TextWidget widget = (TextWidget)MenuWidgets[index];

        if (widget is not null)
        {
            widget.color = color;
        }
    }

    // Adds a separator to the menu
    public void AddSeparator(bool visible = true)
    {
        MenuWidgets.Add(new SeparatorWidget(visible));
    }
    public void InsertSeparator(int index, bool visible = true)
    {
        // adjust the line number of all subsequent widgets

        MenuWidgets.Insert(index, new SeparatorWidget(visible));
    }
    public void ReplaceSeparator(int index, bool visible = true)
    {
        // adjust the line number of all subsequent widgets

        MenuWidgets[index] = new SeparatorWidget(visible);
    }

    // Remove all widgets from the menu.
    public void RemoveAll()
    {
        MenuWidgets.Clear();
    }

    // remove one or more widgets from the menu
    public void Remove(int index)
    {
        index = Math.Clamp(index, 0, MenuWidgets.Count - 1);
        MenuWidgets.RemoveAt(index);
    }
    public void Remove(int start, int end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, MenuWidgets.Count - 1);

        MenuWidgets.RemoveRange(Math.Min(start, end), Math.Abs(end - start) + 1);
    }

    int WidgetLine(int index)
    {
        int line = 0;

        for (int i = 0; i < index && i < MenuWidgets.Count; i++)
        {
            line += MenuWidgets[i].LineCount();
        }

        return line;
    }

    // assembles the entire box, aligned correctly,
    // as a single string and draws it all at once
    public void Draw()
    {
        Console.Write("\x1b[?2026h"); // stop display
        Console.Clear();

        // update box position, without going OoB
        var boxX = (Console.BufferWidth - Size.width) / 2 - 1;
        var boxY = (Console.BufferHeight - Size.height) / 2 - 1;
        Pos.x = Math.Clamp(boxX, 0, Math.Max(Console.BufferWidth - 1, 0)); // have to do max so width of 0 doesnt crash
        Pos.y = Math.Max(boxY, 0); // taller is fine

        // assemble box as single string
        // so it writes instantly
        MoveCursor(0, Pos.y);
        
        string boxString = "";
        boxString += AssembleRow('┏', '━', '┓');
        for (int i = 0; i < Size.height; i++)
        {
            boxString += AssembleRow('┃', ' ', '┃');
        }
        boxString += AssembleRow('┗', '━', '┛');

        // draw box to screen
        Console.Write(boxString);
        Console.Write("\x1b[?2026l"); // start display

        // Potential future version
        int line = 0;
        foreach (IWidget widget in MenuWidgets)
        {
            widget.Draw(this, line);
            line += widget.LineCount();
        }
    }

    // write 1 row of the box to a string
    string AssembleRow(char left, char middle, char right, bool end = false)
    {
        // init string
        string row = "";

        // add spacing according to box position
        for (int i = 0; i < Pos.x; i++)
        {
            row += " ";
        }

        // first char in row
        row += left;

        // fill row with characters until long enough
        // or the edge of the screen has been reached
        for (int i = 0; i < Size.width; i++)
        {
            row += middle;
        }

        // last char in row
        row += right;

        // new line
        if (!end)
        {
            row += "\n";
        }

        // return string to be added to box
        return row;
    }

    // function to align the cursor along the current line
    // in preparation to write text of a given size
    public int AlignPosition(Alignment align)
    {
        int destX = 0;

        switch (align)
        {
            case Alignment.Left:
                destX = Pos.x + 1 + margin;
                break;

            case Alignment.Center:
                destX = Pos.x + 1 + (Size.width / 2);
                break;

            case Alignment.Right:
                destX = Pos.x + 1 + Size.width - margin;
                break;
        }

        // set cursor position along the current line
        return destX;
    }

    // set the cursor position without going OoB
    public static void MoveCursor(int x, int y)
    {
        // have to do max so debugger doesnt crash
        int destX = Math.Clamp(x, 0, Math.Max(Console.BufferWidth - 1, 0));
        int destY = Math.Clamp(y, 0, Math.Max(Console.BufferHeight - 1, 0));
        Console.SetCursorPosition(destX, destY);
    }
}

public enum Alignment
{
    Left,
    Center,
    Right,
}