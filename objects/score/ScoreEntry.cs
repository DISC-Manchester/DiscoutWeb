using System;
namespace Discout.Objects.Score;
[Serializable]
public class ScoreEntry
{
    public string UserName { get; set; } = "";
    public int Score { get; set; } = 0;
    public ScoreEntry() { }

    public ScoreEntry(string userName, int score)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        Score = score;
    }

    public override string? ToString()
    {
        return UserName == string.Empty ? string.Empty : $"{UserName} - Score:{Score}";
    }
}
