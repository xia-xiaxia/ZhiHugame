using TMPro.SpriteAssetUtilities;
using UnityEngine;

namespace JsonB
{
    public static class JsonHelper
    {
        [System.Serializable]
        private class Wrapper<T> { public T[] Items; }

        public static T[] FromJson<T>(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return new T[0];

            // 去除可能的 BOM
            if (raw.Length > 0 && raw[0] == '\uFEFF') raw = raw.Substring(1);

            raw = raw.Trim();

            // 如果文件是单个对象而不是数组，包装为数组
            string itemsJson;
            if (raw.StartsWith("{"))
            {
                // 单个对象 -> 转为数组
                itemsJson = "[" + raw + "]";
            }
            else
            {
                itemsJson = raw;
            }

            string fix = "{\"Items\":" + itemsJson + "}";

            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(fix);
                return wrapper?.Items ?? new T[0];
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JsonHelper] JSON 解析失败: {ex.Message}\nJSON 片段: { (itemsJson.Length>200? itemsJson.Substring(0,200)+"...": itemsJson) }");
                throw;
            }
        }
    }
}

namespace JsonA
{
    public static class JsonHelper
    {
        [System.Serializable]
        private class Wrapper<T> { public T[] Items; }

        public static T[] FromJson<T>(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return new T[0];

            // 去除可能的 BOM
            if (raw.Length > 0 && raw[0] == '\uFEFF') raw = raw.Substring(1);

            raw = raw.Trim();

            // 如果文件是单个对象而不是数组，包装为数组
            string itemsJson;
            if (raw.StartsWith("{"))
            {
                // 单个对象 -> 转为数组
                itemsJson = "[" + raw + "]";
            }
            else
            {
                itemsJson = raw;
            }

            string fix = "{\"Items\":" + itemsJson + "}";

            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(fix);
                return wrapper?.Items ?? new T[0];
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JsonHelper] JSON 解析失败: {ex.Message}\nJSON 片段: { (itemsJson.Length>200? itemsJson.Substring(0,200)+"...": itemsJson) }");
                throw;
            }
        }
    }
}