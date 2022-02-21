using System;
namespace Pico;

public class Border : BufferBase
{
    public BufferBase Child;
    public Border(BufferBase child, int width, int height)
    {
        Child = child;
        (Width, Height) = (width, height);
    }
    public override void Input(CharBuffer buf, ConsoleKeyInfo keyInfo)
    {
        Child.Input(buf, keyInfo);
        RerenderNeeded = RerenderNeeded || Child.RerenderNeeded;
    }
    public override void Render(CharBuffer buffer)
    {
        Child.Width = Width - 2;
        Child.Height = Height - 2;
        Child.Render(new CharBuffer(buffer.Buffer, buffer.OffsetX + 1, buffer.OffsetY + 1, buffer.Width - 2, buffer.Height - 2, buffer.ScreenWidth));
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                    buffer[x, y] = '#';

        RerenderNeeded = false;
    }
}

