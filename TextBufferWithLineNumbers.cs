using System;
namespace Pico;

public class TextBufferWithLineNumbers : BufferBase
{
    public TextBuffer Child { get; set; }
    public override string Name => Child.Name;
    public bool EnabledNumbers = true;
    public bool Edited => Child.Edited;
    private int size = 2;
    public TextBufferWithLineNumbers(TextBuffer child, int w, int h)
    {
        Child = child;
        (Width, Height) = (w, h);
        child.Width = w - 3;
        child.Height = h;
    }
    public override void Input(CharBuffer buffer, ConsoleKeyInfo keyInfo)
    {
        bool redirectToChild = true;
        if (keyInfo.Key == ConsoleKey.N && (keyInfo.Modifiers & ConsoleModifiers.Control) > 0)
        {
            RerenderNeeded = true;
            EnabledNumbers = !EnabledNumbers;
            redirectToChild = false;
        }
        if (EnabledNumbers)
        {
            Child.Width = Width - size;
            Child.Height = Height;
            if (redirectToChild)
                Child.Input(new CharBuffer(buffer.Buffer, buffer.OffsetX + size, buffer.OffsetY, buffer.Width - size, buffer.Height, buffer.ScreenWidth), keyInfo);
        }
        else
        {

            Child.Width = Width;
            Child.Height = Height;
            if (redirectToChild)
                Child.Input(buffer, keyInfo);
        }
        RerenderNeeded = RerenderNeeded || Child.RerenderNeeded;
    }
    public override void Render(CharBuffer buffer)
    {
        if (EnabledNumbers)
        {
            size = Child.Lines.Count.ToString().Length + 1;
            var format = "D" + (size - 1).ToString();
            Child.Width = Width - size;
            Child.Height = Height;
            Child.Render(new CharBuffer(buffer.Buffer, buffer.OffsetX + size, buffer.OffsetY, buffer.Width - size, buffer.Height, buffer.ScreenWidth));
            for (int i = 0; i < Child.Lines.Count && i < Height; i++)
            {
                var istr = (i + 1 + Child.Y).ToString(format);
                for (int x = 0; x < istr.Length; x++)
                {
                    buffer[x, i] = istr[x];
                }
                buffer[istr.Length, i] = ' ';
            }
        }
        else
        {
            Child.Width = Width;
            Child.Height = Height;
            Child.Render(buffer);
        }
        RerenderNeeded = false;
    }

}

