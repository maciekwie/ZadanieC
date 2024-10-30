using UnityEngine;

public class DataEntry
{
    public byte[] Image { get; set; }

    public int Label { get; set; }

    public override string ToString()
    {
        return "Label: " + this.Label;
    }

}
