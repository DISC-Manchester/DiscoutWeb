using Avalonia.OpenGL;
using Discout.Utils;
using System;
using System.IO;
using static Avalonia.OpenGL.GlConsts;
namespace Discout.Renderer;

public sealed class QuadBatchRenderer : IDisposable
{
    private readonly GlInterface GL;
    private readonly int vbo;
    private readonly int vao;
    private readonly int sid;
    private readonly int sid_anti_ghost;
    private int tex_id;
    private readonly Vertex[] quadVertices = new Vertex[714];
    private int vertex_index = 0;


    private static int CreateShader(GlInterface GL, string vertex_source, string fragment_source)
    {
        int vid = GL.CreateShader(GL_VERTEX_SHADER);
        string vertexShaderInfoLog = GL.CompileShaderAndGetError(vid, vertex_source)!;
        if (!string.IsNullOrWhiteSpace(vertexShaderInfoLog))
            throw new FileLoadException($"Vertex shader compilation failed: {vertexShaderInfoLog}");

        int fid = GL.CreateShader(GL_FRAGMENT_SHADER);
        string fragmentShaderInfoLog = GL.CompileShaderAndGetError(fid, fragment_source)!;
        if (!string.IsNullOrWhiteSpace(fragmentShaderInfoLog))
            throw new FileLoadException($"Fragment shader compilation failed: {fragmentShaderInfoLog}");

        int id = GL.CreateProgram();
        GL.AttachShader(id, vid);
        GL.AttachShader(id, fid);
        string programLinkInfoLog = GL.LinkProgramAndGetError(id)!;
        if (!string.IsNullOrWhiteSpace(programLinkInfoLog))
            Console.WriteLine($"Shader program linking has problems: {programLinkInfoLog}");
        GL.DeleteShader(vid);
        GL.DeleteShader(fid);
        return id;
    }

