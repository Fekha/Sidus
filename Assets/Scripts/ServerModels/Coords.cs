using System;
[Serializable]
public class Coords
{
    public int x { get; set; }
    public int y { get; set; }
    public Coords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}