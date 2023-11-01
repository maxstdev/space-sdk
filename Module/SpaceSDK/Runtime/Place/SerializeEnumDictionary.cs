using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializeEnumDictionary<K, V> : Dictionary<K , V>, ISerializationCallbackReceiver where K : Enum
{
    [SerializeField]
    List<K> keys = new List<K>();

    [SerializeField]
    List<V> values = new List<V>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<K, V> pair in this)
        {
            //Debug.Log($"OnBeforeSerialize k : {pair.Key}");
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
        //Debug.Log($"OnBeforeSerialize keys.Count : {keys.Count}");
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0, icount = keys.Count; i < icount; ++i)
        {
            var k = GenKey(i);
            var v = GenValue(i);
            try
            {
                this.Add(k, v);
            }
            catch(Exception)
            {
                try { this.Add(default(K), v); } catch (Exception) { }
            }
        }
    }

    private K GenKey(int index)
    {
        try
        {
            return keys[index];
        }
        catch(Exception)
        {
            return default(K);
        }
    }

    private V GenValue(int index)
    {
        try
        {
            return values[index];
        }
        catch (Exception)
        {
            return default(V);
        }
    }

    public V Find(K key)
    {
        var index = keys.IndexOf(key);
        try
        {
            return values[index];
        }
        catch(Exception)
        {
            return default(V);
        }
    }
}