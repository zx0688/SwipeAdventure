using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Controllers;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public enum GameLoopState {
    IDLE,
    DIABLED
}
public class CardItem {
    public CardData data;
    public bool me;
}
public class GameLoop : MonoBehaviour {

    [SerializeField]
    public GameObject[] cardSwipe;

    public event Action OnQueueTrigger;
    public event Action OnNewItem;

    [HideInInspector]
    public event Action<GameObject> OnNewCard;

    private int currentViewIndex;

    // [SerializeField]
    //public GameObject[] queue;

    private GameLoopState state;

    public static CardItem cardItem;
    private CardIconQueue cardIconQueue;

    [SerializeField]
    private GameObject EndTurnCanvas;

    private Tutorial tutorial;
    private GameObject heads;

    void Awake () {

    }

    void Update () {

    }

    void Start () {

        cardIconQueue = GetComponent<CardIconQueue> ();

        currentViewIndex = 0;
        foreach (GameObject card in cardSwipe) {
            card.SetActive (false);
        }

        heads = GameObject.Find ("Head").gameObject;
        tutorial = GetComponent<Tutorial> ();

        state = GameLoopState.IDLE;
        EndTurnCanvas = GameObject.Find ("EndGameCanvas");
        EndTurnCanvas?.SetActive (false);

        Loop ().Forget ();
    }

    public GameObject GetGameObjectCard () {
        return cardSwipe[currentViewIndex];
    }

    public async UniTask SwipeCard () {
        await OpenNewCard ();

        if(Services.data.tutorStep == 0)
            await tutorial.PostCardAnimation();

        Swipe swipe = cardSwipe[currentViewIndex].GetComponent<Swipe> ();
        await UniTask.WaitUntil (() => swipe.currentChoise != -1);
    }

    private string getRandomHead () {
        string[] names = new string[3] { "Orc", "Demon", "Goblin" };
        return names[UnityEngine.Random.Range (0, names.Length)];
    }

    async UniTaskVoid Loop () {

        await UniTask.WaitUntil (() => Services.isInited);

        cardIconQueue.CreateQueue ();

        heads.GetComponent<EnemyHead> ().UpdateHead (getRandomHead ());
        Services.assets.PlaySound ("start_game").Forget ();

        while (true) {

            await tutorial.CheckCurrentTutor ();

            await UniTask.WaitUntil (() => state == GameLoopState.IDLE);

            int currentChoise = -1;

            cardItem = cardIconQueue.GetFirstItem ();

            tutorial.ChangeCard(cardItem);
           
            var t1 = cardIconQueue.Shift ();
            var t2 = SwipeCard ();
            await UniTask.WhenAll (t1, t2);

            Swipe swipe = cardSwipe[currentViewIndex].GetComponent<Swipe> ();
            currentChoise = swipe.currentChoise;

            int timestamp = GameTime.GetTime ();

            ChoiceData chMeta = null;
            if (cardItem.me == true) {
                chMeta = currentChoise == Swipe.LEFT_CHOICE ? cardItem.data.left : cardItem.data.right;
            } else {
                chMeta = currentChoise == Swipe.LEFT_CHOICE ? cardItem.data.eLeft : cardItem.data.eRight;
            }

            lock (this) {
                Services.player.Execute (cardItem.data, currentChoise, chMeta, cardItem.me, timestamp, Services.enemy);
            }

            Services.data.swipeCount++;
            Services.assets.PlaySound ("card_move").Forget ();
            //check global state
            if (!CheckGlobalState (timestamp)) {

                state = GameLoopState.DIABLED;
                EndTurnCanvas?.SetActive (true);

                bool isWin = Services.data.isWin (timestamp);

                if (isWin == true) {
                    Services.assets.PlaySound ("win").Forget ();
                } else {
                    Services.assets.PlaySound ("lose").Forget ();
                }

                Analytics.CustomEvent ("gameOver", new Dictionary<string, object> { { "isWin", isWin } });
                Analytics.FlushEvents ();

            } else if (chMeta.reward.Exists (r => r.category == GameDataManager.ACTION_ID && r.id == 1)) {
                Services.assets.PlaySound ("attack").Forget ();
            } else if (chMeta.reward.Exists (r => r.category == GameDataManager.RESOURCE_ID && r.id == 1)) {
                Services.assets.PlaySound ("heal").Forget ();
            }
            //await UniTask.Delay(100);

            //shift
            /*for (int i = 1; i < queue.Count; i++) {
                queue[i - 1] = queue[i];
            }
            queue[queue.Count - 1] = currentItem;

            await cardIconQueue.Shift ();
*/
        }
    }

    public async void StartGame () {

        Services.assets.PlaySound ("tap").Forget ();

        Services.data.fingerStep = 0;
        if (Services.data.isWin (0)) {
            Services.data.IncreaseTutor ();
        }

        await Services.player.Init ();
        await Services.enemy.Init ();

        cardIconQueue.CreateQueue ();

        heads.GetComponent<EnemyHead> ().UpdateHead (getRandomHead ());
        EndTurnCanvas?.SetActive (false);

        state = GameLoopState.IDLE;

    }

    private bool CheckGlobalState (int time) {
        if (Services.data.isWin (time)) {
            return false;
        } else if (Services.data.isFail (time)) {
            return false;
        }
        return true;
    }

    public async UniTask OpenNewCard () {

        //cardSwipe[currentViewIndex].SetActive (false);

        currentViewIndex++;
        if (currentViewIndex >= cardSwipe.Length) {
            currentViewIndex = 0;
        }

        cardSwipe[currentViewIndex].SetActive (true);
        //cardSwipe[currentViewIndex].GetComponent<CardController> ().UpdateHUD (currentItem.card);

        // currentItem.card.anim = currentItem.card.anim != null ? currentItem.card.anim : "FadeIn_1";

        //int time = GameTime.GetTime ();
        // SwipeParams swipeParams;
        //swipeParams.leftAvailable = false;
        // Services.data.CheckConditions (currentItem.card.left.conditions, time) && Services.data.CheckAvailableCost (currentItem.card.left.cost);
        // swipeParams.rightAvailable = false;
        // Services.data.CheckConditions (currentItem.card.right.conditions, time) && Services.data.CheckAvailableCost (currentItem.card.right.cost);

        //cardViews[currentViewIndex].GetComponent<Animation> ().Stop();

        cardSwipe[currentViewIndex].GetComponent<CardController> ().UpdateData (cardItem);

        OnNewCard?.Invoke (cardSwipe[currentViewIndex]);

        // await UniTask.DelayFrame (2000);

        await cardSwipe[currentViewIndex].GetComponent<CardController> ().FadeIn ();

        if (Services.data.tutorStep == 0) {
            Services.player.OnShowFinger ();
        }
        /*  var cd = FindObjectsOfType<MonoBehaviour> ().OfType<ICardUpdate<GameObject>> ();
         foreach (ICardUpdate<GameObject> c in cd) {
             c.CardUpdate (cardViews[currentViewIndex]);
         }*/
        //cardViews[currentViewIndex].GetComponent<Animation> ().Play (currentItem.card.anim, PlayMode.StopAll);

    }

}