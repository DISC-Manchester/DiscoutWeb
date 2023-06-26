using System;
using System.IO;
using System.Reflection;
using System.Text;
using OpenTK.Mathematics;

namespace Discout.Utils;

internal static class SoundUtils
{
    /*private static readonly Stream BOUNCE_DATA = AssetUtil.OpenEmbeddedFile("sounds.bounce.wav");
    public static readonly WaveFileReader BOUNCE_SOUND = new WaveFileReader(BOUNCE_DATA);
    private static readonly Stream CLICK_DATA = AssetUtil.OpenEmbeddedFile("sounds.click.wav");
    public static readonly WaveFileReader CLICK_SOUND = new WaveFileReader(CLICK_DATA);*/

    public static void PlaySound(object? stream)
    {
       /* ThreadPool.QueueUserWorkItem(delegate
        {
            using (WaveOutEvent player = new())
            {
                player.Init(stream);
                player.Play();
                while (player.PlaybackState != PlaybackState.Stopped) ;
                player.Dispose();
            }
        });
        if (stream.GetType() == typeof(WaveFileReader))
            ((WaveFileReader)stream).Position = 0;*/
    }

    public static void CleanUp()
    {
        /*BOUNCE_SOUND.Dispose();
        BOUNCE_DATA.Dispose();
        CLICK_SOUND.Dispose();
        CLICK_DATA.Dispose();*/
    }
}

internal static class VertexUtils
{
    // pre calculate the vertex so that we don't need to spent cpu cycles at rime-time to calculate the vertex
    // the can be compliantly expensive 
    public static Vertex[] PreMakeQuad(float x, float y, float width, float height, float color)
    {
        var quad = new Vertex[6]; // Use 6 vertices for a quad triangle strip
        UpdateQuad(x, y, width, height, ref quad, 0);
        quad[0].ColorID = color;
        quad[0].UvID = 0;
        quad[1].ColorID = color;
        quad[1].UvID = 1;
        quad[2].ColorID = color;
        quad[2].UvID = 2;
        quad[3].ColorID = color;
        quad[3].UvID = 3;
        quad[4].ColorID = color;
        quad[4].UvID = 2;
        quad[5].ColorID = color;
        quad[5].UvID = 1;
        return quad;
    }

    // pre calculate the vertex so that we don't need to spent cpu cycles at rime-time to calculate the vertex
    // the can be compliantly expensive 
    public static void PreMakeQuad(float x, float y, float width, float height, float color, ref Vertex[] vertices, int index)
    {
        UpdateQuad(x, y, width, height, ref vertices, index);
        vertices[index].ColorID = color;
        vertices[index].UvID = 0;
        vertices[index + 1].ColorID = color;
        vertices[index + 1].UvID = 1;
        vertices[index + 2].ColorID = color;
        vertices[index + 2].UvID = 2;
        vertices[index + 3].ColorID = color;
        vertices[index + 3].UvID = 3;
        vertices[index + 4].ColorID = color;
        vertices[index + 4].UvID = 2;
        vertices[index + 5].ColorID = color;
        vertices[index + 5].UvID = 1;
        index += 6;
    }

    // pre calculate the vertex so that we don't need to spent cpu cycles at rime-time to calculate the vertex
    // the can be compliantly expensive 
    public static Vertex[] UpdateQuad(float x, float y, float width, float height, ref Vertex[] quad, int index)
    {
        // Apply model transformation
        Matrix4 modelTransform = Matrix4.Identity;
        modelTransform *= Matrix4.CreateTranslation(new Vector3(x, y, -1.0f));
        modelTransform *= Matrix4.CreateScale(new Vector3(width, height, 1.0f));

        Vector4 transform_0 = new Vector4(-2.8099866f, -2.414214f, -4.1981983f, 1f) * modelTransform;
        quad[index].X = transform_0.X / transform_0.Z;
        quad[index].Y = transform_0.Y / transform_0.Z;

        Vector4 transform_1 = new Vector4(2.8099866f, -2.414214f, -4.1981983f, 1f) * modelTransform;
        quad[index + 1].X = transform_1.X / transform_1.Z;
        quad[index + 1].Y = transform_1.Y / transform_1.Z;

        Vector4 transform_2 = new Vector4(-2.8099866f, 2.414214f, -4.1981983f, 1f) * modelTransform;
        quad[index + 2].X = transform_2.X / transform_2.Z;
        quad[index + 2].Y = transform_2.Y / transform_2.Z;

        Vector4 transform_3 = (new Vector4(2.8099866f, 2.414214f, -4.1981983f, 1f) * modelTransform);
        quad[index + 3].X = transform_3.X / transform_3.Z;
        quad[index + 3].Y = transform_3.Y / transform_3.Z;

        Vector4 transform_4 = (new Vector4(-2.8099866f, 2.414214f, -4.1981983f, 1f) * modelTransform);
        quad[index + 4].X = transform_4.X / transform_4.Z;
        quad[index + 4].Y = transform_4.Y / transform_4.Z;

        Vector4 transform_5 = (new Vector4(2.8099866f, -2.414214f, -4.1981983f, 1f) * modelTransform);
        quad[index + 5].X = transform_5.X / transform_5.Z;
        quad[index + 5].Y = transform_5.Y / transform_5.Z;
        return quad;
    }
}

internal static class CollisonUtil
{
    public static bool TestAABB(Aabb obj_0, Aabb obj_1)
    {
        // Check if there is a gap between the two AABBs along any axis
        if (obj_0.MaxX < obj_1.MinX || obj_0.MinX > obj_1.MaxX) return false; // No overlap along X-axis
        if (obj_0.MaxY < obj_1.MinY || obj_0.MinY > obj_1.MaxY) return false; // No overlap along Y-axis
        return true; // Overlapping along both X-axis and Y-axis
    }
}

internal static class AssetUtil
{
    private static readonly Assembly exe_base = typeof(App).GetTypeInfo().Assembly;

    public static string ReadEmbeddedFile(string path)
    {
        using Stream? stream = exe_base.GetManifestResourceStream("Discout.game_assets." + path) ?? throw new FieldAccessException("could not find embedded file:" + path);
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }


    public static Stream OpenEmbeddedFile(string path)
        => exe_base.GetManifestResourceStream("Discout.game_assets." + path) ?? throw new FieldAccessException("could not find embedded file:" + path);

    public static Stream OpenFile(string path, bool create = true)
        => File.Open(Path.GetDirectoryName(exe_base.Location) + '/' + path, create ? FileMode.OpenOrCreate : FileMode.Open);

}
