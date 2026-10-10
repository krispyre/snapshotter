using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class MissionSave
{
    public float points = 100;
    public int checkpointId;
}

[Serializable]
public class SaveData
{
    public List<MissionSave> missions = new List<MissionSave>();
}

public static class SaveSystem
{
    [Serializable]
    private class SaveFile
    {
        public SaveData saveData = new SaveData();
    }

    private static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData _data;

    public static SaveData data
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    public static MissionSave GetMission(int missionId)
    {
        while (data.missions.Count <= missionId)
        {
            data.missions.Add(new MissionSave());
        }
        return data.missions[missionId];
    }

    public static void Load()
    {
        _data = null;
        if (File.Exists(FilePath))
        {
            try
            {
                _data = JsonUtility.FromJson<SaveFile>(File.ReadAllText(FilePath))?.saveData;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read {FilePath}: {e.Message}");
            }
        }
        if (_data == null)
        {
            _data = new SaveData();
        }
    }

    public static void Save()
    {
        var file = new SaveFile { saveData = data };
        File.WriteAllText(FilePath, JsonUtility.ToJson(file, true));
    }
}
