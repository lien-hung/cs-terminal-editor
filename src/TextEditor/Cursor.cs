namespace TextEditor
{
  class Cursor(int row = 0, int col = 0)
  {
    public int Row { get; private set; } = row;
    public int Col { get; private set; } = col;

    public Cursor Up(Buffer buffer)
    {
      int newRow = Row > 0 ? Row - 1 : Row;
      int newCol = Math.Min(Col, buffer.LineLength(newRow));
      return new Cursor(newRow, newCol);
    }

    public Cursor Down(Buffer buffer)
    {
      int newRow = Row < buffer.LineCount - 1 ? Row + 1 : Row;
      int newCol = Math.Min(Col, buffer.LineLength(newRow));
      return new Cursor(newRow, newCol);
    }

    public Cursor Left()
    {
      return new Cursor(Row, (Col <= 0) ? 0 : Col - 1);
    }

    public Cursor Right(Buffer buffer)
    {
      return new Cursor(Row, Col >= buffer.LineLength(Row) ? Col : Col + 1);
    }

    internal Cursor MoveToCol(int col)
    {
      return new Cursor(Row, col);
    }
  }
}
