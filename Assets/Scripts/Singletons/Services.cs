using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers {
    public enum MState {
        INITED,
        INITIALIZATING
    }
    public class Services : MonoBehaviour {

        private static readonly object threadlock = new object ();

        /*public static PlayerManager player {
            get { lock (threadlock) { return player; } }
        }*/
        public static volatile PlayerManager player;
        public static volatile EnemyManager enemy;
        public static volatile GameDataManager data;
        public static volatile NetworkManager network;
        public static volatile AssetsManager assets;
        public static volatile Services instance;

        public static event Action OnInited;

        public static bool isInited {
            get {
                return instance != null && instance.state == MState.INITED;
            }
        }
        public MState state = MState.INITIALIZATING;
        //[SerializeField]
        // private Text loadText;

        [SerializeField]
        private Slider slider;

        void Awake () {
            state = MState.INITIALIZATING;

            if (instance == null) {
                instance = this;

                player = GetComponent<PlayerManager> ();
                data = GetComponent<GameDataManager> ();
                network = GetComponent<NetworkManager> ();
                assets = GetComponent<AssetsManager> ();
                enemy = GetComponent<EnemyManager> ();

               // DontDestroyOnLoad (gameObject);

            } else {
                DestroyImmediate (gameObject);
            }

        }
        // Start is called before the first frame update
        void Start () {

            Init ().Forget ();
        }

        public async UniTaskVoid Init () {

            DontDestroyOnLoad (this);
            GameTime.Init (DateTime.Now.Second);

            // UpdateTextUI ("Loading assets...");
            await assets.Init (Progress.Create<float> (x => UpdateProgressUI (x)));

            // UpdateTextUI ("Loading network...");
            await network.Init (Progress.Create<float> (x => UpdateProgressUI (x)));

            // UpdateTextUI ("Loading game data...");
            await data.Init (Progress.Create<float> (x => UpdateProgressUI (x)));

            // UpdateTextUI ("Loading profile...");
            await player.Init (Progress.Create<float> (x => UpdateProgressUI (x)));
            await enemy.Init (Progress.Create<float> (x => UpdateProgressUI (x)));

            //UpdateTextUI ("Loading scene...");

            Scene s = SceneManager.GetActiveScene ();
            if (s.name != "Main") {
                await SceneManager.LoadSceneAsync ("Main").ToUniTask (Progress.Create<float> (x => UpdateProgressUI (x)));
            }

            //await UniTask.DelayFrame (2);
            //UpdateProgressUI (1);

            state = MState.INITED;
            OnInited?.Invoke ();
        }

        private void UpdateTextUI (string text) {
            // if (loadText != null)
            //    loadText.text = text;
        }
        private void UpdateProgressUI (float progress) {
            if (slider != null)
                slider.value = progress;
        }

        // Update is called once per frame
        void Update () {

        }
    }

}