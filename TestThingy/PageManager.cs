namespace TestThingy;

class PageManager
{
    List<Page> pageList = [];
    public int pageCount => pageList.Count;
    public bool IsEmpty => pageList.Count == 0;

    // Handles Cancel button for now, may move
    // that to a per-page thing.
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

    public void AddPage(Page page)
    {
        pageList.Add(page);
    }

    public void RemovePage()
    {
        if (pageCount > 0)
        {
            pageList.RemoveAt(pageList.Count - 1);
        }
    }

    public void Exit()
    {
        pageList.Clear();
    }

    void SendInput(ConsoleKey inputKey)
    {
        if (pageCount > 0)
        {
            pageList.Last().OnKeyInput(inputKey);
        }
    }

    public void DrawPage()
    {
        if (pageCount > 0)
        {
            pageList.Last().Draw();
        }
    }
}