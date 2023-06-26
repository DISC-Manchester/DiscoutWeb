namespace Discout.Utils;
public struct Vertex
{
    public float X { get; set; } = 0f;
    public float Y { get; set; } = 0f;
    public float ColorID { get; set; } = 0f;
    public float UvID { get; set; } = 0f;

    public Vertex() { }
}

public struct Aabb
{
    public float MinX { get; set; }
    public float MinY { get; set; }
    public float MaxX { get; set; }
    public float MaxY { get; set; }

    public Aabb(ref Vertex[] quad)
    {
        MinX = (quad[3].X + 1f) * 1000 / 2;
        MaxX = (quad[0].X + 1f) * 1000 / 2;
        MinY = (quad[3].Y + 1f) * 1000 / 2;
        MaxY = (quad[0].Y + 1f) * 1000 / 2;
    }
}
