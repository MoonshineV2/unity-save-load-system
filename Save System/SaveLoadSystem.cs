using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Demo;
using Save_System.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save_System
{
    [Serializable] public class GameData
    {
        public string name;
        public string currentLevelName;
        public PlayerData playerData;
    }

    public interface ISaveable
    {
        SerializableGuid Id { get; set; }
    }

    public interface IBind<TData> where TData : ISaveable
    {
        SerializableGuid Id { get; set; }
        void Bind(TData data);
    }

    public class SaveLoadSystem: PersistentSingleton<SaveLoadSystem>
    {
        [SerializeField] public GameData gameData;
        
        IDataService _dataService;
        
        private static SaveLoadSystem _instance;

        protected override void Awake()
        {
            base.Awake();
            _dataService = new FileDataService(new JsonSerializer());
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Debug.Log(gameData.playerData.position);
            Bind<FPSController, PlayerData>(gameData.playerData);
        }

        void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null)
            {
                if (data == null)
                {
                    data = new TData { Id = entity.Id};
                }
                entity.Bind(data);
            }
        }
        
        void Bind<T, TData>(List<TData> datas) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entities = FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (var entity in entities)
            {
                var data = datas.FirstOrDefault(d => d.Id == entity.Id);
                if (data == null)
                {
                    data = new TData { Id = entity.Id };
                    datas.Add(data);
                }
                entity.Bind(data);
            }
        }

        public void NewGame()
        {
            gameData = new GameData
            {
                name = "New Game",
                currentLevelName = "Demo"
            };
            SceneManager.LoadScene(gameData.currentLevelName);
        }

        public void SaveGame()
        {
            _dataService.Save(gameData);
        }

        public void LoadGame(string gameName)
        {
            gameData = _dataService.Load(gameName);

            if (String.IsNullOrWhiteSpace(gameData.currentLevelName))
            {
                gameData.currentLevelName = "Demo";
            }
            
            SceneManager.LoadScene(gameData.currentLevelName);
        }

        public void ReloadGame()
        {
            LoadGame(gameData.name);
        }

        public void DeleteGame(string gameName)
        {
            _dataService.Delete(gameName);
        }
    }
}