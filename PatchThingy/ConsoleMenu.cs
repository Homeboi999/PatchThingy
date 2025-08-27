using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using ImageMagick;

public class ConsoleMenu
{
    int pos; // space from left edge
    public List<MenuLine> lines = [];

    // Constructor
    public ConsoleMenu(int boxWidth, int boxHeight, int offsetCol)
    {
        pos = offsetCol;

        lines.Add(new MenuLine(LineType.Top, boxWidth, offsetCol));

        for (int i = 0; i < boxHeight; i++)
        {
            lines.Add(new MenuLine(LineType.Body, boxWidth, offsetCol));
        }

        lines.Add(new MenuLine(LineType.Bottom, boxWidth, offsetCol));
    }

    // Display lines
    public void DrawAllLines()
    {
        Console.Clear();
        Console.CursorVisible = false;

        foreach (MenuLine line in lines)
        {
            line.Draw();
        }
    }
    public void DrawLine(int line)
    {
        Console.SetCursorPosition(0, line);
        lines[line].Draw();
    }
    public void ClearAll()
    {
        // for loop intentionally skips
        // the first and last lines
        for (int i = 1; i < lines.Count - 1; i++)
        {
            // reset line to empty
            lines[i].SetText("");
            lines[i].SetType(LineType.Body);
            lines[i].SetColor();
        }

        DrawAllLines();
    }

    // cursor variables shared for all boxes
    static class MenuHeart
    {
        static string sprite = "♥️";
        static int x = 0;          
        static int y = 0;
        static bool visible = false;

        public static void MoveTo(int newX, int newY)
        {
            // stop if theres no change
            if (x == newX && y == newY)
            {
                return;
            }

            // clear the heart from the old position
            if (visible)
            {
                Hide();
            }

            // draw the heart in the new position
            visible = true;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(newX, newY);
            Console.Write(sprite);
            Console.ResetColor();
            
            // save new heart position
            x = newX;
            y = newY;
        }
        
        public static void Hide()
        {
            visible = false;
            Console.SetCursorPosition(x, y);
            Console.Write("  ");
        }
    }

    // build a list and make the user choose an option
    // using Deltarune controls (bc why not)
    public int PromptUserInput(int[] choiceLines)
    {
        int curPos = 0; // line that the cursor is on
        int output = -1;

        while (output < 0)
        {
            // move cursor to selected option
            MenuHeart.MoveTo(pos + 3, choiceLines[curPos]);

            // ONLY 1 READKEY AT A TIME!!!
            // the program pauses to wait for each input separately
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    curPos = WrapCursor(-1, curPos, choiceLines.Length - 1);
                    break;

                case ConsoleKey.DownArrow:
                    curPos = WrapCursor(1, curPos, choiceLines.Length - 1);
                    break;

                case ConsoleKey.Enter: // alt scheme
                case ConsoleKey.Z: // deltarune controls
                    output = curPos;
                    break;

                case ConsoleKey.Escape:
                    return -1;
            }
        }

        // select choice in the menu
        lines[choiceLines[output]].SetColor(ConsoleColor.Yellow);
        DrawLine(choiceLines[output]);
        return output;
    }

    // use the given line for a yes/no choicer
    public bool ConfirmUserInput(int line)
    {
        // navigation variables
        bool curPos = true; // true = left, false = right
        bool output = false;
        bool inputted = false;

        // format line correctly
        lines[line].SetText(" Confirm                  Cancel", true);
        DrawLine(line);

        while (!inputted)
        {
            if (curPos)
            { 
                // draw the cursor on the left
                MenuHeart.MoveTo(pos + 8, line);
            }
            else
            {
                // draw the cursor on the right
                MenuHeart.MoveTo(pos + 33, line);
            }

            // ONLY 1 READKEY AT A TIME!!!
            // the program pauses to wait for each input separately
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.LeftArrow:
                    curPos = true; // no wrapping cuz only 2 spots
                    break;

                case ConsoleKey.RightArrow:
                    curPos = false; // no wrapping cuz only 2 spots
                    break;

                case ConsoleKey.Enter: // alt scheme
                case ConsoleKey.Z: // deltarune controls
                    // confirm selection
                    inputted = true;
                    output = curPos;

                    break;

                case ConsoleKey.Escape: // shift doesnt exist lol
                case ConsoleKey.X:
                    // cancel selection
                    inputted = true;
                    output = false;
                    break;
            }
        }

        MenuHeart.Hide();
        return output;
    }

    // + or - a number while keeping it
    // between 0 and a given maximum.
    int WrapCursor(int amount, int index, int max)
    {
        if (index + amount < 0)
        {
            index = max;
        }
        else if (index + amount > max)
        {
            index = 0;
        }
        else
        {
            index += amount;
        }

        return index;
    }
}

public class MenuLine
{
    string boxPart = "";
    int pos;
    int size;

    public MenuLine(LineType type, int width, int margin)
    {
        pos = margin;
        size = width;
        SetType(type);
    }
    string contentText = "";
    bool contentCentered = false;
    ConsoleColor? contentColor = null;

    public void Draw()
    {
        Console.Write(boxPart);

        if (contentText.Length > 0)
        {
            if (contentColor is not null)
                Console.ForegroundColor = contentColor.Value;

            AlignCursor(contentCentered);
            Console.Write(contentText);
        }

        // New line, reset color
        Console.ResetColor();
        Console.WriteLine();
    }

    public void AlignCursor(bool centered = false)
    {
        int destX = 0;
        
        if (contentCentered)
        {
            destX = pos + (((size + 2) - contentText.Length) / 2);
        }
        else
        {
            destX = pos + 1;
        }

        Console.SetCursorPosition(destX, Console.CursorTop);
    }

    public void SetText(string text, bool centered = false)
    {
        contentText = text;
        contentCentered = centered;
    }
    public string GetText()
    {
        return contentText;
    }

    public void SetColor(ConsoleColor? color = null)
    {
        contentColor = color;
    }

    public void SetType(LineType type)
    {
        string[] parts = ["L", "", "R"];

        switch (type)
        {
            case LineType.Top:
                parts = ["┏", "━", "┓"];
                break;

            case LineType.Separator:
                parts = ["┠", "─", "┨"];
                break;

            case LineType.Body:
                parts = ["┃", " ", "┃"];
                break;
                
            case LineType.Bottom:
                parts = ["┗", "━", "┛"];
                break;
                
            case LineType.Blank:
                parts = ["", "", ""];
                break;
        }

        boxPart = "";

        // Left padding
        for (int i = 0; i < pos; i++)
            boxPart += " ";

        // Box parts
        boxPart += parts[0];
        for (int i = 0; i < size; i++)
        {
            boxPart += parts[1];
        }
        boxPart += parts[2];
    }
}

public enum LineType
{
    Top,
    Body,
    Bottom,
    Separator,
    Blank
}