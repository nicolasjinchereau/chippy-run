using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using JsonFx;
using System.Globalization;

public class HighScore
{
    public int id;
    public string name;
    public int score;
    public DateTime date;
}

public class Profile
{
    public string userID;
    public List<HighScore> scores;
}

public class ScoreService : MonoBehaviour
{
    public const int MaxNameLength = 16;

    public static readonly JsonWriterSettings WriterSettings = new JsonWriterSettings()
    {
        DateTimeSerializer = (JsonWriter writer, DateTime value) => {
            writer.Write(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    };

    public static readonly JsonReaderSettings ReaderSettings = new JsonReaderSettings()
    {
        DateTimeDeserializer = (JsonReader reader) => {
            var str = (string)reader.Read(typeof(string), false);
            return DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }
    };

    static Profile LoadProfile()
    {
        var json = PlayerPrefs.GetString("profile", "{}");
        Profile prof = JsonReader.Deserialize<Profile>(json, ReaderSettings);

        if (string.IsNullOrEmpty(prof.userID))
        {
            prof.userID = Guid.NewGuid().ToString();
            prof.scores = new List<HighScore>();
        }

        return prof;
    }

    static void SaveProfile(Profile prof)
    {
        if (prof == null)
            throw new ArgumentNullException(nameof(prof));

        var json = JsonWriter.Serialize(prof, WriterSettings);
        PlayerPrefs.SetString("profile", json);
        PlayerPrefs.Save();
    }

    public static int PutScore(string name, int score)
    {
        var prof = LoadProfile();
        int scoreID = prof.scores.Count;

        var highScore = new HighScore() {
            id = scoreID,
            date = DateTime.Now,
            name = name,
            score = score
        };
        
        prof.scores.Add(highScore);
        SaveProfile(prof);
        
        return scoreID;
    }

    public static List<HighScore> GetScores()
    {
        var prof = LoadProfile();
        return prof.scores;
    }

    public static void ClearScores()
    {
        var prof = LoadProfile();
        prof.scores = new List<HighScore>();
        SaveProfile(prof);
    }
}
