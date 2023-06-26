using Discout.Objects.Components;
using Discout.Objects.Components.Bricks;
using Discout.Objects.Components.Bricks.Types;
using Discout.renderer.Windows;
using Discout.Renderer;
using Discout.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Discout.Objects;

public class Level
{
    protected List<Brick> bricks = new(114);
    protected Ball ball;

    protected class BrickData
    {
        public string Colour { get; set; } = "";
        public BrickType Type { get; set; } = BrickType.NORMAL;
        [JsonPropertyName("Repeat")]
        public int Loop { get; set; } = 0;
    }

    protected struct LevelData
    {
        public float BaseBallSpeed { get; set; }
        public BrickData[] Bricks { get; set; }
    };

    protected enum BrickDataColour
    {
        DiscPink,
        DiscBlue,
        DiscOrange,
        DiscGreen,
    };

    private Brick MakeBrick(BrickType type, float x, float y, string colour_str)
    {
        uint colour = (BrickDataColour)Enum.Parse(typeof(BrickDataColour), colour_str) switch
        {
            BrickDataColour.DiscPink => 3,
            BrickDataColour.DiscBlue => 4,
            BrickDataColour.DiscOrange => 5,
            BrickDataColour.DiscGreen => 6,
            _ => throw new ArgumentException("a colour provided in the level is not one implemented in the gmae"),
        };
        return type switch
        {
            BrickType.AIR => new NormalBrick(x, y, colour, this),
            BrickType.NORMAL => new NormalBrick(x, y, colour, this),
            BrickType.LIFE => new LifeBrick(x, y, this),
            _ => throw new ArgumentException("a type provided in the level is not one implemented in the gmae"),
        };
    }

    private void AddBrick(BrickData brick, ref float x, ref float y)
    {
        if (bricks.Count >= 114) throw new ArgumentException("trying to add to many brings to the live");
        Brick created = MakeBrick(brick.Type, x, y, brick.Colour);
        bricks.Add(created);
        x += 6f;
        if (x > 39.5f)
        {
            x = -39f;
            y -= 6;
        }
    }

    public Level(string json_level)
    {
        LevelData data = JsonSerializer.Deserialize<LevelData>(AssetUtil.ReadEmbeddedFile(json_level));
        ball = new(data.BaseBallSpeed);
        float x = -39f;
        float y = 100;
        foreach (BrickData brick in data.Bricks)
        {
            if (brick.Loop == 0)
                AddBrick(brick, ref x, ref y);
            else for (int i = 0; i < brick.Loop; i++)
                    AddBrick(brick, ref x, ref y);
        }
    }

    public Ball GetBall() => ball;
    public List<Brick> GetBricks() => bricks;

    public sbyte bricks_to_gc = 5;
    public void OnBrickDeath(object sender, EventArgs e)
    {
        bricks.Remove((Brick)sender);
        bricks_to_gc--;
        if (bricks_to_gc < 0)
        {
            bricks_to_gc = 5;
            GC.Collect();
        }
    }

    public void OnRendering(object sender)
    {
        foreach (Brick brick in bricks)
            brick.OnRendering(sender);
        ((QuadBatchRenderer)sender).Flush();
        ball.OnRendering(sender);
        ((QuadBatchRenderer)sender).FlushAntiGhost();
    }

    public void OnUpdate()
    {
        if (bricks.Count <= 0)
        {
            DiscWindow.Instance.LevelWon();
            return;
        }
        ball.OnUpdate(this);
    }
}
