using System;
using System.IO;
using Junk.Entities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Junk.Entities
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MenuSystemGroup))]
    public partial class OptionsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if(Keyboard.current!=null && Keyboard.current.oKey.isPressed)
            {
                SaveSettings();
            }
        }
        
        void SaveSettings()
        {
            var     path = Application.persistentDataPath + "/settings.json";
            var      data = new Settings();
            data.cameraMode = 1;
            data.musicVolume = 0.5f;
            data.sfxVolume = 0.5f;
            data.fullscreen = true;
            data.resolution = new int2(1920, 1080);

            using(var writer = new StreamWriter(path, false))
            {
                var jsonData = JsonUtility.ToJson(data, true);
                writer.Write(jsonData);
                writer.Close();
            }
            
            if (!File.Exists(Application.persistentDataPath + "/settings.json"))
            {

            }
            else
            {
                /*using(var reader = new StreamReader(path))
                {
                    var jsonData = reader.ReadToEnd();
                    data = JsonUtility.FromJson<ConfigData>(jsonData);
                    reader.Close();
                }*/
            }



            
            
            
            
            
            
            //data.cameraMode  = cameraMode;
            //data.resolutionX = resolutionX;
            //data.resolutionY = resolutionY;
            //bf.Serialize(file, data);
            //file.Close();
            Debug.Log("Game data saved!");
        }

        [Serializable]
        private class Settings
        {
            public int   cameraMode;
            public int   resolutionX;
            public int   resolutionY;
            public int2  resolution;
            public float musicVolume;
            public float sfxVolume;
            public bool  fullscreen;
        }
        

    }

}
    
    
