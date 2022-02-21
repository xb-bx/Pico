using System.IO;
using System.Collections.Generic;
using System;
namespace Pico;

public class TextBuffer : BufferBase
{
    public List<string> Lines { get; private set; }
    public readonly string Path;
    private string unsaved, saved;
    public bool Editable;
    public int CursorX, CursorY;
    public int X, Y;
    public TextBuffer(string path, string name, int width, int height, bool editable = true)
    {
        (Path, Name, Width, Height, Editable) = (path, name, width, height, editable);
        unsaved = Name + '*';
        saved = Name;
        Lines = new(64);

    }
    public override void Input(CharBuffer buf, ConsoleKeyInfo keyinfo)
    {
        switch (keyinfo.Key)
        {
            case ConsoleKey.R when (keyinfo.Modifiers & ConsoleModifiers.Control) > 0 && Editable:
                {
                    Lines.Clear();
                    Lines.AddRange(File.ReadAllLines(Path));
                    RerenderNeeded = true;
                    Name = saved;
                }
                break;
            case ConsoleKey.LeftArrow when (keyinfo.Modifiers & ConsoleModifiers.Control) > 0:
                {
                    if (CursorX > Lines[CursorY + Y].Length)
                    {
                        CursorX = Lines[CursorY + Y].Length - 1;
                        X = 0;
                        RerenderNeeded = true;
                    }
                    else if (X + CursorX < Lines[CursorY + Y].Length)
                    {
                        var l = Lines[CursorY + Y];
                        CursorX--;
                        do
                        {
                            CursorX--;
                        }
                        while (X + CursorX > 0 && l[X + CursorX] != ' ');
                        if (CursorX < 0)
                        {
                            CursorX = 0;
                            X += CursorX;
                        }
                        if (X < 0)
                        {
                            X = 0;
                            RerenderNeeded = true;
                        }
                    }
                    break;
                }
            case ConsoleKey.LeftArrow:
                {
                    CursorX--;
                    break;
                }
            case ConsoleKey.RightArrow when (keyinfo.Modifiers & ConsoleModifiers.Control) > 0:
                {
                    if (X + CursorX < Lines[CursorY + Y].Length)
                    {
                        var l = Lines[CursorY + Y];
                        CursorX++;
                        do
                        {
                            CursorX++;
                        }
                        while (X + CursorX < l.Length && l[X + CursorX] != ' ');
                        if (X + CursorX > Width)
                        {
                            var x = X;
                            X += CursorX - Width;
                            CursorX -= X;
                            RerenderNeeded = true;
                        }
                        if (X < 0)
                        {
                            X = 0;
                            RerenderNeeded = true;
                        }
                    }
                    break;
                }
            case ConsoleKey.RightArrow:
                {
                    CursorX++;
                    break;
                }
            case ConsoleKey.UpArrow:
                {
                    CursorY--;
                    break;
                }
            case ConsoleKey.DownArrow:
                {
                    CursorY++;
                    break;
                }
            case ConsoleKey.S when (keyinfo.Modifiers & ConsoleModifiers.Control) > 0 && Editable:
                File.WriteAllLines(Path, Lines);
                Name = saved;
                RerenderNeeded = true;
                break;
            case ConsoleKey.Enter when Editable:
                {
                    Lines.Insert(Y + CursorY, "");
                    CursorY++;
                    Name = unsaved;
                    X = 0;
                    CursorX = 0;
                    RerenderNeeded = true;
                    break;
                }
            case ConsoleKey.Backspace when Editable:
                {
                    var line = Lines[Y + CursorY];
                    var start = X + CursorX - 1;
                    if (start >= 0 && start < line.Length)
                    {
                        Lines[Y + CursorY] = line.Remove(X + CursorX - 1, 1);
                        CursorX--;
                        Name = unsaved;
                        RerenderNeeded = true;
                    }
                    else if (start == -1 && string.IsNullOrWhiteSpace(line))
                    {
                        Lines.RemoveAt(Y + CursorY);
                        CursorY--;
                        if (Y + CursorY > 0 && Lines.Count > 0)
                        {
                            line = Lines[Y + CursorY];

                            if (line.Length < Width)
                            {
                                X = 0;
                                CursorX = line.Length + 1;
                            }
                            else
                            {
                                X = line.Length - Width + 1;
                                CursorX = line.Length + 1;
                            }
                        }
                        else
                        {
                            X = CursorX = 0;
                        }
                        if(Lines.Count == 0)
                            Lines.Add("");
                        Name = unsaved;
                        RerenderNeeded = true;
                    }
                    else if (start == -1 && Y + CursorY > 0)
                    {
                        Lines.RemoveAt(Y + CursorY);
                        Lines[Y + CursorY - 1] += line;
                        var newline = Lines[Y + CursorY - 1];
                        CursorY--;
                        if (newline.Length < Width)
                        {
                            X = 0;
                            CursorX = newline.Length - 1;
                        }
                        else
                        {
                            X = newline.Length - Width;
                            CursorX = newline.Length;
                        }
                        Name = unsaved;
                        RerenderNeeded = true;
                    }
                }
                break;
            case ConsoleKey k when Editable:
                {
                    var line = Lines[Y + CursorY];
                    //if(X + CursorX >= line.Length)
                        
                    Lines[Y + CursorY] = line.Insert(X + CursorX, keyinfo.KeyChar.ToString());
                    Name = unsaved;
                    CursorX++;
                    RerenderNeeded = true;
                }
                break;

        }
        int rawx = CursorX, rawy = CursorY;
        if (CursorY >= Height && CursorY < Lines.Count)
        {
            Y++;
            RerenderNeeded = true;
        }
        else if (CursorY < 0)
        {
            Y--;
            RerenderNeeded = true;
        }
        //CursorY.Clamp(0, Math.Min(Lines.Count + 1, Height + 1));
        CursorY.Clamp(0, Lines.Count);
        int curycl = CursorY;
        if (CursorX >= Width && Lines[CursorY + Y].Length > Width && (X + 1 + CursorX) < Lines[CursorY + Y].Length)
        {
            X++;
            RerenderNeeded = true;
        }
        else if (CursorX < 0)
        {
            X--;
            RerenderNeeded = true;
        }
        CursorY.Clamp(0, Math.Min(Lines.Count, Height));
        var xb = X;
        X.Clamp(0, int.MaxValue);
        Y.Clamp(0, Lines.Count - CursorY);
        CursorX.Clamp(0, Math.Min(Lines[Y + CursorY].Length - X + 1, Width));
        int actualX = buf.OffsetX + CursorX;
        int actualY = buf.OffsetY + CursorY;
        DebugLine.Instance.Debug = $"{Y + 1 + CursorY} {X + 1 + CursorX}";
        Console.CursorLeft = actualX;
        Console.CursorTop = actualY;
    }
    public override void Render(CharBuffer buf)
    {
        for (int line = 0; line < Height; line++)
        {
            int linei = line + Y;
            if (linei < Lines.Count)
            {
                var l = Lines[linei];
                if (X < l.Length)
                {
                    for (int x = X, i = 0; x < l.Length && i < Width; x++)
                    {
                        buf[i, line] = l[x];
                        i++;
                    }
                }
            }
        }
        RerenderNeeded = false;
        int actualX = buf.OffsetX + CursorX;
        int actualY = buf.OffsetY + CursorY;
        Console.CursorLeft = actualX;
        Console.CursorTop = actualY;
    }
}

