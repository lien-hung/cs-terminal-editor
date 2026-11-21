Console.Write("\u001b7\u001b[?47h");
var path = args.FirstOrDefault() ?? "";
var editor = new TextEditor.Editor(path);
await editor.Run();
