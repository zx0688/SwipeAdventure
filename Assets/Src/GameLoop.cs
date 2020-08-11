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

    public CardItem cardItem;
    private CardIconQueue cardIconQueue;

    private GameObject EndTurnCanvas;

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

        //queue = new List<QueueItem> ();
        state = GameLoopState.IDLE;
        EndTurnCanvas = GameObject.Find ("EndGameCanvas");
        EndTurnCanvas?.SetActive (false);

        Loop ().Forget ();
    }

    public GameObject GetGameObjectCard () {
        return cardSwipe[currentViewIndex];
    }

    public async UniTask SwipeCard () {
        OpenNewCard ();
        Swipe swipe = cardSwipe[currentViewIndex].GetComponent<Swipe> ();
        await UniTask.WaitUntil (() => swipe.currentChoise != -1);
    }

    async UniTaskVoid Loop () {

        await UniTask.WaitUntil (() => Services.isInited);

        cardIconQueue.CreateQueue ();

        while (true) {

            await UniTask.WaitUntil (() => state == GameLoopState.IDLE);

            int currentChoise = -1;

            cardItem = cardIconQueue.GetFirstItem ();

            var t1 = cardIconQueue.Shift ();
            var t2 = SwipeCard ();
            await UniTask.WhenAll (t1, t2);

            Swipe swipe = cardSwipe[currentViewIndex].GetComponent<Swipe> ();
            currentChoise = swipe.currentChoise;

            int timestamp = GameTime.GetTime ();

            lock (this) {
                Services.player.Execute (cardItem.data, currentChoise, cardItem.me, timestamp, Services.enemy);
                //Services.enemy.Execute (currentItem.card, swipe.currentChoise, true, timestamp, Services.player);
                // Services.player.Execute (currentItem.card, choiseP, timestamp, Services.enemy);
                // Services.enemy.Execute (currentItem.card, choiseE, timestamp, Services.player);
            }

            // await cardIconQueue.Execute (currentItem, currentChoise);

            //check global state
            if (!CheckGlobalState (timestamp)) {

                state = GameLoopState.DIABLED;
                EndTurnCanvas?.SetActive (true);

                Analytics.CustomEvent ("gameOver", new Dictionary<string, object> { { "isWin", Services.data.isWin (timestamp) } });
                Analytics.FlushEvents ();

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
        await Services.player.Init ();
        await Services.enemy.Init ();

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

    private void OpenNewCard () {

        cardSwipe[currentViewIndex].SetActive (false);

        currentViewIndex++;
        if (currentViewIndex >= cardSwipe.Length) {
            currentViewIndex = 0;
        }

        cardSwipe[currentViewIndex].SetActive (true);
        //cardSwipe[currentViewIndex].GetComponent<CardController> ().UpdateHUD (currentItem.card);

        // currentItem.card.anim = currentItem.card.anim != null ? currentItem.card.anim : "FadeIn_1";

        int time = GameTime.GetTime ();
        // SwipeParams swipeParams;
        //swipeParams.leftAvailable = false;
        // Services.data.CheckConditions (currentItem.card.left.conditions, time) && Services.data.CheckAvailableCost (currentItem.card.left.cost);
        // swipeParams.rightAvailable = false;
        // Services.data.CheckConditions (currentItem.card.right.conditions, time) && Services.data.CheckAvailableCost (currentItem.card.right.cost);

        //cardViews[currentViewIndex].GetComponent<Animation> ().Stop();

        cardSwipe[currentViewIndex].GetComponent<CardController> ().UpdateData (cardItem);

        OnNewCard?.Invoke (cardSwipe[currentViewIndex]);

        cardSwipe[currentViewIndex].GetComponent<CardController> ().FadeIn ().Forget ();
        /*  var cd = FindObjectsOfType<MonoBehaviour> ().OfType<ICardUpdate<GameObject>> ();
         foreach (ICardUpdate<GameObject> c in cd) {
             c.CardUpdate (cardViews[currentViewIndex]);
         }*/
        //cardViews[currentViewIndex].GetComponent<Animation> ().Play (currentItem.card.anim, PlayMode.StopAll);

    }

}