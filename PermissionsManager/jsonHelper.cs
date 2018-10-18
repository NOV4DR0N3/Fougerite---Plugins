using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using UnityEngine;
using JSON;
using System.Runtime.Serialization;

namespace JsonC
{
    class JsonHelper
    {
        public static void SaveFile<T>(T obj, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                string text = JsonConvert.SerializeObject(obj, Formatting.Indented);
                File.WriteAllText(path, text);
            }catch(Exception ex)
            {
                Debug.Log("Error:" + ex.Message);
            }
        }
        public static T ReadyFile<T>(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error saveFile: " + ex);
                return default(T);
            }

        }
    }
}