    public QuadBatchRenderer(GlInterface gl)
    {
        Console.WriteLine("load gl");
        GL = gl;
        string vertex_source = "#version 330 core\r\n// input\r\nlayout(location = 0) in vec2 position;\r\nlayout(location = 1) in float colour;\r\nlayout (location = 2) in float uv;\r\n// output\r\nout vec3 vertex_colour;\r\nout vec2 texture_coord;\r\n\r\n//colour look up\r\nvec3 colours[] = vec3[](vec3(255,255,255),vec3(157, 6, 241),vec3(255, 127, 80),vec3(251, 0, 250),vec3(0, 192, 237),vec3(249, 185, 0),vec3(0, 238, 0));\r\n\r\nvec2 uvs[] = vec2[](vec2(0.0, 0.0),vec2(1.0, 0.0),vec2(0.0, 1.0),vec2(1.0, 1.0));\r\n\r\nvoid main()\r\n{\r\n    gl_Position = vec4(position, 0.0, 1.0);\r\n    vertex_colour = normalize(colours[int(colour)]);\r\n    texture_coord = uvs[int(uv)];\r\n}";
        string fragment_source_basic = "#version 330 core\r\n\r\n// input\r\nin vec3 vertex_colour;\r\nin vec2 texture_coord; //mask texture_coord\r\nuniform sampler2D mask;\r\n// output\r\nout vec4 frag_colour;\r\n\r\nvoid main()\r\n{\r\n        precision highp float;\r\n\r\n        vec4 texColor = texture2D(mask, texture_coord);\r\n\r\n        if (texColor.a < 0.9)\r\n            discard;\r\n\r\n        float grayscale = dot(texColor.rgb, vec3(0.2126, 0.7152, 0.0722));\r\n        vec3 grayscaleColor = vec3(grayscale, grayscale, grayscale);\r\n        \r\n        frag_colour = vec4(vertex_colour - grayscaleColor,texColor.a);\r\n}";
        string fragment_source_ghost = "#version 330 core\r\n\r\n// input\r\nin vec3 vertex_colour;\r\nin vec2 texture_coord;\r\nuniform sampler2D mask;\r\n// output\r\nout vec4 frag_colour;\r\n\r\nvoid main()\r\n{\r\n    precision highp float;\r\n\r\n    vec4 texColor = texture2D(mask, texture_coord);\r\n\r\n    if (texColor.a < 0.9)\r\n        discard;\r\n    frag_colour = vec4(vertex_colour,texColor.a);\r\n}";
        sid = CreateShader(gl, vertex_source, fragment_source_basic);
        sid_anti_ghost = CreateShader(gl, vertex_source, fragment_source_ghost);
        unsafe
        {
            fixed (int* ptr = &tex_id)
                GL.GenTextures(1, ptr);
            GL.ActiveTexture(GL_TEXTURE0);
            GL.BindTexture(GL_TEXTURE_2D, tex_id);
            byte[] texture = new byte[1045]{
    0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49,
    0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0xa0, 0x00, 0x00, 0x00, 0x14, 0x08, 0x06,
    0x00, 0x00, 0x00, 0x0a, 0x94, 0x36, 0x4e, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48,
    0x59, 0x73, 0x00, 0x00, 0x0b, 0x13, 0x00, 0x00, 0x0b, 0x13, 0x01, 0x00, 0x9a,
    0x9c, 0x18, 0x00, 0x00, 0x03, 0xc7, 0x49, 0x44, 0x41, 0x54, 0x68, 0x81, 0xed,
    0x9a, 0x4f, 0x6e, 0xdb, 0x46, 0x14, 0x87, 0x7f, 0x24, 0x67, 0x86, 0xff, 0x34,
    0x96, 0x96, 0x6d, 0xe3, 0x55, 0x51, 0xf4, 0x02, 0x6d, 0x21, 0x78, 0x23, 0xc0,
    0x37, 0x70, 0x90, 0xb8, 0xa7, 0xe8, 0xa6, 0x05, 0xea, 0x20, 0x3d, 0x40, 0x10,
    0xb8, 0xeb, 0x5e, 0xa2, 0x0e, 0xda, 0xf4, 0x04, 0x06, 0xb4, 0xb1, 0x0c, 0xa4,
    0x28, 0xd0, 0x75, 0xba, 0x8b, 0x91, 0xad, 0x29, 0x51, 0x43, 0x72, 0x66, 0x48,
    0x76, 0xa3, 0x47, 0x50, 0x94, 0xd2, 0x78, 0xd5, 0x8d, 0xe6, 0x03, 0x06, 0x20,
    0x0c, 0x93, 0xda, 0x7c, 0x98, 0x37, 0xef, 0x37, 0xcf, 0x6b, 0xdb, 0x16, 0x43,
    0x4e, 0x4e, 0x4e, 0x44, 0x18, 0x86, 0x67, 0x42, 0x88, 0x33, 0x21, 0xc4, 0x09,
    0xe7, 0xfc, 0x33, 0xc6, 0x98, 0x60, 0x8c, 0xc1, 0xf7, 0x7d, 0xf8, 0xbe, 0x0f,
    0xcf, 0xf3, 0xba, 0xe5, 0x38, 0x4c, 0xc8, 0x9d, 0xb6, 0x6d, 0xd1, 0xb6, 0x2d,
    0x9a, 0xa6, 0x41, 0xd3, 0x34, 0xb0, 0xd6, 0xc2, 0x5a, 0x9b, 0x1b, 0x63, 0xde,
    0x69, 0xad, 0xff, 0xd2, 0x5a, 0xff, 0x5e, 0x55, 0xd5, 0x1f, 0x37, 0x37, 0x37,
    0x7a, 0xf8, 0x0d, 0x6f, 0x28, 0xe0, 0x6c, 0x36, 0x7b, 0x1c, 0xc7, 0xf1, 0x65,
    0x14, 0x45, 0x5f, 0x44, 0x51, 0x84, 0x30, 0x0c, 0x21, 0x84, 0x00, 0xe7, 0x1c,
    0x8c, 0x31, 0x04, 0x41, 0xe0, 0x04, 0x74, 0x74, 0x90, 0x7c, 0x24, 0x60, 0x5d,
    0xd7, 0xb0, 0xd6, 0xc2, 0x18, 0x03, 0xad, 0x35, 0xaa, 0xaa, 0x42, 0x59, 0x96,
    0x28, 0xcb, 0xf2, 0x6d, 0x51, 0x14, 0xcf, 0xe6, 0xf3, 0xf9, 0x6f, 0xfd, 0xf7,
    0x3b, 0x01, 0xa7, 0xd3, 0x69, 0x90, 0x24, 0xc9, 0x8b, 0x34, 0x4d, 0x2f, 0xd2,
    0x34, 0x45, 0x92, 0x24, 0x88, 0xe3, 0x18, 0x24, 0x21, 0xe7, 0x1c, 0x9c, 0x73,
    0x04, 0x41, 0x80, 0x20, 0x08, 0x9c, 0x80, 0x0e, 0x00, 0xdb, 0x02, 0xd6, 0x75,
    0x8d, 0xba, 0xae, 0x77, 0xe4, 0x2b, 0x8a, 0x02, 0x45, 0x51, 0x60, 0xbd, 0x5e,
    0x63, 0xbd, 0x5e, 0x5f, 0x2a, 0xa5, 0x7e, 0xba, 0xbd, 0xbd, 0xad, 0x01, 0x80,
    0xd1, 0x87, 0x92, 0x24, 0x79, 0x21, 0xa5, 0xbc, 0x18, 0x8d, 0x46, 0xa0, 0x45,
    0x12, 0x86, 0x61, 0xd8, 0x49, 0xe8, 0x04, 0x74, 0xf4, 0xf9, 0x90, 0x80, 0x55,
    0x55, 0xa1, 0xaa, 0xaa, 0x4e, 0x3c, 0x21, 0x04, 0x55, 0xcf, 0x8b, 0xcd, 0xab,
    0xcf, 0x80, 0xcd, 0x0e, 0x38, 0x9b, 0xcd, 0x9e, 0x4a, 0x29, 0xaf, 0xc6, 0xe3,
    0x31, 0x8e, 0x8e, 0x8e, 0x20, 0xa5, 0x84, 0x94, 0x12, 0x69, 0x9a, 0x22, 0x8e,
    0xe3, 0x4e, 0x42, 0x2a, 0xc3, 0x74, 0x0e, 0x04, 0xe0, 0x04, 0x3c, 0x70, 0x86,
    0x25, 0xd8, 0x5a, 0x0b, 0xad, 0x35, 0xb4, 0xd6, 0xdd, 0xce, 0x97, 0xe7, 0x39,
    0xf2, 0x3c, 0xc7, 0x6a, 0xb5, 0xc2, 0x72, 0xb9, 0x44, 0x96, 0x65, 0x58, 0xad,
    0x56, 0xe7, 0xf3, 0xf9, 0xfc, 0x95, 0x37, 0x9d, 0x4e, 0x85, 0x94, 0xf2, 0x9f,
    0xc9, 0x64, 0x72, 0x3c, 0x99, 0x4c, 0x30, 0x1e, 0x8f, 0x31, 0x1e, 0x8f, 0x21,
    0xa5, 0xdc, 0xd9, 0x05, 0x85, 0x10, 0x4e, 0x40, 0xc7, 0x16, 0x74, 0x84, 0xeb,
    0x37, 0x20, 0x54, 0x7e, 0x8b, 0xa2, 0x80, 0x52, 0xaa, 0x93, 0x2f, 0xcb, 0x32,
    0x64, 0x59, 0x86, 0xfb, 0xfb, 0x7b, 0x64, 0x59, 0x76, 0xb7, 0x5c, 0x2e, 0x3f,
    0x67, 0x9c, 0xf3, 0xf3, 0x30, 0x0c, 0x8f, 0xa3, 0x28, 0x42, 0x92, 0x24, 0x48,
    0xd3, 0x14, 0xa3, 0xd1, 0xa8, 0x13, 0x90, 0xce, 0x83, 0x43, 0x01, 0xfb, 0xe5,
    0xd7, 0x49, 0x78, 0x98, 0xec, 0xeb, 0x82, 0xfb, 0x02, 0xd2, 0x91, 0xcd, 0xf3,
    0x3c, 0xb4, 0x6d, 0xdb, 0x35, 0x27, 0x9b, 0x73, 0xe1, 0x23, 0xce, 0xf9, 0xb7,
    0x8c, 0x73, 0x7e, 0x26, 0x84, 0xe8, 0xce, 0x79, 0x54, 0x72, 0xfb, 0x32, 0xf6,
    0x05, 0xe4, 0x9c, 0xef, 0x08, 0xe8, 0x38, 0x6c, 0xfa, 0x02, 0x52, 0x03, 0x42,
    0x9e, 0x00, 0xe8, 0xfe, 0x4e, 0xbb, 0x62, 0xcf, 0xa5, 0x33, 0x16, 0x04, 0xc1,
    0x37, 0x8c, 0x31, 0x70, 0xce, 0x21, 0x84, 0x40, 0x5f, 0xc6, 0x28, 0x8a, 0xb6,
    0x96, 0x2b, 0xc1, 0x8e, 0x21, 0xc3, 0x12, 0x4c, 0x51, 0x9d, 0xe7, 0x79, 0x5b,
    0x3b, 0x22, 0x49, 0x37, 0x88, 0xf4, 0xbe, 0x66, 0x9e, 0xe7, 0x7d, 0x42, 0x42,
    0xf9, 0xbe, 0xdf, 0x75, 0xb9, 0xf4, 0x21, 0x7a, 0xee, 0x2f, 0xfa, 0x01, 0xc0,
    0x09, 0x78, 0xe8, 0x0c, 0x05, 0xa4, 0xe7, 0xa1, 0x3f, 0xf4, 0xdc, 0x77, 0xcd,
    0xf7, 0xfd, 0x4f, 0x7d, 0x00, 0x3b, 0xe9, 0xb4, 0xc3, 0xf1, 0x7f, 0xc1, 0xda,
    0xb6, 0x7d, 0xdf, 0x34, 0x8d, 0x24, 0x83, 0x29, 0xcb, 0xb1, 0xd6, 0x6e, 0x3d,
    0x5b, 0x6b, 0xb7, 0x6a, 0xba, 0x2b, 0xc1, 0x0e, 0x60, 0x7f, 0x17, 0x4c, 0x6b,
    0x9f, 0x4b, 0xf4, 0x7f, 0x9b, 0xf5, 0x9e, 0xd5, 0x75, 0xfd, 0xb7, 0xb5, 0xf6,
    0x4b, 0x3a, 0x3c, 0x52, 0x07, 0x43, 0x29, 0x36, 0x9d, 0xf9, 0x28, 0x68, 0x74,
    0x4d, 0x88, 0x63, 0xc8, 0xbe, 0x26, 0xa4, 0x77, 0x05, 0xd7, 0xf9, 0x44, 0x7e,
    0x19, 0x63, 0x48, 0xca, 0x37, 0xcc, 0x18, 0xf3, 0x5a, 0x6b, 0xfd, 0xb4, 0x9f,
    0x5c, 0x53, 0xe8, 0x3c, 0xec, 0x62, 0x5c, 0x0c, 0xe3, 0xe8, 0xf3, 0xb1, 0x18,
    0x46, 0x29, 0xd5, 0x2d, 0x0a, 0xa5, 0xc9, 0x33, 0x63, 0x0c, 0x8c, 0x31, 0xaf,
    0x99, 0x31, 0xe6, 0xaa, 0xaa, 0xaa, 0x97, 0x65, 0x59, 0x1e, 0x2b, 0xa5, 0xba,
    0x0e, 0x85, 0xb2, 0x1b, 0xba, 0x5a, 0x71, 0x41, 0xb4, 0x63, 0x1f, 0x0f, 0x0d,
    0xa2, 0xf3, 0x3c, 0xc7, 0x7a, 0xbd, 0x86, 0x52, 0x8a, 0x76, 0xc5, 0x3b, 0x63,
    0xcc, 0x15, 0x5b, 0x2c, 0x16, 0x7a, 0x36, 0x9b, 0xfd, 0xa0, 0x94, 0xfa, 0x75,
    0x18, 0x1c, 0xd6, 0x75, 0xdd, 0x7d, 0xcc, 0x5d, 0xc5, 0x39, 0xf6, 0xb1, 0x4f,
    0xc0, 0x7e, 0xe6, 0x47, 0x77, 0xc1, 0xab, 0xd5, 0xaa, 0x93, 0xb0, 0x28, 0x0a,
    0x94, 0x65, 0xf9, 0xfd, 0x62, 0xb1, 0xa8, 0xba, 0x69, 0x98, 0xd3, 0xd3, 0xd3,
    0x4b, 0x29, 0xe5, 0x8f, 0x1f, 0x1b, 0x46, 0x18, 0x96, 0x60, 0x27, 0xe0, 0x61,
    0xb3, 0xef, 0x2e, 0x78, 0x38, 0x8c, 0xd0, 0xdf, 0x05, 0x37, 0xd7, 0x72, 0x3f,
    0x5f, 0x5f, 0x5f, 0x5f, 0x00, 0xbd, 0x69, 0x18, 0xa5, 0xd4, 0x73, 0x00, 0x71,
    0xd3, 0x34, 0xdf, 0x51, 0xd9, 0x2d, 0xcb, 0xd2, 0x8d, 0x63, 0x39, 0xfe, 0x93,
    0x0f, 0x4d, 0xc3, 0x90, 0x84, 0x34, 0x8e, 0xa5, 0x94, 0xa2, 0x71, 0xac, 0x5f,
    0x36, 0xae, 0x01, 0xd8, 0x3f, 0x90, 0xfa, 0x24, 0x8e, 0xe3, 0x97, 0x6e, 0x20,
    0xd5, 0xf1, 0x10, 0x1e, 0x32, 0x90, 0xba, 0xd9, 0x09, 0xdf, 0x16, 0x45, 0xf1,
    0x7c, 0x3e, 0x9f, 0xbf, 0xea, 0xbf, 0xbf, 0x23, 0x20, 0xd0, 0x8d, 0xe4, 0x9f,
    0x0b, 0x21, 0x1e, 0x0b, 0x21, 0xbe, 0xe2, 0x9c, 0x3f, 0x62, 0x8c, 0x71, 0x37,
    0x92, 0xef, 0x18, 0x32, 0x14, 0xb0, 0x97, 0x05, 0x1a, 0x63, 0xcc, 0x9d, 0xd6,
    0xfa, 0xcf, 0xcd, 0x48, 0xfe, 0xd5, 0xbe, 0x91, 0xfc, 0x7f, 0x01, 0xab, 0xfd,
    0xa4, 0x3d, 0xf6, 0x0f, 0x54, 0x68, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e,
    0x44, 0xae, 0x42, 0x60, 0x82 };
            fixed (void* pdata = texture)
                GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 160, 20, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(pdata));
            GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            //GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            //GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            //GL.GenerateMipmap(GL_TEXTURE_2D);
        }
        vbo = GL.GenBuffer();
        unsafe
        {
            fixed (int* ptr = &vao)
                GL.GenVertexArrays(1, ptr);
        }
        GL.BindVertexArray(vao);
        GL.BindBuffer(GL_ARRAY_BUFFER, vbo);
        GL.VertexAttribPointer(0, 2, GL_FLOAT, 0, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 1, GL_FLOAT, 0, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 1, GL_FLOAT, 0, 4 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.BindVertexArray(0);
    }

