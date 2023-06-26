using System;
namespace Discout.Objects.Components.Bricks.Types;
public class LifeBrick : ChangeableBrick
{
    private int wait = 0;
    public LifeBrick(float x, float y, Level level) : base(x, y, 0.0f, level)
    {
    }

    public override BrickType GetBrickType() => BrickType.LIFE;

    public override void OnRendering(object sender)
    {
        OnUpdate();
        base.OnRendering(sender);
    }

    public override void OnUpdate()
    {
        if (wait < 10)
        {
            wait++;
            return;
        }
        wait = 0;

        UpdateColour(Random.Shared.NextSingle() * 10 % 5);
    }
}
