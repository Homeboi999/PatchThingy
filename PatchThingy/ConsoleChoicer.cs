using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using ImageMagick;

public partial class ConsoleMenu
{
    public class MenuChoicer : IWidget
    {
        ChoicerType type;
        string[] choices = [];
        int[] columnPos = [0, 0];

        int line;

        public MenuChoicer (ChoicerType type, int line, string[] choices)
        {
            this.type = type;
            this.line = line;
            this.choices = choices;
        }

        public void Draw(ConsoleMenu box)
        {
            AlignColumns(box);

            switch (type)
            {
                case ChoicerType.List:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        MoveCursor(columnPos[0], box.y + 1 + line + i);
                        Console.Write(choices[i]);
                    }
                    break;

                case ChoicerType.Grid:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        MoveCursor(columnPos[i % 2], (box.y + 1 + line) + i / 2);
                        Console.Write(choices[i]);
                    }
                    break;

                case ChoicerType.InLine:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        MoveCursor(columnPos[i], box.y + 1 + line);
                        Console.Write(choices[i]);
                    }
                    break;
            }
        }

        void AlignColumns(ConsoleMenu box)
        {
            // array for grid layout
            int[] wordLengths = [0, 0];

            switch (type)
            {
                case ChoicerType.Grid:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        wordLengths[i % 2] = Math.Max(wordLengths[i % 2], choices[i].Length);
                    }

                    columnPos[0] = box.AlignPosition(Alignment.Left);
                    columnPos[1] = box.AlignPosition(Alignment.Right) - wordLengths[1];
                    break;

                case ChoicerType.List:
                    columnPos[0] = box.AlignPosition(Alignment.Left);
                    break;

                case ChoicerType.InLine:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        columnPos[i] = box.AlignPosition(Alignment.Center);
                        columnPos[i] += (i * box.width / choices.Length) - box.width / 2;
                    }
                    break;

            }
        }


        public int GetLine()
        {
            return line;
        }

        public int LineCount()
        {
            switch(type)
            {
                case ChoicerType.List:
                    return choices.Length;

                case ChoicerType.Grid:
                    return choices.Length / 2;
                    
                case ChoicerType.InLine:
                default:
                    return 1;
            }
        }
    }

    public void AddChoicer(ChoicerType type, int line, string[] choices)
    {
        MenuWidgets.Add(new MenuChoicer(type, line, choices));
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
            MenuHeart.MoveTo(x + 3, choiceLines[curPos]);

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
        // lines[choiceLines[output]].SetColor(ConsoleColor.Yellow);
        // DrawLine(choiceLines[output]);
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
        // lines[line].SetText(" Confirm                  Cancel", true);
        // DrawLine(line);

        while (!inputted)
        {
            if (curPos)
            { 
                // draw the cursor on the left
                MenuHeart.MoveTo(x + 8, y + line);
            }
            else
            {
                // draw the cursor on the right
                MenuHeart.MoveTo(x + 33, y + line);
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

    // heart cursor shared for all boxes
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
}

public enum ChoicerType
{
    Grid,
    List,
    InLine,
}