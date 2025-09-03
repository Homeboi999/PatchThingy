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
    public int x;
    public int y;
    int width;
    int height;
    int margin;

    // Constructor
    public ConsoleMenu(int boxWidth, int boxHeight, int boxMargins = 1)
    {
        margin = boxMargins;
        width = boxWidth;
        height = boxHeight;
    }

    // menu content setup
    interface IWidget
    {
        public int GetLine();
        public int LineCount();
        public void Draw(ConsoleMenu menu);
    }

    List<IWidget> MenuWidgets = [];

    public class MenuText : IWidget
    {
        string content;
        ConsoleColor? color = null;
        Alignment align;

        int line;

        public MenuText (int line, string content, Alignment align = Alignment.Left)
        {
            this.content = content;
            this.align = align;
            this.line = line;
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

        public void Draw(ConsoleMenu menu)
        {
            int x = 0;
            int y = menu.y + 1 + line;

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

        // functions to keep track of space
        public int GetLine()
        {
            return line;
        }
        
        public int LineCount()
        {
            return 1;
        }
    }

    public class MenuSeparator : IWidget
    {
        int line;

        public MenuSeparator (int line)
        {
            this.line = line;
        }

        public void Draw(ConsoleMenu menu)
        {
            MoveCursor(0, menu.y + 1 + line);

            string sepString = menu.AssembleRow('┠', '─', '┨');

            Console.Write(sepString);
        }

        public int GetLine()
        {
            return line;
        }
        
        public int LineCount()
        {
            return 1;
        }
    }

    // Remove a specific line from the list.
    public void ClearLine(int line)
    {
        for (int i = 0; i < MenuWidgets.Count; i++)
        {
            if (MenuWidgets[i].GetLine() == line)
            {
                MenuWidgets.RemoveAt(i);
            }
            else if (MenuWidgets[i].GetLine() > line && MenuWidgets[i].GetLine() + MenuWidgets[i].LineCount() <= i)
            {
                MenuWidgets.RemoveAt(i);
            }
        }
    }

    // Remove all lines from the list.
    public void ClearAll()
    {
        MenuWidgets.Clear();
    }

    // Adds a new text line with the chosen parameters
    public void AddText (string content, int line, Alignment align = Alignment.Left, ConsoleColor? color = null)
    {
        MenuText newText = new(line, content);
        newText.SetAlignment(align);
        newText.SetColor(color);
        MenuWidgets.Add(newText);
    }
    public void AddText (MenuText newText)
    {
        MenuWidgets.Add(newText);
    }
    
    // turns the chosen line into a separator
    public void AddSeparator (int line)
    {
        MenuWidgets.Add(new MenuSeparator(line));
    }

    // assembles the entire box, aligned correctly,
    // as a single string and draws it all at once
    public void Draw()
    {
        Console.Write("\x1b[?2026h"); // stop display
        Console.Clear();
        
        // update box position, without going OoB
        var boxX = (Console.BufferWidth - width) / 2 - 1;
        var boxY = (Console.BufferHeight - height) / 2 - 1;
        x = Math.Clamp(boxX, 0, Console.BufferWidth - 1);
        y = Math.Max(boxY, 0); // taller is fine

        // assemble box as single string
        // so it writes instantly
        MoveCursor(0, y);
        string boxString = "";
        boxString += AssembleRow('┏', '━', '┓');
        for (int i = 0; i < height; i++)
        {
            boxString += AssembleRow('┃', ' ', '┃');
        }
        boxString += AssembleRow('┗', '━', '┛');

        // draw box to screen
        Console.Write(boxString);
        Console.Write("\x1b[?2026l"); // start display

        foreach (IWidget widget in MenuWidgets)
        {
            widget.Draw(this);
        }
    }

    string AssembleRow(char left, char middle, char right)
    {
        // init string
        string row = "";

        // add spacing according to box position
        for (int i = 0; i < x && x < Console.BufferWidth - 2; i++)
        {
            row += " ";
        }

        // first char in row
        row += left;

        // fill row with characters until long enough
        // or the edge of the screen has been reached
        for (int i = 0; i < width && x + i < Console.BufferWidth - 2; i++)
        {
            row += middle;
        }

        // last char in row, and make a new line
        row += right + "\n";

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
                destX = x + 1 + margin;
                break;

            case Alignment.Center:
                destX = x + 1 + (width / 2);
                break;

            case Alignment.Right:
                destX = x + 1 + width - margin;
                break;
        }

        // set cursor position along the current line
        return destX;
    }

    // set the cursor position without going OoB
    public static void MoveCursor(int x, int y)
    {
        int destX = Math.Clamp(x, 0, Console.BufferWidth - 1);
        int destY = Math.Clamp(y, 0, Console.BufferHeight - 1);
        Console.SetCursorPosition(destX, destY);
    }
}

public enum Alignment
{
    Left,
    Center,
    Right,
}