    public void AddQuad(Vertex[] Vertices)
    {
        int i = 0;
        for (; vertex_index < 714 && i != Vertices.Length;)
            quadVertices[vertex_index++] = Vertices[i++];
    }

    public Vertex[] AddQuad(float x, float y, float width, float height, float color)
    {
        var quad = VertexUtils.PreMakeQuad(x, y, width, height, color);
        AddQuad(quad);
        return quad;
    }

    private void ShaderFlush(int id)
    {
        GL.BindVertexArray(vao);
        unsafe
        {
            fixed (void* pdata = quadVertices)
                GL.BufferData(GL_ARRAY_BUFFER, quadVertices.Length * (4 * sizeof(float)), new IntPtr(pdata), GL_STATIC_DRAW);
        }
        GL.BindTexture(GL_TEXTURE_2D, tex_id);
        GL.UseProgram(id);
        GL.DrawArrays(GL_TRIANGLES, 0, quadVertices.Length);
        GL.BindVertexArray(0);
        vertex_index = 0;
        for (int i = 0; i < quadVertices.Length; i++)
        {
            quadVertices[i].UvID = 0f;
            quadVertices[i].ColorID = 0f;
            quadVertices[i].X = 0f;
            quadVertices[i].Y = 0f;
        }
    }

    public void Flush() => ShaderFlush(sid);
    public void FlushAntiGhost() => ShaderFlush(sid_anti_ghost);

    public void Dispose()
    {
        unsafe
        {
            fixed (int* ptr = &vbo)
                GL.DeleteBuffers(1, ptr);
            fixed (int* ptr = &vao)
                GL.DeleteVertexArrays(1, ptr);
            GL.DeleteProgram(sid);
            GL.DeleteProgram(sid_anti_ghost);
            fixed (int* ptr = &tex_id)
                GL.DeleteTextures(1, ptr);
        }
    }
}