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
    (int width, int height) Size
    {
        get
        {
            return (Math.Min(maxWidth, Console.BufferWidth - 2), Math.Max(minHeight, WidgetLine(MenuWidgets.Count)));
        }
    }

    int maxWidth;
    int minHeight;
    int margin;

    // Constructor
    public ConsoleMenu(int boxWidth, int boxHeight, int boxMargins = 1)
    {
        margin = boxMargins;
        maxWidth = boxWidth;
        minHeight = boxHeight;
    }

    // menu content setup
    interface IWidget
    {
        //TODO: this is dumb
        public int LineCount();
        public void Draw(ConsoleMenu menu, int line);
    }

    List<IWidget> MenuWidgets = [];

    class TextWidget : IWidget
    {
        string content;
        ConsoleColor? color = null;
        Alignment align;

        public TextWidget (string content, Alignment align = Alignment.Left)
        {
            this.content = content;
            this.align = align;
        }

        public void SetText (string content)
        {
            this.content = content;
        }

        public void SetColor(ConsoleColor? color)
        {
            this.color = color;
        }

        public void SetAlignment(Alignment align)
        {
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

    // Add a text widget to the menu
    public void AddText (string content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        TextWidget newText = new(content);
        newText.SetAlignment(align);
        newText.SetColor(color);
        MenuWidgets.Add(newText);
    }
    public void InsertText (int index, string content, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        TextWidget newText = new(content);
        newText.SetAlignment(align);
        newText.SetColor(color);
        MenuWidgets.Insert(index, newText);
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

    // Remove all widgets from the list.
    public void RemoveAll()
    {
        MenuWidgets.Clear();
    }

    // remove a widget from the menu
    public void RemoveWidget(int index)
    {
        if (0 <= index && index < MenuWidgets.Count)
        {
            MenuWidgets.RemoveAt(index);
        }
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
        var boxX = (Console.BufferWidth - maxWidth) / 2 - 1;
        var boxY = (Console.BufferHeight - minHeight) / 2 - 1;
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