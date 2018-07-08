using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using UnityEngine;

namespace PermissionsManager
{
    class JsonHelper
    {
        
        public static T Deserialize<T>(string stringJson)
        {
            return JsonConvert.DeserializeObject<T>(stringJson);
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static void SaveFile<T>(T obj, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                StreamWriter sw = File.CreateText(path);

                using (JsonWriter writerr = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writerr, obj);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error saveFile: " + ex);
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



        //public class Product
        //{
        //    public string Name;
        //    public DateTime ExpiryDate;
        //    public decimal Price;
        //    public string[] Sizes;
        //}

        //public static void Deserialize2() {
        //    Product product = new Product();

        //    product.Name = "Apple";
        //    product.ExpiryDate = new DateTime(2008, 12, 28);
        //    product.Price = 3.99M;
        //    product.Sizes = new string[] { "Small", "Medium", "Large" };

        //    string output = JsonConvert.SerializeObject(product);
        //    // { teste git

        //    //    "Name": "Apple",
        //    //      "ExpiryDate": "2008-12-28T00:00:00",
        //    //      "Price": 3.99,
        //    //      "Sizes": [
        //    //        "Small",
        //    //        "Medium",
        //    //        "Large"
        //    //      ]
        //    //}
        //    Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
        //}

        //public static void Serialize2() {
        //    try
        //    {
        //        Debug.Log("teste");
        //        Product product = new Product();
        //        product.Name = "Apple";
        //        product.ExpiryDate = new DateTime(2008, 12, 28);

        //        Debug.Log("teste2");
        //        JsonSerializer serializer = new JsonSerializer();
        //        serializer.Converters.Add(new JavaScriptDateTimeConverter());
        //        Debug.Log("teste3");
        //        serializer.NullValueHandling = NullValueHandling.Ignore;

        //        try
        //        {
        //            StreamWriter sw = File.CreateText(@"C:\Users\Alexandre\Documents\teste.json");// new StreamWriter(@"C:\Users\Alexandre\Documents\teste.json");
        //            if(sw == null){
        //                Debug.Log("arquivo null");
        //                return;
        //            }

        //            Debug.Log("teste4");
        //            try
        //            {
        //                //JsonWriter writer = new JsonTextWriter(sw);
        //                //if(writer == null)
        //                //{
        //                //    Debug.Log("writer null");
        //                //    return;
        //                //}
        //                try
        //                {
        //                        using(JsonWriter writerr = new JsonTextWriter(sw))
        //                        {
        //                            serializer.Serialize(writerr, product);
        //                        }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.Log("teste6");
        //                    Debug.Log(ex);
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.Log("teste5");
        //                Debug.Log(ex);
        //            }



        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.Log("teste6");
        //            Debug.Log(ex);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Log(ex);
        //    }

        //}
    }

}

//public class js : ConsoleSystem
//{
//    [Admin]
//    public static void teste(ref ConsoleSystem.Arg arg)
//    {
//        jsonHelper.Serialize();
//    }
//}