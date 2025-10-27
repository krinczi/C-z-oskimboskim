using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace POMIDOR.Services.Storage
{
    public sealed class JsonFileRepository<T> : IRepository<T>
    {
        private readonly string _path;

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public JsonFileRepository(string path) => _path = path;

        public IReadOnlyList<T> LoadAll()
        {
            try
            {
                if (!File.Exists(_path)) return Array.Empty<T>();
                var json = File.ReadAllText(_path);
                return (JsonSerializer.Deserialize<List<T>>(json, _opts) ?? new List<T>()).AsReadOnly();
            }
            catch
            {
                return Array.Empty<T>();
            }
        }

        public void SaveAll(IEnumerable<T> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(items.ToList(), _opts);
            File.WriteAllText(_path, json);
        }

        public void Append(T item)
        {
            var all = LoadAll().ToList();
            all.Add(item);
            SaveAll(all);
        }
    }
}
