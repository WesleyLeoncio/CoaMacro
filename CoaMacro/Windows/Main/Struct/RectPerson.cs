using System.Runtime.InteropServices;

namespace CoaMacro.Windows.Main.Struct;

[StructLayout(LayoutKind.Sequential)]
public struct RectPerson
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}