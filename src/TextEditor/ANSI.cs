namespace TextEditor
{
  class ANSI
  {
    public static void ClearScreen()
    {
      Console.Clear();
      Console.Write("\x1b[3J");
    }

    public static void MoveCursor(int row, int col)
    {
      Console.CursorTop = row;
      Console.CursorLeft = col;
    }

    public static void RenderStatusBar(string message)
    {
      MoveCursor(Constants.MaxRows, 0);
      Console.WriteLine(new string('─', Constants.MaxCols));
      Console.Write(message);
    }
  }
}
