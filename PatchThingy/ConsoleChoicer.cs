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
        public int curSelection = 0;
        public bool Chosen = false;
        public bool Focused = false;
        public bool Visible = false;

        public ChoicerWidget (ChoicerType type, string[] choices)
        {
            this.type = type;
            this.choices = choices;
        }

        public void Draw(ConsoleMenu box, int line)
        {
            if (!Visible)
            {
                return;
            }

            AlignColumns(box);
            int startLine = box.Pos.y + line;

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
            }
        }

        void WriteChoice (string text, int x, int y, bool selected)
        {
            if (Focused && !Chosen && selected)
            {
                // move heart
                MoveCursor(x - 3, y);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("♥️");
                Console.ResetColor();
            }
            else
            {
                // clear heart
                MoveCursor(x - 3, y);
                Console.Write("  ");
            }

            if (Chosen && selected)
            {
                // set color
                Console.ForegroundColor = ConsoleColor.Yellow;
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
                        Chosen = true;
                        return curSelection;

                    // CANCEL: set chosen to true
                    // set output to -1
                    case ConsoleKey.Escape:
                    case ConsoleKey.X:
                        Chosen = true;
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

            }
        }

        public int LineCount()
        {
            if (!Visible)
            {
                return 0;
            }

            switch(type)
            {
                case ChoicerType.List:
                    return choices.Length;

                case ChoicerType.Grid:
                    return choices.Length / 2 + (choices.Length % 2);
                    
                default:
                    return 1;
            }
        }
    }

    // Add a choicer to the menu
    public int AddChoicer(ChoicerType type, string[] choices)
    {
        MenuWidgets.Add(new ChoicerWidget(type, choices));
        return MenuWidgets.Count - 1;
    }
    public void InsertChoicer(int index, ChoicerType type, string[] choices)
    {
        MenuWidgets.Insert(index, new ChoicerWidget(type, choices));
    }
    public void ReplaceChoicer(int index, ChoicerType type, string[] choices)
    {
        MenuWidgets[index] = new ChoicerWidget(type, choices);
    }

    // activate the choicer at the given index
    public int PromptChoicer(int index, bool showPrevious = false)
    {
        // check if the widget is a choicer
        if (MenuWidgets[index] is not ChoicerWidget)
        {
            return -1;
        }

        // hide all other choicers if told 
        foreach (ChoicerWidget widget in MenuWidgets.OfType<ChoicerWidget>())
        {
            if (!showPrevious)
            {
                widget.Visible = false;
            }
        }

        // have to make a sep variable for some reason
        var choicer = (ChoicerWidget)MenuWidgets[index];
        choicer.Chosen = false;
        choicer.Focused = true;
        choicer.Visible = true;
        Draw();

        // values from last loop
        int prevSelection = choicer.curSelection;
        bool prevChosen = false;

        int output = -1;

        // while there isnt a confirm/cancel
        while (!choicer.Chosen)
        {
            // set input value
            output = choicer.GetUserInput();

            // draw after every key press
            choicer.Draw(this, WidgetLine(index));

            // update past values
            prevSelection = choicer.curSelection;
            prevChosen = choicer.Chosen;
        }

        if (output == -1)
        {
            choicer.Chosen = false;
        }

        // unfocus and hide
        choicer.Focused = false;
        Draw();

        // return output to larger switch/case
        return output;
    }

    public bool ConfirmChoicer(string[] message)
    {
        // remember the starting index
        int start = MenuWidgets.Count;
        bool result;

        // add separated section with choicer
        AddSeparator();
        AddSeparator(false);
        foreach (string line in message)
        {
            AddText(line, Alignment.Center);
        }

        if (message.Length > 1)
        {
            AddSeparator(false);
        }
        
        int choicer = AddChoicer(ChoicerType.Grid, ["Confirm", "Cancel"]);
        AddSeparator(false);

        // remove widgets after choice
        result = PromptChoicer(choicer, true) == 0;
        Remove(start, choicer + 1);

        // get result of choicer
        return result;
    }
}

public enum ChoicerType
{
    Grid,
    List,
}