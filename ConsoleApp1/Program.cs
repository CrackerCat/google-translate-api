using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(Translate("你好啊阿斯顿"));
        }

        
        static int CalcHash(int a, string b)
        {
            for(var c = 0;  c < b.Length - 2; c +=3)
            {
                int d = (byte)b[c + 2];
                d = (byte)'a' <= d ? d - 87 : Convert.ToInt32(((char)d).ToString());
                d = '+' == b[c + 1] ? (int)(((uint)a) >> d) : a << d;
                a = '+' == b[c] ? a + d : a ^ d;

            }
            return a;
        }

        static string GenerateToken(string content, string tkk= "443633.4291054061")
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            string[] tkkArray = tkk.Split(".");
            
            int leftNum = Convert.ToInt32(tkkArray[0]);
            uint rightNum = Convert.ToUInt32(tkkArray[1]);

            int hash = leftNum;
            foreach (var c in data)
            {
                hash += Convert.ToInt32(c);
                hash = CalcHash(hash, "+-a^+6");
            }
            hash = CalcHash(hash, "+-3^+b+-f");
            long result = hash & 0xffffffff;
            result ^= rightNum | 0;
            if(0 > result)
            {
                result = (result & 0x7fffffff) + 0x80000000;
            }
            result = (long)(result % 1e6);
            return result.ToString() + "." + (result ^ leftNum).ToString();
        }

        static string Translate(string content, string sourceLang="zh-Hans", string toLang="en")
        {
            try
            {
                string url = String.Format("https://translate.google.cn/translate_a/single" +
                    "?client=t&sl={0}&tl={1}&hl={0}&dt=at&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&tk={2}&ie=UTF-8&oe=UTF-8&pc=1&kc=1&ssel=0&otf=1",
                    sourceLang, toLang, GenerateToken(content));

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Referer", "https://translate.google.cn/");
                var form = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "q", content}
            });
                var resp = client.PostAsync(url, form).Result;
                var src = resp.Content.ReadAsStringAsync().Result;
                var json = JsonDocument.Parse(src);
                string result = json.RootElement[0][0][0].GetString();
                return result;
            }catch(Exception)
            {
                return "";
            }

        }
    }
}
