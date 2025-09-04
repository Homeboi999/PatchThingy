using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;
using System.Diagnostics;
using ImageMagick;

public partial class ConsoleMenu
{
    class ChoicerWidget : IWidget
    {
        ChoicerType type;
        string[] choices = [];
        int[] columnPos = [0, 0];

        int line;
        public int curSelection = 0;
        public bool chosen = false;

        public ChoicerWidget (ChoicerType type, int line, string[] choices)
        {
            this.type = type;
            this.line = line;
            this.choices = choices;
        }

        public void Draw(ConsoleMenu box)
        {
            AlignColumns(box);
            int startLine = box.y + line;

            switch (type)
            {
                case ChoicerType.List:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        WriteChoice(choices[i], columnPos[0], startLine + i, curSelection == i);
                    }
                    break;

                case ChoicerType.Grid:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        WriteChoice(choices[i], columnPos[i % 2], startLine + (i / 2), curSelection == i);
                    }
                    break;

                case ChoicerType.InLine:
                    for (int i = 0; i < choices.Length; i++)
                    {
                        WriteChoice(choices[i], columnPos[i], startLine, curSelection == i);
                    }
                    break;
            }
        }

        void WriteChoice (string text, int x, int y, bool selected)
        {
            if (selected)
            {
                if (chosen)
                {
                    // set color
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    // move MenuHeart
                    MenuHeart.MoveTo(x - 3, y);
                }
            }
            // draw text at position
            MoveCursor(x, y);
            Console.Write(text);

            // reset color
            Console.ResetColor();
        }

        // responsible for selecting an option from
        // the choicer, setting curSelection accordingly.
        public int GetUserInput()
        {
            // while loop to retrigger ReadKey
            // if an invalid key is pressed
            while (true)
            {
                // ONLY 1 READKEY AT A TIME!!!
                // the program pauses to wait for each input separately
                switch (Console.ReadKey(true).Key)
                {
                    // UP/DOWN: only wrap on List
                    // do nothing on InLine
                    case ConsoleKey.UpArrow:
                        if (type == ChoicerType.List)
                        {
                            if (curSelection - 1 < 0)
                            {
                                curSelection = choices.Length - 1;
                            }
                            else
                            {
                                curSelection--;
                            }
                        }
                        else if (type == ChoicerType.Grid)
                        {
                            if (curSelection - 2 >= 0)
                            {
                                curSelection -= 2;
                            }
                        }
                        return -1;

                    case ConsoleKey.DownArrow:
                        if (type == ChoicerType.List)
                        {
                            if (curSelection + 1 >= choices.Length)
                            {
                                curSelection = 0;
                            }
                            else
                            {
                                curSelection++;
                            }
                        }
                        else if (type == ChoicerType.Grid)
                        {
                            if (curSelection + 2 < choices.Length)
                            {
                                curSelection += 2;
                            }
                        }
                        return -1;

                    // LEFT/RIGHT: dont move right from end of list
                    // do nothing on List
                    case ConsoleKey.LeftArrow:
                        if (type != ChoicerType.List)
                        {
                            if (curSelection % 2 == 1)
                            {
                                curSelection--;
                            }
                        }
                        return -1;

                    case ConsoleKey.RightArrow:
                        if (type != ChoicerType.List)
                        {
                            if (curSelection % 2 == 0 && curSelection + 1 < choices.Length)
                            {
                                curSelection++;
                            }
                        }
                        return -1;

                    // CONFIRM: set chosen to true
                    // set output to current selection
                    case ConsoleKey.Enter: // alt scheme
                    case ConsoleKey.Z: // deltarune controls
                        chosen = true;
                        return curSelection;

                    // CANCEL: set chosen to true
                    // set output to -1
                    case ConsoleKey.Escape:
                    case ConsoleKey.X:
                        chosen = true;
                        return -1;

                    // other keys are ignored
                    default:
                        break;
                }
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
                    columnPos[1] = box.AlignPosition(Alignment.Center) + box.margin / 2 + 1;
                    break;

                case ChoicerType.List:
                    columnPos[0] = box.AlignPosition(Alignment.Left);
                    break;

                case ChoicerType.InLine:
                    columnPos[0] = box.AlignPosition(Alignment.Left);
                    columnPos[1] = box.AlignPosition(Alignment.Center) + box.margin / 2 + 1;
                    break;

            }
        }

        public int GetLine()
        {
            return line;
        }

        public void MoveLine(int amount)
        {
            line += amount;
        }

        public int LineCount()
        {
            switch(type)
            {
                case ChoicerType.List:
                    return choices.Length;

                case ChoicerType.Grid:
                    return choices.Length / 2 + (choices.Length % 2);
                    
                case ChoicerType.InLine:
                default:
                    return 1;
            }
        }
    }

    // Add a choicer to the menu
    public void AddChoicer(ChoicerType type, string[] choices)
    {
        MenuWidgets.Add(new ChoicerWidget(type, NextWidgetLine(), choices));
    }
    public void InsertChoicer(int index, ChoicerType type, string[] choices)
    {
        MenuWidgets.Insert(index, new ChoicerWidget(type, NextWidgetLine(index), choices));
    }

    // activate the choicer at the given index
    public int PromptChoicer(int index, int confirmLine = -1)
    {
        // check if the widget is a choicer
        if (MenuWidgets[index] is not ChoicerWidget)
        {
            return -1;
        }

        // have to make a sep variable for some reason
        var choicer = (ChoicerWidget)MenuWidgets[index];
        choicer.curSelection = 0;
        choicer.chosen = false;
        choicer.Draw(this);

        // values from last loop
        int prevSelection = 0;
        bool prevChosen = false;

        int output = -1;

        // while there isnt a confirm/cancel
        while (!choicer.chosen)
        {
            // set input value
            output = choicer.GetUserInput();

            // draw after every key press
            choicer.Draw(this);

            // update past values
            prevSelection = choicer.curSelection;
            prevChosen = choicer.chosen;

            // check if something was selected
            // (and the choice wasnt cancelled)
            if (choicer.chosen && output >= 0)
            {
                // check if we should do the confirmation
                if (confirmLine > -1 && MenuWidgets[confirmLine] is ChoicerWidget)
                {
                    // prompt the other choicer for yes/no
                    if (PromptChoicer(confirmLine) != 0)
                    {
                        // if no, reset variable to continue loop
                        choicer.chosen = false;
                        choicer.Draw(this);
                    }
                }
            }
        }

        // dont have lingering selection after choice is made
        choicer.chosen = false;
        choicer.Draw(this);
        MenuHeart.Hide();

        // return output to larger switch/case
        return output;
    }

    // heart cursor shared for all choicers
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
            MoveCursor(newX, newY);
            Console.Write(sprite);
            Console.ResetColor();
            
            // save new heart position
            x = newX;
            y = newY;
        }
        
        public static void Hide()
        {
            visible = false;
            MoveCursor(x, y);
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