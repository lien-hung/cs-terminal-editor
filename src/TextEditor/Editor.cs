namespace TextEditor
{
  class Editor
  {
    string _filePath;
    Buffer _buffer;
    Cursor _cursor;
    int _charPos;
    string _status = string.Empty;

    public Editor(string filePath)
    {
      var lines = File.Exists(filePath)
        ? File.ReadAllLines(filePath)
            .Where(x => x != Environment.NewLine)
            .ToList()
        : [""];

      _filePath = filePath;
      _buffer = new Buffer(lines);
      _cursor = new Cursor();
    }

    public async Task Run()
    {
      while (true)
      {
        Scroll();
        await Render();
        await HandleInput();
      }
    }

    private async Task HandleInput()
    {
      var character = Console.ReadKey();

      if (character.Modifiers == ConsoleModifiers.Control)
      {
        switch (character.Key)
        {
          case ConsoleKey.S:
            {
              if (string.IsNullOrWhiteSpace(_filePath))
              {
                string? inputPath = RenderWithInput("Enter a file name or path: ");
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                  await RenderWithMessage("A file name was not provided.");
                  break;
                }
                else
                {
                  _filePath = inputPath.Trim();
                }
              }
              File.WriteAllLines(_filePath, _buffer.Lines);
              await RenderWithMessage($"{_buffer.LineCount}L ({_buffer.TotalBytes}B) written to \"{_filePath}\"");
              break;
            }

          case ConsoleKey.Q:
            {
              Console.Write("\u001b[2J\u001b[?47l\x1b[H");
              Environment.Exit(0);
              break;
            }
        }
      }
      else
      {
        switch (character.Key)
        {
          case ConsoleKey.Backspace:
            {
              if (_cursor.Col > 0)
              {
                _buffer = _buffer.Delete(_cursor.Row, _cursor.Col - 1);
                _cursor = _cursor.Left();
              }
              else
              {
                if (_cursor.Row == 0) break;
                int prevRowLen = _buffer.LineLength(_cursor.Row - 1);
                _cursor = _cursor.Up(_buffer).MoveToCol(prevRowLen);
                _buffer = _buffer.JoinNextLine(_cursor.Row);
              }
              break;
            }

          case ConsoleKey.Enter:
            {
              _buffer = _buffer.SplitLine(_cursor.Row, _cursor.Col);
              _cursor = _cursor.Down(_buffer).MoveToCol(0);
              break;
            }

          case ConsoleKey.UpArrow:
            _cursor = _cursor.Up(_buffer);
            break;

          case ConsoleKey.DownArrow:
            _cursor = _cursor.Down(_buffer);
            break;

          case ConsoleKey.LeftArrow:
            _cursor = _cursor.Left();
            break;

          case ConsoleKey.RightArrow:
            _cursor = _cursor.Right(_buffer);
            break;

          case ConsoleKey.Tab:
            {
              _buffer = _buffer.Insert("\t", _cursor.Row, _cursor.Col);
              _cursor = _cursor.Right(_buffer);
              break;
            }

          default:
            {
              if (IsTextChar(character))
              {
                _buffer = _buffer.Insert(character.KeyChar.ToString(), _cursor.Row, _cursor.Col);
                _cursor = _cursor.Right(_buffer);
              }
              break;
            }
        }
      }
    }

    private async Task Render()
    {
      RenderBuffer();
      UpdateStatus();
      ANSI.RenderStatusBar(_status);
      _charPos = GetCharPos();
      ANSI.MoveCursor(_cursor.Row - _buffer.RowOff, _charPos - _buffer.ColOff);
    }

    private async Task RenderWithMessage(string message)
    {
      RenderBuffer();
      ANSI.RenderStatusBar(message);
      await Task.Delay(3000);
      await Render();
    }

    private string? RenderWithInput(string message)
    {
      RenderBuffer();
      ANSI.RenderStatusBar(message);
      return Console.ReadLine();
    }

    private void RenderBuffer()
    {
      ANSI.ClearScreen();
      ANSI.MoveCursor(0, 0);
      _buffer.Render();
    }

    private void Scroll()
    {
      _charPos = _cursor.Row < _buffer.LineCount ? GetCharPos() : 0;

      if (_cursor.Row < _buffer.RowOff)
      {
        _buffer.RowOff = _cursor.Row;
      }
      if (_cursor.Row >= _buffer.RowOff + Constants.MaxRows)
      {
        _buffer.RowOff = _cursor.Row - Constants.MaxRows + 1;
      }
      if (_charPos < _buffer.ColOff)
      {
        _buffer.ColOff = _charPos;
      }
      if (_charPos >= _buffer.ColOff + Constants.MaxCols)
      {
        _buffer.ColOff = _charPos - Constants.MaxCols + 1;
      }
    }

    private static bool IsTextChar(ConsoleKeyInfo character)
    {
      return !char.IsControl(character.KeyChar);
    }

    private void UpdateStatus()
    {
      string fileName = !string.IsNullOrWhiteSpace(_filePath) ? _filePath : "[no name]";
      string leftStatus = $"{fileName} ({_buffer.LineCount} lines)";
      string rightStatus = $"{_cursor.Row + 1}:{_charPos + 1}";

      int whitespaceCount = Constants.MaxCols - leftStatus.Length - rightStatus.Length;
      _status = $"{leftStatus}{new string(' ', whitespaceCount)}{rightStatus}";
    }

    private int GetCharPos()
    {
      int pos = 0;
      for (int i = 0; i < _cursor.Col; i++)
      {
        if (_buffer.Lines[_cursor.Row][i] == '\t')
        {
          pos += Constants.TabSize - 1 - (pos % Constants.TabSize);
        }
        pos++;
      }
      return pos;
    }
  }
}
