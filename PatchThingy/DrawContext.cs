namespace PatchThingy;

public record DrawContext(int x, int y, int width, int height)
{
    // set the cursor position without going OoB
    public static void MoveCursor(int x, int y)
    {
        // have to do max so debugger doesnt crash
        int destX = Math.Clamp(x, 0, Math.Max(Console.BufferWidth - 1, 0));
        int destY = Math.Clamp(y, 0, Math.Max(Console.BufferHeight - 1, 0));
        Console.SetCursorPosition(destX, destY);
    }

    // function to align the cursor along the current line
    // in preparation to write text of a given size
    public int AlignPosition(Alignment align, int margin = 8)
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

    // Find the desired x coordinate for Choicer columns
    public int[] AlignColumns(int margin = 8)
    {
        return [AlignPosition(Alignment.Left), AlignPosition(Alignment.Center) + margin / 2 + 1];
    }

    // write 1 row of the box to a string
    public string AssembleRow(char left, char middle, char right, bool end = false)
    {
        // init string
        string row = "";

        // add spacing according to box position
        for (int i = 0; i < x; i++)
        {
            row += " ";
        }

        // first char in row
        row += left;

        // fill row with characters until long enough
        // or the edge of the screen has been reached
        for (int i = 0; i < width; i++)
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
}