using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Save_System
{
    public class FileDataService : IDataService
    {
        private ISerializer _serializer;
        string _dataPath;
        string _fileExtension;

        public FileDataService(ISerializer serializer)
        {
            _serializer = serializer;
            _dataPath = Application.persistentDataPath;
            _fileExtension = "json";
        }

        string GetPathToFile(string fileName)
        {
            return Path.Combine(_dataPath, string.Concat(fileName, ".", _fileExtension));
        }

        public void Save(GameData data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.name);

            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException($"File '{data.name}.{_fileExtension}' already exists and cannot be overwritten.");
            }
            
            File.WriteAllText(fileLocation, _serializer.Serialize(data));
        }

        public GameData Load(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (!File.Exists(fileLocation))
            {
                throw new ArgumentException($"No persistent Gamedata with Name '{name}'.");
            }
            
            return _serializer.Deserialize<GameData>(File.ReadAllText(fileLocation));
        }

        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        public void DeleteAll()
        {
            foreach (var filePath in Directory.GetFiles(_dataPath))
            {
                File.Delete(filePath);
            }
        }

        public IEnumerable<string> ListSaves()
        {
            foreach (var path in Directory.EnumerateFiles(_dataPath))
            {
                if (Path.GetExtension(path) == _fileExtension)
                {
                    yield return Path.GetFileNameWithoutExtension(path);
                }
            }
        }
    }
}