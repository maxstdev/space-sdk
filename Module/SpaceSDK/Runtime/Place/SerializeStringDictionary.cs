using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializeStringDictionary<V> : Dictionary<string, V>, ISerializationCallbackReceiver
{
    [SerializeField]
    List<string> keys = new List<string>();

    [SerializeField]
    List<V> values = new List<V>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<string, V> pair in this)
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
                try { this.Add("", v); } catch (Exception) { }
            }
        }
    }

    private string GenKey(int index)
    {
        try
        {
            return keys[index];
        }
        catch(Exception)
        {
            return "";
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

    public V Find(string key)
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