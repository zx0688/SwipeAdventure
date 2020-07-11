using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers {
    public class GameDataManager : MonoBehaviour {

        public static GameDataManager instance = null;
        public static readonly string EFFECT = "effect";

#if UNITY_STANDALONE_WIN
        private static readonly string GOOGLE_DRIVE = "https://drive.google.com/uc?export=download&id=1tWCVbt3hUimhZPh6lABPLJefNZYotS8K";
#else
        private static readonly string GOOGLE_DRIVE = "https://drive.google.com/uc?export=download&id=1tWCVbt3hUimhZPh6lABPLJefNZYotS8K";
#endif
        private static readonly string URL_META = "";
        private static readonly string URL_VERSION = "";

        //private delegate 

        public GameData game;
        public int version;
        public event Action OnUpdate;

        void Awake () {
            version = -1;
        }

        void Start () {

        }

        public ResourceData ResInfo (int id) {
            ResourceData r = game.resources.Find (_r => _r.id == id);
            return r;
        }
        public ActionData ActionInfo (int id) {
            ActionData a = game.actions.Find (_a => _a.id == id);
            return a;
        }

        public int MaxResource (int id) {
            ResourceData r = game.resources.Find (_r => _r.id == id);
            return r.maxValue == 0 ? 99999999 : r.maxValue;
        }

        public QueueItem GetNewCard (CardData card, bool me, int time) {
            int ID = (int) UnityEngine.Random.Range (0, game.cards.Count - 1);

            QueueItem q = new QueueItem ();
            q.me = me;
            q.card = game.cards[ID];

            return q;
        }

        private QueueItem CardSelection (List<QueueItem> queue) {
            queue.Sort ((p1, p2) => p1.card.priority.CompareTo (p2.card.priority));
            return queue.Count == 0 ? null : queue[0];
        }

        public bool CheckGlobalState (PlayerManager player, PlayerManager enemy) {
            return true;
        }

        public bool CheckAvailableCost (List<RewardData> rewards) {
            foreach (RewardData r in rewards) {
                PlayerManager player = r.enemy == true ? Services.enemy : Services.player;
                int available = player.AvailableResource (r.id);
                if (r.count > available)
                    return false;
            }
            return true;
        }

        public bool CheckConditions (List<ConditionData> conditions, int time) {
            //bool resule = true;
            foreach (ConditionData c in conditions) {
                PlayerManager player = c.enemy == true ? Services.enemy : Services.player;

                if (c.location != null && c.location != "" && player.playerData.currentLocation != c.location)
                    return false;

                switch (c.category) {
                    case 0:
                        if (player.GetExecutedCard (c.id) == null)
                            return false;
                        break;
                    case 1:

                        int value = player.AvailableResource (c.id);
                        switch (c.sign) {
                            case ">":
                                if (!(value > c.count))
                                    return false;
                                break;
                            case "==":
                                if (!(c.count == value))
                                    return false;
                                break;
                            case "<=":
                                if (!(value <= c.count))
                                    return false;
                                break;
                            case ">=":
                                if (!(value >= c.count))
                                    return false;
                                break;
                            case "<":
                                if (!(value < c.count))
                                    return false;
                                break;
                            default:
                                if (value == 0)
                                    return false;
                                break;
                        }

                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        public bool isWin (int time) {
            return CheckConditions (game.config.win, time);
        }

        public bool isFail (int time) {
            return CheckConditions (game.config.fail, time);
        }

        /*private void CheckTrigger (int time) {

        GameMeta gameMeta = gameData.GetComponent<MetaManager> ().gameMeta;
        queue = new List<QueueItem> ();

        foreach (CardMeta card in gameMeta.cards) {
            //always true
            if (card.triggers == null || card.triggers.Length == 0) {
                if (CheckConditions (card, time))
                    // queue.Add (card);
                    continue;
            }

            TriggerMeta[] ts = card.triggers;
            foreach (TriggerMeta t in ts) {
                bool applyed = false;
                switch (t.category) {
                    case 0:
                        applyed = (t.id == current.card.id) && (t.choise == Swipe.ANY || t.choise == playerData.currentChoise);
                        break;
                    case 1:

                        switch (playerData.currentChoise) {
                            case 0:
                                break;
                            case 1:
                                break;
                        }
                        //ArrayUtility.Find (current.LCost)

                        //applyed =
                        break;
                    case 2:
                        applyed = ArrayUtility.Contains<string> (current.card.tags, t.tags[0]);
                        break;
                }

                if (applyed && CheckConditions (card, time)) {
                    //queue.Add (card);
                }

            }

        }*/

        public async UniTask Init (IProgress<float> progress = null) {
            int mversion = SecurePlayerPrefs.GetInt ("meta_version");

            var asset = await Services.assets.GetJson("meta", false, GOOGLE_DRIVE, false, progress);
            
            Debug.Log (asset);
            
            game = JsonUtility.FromJson<GameData> (asset);
         
            version = game.timestamp;
            if (mversion != version) {
                SecurePlayerPrefs.SetInt ("meta_version", version);
                OnUpdate?.Invoke ();
            }
        }

        // Update is called once per frame
        void Update () {

        }
    }
}