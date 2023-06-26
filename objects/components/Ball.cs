using Avalonia.Input;
using Discout.Objects.Components.Bricks;
using Discout.renderer.Windows;
using Discout.Renderer;
using Discout.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Discout.Objects.Components;

public class Ball
{
    private int LastScore = 0;
    public int Score { get; set; }
    private Vector2 Position;
    private Vector2 Velocity;
    private Vertex[] Vertices;
    private float Speed;
    private readonly float LaunchSpeed;
    private bool Released = false;
    private int CoolDown = 0;
    public Ball(float speed)
    {
        LaunchSpeed = Speed = speed;
        Position = new(DiscWindow.Instance.Paddle.GetPositionX(), -150f);
        Position.X += 0;
        Position.Y -= 10;
        Vertices = VertexUtils.PreMakeQuad(0, 0, 0, 0, 0f);
    }

    public void ResetBall()
    {
        Velocity = Vector2.Zero;
        Released = false;
    }

    public int GetScore()
    {
        LastScore = Score;
        Score = 0;
        return LastScore;
    }

    public bool IsAlive() => Released;

    public void OnKeyDown(HashSet<Key> keys)
    {
        if (keys.Contains(Key.Space) && !Released)
        {
            SoundUtils.PlaySound(/*SoundUtils.CLICK_SOUND*/ null);
            DiscWindow.Instance.DisplayText.Text = "";
            Velocity.Y = LaunchSpeed;
            Velocity.X = Random.Shared.NextSingle() > 0.5f ? -LaunchSpeed : LaunchSpeed;
            Released = true;
        }
    }

    public void OnUpdate(Level level)
    {
        if (Released is false)
        {
            Position = new(DiscWindow.Instance.Paddle.GetPositionX(), -150f);
            Position.X *= 10;
            Position.Y += 5;
            VertexUtils.UpdateQuad(Position.X, Position.Y, 0.03f, 0.03f, ref Vertices,0);
            return;
        }

        Position += Velocity;
        Position.X = Math.Clamp(Position.X, -171, 171);
        Position.Y = Math.Clamp(Position.Y, -171, 171);

        if (DiscWindow.Instance.Paddle.DoseFullIntersects(ref Vertices))
        {
            SoundUtils.PlaySound(/*SoundUtils.BOUNCE_SOUND*/ null);
            Velocity.Y = Math.Abs(Velocity.Y);
        }
        else if (Position.Y >= 170)
        {
            SoundUtils.PlaySound(/*SoundUtils.BOUNCE_SOUND*/ null);
            Velocity.Y = -Math.Abs(Velocity.Y);
        }
        else if (Position.X <= -170)
        {
            SoundUtils.PlaySound(/*SoundUtils.BOUNCE_SOUND*/ null);
            Velocity.X = Math.Abs(Velocity.X);
        }
        else if (Position.X >= 170)
        {
            SoundUtils.PlaySound(/*SoundUtils.BOUNCE_SOUND*/ null);
            Velocity.X = -Math.Abs(Velocity.X);
        }
        else
        {
            foreach (Brick brick in level.GetBricks())
            {
                if (brick.DoseFullIntersects(ref Vertices))
                {
                    SoundUtils.PlaySound(/*SoundUtils.BOUNCE_SOUND*/ null);
                    if (CoolDown == 0)
                    {
                        CoolDown = 5;
                        brick.Die();
                        Speed *= 1.1f;
                        Score++;
                        if (brick.GetBrickType() == BrickType.LIFE)
                            DiscWindow.Instance.Paddle.AddLife();
                        Velocity.Y = -(Math.Abs(Velocity.Y) + Speed);
                    }
                    else
                        Velocity.Y = -Math.Abs(Velocity.Y);

                    if (Random.Shared.NextSingle() <= 0.2f)
                    {
                        if (Random.Shared.NextSingle() <= 0.5f)
                            Velocity.X -= Random.Shared.NextSingle() > 0.5f ? 0 : Speed;
                        else
                            Velocity.X += Random.Shared.NextSingle() > 0.5f ? 0 : Speed;
                    }
                    else
                        Velocity.X = Random.Shared.NextSingle() > 0.25f ? Velocity.X + Speed : Math.Abs(Velocity.X) + Speed;

                    Velocity.X = Math.Clamp(Velocity.X, -LaunchSpeed * 2, LaunchSpeed * 2);
                    Velocity.Y = Math.Clamp(Velocity.Y, -LaunchSpeed * 2, LaunchSpeed * 2);
                    break;
                }
            }
        }

        if (CoolDown != 0)
            CoolDown--;

        if (Velocity.Y == 0)
            Velocity.Y += LaunchSpeed;

        VertexUtils.UpdateQuad(Position.X, Position.Y, 0.03f, 0.03f, ref Vertices,0);

        if (Position.Y <= -170)
        {
            ResetBall();
            LastScore = Score;
            Speed = LaunchSpeed;
            DiscWindow.Instance.Paddle.ResetPaddle();
        }
    }
    public void OnRendering(object sender)
    {
        DiscWindow.Instance.ScoreText.Text = "Score: " + Score.ToString();
        ((QuadBatchRenderer)sender).AddQuad(Vertices);
    }
}
