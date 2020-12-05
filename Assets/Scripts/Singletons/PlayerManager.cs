using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers {

    public class PlayerManager : MonoBehaviour {

        private static readonly string URL = "";
        public event Action<int, int> OnResourceUpdated;
        public event Action OnProfileUpdated;
        public event Action ShowFinger;

        public PlayerData playerData;

        public int currentChoise;

        void Awake () {

        }

        void Start () {

            if (SecurePlayerPrefs.HasKey ("profile")) {
                Recovery ();
            }

            //Facade.meta.OnUpdate += OnMetaUpdate;
        }

        public async UniTask Init (IProgress<float> progress = null) {
            RequestVO r = new RequestVO ("profile");
            Services.network.AddRequestToPool (r);

            await UniTask.DelayFrame (2);

            //if the answer is none 
            if (!SecurePlayerPrefs.HasKey ("profile")) {
                //Services.data.game.profiles.timestamp = GameTime.GetTime ();
                //string json = JsonUtility.ToJson (Services.data.game.profiles);
                // Debug.Log(json);
                // PlayerData[] players = JsonUtility.FromJson<PlayerData[]> (json);
                playerData = new PlayerData ();

                foreach (PlayerData p in Services.data.game.profiles) {
                    if (this == Services.player && p.tags.Contains ("player")) {
                        //playerData = Utils.DeepCopy (p);
                        playerData = Utils.DeepCopyClass (p);
                        break;
                    } else if (this == Services.enemy && p.tags.Contains ("enemy")) {
                        playerData = Utils.DeepCopyClass (p);
                        break;
                    }

                }

                switch (Services.data.tutorStep) {
                    case 0:
                        playerData.resources = Utils.DeepCopyList (playerData.tutorial0.current);
                        break;
                    case 1:
                        playerData.resources = Utils.DeepCopyList (playerData.tutorial1.current);
                        break;
                    default:

                        break;
                }

                // playerData.resources = Utils.DeepCopyList (playerData.t);
                //SecurePlayerPrefs.SetString ("profile", json);

            }

            OnProfileUpdated?.Invoke ();
        }

        /*public List<ResourceItem> GetEffects () {
            List<ResourceItem> result = new List<ResourceItem> ();
            foreach (ResourceItem r in playerData.resources) {
                if (r.count == 0)
                    continue;
                ResourceData d = Services.data.ResInfo (r.id);
                if (d.tags != null && d.tags.Equals (GameDataManager.EFFECT))
                    result.Add (r);
            }
            return result;
        }*/

        public int MaxResourceValue (int id) {

            ResourceItem rd = null;
            switch (Services.data.tutorStep) {
                case 0:
                    rd = playerData.tutorial0.max.Find (_m => _m.id == id);
                    break;
                case 1:
                    rd = playerData.tutorial1.max.Find (_m => _m.id == id);
                    break;
                default:
                    rd = playerData.maxValue.Find (_m => _m.id == id);
                    break;
            }

            return rd == null ? 0 : rd.count;
        }

        public static int GetAvailableResource (RewardData r) {
            return r.enemy ? Services.enemy.AvailableResource (r.id) : Services.player.AvailableResource (r.id);
        }
        public int AvailableResource (int id) {
            ResourceItem r = playerData.resources.Find (_r => _r.id == id);
            return r == null ? 0 : r.count;
        }

        public CardExecutedItem GetExecutedCard (int id) {
            CardExecutedItem card = playerData.cards.Find (c => c.id == id);
            return card;
        }

        public void ClearResource (int id) {
            ResourceItem res = playerData.resources.Find (r => r.id == id);

            if (res == null || res.count == 0)
                return;

            int count = res.count;
            res.count = 0;
            SaveResLocal (res);
            OnResourceUpdated?.Invoke (res.id, -count);
        }

        public void SubResource (int id, int count) {
            ResourceItem res = playerData.resources.Find (r => r.id == id);

            if (res == null) {
                res = new ResourceItem ();
                res.id = id;
                res.count = 0;
                playerData.resources.Add (res);
            }
            if (res.count - count <= 0)
                res.count = 0;
            else
                res.count -= count;

            SaveResLocal (res);
            OnResourceUpdated?.Invoke (res.id, -count);

        }

        public void AddResource (int id, int count) {
            ResourceItem res = playerData.resources.Find (r => r.id == id);

            if (res == null) {
                res = new ResourceItem ();
                res.id = id;
                res.count = 0;
                playerData.resources.Add (res);
            }

            int max = MaxResourceValue (id);

            if (res.count + count > max)
                count = max - res.count;

            res.count += count;

            SaveResLocal (res);
            OnResourceUpdated?.Invoke (res.id, count);
        }

        public void OnShowFinger () {
            Services.player.ShowFinger?.Invoke ();
        }

        public void Execute (CardData cardData, int choice, ChoiceData chMeta, bool me, int time, PlayerManager enemy) {

            if (Services.data.CheckConditions (chMeta.conditions, time, false, me)) {
                List<RewardData> result = new List<RewardData> ();
                Services.data.GetResourceReward (result, chMeta.reward, chMeta.cost, time, true, false, me);
                foreach (RewardData r in result) {
                    if (r.count > 0) {
                        if (r.enemy)
                            enemy.AddResource (r.id, r.count);
                        else
                            AddResource (r.id, r.count);
                    } else {
                        if (r.enemy)
                            enemy.SubResource (r.id, Math.Abs (r.count));
                        else
                            SubResource (r.id, Math.Abs (r.count));
                    }
                }

            }

            CardExecutedItem cardVO = playerData.cards.Find (c => c.id == cardData.id);
            if (cardVO == null) {
                cardVO = new CardExecutedItem ();
                playerData.cards.Add (cardVO);
            }

            cardVO.id = cardData.id;
            cardVO.choice = choice;
            cardVO.executed = time;
            SaveCardLocal (cardVO);

            enemy.OnProfileUpdated?.Invoke ();
            OnProfileUpdated?.Invoke ();

            //only for me
            if (this == Services.player) {
                Services.network.AddRequestToPool (new RequestVO ("choise"));
            }
        }

        private void SaveCardLocal (CardExecutedItem card) {

        }
        private void SaveResLocal (ResourceItem res) {

        }
        private void Recovery () {

            return;

        }

    }
    //================================

    [Serializable]
    public class PlayerData {
        public List<CardExecutedItem> cards;
        public List<QuestData> activeQuests;
        public List<QuestExecutedItem> completeQuests;
        public List<ResourceItem> resources;
        public List<ResourceItem> maxValue;
        public string currentLocation;

        public TutorialData tutorial0;
        public TutorialData tutorial1;

        public string[] tags;
        public int timestamp;
        public bool first;
    }

    [Serializable]
    public class TutorialData {
        public List<ResourceItem> current;
        public List<ResourceItem> max;
    }

    [Serializable]
    public class CardExecutedItem {
        public int id;
        public int executed;
        //0 - left, 1 - right..
        public int choice;

    }

    [Serializable]
    public class ResourceItem {

        public int id;
        public int count;
    }

    [Serializable]
    public class QuestExecutedItem {
        public int id;
        public int activated;
        public int executed;
        public int expired;
    }
}