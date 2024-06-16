using Models;
using System;
[Serializable]
public class Coords
{
    public int x { get; set; }
    public int y { get; set; }
    public Coords(int? _x, int? _y)
    {
        x = (int)_x;
        y = (int)_y;
    }
    public Coords AddCoords(Coords coord)
    {
        return new Coords(GridManager.i.WrapAround(x + coord.x), GridManager.i.WrapAround(y + coord.y));
    }
    public bool CoordsEquals(Coords coord)
    {
        return x == coord.x && y == coord.y;
    }
}