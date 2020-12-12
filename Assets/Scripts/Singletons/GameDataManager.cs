using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        //#if UNITY_EDITOR_WIN
        private static readonly string GOOGLE_DRIVE = "https://drive.google.com/uc?export=download&id=10u6GCbjRsoW0yIfk5VpSbPsHvNnn-C4y";
        //#else
        //https://drive.google.com/file/d/10u6GCbjRsoW0yIfk5VpSbPsHvNnn-C4y/view?usp=sharing
        //       private static readonly string GOOGLE_DRIVE = "https://drive.google.com/uc?export=download&id=1tWCVbt3hUimhZPh6lABPLJefNZYotS8K"; //"https://drive.google.com/uc?export=download&id=1tWCVbt3hUimhZPh6lABPLJefNZYotS8K";
        //#endif
        private static readonly string URL_META = "";
        private static readonly string URL_VERSION = "";

        //private delegate 
        public int tutorStep;
        public int swipeCount;

        public GameData game;
        public int version;
        public int fingerStep;
        public event Action OnUpdate;

        void Awake () {
            version = -1;
        }

        void Start () {
            tutorStep = 0;
            fingerStep = 0;

        }
        public void IncreaseTutor () {
            tutorStep++;
        }

        public void GetResourceReward (List<RewardData> result, List<RewardData> reward, List<RewardData> cost, int time, bool swipe, bool isAction, bool me) {

            foreach (RewardData r in reward) {

                if (r.category == RESOURCE_ID) {
                    RewardData _r = GetRewardItem (result, r, isAction, me);
                    _r.count += r.count;
                } else if (r.category == ACTION_ID) {
                    ActionData a = ActionInfo (r.id);
                    if (!Services.data.CheckConditions (a.conditions, time, true, me))
                        continue;

                    int count = r.count;
                    foreach (BuffData b in Services.data.game.buffs) {
                        if (b.swipe == true && b.swipe != swipe)
                            continue;
                        if (!Services.data.CheckConditions (b.conditions, time, true, me))
                            continue;
                        if (b.trigger != a.id)
                            continue;

                        GetResourceReward (result, b.reward, b.cost, time, swipe, true, me);

                        if (b.disable) {
                            count = 0;
                            continue;
                        }

                        if (b.factor > 0) {
                            count *= b.factor;
                            continue;
                        }

                    }

                    for (int i = 0; i < count; i++)
                        GetResourceReward (result, a.reward, a.cost, time, swipe, true, me);
                }
            }

            foreach (RewardData r in cost) {
                if (!Services.data.CheckConditions (r.conditions, time, isAction, me))
                    continue;
                if (r.category == RESOURCE_ID) {
                    RewardData _r = GetRewardItem (result, r, isAction, me);
                    _r.count -= r.count;
                }
            }
        }

        private RewardData GetRewardItem (List<RewardData> list, RewardData item, bool isAction, bool me) {
            bool enemy = isAction == true && me == false ? (!item.enemy) : item.enemy;
            RewardData r = list.Find (i => i.id == item.id && i.enemy == enemy && i.category == item.category);
            if (r == null) {
                r = new RewardData ();
                r.count = 0;
                r.id = item.id;
                r.enemy = enemy;
                r.conditions = new List<ConditionData>();
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

        public CardItem GetNewCard (CardData card, bool me, int time) {

            List<CardData> available = new List<CardData> ();
            int maxCard = tutorStep;
            for (int i = 0; i <= maxCard && i < game.config.tutorial.Count; i++) {
                int id = game.config.tutorial[i];
                available.Add (game.cards.Find (c => c.id == id));
            }

            int ID = (int) UnityEngine.Random.Range (0, available.Count);

            CardItem q = new CardItem ();
            q.me = me;
            q.data = available[ID];

            return q;
        }

        private CardItem CardSelection (List<CardItem> queue) {
            queue.Sort ((p1, p2) => p1.data.priority.CompareTo (p2.data.priority));
            return queue.Count == 0 ? null : queue[0];
        }

        public bool CheckGlobalState (PlayerManager player, PlayerManager enemy) {
            return true;
        }

        public List<ConditionData> GetUnavailableConditions (List<ConditionData> conditions, int time, bool isAction, bool me) {

            List<ConditionData> result = new List<ConditionData> ();
            foreach (ConditionData c in conditions) {
                List<ConditionData> checkList = new List<ConditionData> ();
                checkList.Add (c);
                if (!CheckConditions (checkList, time, isAction, me))
                    result.Add (c);
            }
            return result;
        }

        public bool CheckConditions (List<ConditionData> conditions, int time, bool isAction, bool me) {
            //bool resule = true;
            foreach (ConditionData c in conditions) {

                bool enemy = isAction == true && me == false ? (!c.enemy) : c.enemy;
                PlayerManager player = enemy == true ? Services.enemy : Services.player;

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
            return CheckConditions (game.config.win, time, false, true);
        }

        public bool isFail (int time) {
            return CheckConditions (game.config.fail, time, false, true);
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

            game.cards = game.cards.Where (c => c.off != true).ToList ();
            
        }

        // Update is called once per frame
        void Update () {

        }
    }

}