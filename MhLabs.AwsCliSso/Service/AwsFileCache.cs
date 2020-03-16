using MhLabs.AwsCliSso.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MhLabs.AwsCliSso.Service
{
    public class AwsFileCache
    {
        public RegisteredClient Config { get; set; }

        private readonly string _workingDir;

        public AwsFileCache() : this("boto", "cache")
        {
        }

        public AwsFileCache(params string[] path)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] ss = new string[path.Length + 2];
            ss[0] = home;
            ss[1] = ".aws";
            Array.Copy(path, 0, ss, 2, path.Length);

            _workingDir = Path.Combine(ss);
        }

        public bool Contains(string key)
        {
            var fullPath = ConvertCacheKey(key);
            return File.Exists(fullPath);
        }

        private string ConvertCacheKey(string key)
        {
            return Path.Combine(_workingDir, $"{key}.json");
        }

        public T GetItem<T>(string cacheKey) where T : new()
        {
            var fullPath = ConvertCacheKey(cacheKey);

            if (!File.Exists(fullPath))
            {
                return new T();
            }

            var findCache = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<T>(findCache, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ssUTC" });
        }

        public async Task SetItem<T>(string cacheKey, T value)
        {
            var fullkey = ConvertCacheKey(cacheKey);
            var fileContent = JsonConvert.SerializeObject(value, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ssUTC" });

            if (!Directory.Exists(_workingDir))
            {
                Directory.CreateDirectory(_workingDir);
            }

            using (var sw = File.CreateText(fullkey))
            {
                await sw.WriteAsync(fileContent);
            }
        }

        public static string GetSha1(string url)
        {
            byte[] hash;
            using (var sha1 = new SHA1Managed())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url));
            }

            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
