// One pending arrival across a scene load. Add a field here when another fact has to come with the player.
public class SceneHandoff
{
    public string spawnId;

    static SceneHandoff pending;

    public static void Set(string spawnId)
    {
        pending = string.IsNullOrEmpty(spawnId) ? null : new SceneHandoff { spawnId = spawnId };
    }

    public static void Clear()
    {
        pending = null;
    }

    public static SceneHandoff Take()
    {
        SceneHandoff value = pending;
        pending = null;
        return value;
    }
}
