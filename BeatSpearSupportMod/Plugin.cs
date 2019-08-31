using IPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatSpearSupportMod
{
    public class Plugin : IBeatSaberPlugin
    {
        private const string MenuScene = "MenuCore";
        private const string GameScene = "GameCore";

        private BeatSpearSupport beatSpearSupport;

        public string Name => BeatSpearSupport.Name;
        public string Version => "0.1.0";

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == GameScene)
            {
                SharedCoroutineStarter.instance.StartCoroutine(this.beatSpearSupport.TransformBeatMap());
            }
        }

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
            beatSpearSupport = new GameObject(nameof(BeatSpearSupport)).AddComponent<BeatSpearSupport>();
        }

        public void OnFixedUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == MenuScene)
            {
                this.beatSpearSupport.CreateBeatSpearSupportModifiers();
            }
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        public void OnUpdate()
        {
        }
    }
}
