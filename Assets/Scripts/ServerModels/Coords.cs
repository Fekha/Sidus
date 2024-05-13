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
    public Coords Add(Coords coord)
    {
        return new Coords(x + coord.x, y + coord.y);
    }
    public bool Equals(Coords coord)
    {
        return x == coord.x && y == coord.y;
    }
}