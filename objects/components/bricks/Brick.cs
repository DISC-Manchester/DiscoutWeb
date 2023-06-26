using Discout.Renderer;
using Discout.Utils;
using System;
namespace Discout.Objects.Components.Bricks;

public abstract class Brick
{
    protected Vertex[] Vertices;
    private Aabb hitbox;
    public delegate void OnBrickDeathEventHandler(object sender, EventArgs e);
    public event OnBrickDeathEventHandler OnBrickDeath;
    public abstract BrickType GetBrickType();
    protected Brick(float x, float y, float colour, Level level)
    {
        Vertices = VertexUtils.PreMakeQuad(x,y,0.1225f, 0.04f, colour);
        hitbox = new(ref Vertices);
        OnBrickDeath += level.OnBrickDeath;
    }

    public void Die() => OnBrickDeath(this, EventArgs.Empty);
    public virtual void OnRendering(object sender) => ((QuadBatchRenderer)sender).AddQuad(Vertices);
    public bool DoseFullIntersects(ref Vertex[] testing_quad) => CollisonUtil.TestAABB(hitbox, new(ref testing_quad));
}
