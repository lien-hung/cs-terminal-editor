using System.Text;

namespace TextEditor
{
  class Buffer(List<string> lines, int rowOff = 0, int colOff = 0)
  {
    public List<string> Lines { get; } = lines;
    public int RowOff { get; set; } = rowOff;
    public int ColOff { get; set; } = colOff;

    public int LineCount => Lines.Count;
    public int TotalBytes => Lines.Sum(Encoding.UTF8.GetByteCount);

    public void Render()
    {
      for (int y = 0; y < Constants.MaxRows; y++)
      {
        int lineIndex = y + RowOff;
        if (lineIndex >= LineCount)
        {
          Console.WriteLine("~");
        }
        else
        {
          int len = Lines[lineIndex].Length - ColOff;
          if (len < 0)
          {
            Console.WriteLine();
            continue;
          }
          len = Math.Min(len, Constants.MaxCols);
          Console.WriteLine(Lines[lineIndex].AsSpan(ColOff, len));
        }
      }
    }

    public int LineLength(int row)
    {
      return Lines[row].Length;
    }

    public Buffer Insert(string character, int row, int col)
    {
      var copy = Lines.Select(x => x).ToList();
      copy[row] = copy[row].Insert(col, character);
      return new Buffer(copy, RowOff, ColOff);
    }

    public Buffer Delete(int row, int col)
    {
      var copy = Lines.Select(x => x).ToList();
      copy[row] = copy[row].Remove(col, 1);
      return new Buffer(copy, RowOff, ColOff);
    }

    public Buffer DeleteLine(int row)
    {
      var copy = Lines.Select(x => x).ToList();
      if (copy.Count > 1) copy.RemoveAt(row);
      return new Buffer(copy, RowOff, ColOff);
    }

    public Buffer JoinNextLine(int row)
    {
      var copy = Lines.Select(x => x).ToList();
      if (row >= 0 && row < copy.Count - 1)
      {
        var line = copy[row];
        var nextLine = copy[row + 1];
        copy[row] = line + nextLine;
        copy.RemoveAt(row + 1);
      }
      return new Buffer(copy, RowOff, ColOff);
    }

    public Buffer SplitLine(int row, int col)
    {
      var copy = Lines.Select(x => x).ToList();

      var line = copy[row];

      var newLines = new[] { line[..col], line.Substring(col, line.Length - line[..col].Length) };
      copy[row] = newLines[0];
      copy.Insert(row + 1, newLines[1]);

      return new Buffer(copy, RowOff, ColOff);
    }
  }
}
