public class SliderSetting : Setting
{
    private float _value;
    public float Value { get => _value; set
        {
            if (!Equals(_value, value))
            {
                _value = value;
            }
        }
    }

    public float Min { get; }
    public float Max { get; }
    public float Step { get; }

    public SliderSetting(
        string name,
        float value,
        float min,
        float max,
        float step = 1f,
        string description = "")
        : base(name, description)
    {
        Value = value;
        Min = min;
        Max = max;
        Step = step;
    }
}