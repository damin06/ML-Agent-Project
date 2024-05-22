using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SerializableDict<KEY, VALUE>
{
    [SerializeField] private List<SerializeData<KEY, VALUE>> data = new List<SerializeData<KEY, VALUE>>();
    private Dictionary<KEY, VALUE> dict = new Dictionary<KEY, VALUE>();

    public SerializableDict()
    {
        UpDateDict();
    }

    public VALUE this[KEY _key]
    {
        get
        {
            return GetValue(_key);
        }
    }

    public Dictionary<KEY, VALUE> GetDict()
    {
        UpDateDict();
        return dict;
    }

    public VALUE GetValue(KEY _key)
    {
        if (dict.Count <= 0)
            UpDateDict();
        if (!dict.ContainsKey(_key))
        {
            Debug.LogWarning($"{_key.ToString()} is not included");
            return default;
        }
        return dict[_key];
    }

    public void UpDateDict()
    {
        dict.Clear();

        for (int i = 0; i < data.Count; i++)
        {
            dict.Add(data[i].Key, data[i].Value);
        }
    }
}

[Serializable]
public struct SerializeData<KEY, VALUE>
{
    public KEY Key;
    public VALUE Value;
}

