using IPA;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatSpearSupport
{
    public class Plugin : IBeatSaberPlugin
    {
        public const string assemblyName = "BeatSpearSupport";
        private const string MenuScene = "MenuCore";
        private const string GameScene = "GameCore";

        private static BeatSpearSupport beatSpearSupport;
        private BeatSpearModifiersUI beatSpearModifiersUI;

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == GameScene)
            {
                SharedCoroutineStarter.instance.StartCoroutine(WaitForGameScene());
            }
        }

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
            PersistentSingleton<ConfigOptions>.TouchInstance();
            beatSpearModifiersUI = new GameObject(nameof(BeatSpearModifiersUI)).AddComponent<BeatSpearModifiersUI>();
        }

        public void OnFixedUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == MenuScene)
            {
                this.beatSpearModifiersUI.CreateBeatSpearSupportModifiers();
            }
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        public void OnUpdate()
        {
        }

        private static IEnumerator WaitForGameScene()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (beatSpearSupport == null) beatSpearSupport = new GameObject(nameof(BeatSpearSupport)).AddComponent<BeatSpearSupport>();
            beatSpearSupport.BeginGameCoreScene();
        }
    }
}
