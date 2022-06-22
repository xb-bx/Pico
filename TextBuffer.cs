using System.IO;
using System.Collections.Generic;
using System;
namespace Pico;

public class TextBuffer : BufferBase
{
    public List<string> Lines { get; private set; }
    public override string Name 
    {
        get => Edited ? unsaved : saved;
    }
    public readonly string Path;
    private string unsaved, saved;
    public bool Editable;
    public bool Edited { get; private set; } 
    public int CursorX, CursorY;
    public int X, Y;
    public TextBuffer(string path, string name, int width, int height, bool editable = true)
    {
        (Path, Width, Height, Editable) = (path, width, height, editable);
        unsaved = name + '*';
        saved = name;
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
                    Edited = false;
                }
                break;
            case ConsoleKey.LeftArrow:
                {
                    CursorX--;
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
                Edited = false;
                RerenderNeeded = true;
                break;
            case ConsoleKey.Enter when Editable:
                {
                    string split = "";
                    var line = Lines[CursorY];
                    if(CursorX < line.Length)
                    {
                        split = line.Substring(CursorX);
                        Lines[CursorY] = line.Remove(CursorX);
                    }
                    Lines.Insert(CursorY + 1, split);
                    CursorY++;
                    Edited = true;
                    X = 0;
                    CursorX = 0;
                    RerenderNeeded = true;
                    break;
                }
            case ConsoleKey.Backspace when Editable:
                {
                    var line = Lines[CursorY];
                    var start = CursorX - 1;
                    if (start >= 0 && start < line.Length)
                    {
                        Lines[CursorY] = line.Remove(CursorX - 1, 1);
                        CursorX--;
                        Edited = true;
                        RerenderNeeded = true;
                    }
                    else if (start == -1 && string.IsNullOrWhiteSpace(line))
                    {
                        Lines.RemoveAt(CursorY);
                        CursorY--;
                        if (CursorY >= 0 && Lines.Count >= 0)
                        {
                            line = Lines[CursorY];

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
                        Edited = true;
                        RerenderNeeded = true;
                    }
                    else if (start == -1 && CursorY > 0)
                    {
                        Lines.RemoveAt(CursorY);
                        var oldlen = Lines[CursorY - 1].Length;
                        Lines[CursorY - 1] += line;
                        var newline = Lines[CursorY - 1];
                        CursorY--;
                        if (newline.Length < Width)
                        {
                            X = 0;
                            CursorX = oldlen;
                        }
                        else
                        {
                            X = newline.Length - Width;
                            CursorX = oldlen;
                        }
                        Edited = true;
                        RerenderNeeded = true;
                    }
                }
                break;
            case ConsoleKey k when Editable:
                {
                    var line = Lines[CursorY];
                    Lines[CursorY] = line.Insert(CursorX, keyinfo.KeyChar.ToString());
                    Edited = true;
                    CursorX++;
                    RerenderNeeded = true;
                }
                break;

        }
        int rawx = CursorX, rawy = CursorY;
        if (CursorY - Y >= Height && CursorY - Y < Lines.Count)
        {
            Y++;
            RerenderNeeded = true;
        }
        else if (CursorY - Y < 0)
        {
            Y--;
            RerenderNeeded = true;
        }
        CursorY.Clamp(0, Lines.Count);
        int curycl = CursorY;
        CursorX.Clamp(0, Lines[CursorY].Length + 1);
        if (CursorX - X >= Width && Lines[CursorY].Length >= Width && CursorX <= Lines[CursorY].Length)
        {
            X++;
            RerenderNeeded = true;
        }
        else if (CursorX - X < 0)
        {
            X = CursorX - Width;
            RerenderNeeded = true;
        }
        var xb = X;
        X.Clamp(0, int.MaxValue);
        Y.Clamp(0, Lines.Count);
        int actualX = CursorX - X;
        actualX.Clamp(0, Width);
        actualX += buf.OffsetX;
        DebugLine.Instance.Debug = $"{CursorY} {CursorX}";
        int actualY = buf.OffsetY + (CursorY - Y);
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
        int actualX = CursorX - X;
        actualX.Clamp(0, Width);
        actualX += buf.OffsetX;
        int actualY = buf.OffsetY + CursorY;
        Console.CursorLeft = actualX;
        Console.CursorTop = actualY;
    }
}

