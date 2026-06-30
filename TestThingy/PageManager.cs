namespace TestThingy;

class PageManager
{
    List<IPage> pageList = [];
    public bool IsEmpty => pageList.Count == 0;

    public void OnKeyInput(ConsoleKey inputKey)
    {
        switch (inputKey)
        {
            case ConsoleKey.X:
                RemovePage();
                break;

            default:
                SendInput(inputKey);
                break;
        }
    }

    public void AddPage(IPage page)
    {
        pageList.Add(page);
    }

    void RemovePage()
    {
        if (pageList.Count > 0)
        {
            pageList.RemoveAt(pageList.Count - 1);
        }
    }

    void SendInput(ConsoleKey inputKey)
    {
        if (pageList.Count > 0)
        {
            pageList.Last().OnKeyInput(inputKey);
        }
    }

    public void DrawPage()
    {
        if (pageList.Count > 0)
        {
            pageList.Last().Draw();
        }

        Console.SetCursorPosition(0, 0);
        Console.Write(pageList.Count);
    }
}