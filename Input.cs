public static class Input
{
    private static readonly bool[] _current = new bool[256];
    private static readonly bool[] _previous = new bool[256];

    public static void Update()
    {
        Array.Copy(_current, _previous, _current.Length);

        for (int i = 0; i < _current.Length; i++)
            _current[i] = (CS2.Win32.GetAsyncKeyState(i) & 0x8000) != 0;
    }

    public static bool IsDown(int key)
        => IsValidKey(key) && (CS2.Win32.GetAsyncKeyState(key) & 0x8000) != 0;

    public static bool IsPressed(int key)
        => IsValidKey(key) && _current[key] && !_previous[key];

    public static bool IsReleased(int key)
        => IsValidKey(key) && !_current[key] && _previous[key];

    private static bool IsValidKey(int key)
        => key >= 0 && key < _current.Length;
}
