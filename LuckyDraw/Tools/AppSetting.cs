/*************************************************************************************
 *
 * 文 件 名:   AppSetting.cs
 * 描    述:   工程配置文件工具类
 * 
 * 创 建 者：  BugLord 
 * 创建时间：  2022/06/18 13:03:52
*************************************************************************************/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace LuckyDraw.Tools
{
    public class AppSetting
    {
        private JToken token;

        public AppSetting(String configFile)
        {
            token = readJson(configFile, Encoding.UTF8);
        }

        public String this[String key]
        {
            get
            {
                String result = String.Empty;
                if (token != null && key.Contains(":"))
                {
                    String[] keys = key.Split(':');
                    JToken jToken = token.DeepClone();
                    foreach (String p in keys)
                    {
                        jToken = jToken[p];
                    }
                    result = jToken.ToString();
                }
                else
                {
                    if (token != null)
                    {
                        result = token[key].ToString();
                    }
                }
                return result;
            }
        }

        public T getObjcet<T>(String key)
        {
            if (token != null && key.Contains(":"))
            {
                String[] keys = key.Split(':');
                JToken jToken = token.DeepClone();
                foreach (String p in keys)
                {
                    jToken = jToken[p];
                }
                return jToken.ToObject<T>();
            }
            else
            {
                if (token != null)
                {
                    return token[key].ToObject<T>();
                }
            }
            return default;
        }

        public static JToken readJson(String jsonFile, Encoding encoding)
        {
            using (StreamReader sr = new StreamReader(jsonFile, encoding == null ? Encoding.Default : encoding))
            {
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    var array = JToken.ReadFrom(reader);
                    return array;
                }
            }
        }
    }
}
