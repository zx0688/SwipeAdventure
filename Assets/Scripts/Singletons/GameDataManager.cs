using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers {

    public class GameDataManager : MonoBehaviour {

        public static readonly int CARD_ID = 0;
        public static readonly int RESOURCE_ID = 1;
        public static readonly int ACTION_ID = 3;

        public static GameDataManager instance = null;
        public static readonly string EFFECT = "effect";

#if UNITY_STANDALONE_WIN
        private static readonly string GOOGLE_DRIVE = "https://drive.google.com/uc?export=download&id=10u6GCbjRsoW0yIfk5VpSbPsHvNnn-C4y";
#else
        private static readonly string GOOGLE_DRIVE = "https://drive.google.com/uc?export=download&id=10u6GCbjRsoW0yIfk5VpSbPsHvNnn-C4y"; //"https://drive.google.com/uc?export=download&id=1tWCVbt3hUimhZPh6lABPLJefNZYotS8K";
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

        public void GetResourceReward (List<RewardData> result, List<RewardData> reward, List<RewardData> cost, int time) {

            foreach (RewardData r in reward) {
                if (!Services.data.CheckConditions (r.conditions, time))
                    continue;
                if (r.category == RESOURCE_ID) {
                    RewardData _r = GetRewardItem (result, r);
                    _r.count += r.count;
                } else if (r.category == ACTION_ID) {
                    ActionData a = ActionInfo (r.id);
                    for (int i = 0; i < r.count; i++)
                        GetResourceReward (result, a.reward, a.cost, time);
                }
            }
            foreach (RewardData r in cost) {
                if (!Services.data.CheckConditions (r.conditions, time))
                    continue;
                if (r.category == RESOURCE_ID) {
                    RewardData _r = GetRewardItem (result, r);
                    _r.count -= r.count;
                }
            }
        }

        private RewardData GetRewardItem (List<RewardData> list, RewardData item) {
            RewardData r = list.Find (i => i.id == item.id && i.enemy == item.enemy && i.category == item.category);
            if (r == null) {
                r = new RewardData ();
                r.count = 0;
                r.id = item.id;
                r.enemy = item.enemy;
                r.category = item.category;
                list.Add (r);
            }
            return r;
        }

        public ResourceData ResInfo (int id) {
            ResourceData r = game.resources.Find (_r => _r.id == id);
            return r;
        }
        public ActionData ActionInfo (int id) {
            ActionData a = game.actions.Find (_a => _a.id == id);
            return a;
        }

        public int MaxResourceValue (int id) {
            ResourceData r = game.resources.Find (_r => _r.id == id);
            return r.maxValue == 0 ? 99999999 : r.maxValue;
        }

        public CardItem GetNewCard (CardData card, bool me, int time) {
            int ID = (int) UnityEngine.Random.Range (0, game.cards.Count - 1);

            CardItem q = new CardItem ();
            q.me = me;
            q.data = game.cards[ID];

            return q;
        }

        private CardItem CardSelection (List<CardItem> queue) {
            queue.Sort ((p1, p2) => p1.data.priority.CompareTo (p2.data.priority));
            return queue.Count == 0 ? null : queue[0];
        }

        public bool CheckGlobalState (PlayerManager player, PlayerManager enemy) {
            return true;
        }

        public List<ConditionData> GetUnavailableConditions (List<ConditionData> conditions, int time) {

            List<ConditionData> result = new List<ConditionData> ();
            foreach (ConditionData c in conditions) {
                List<ConditionData> checkList = new List<ConditionData> ();
                checkList.Add (c);
                if (!CheckConditions (checkList, time))
                    result.Add (c);
            }
            return result;
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

            var asset = await Services.assets.GetJson ("meta", false, GOOGLE_DRIVE, false, progress);

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