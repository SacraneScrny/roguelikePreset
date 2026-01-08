using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Sackrany.SerializableData.Components;
using Sackrany.SerializableData.Converters;
using Sackrany.SerializableData.Entities;
using Sackrany.Utils;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Sackrany.SerializableData
{
    public class DataManager : AManager<DataManager>
    {
        private static readonly string Datapath = Application.dataPath + "/Saves/";
        private static readonly string DatafileName = "saveData";
        private static readonly string FileExtension = ".json";

        public event System.Action<List<object>> OnSaveDataCall;
        
        private SaveDataStructure saveData;
        private SerializationContainer serializationContainer = null;
        private bool _isInitialized = false;
        
        private IEnumerator Start()
        {
            int serializables = FindObjectsByType<DataManager>(FindObjectsSortMode.None).Length;
            Instance.serializationContainer ??= LoadData<SerializationContainer>();
            yield return new WaitWhile(() => serializationContainer.TemporaryContainer.Count < serializables);
            
            Instance.Initialize();
        }
        
        private protected override void OnManagerAwake()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = { new Vector3Converter(), new Vector2Converter(), new QuaternionConverter(), new Vector2IntConverter() },
            };
        }
        private void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            serializationContainer.DeserializeAll();
            OnSaveDataCall += (l) =>
            {
                serializationContainer.SerializeAll();
                l.Add(serializationContainer);
            };
        }
        public static void RegisterSerializable(SerializableBehaviour serializable)
        {
            Instance.serializationContainer ??= LoadData<SerializationContainer>();
            Instance.serializationContainer.TemporaryContainer.TryAdd(serializable.Guid, serializable);
        }
        
        private static T LoadData<T>(string customFolder = "") where T : new()
        {
            if (Instance.saveData != null)
            {
                if (Instance.saveData[typeof(T).Name] is not T)
                    Instance.saveData[typeof(T).Name] = new T();
                Instance.saveData[typeof(T).Name] ??= new T();
                return (T)Instance.saveData[typeof(T).Name];
            }
            
            if (customFolder != "" && !Directory.Exists(Datapath + customFolder + "/")) 
                Directory.CreateDirectory(Datapath + customFolder + "/");
            
            string dataStream;
            var savefile = Datapath + customFolder + "/" + DatafileName + FileExtension;

            if (!Directory.Exists(Datapath)) Directory.CreateDirectory(Datapath);

            if (!File.Exists(savefile))
            {
                SaveData(new T(), customFolder);

                return (T)Instance.saveData[typeof(T).Name];
            }

            dataStream = File.ReadAllText(savefile);
            Instance.saveData = JsonConvert.DeserializeObject<SaveDataStructure>(dataStream);
            Instance.saveData ??= new SaveDataStructure();
            
            if (Instance.saveData[typeof(T).Name] is not T)
                Instance.saveData[typeof(T).Name] = new T();
            Instance.saveData[typeof(T).Name] ??= new T();
            
            return (T)Instance.saveData[typeof(T).Name];
        }
        private static void SaveData<T>(T data, string customFolder = "") where T : new()
        {
            if (customFolder != "" && !Directory.Exists(Datapath + customFolder + "/")) 
                Directory.CreateDirectory(Datapath + customFolder + "/");
         
            if (Instance.saveData == null)
                Instance.saveData = new SaveDataStructure();
            
            StreamWriter dataStream;
            var savefile = Datapath + customFolder + "/" + DatafileName + FileExtension;

            dataStream = File.CreateText(savefile);
            Instance.saveData[typeof(T).Name] = data ?? new T();
            dataStream.Write(JsonConvert.SerializeObject(Instance.saveData, Formatting.Indented));
            dataStream.Dispose();
            dataStream.Close();
        }
        public static void SaveAllData(string customFolder = "")
        {
            if (!Instance._isInitialized) return;
            
            if (customFolder != "" && !Directory.Exists(Datapath + customFolder + "/")) 
                Directory.CreateDirectory(Datapath + customFolder + "/");
         
            if (Instance.saveData == null)
                Instance.saveData = new SaveDataStructure();
            
            StreamWriter dataStream;
            var savefile = Datapath + customFolder + "/" + DatafileName + FileExtension;
            
            List<object> data = new List<object>();
            Instance.OnSaveDataCall?.Invoke(data);

            dataStream = File.CreateText(savefile);
            foreach (var d in data)
                Instance.saveData[d.GetType().Name] = d;
            dataStream.Write(JsonConvert.SerializeObject(Instance.saveData, Formatting.Indented));
            dataStream.Dispose();
            dataStream.Close();
        }

        #if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                SaveAllData();
            }
        }
        #endif
    }
    
    public class SaveDataStructure
    {
        public Dictionary<string, object> _saveData = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                _saveData ??= new Dictionary<string, object>();
                if (_saveData.TryGetValue(key, out var value))
                    return value;
                _saveData.Add(key, null);
                return "";
            }
            set
            {
                _saveData ??= new Dictionary<string, object>();
                if (!_saveData.TryGetValue(key, out var val))
                    _saveData.Add(key, value);
                else
                 _saveData[key] = value;
            }
        }
    }
}