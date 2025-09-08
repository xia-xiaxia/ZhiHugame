using UnityEngine;

public static class JsonHelper
{
    [System.Serializable]
    private class Wrapper<T> { public T[] Items; }

    public static T[] FromJson<T>(string raw)
    {
        string fix = "{\"Items\":" + raw + "}";
        return JsonUtility.FromJson<Wrapper<T>>(fix).Items;
    }
}