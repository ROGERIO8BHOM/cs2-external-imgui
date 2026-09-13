public enum KeyMode
{
    Hold,
    Toggle,
    Always
}

public class KeybindSetting : Setting
{
    public int Key { get; set; }

    public KeyMode Mode { get; set; }

    private bool _active;

    public KeybindSetting(
        string name,
        int key = 0,
        KeyMode mode = KeyMode.Toggle,
        string description = "")
        : base(name, description)
    {
        Key = key;
        Mode = mode;
    }

    /// <summary>
    /// Deve ser chamado uma vez por frame.
    /// </summary>
    public void Update()
    {
        switch (Mode)
        {
            case KeyMode.Always:
                _active = true;
                break;

            case KeyMode.Hold:
                _active = Input.IsDown(Key);
                break;

            case KeyMode.Toggle:
                if (Input.IsPressed(Key))
                    _active = !_active;
                break;
        }
    }

    public bool IsActive()
    {
        return _active;
    }

    public bool IsDown()
    {
        return Input.IsDown(Key);
    }

    public bool IsPressed()
    {
        return Input.IsPressed(Key);
    }

    public bool IsReleased()
    {
        return Input.IsReleased(Key);
    }

    public void Reset()
    {
        _active = false;
    }
}