using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using ImageMagick;

// One class to manage all changes to the data.win
//
// This file contains functions that help format
// the console output from the apply/generate functions.
public class ConsoleMenu
{
    int pos;
    public List<MenuLine> lines = [];

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

    public void Draw()
    {
        Console.Clear();

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

    public int PromptUserInput(int[] choiceLines)
    {
        string cursor = "♥️";
        int curPos = 0;    
        int prevPos = 0;
        int output = -1;

        while (output < 0)
        {
            Console.SetCursorPosition(pos + 3, choiceLines[curPos]);
            Console.Write(cursor);

            if (prevPos != curPos)
            {
                Console.SetCursorPosition(pos + 3, choiceLines[prevPos]);
                Console.Write(" ");
            }

            prevPos = curPos;

            switch (Console.ReadKey(false).Key)
            {
                case ConsoleKey.UpArrow:
                    curPos = WrapCursor(-1, curPos, choiceLines.Length - 1);
                    break;

                case ConsoleKey.DownArrow:
                    curPos = WrapCursor(1, curPos, choiceLines.Length - 1);
                    break;

                case ConsoleKey.Z:
                    output = curPos;
                    break;
            }
        }

        return output;
    }

    public bool ConfirmUserInput(int line)
    {
        string cursor = "♥️";
        bool output = false;
        bool inputted = false;

        Console.SetCursorPosition(pos + 3, line);
        Console.Write(cursor);

        while (!inputted)
        {
            switch (Console.ReadKey(false).Key)
            {
                case ConsoleKey.Enter: // alt scheme
                case ConsoleKey.Z: // deltarune controls
                    inputted = true;
                    output = true;
                    break;

                case ConsoleKey.Escape: // shift doesnt exist lol
                case ConsoleKey.X:
                    inputted = true;
                    output = false;
                    break;
            }
        }

        return output;
    }

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
    string[] parts = ["", "", ""];
    int pos;
    int size;
    int textOffset = 5;

    public MenuLine(LineType type, int width, int margin)
    {
        SetType(type);
        pos = margin;
        size = width;
    }
    public string contentText = "";
    public bool contentCentered = false;

    public void Draw()
    {
        // Left padding

        for (int i = 0; i < pos; i++)
            Console.Write(" "); // idk why the int

        // Box parts
        Console.Write(parts[0]); // left box graphic

        for (int i = 0; i < size; i++)
            Console.Write(parts[1]);

        Console.Write(parts[2]);

        if (contentText.Length > 0)
        {
            AlignCursor(contentCentered);
            Console.Write(contentText);
        }

        // New line
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
            destX = pos + textOffset + 1;
        }

        Console.SetCursorPosition(destX, Console.CursorTop);
    }

    public void SetText(string text, bool centered = false)
    {
        contentText = text;
        contentCentered = centered;
    }

    public void SetType(LineType type)
    {
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