using Discout.Utils;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Discout.Objects.Score;

[XmlRoot("ScoreBoard", IsNullable = false)]
public class ScoreBoard
{
    [XmlArray("ScoreEntryStack")]
    public ScoreEntry[] entry = new ScoreEntry[3];

    public void addScore(ScoreEntry new_entry)
    {
        entry[2] = entry[1];
        entry[1] = entry[0];
        entry[0] = new_entry;
    }

    public override string? ToString()
    {
        StringBuilder builder = new();
        builder.Append("Current ScoreBoard:\n");
        foreach (ScoreEntry e in entry)
            builder.Append("    " + e.ToString() + "\n");
        return builder.ToString();
    }

    public static void Save(ScoreBoard obj)
    {
        XmlSerializer bf = new(typeof(ScoreBoard));
        try
        {
            using (var file = new StreamWriter(AssetUtil.OpenFile("ScoreBoard.xml")!))
            {
                bf.Serialize(file, obj);
            }
        }
        catch (Exception)
        {
            return;
        }
    }

    public static ScoreBoard Load()
    {
        try
        {
            XmlSerializer bf = new(typeof(ScoreBoard));
            using (Stream file = AssetUtil.OpenFile("ScoreBoard.xml", false)!)
            {
                return (ScoreBoard)bf.Deserialize(file)!;
            }
        }
        catch (Exception)
        {
            ScoreBoard obj = new ScoreBoard();
            obj.addScore(new());
            obj.addScore(new());
            obj.addScore(new());
            return obj;
        }
    }
}
