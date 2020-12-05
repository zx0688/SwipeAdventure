using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    public class CardController : MonoBehaviour, IUpdateData<CardItem> {
        // Start is called before the first frame update

        // [SerializeField]
        // public GameObject actionText;
        public static event Action<ConditionData> OnUnavailableCondition;

        [SerializeField]
        public GameObject actionPanelContainer;

        [SerializeField]
        public GameObject actionPanelItems;

        public CardData data;
        public bool me;
        private Swipe swipe;
        private CanvasGroup canvasGroup;
        private bool left;
        private bool tutor;

        private ResourceOneStateController oneStateController;

        public Sprite sme;
        public Sprite senemy;

        public bool rightAvailable;
        public bool leftAvailable;

        public void UpdateData (CardItem data) {
            this.data = data.data;
            this.me = data.me;

            // swipeParams.rightAvailable = false;
            // leftConditions = Services.data.GetUnavailableConditions (this.data.right.conditions, time) && Services.data.CheckAvailableCost (currentItem.card.right.cost);
            tutor = false;//Services.data.preTutorStep == 0;
            leftAvailable = true;
            rightAvailable = true;

            oneStateController.changedPlayer (me);
            oneStateController.OnUpdateCountP ();

            UpdateHUD ().Forget();
            //UpdateEffectPanel ();

            left = true;
            UpdateSide ();

        }

        public async UniTask FadeIn () {

            swipe.ConstructNewSwipe ();

            RectTransform rectTransform = GetComponent<RectTransform> ();
            rectTransform.anchoredPosition = new Vector2 (0f, 130f);

            rectTransform.DOKill ();
            rectTransform.DOAnchorPosY (0f, 0.5f).SetAutoKill ();
            await UniTask.WaitUntil (() => rectTransform.anchoredPosition.y <= 0);

            //while (GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
            //    await UniTask.Yield ();
            //}

            swipe.StartSwipe ();
        }

        /* void UpdateEffectPanel () {

             List<RewardData> rs = new List<RewardData> ();

             if (me == true) {
                 List<ResourceItem> res = Services.player.GetEffects ();
                 foreach (ResourceItem r in res) {
                     RewardData rd = new RewardData ();
                     rd.id = r.id;
                     rd.category = 1;
                     rd.count = r.count;
                     rs.Add (rd);
                 }
             }

             Transform trActionPanel = effectPanel.transform;
             int children = trActionPanel.childCount;
             for (int i = 0; i < children; ++i) {
                 Transform t = trActionPanel.GetChild (i);
                 GameObject g = t.gameObject;
                 if (i < rs.Count) {
                     g.GetComponent<ResourceActionController> ().UpdateData (rs[i]);
                     g.SetActive (true);
                 } else {
                     g.SetActive (false);
                 }
             }
         }*/

        async UniTaskVoid UpdateHUD () {

            foreach (Transform g in transform.GetComponentsInChildren<Transform> ()) {

                switch (g.name) {
                    case "Border":
                        Image border = g.GetComponent<Image> ();
                        border.sprite = me == true ? sme : senemy;
                        break;
                    case "Image":
                        Image icon = g.GetComponent<Image> ();

                        string iconType = me == true ? "me" : "enemy";
                        icon.sprite = await Services.assets.GetSprite ("Cards/" + data.id + "/" + iconType, true);
                        break;
                    case "Header":
                        g.GetComponent<Text> ().text = data.header;
                        break;
                    case "Description":
                        g.GetComponent<Text> ().text = data.description;
                        break;
                    default:
                        break;
                }
            }

            if (actionPanelContainer.activeSelf == true || tutor == true) {
                actionPanelContainer.SetActive (false);
            }
        }

        void UpdateSide () {

            ChoiceData chd = null;
            if (me == true) {
                chd = left ? data.left : data.right;
            } else {
                chd = left ? data.eLeft : data.eRight;
            }

            List<RewardData> rs = new List<RewardData> ();

            //Text at = actionText.GetComponent<Text> ();
            //at.text = chd.text;

            foreach (RewardData r in chd.cost) {
                rs.Add (r);
            }

            int increaseCount = me == true ? Services.player.AvailableResource (4) : Services.enemy.AvailableResource (4);
            foreach (RewardData r in chd.reward) {
                RewardData _r = Utils.DeepCopyClass<RewardData> (r);

                if (increaseCount > 0 && _r.category == GameDataManager.ACTION_ID && _r.id == 2)
                    _r.count *= 2;

                rs.Add (r);
            }

            // ActionSide?.Invoke (chd.cost, chd.reward);

            Transform trActionPanel = actionPanelItems.transform;
            int children = trActionPanel.childCount;
            for (int i = 0; i < children; ++i) {
                Transform t = trActionPanel.GetChild (i);
                GameObject g = t.gameObject;
                if (i < rs.Count) {
                    bool add = chd.reward.Contains (rs[i]);
                    g.GetComponent<ResourceActionController> ().UpdateDataSign (rs[i], add);
                    g.SetActive (true);
                } else {
                    g.SetActive (false);
                }
            }
        }

        void Start () {

            Swipe.OnChangeDirection += OnChangeDirection;
            //canvasGroup = actionPanel.GetComponent<CanvasGroup> ();
        }

        void Awake () {
            swipe = GetComponent<Swipe> ();
            oneStateController = transform.Find ("Increase").GetComponent<ResourceOneStateController> ();
        }

        public void OnChangeDirection (SwipeDirection direction) {
            leftAvailable = true;
            rightAvailable = true;

            if (Swipe.state != SwipeState.DRAG || !gameObject.activeSelf)
                return;

            if (this.me == false)
                return;

            List<ConditionData> _conditions = null;
            if (me == true) {
                _conditions = direction == SwipeDirection.RIGHT ? this.data.right.conditions : this.data.left.conditions;
            } else {
                _conditions = direction == SwipeDirection.RIGHT ? this.data.eRight.conditions : this.data.eLeft.conditions;
            }

            int time = GameTime.GetTime ();
            List<ConditionData> conditions = Services.data.GetUnavailableConditions (_conditions, time, false, me);

            if (conditions.Count == 0)
                return;

            //sort conditions
            ConditionData one = conditions.Find (c => c.enemy == false);

            if (one == null)
                return;

            if (direction == SwipeDirection.RIGHT)
                rightAvailable = false;
            else
                leftAvailable = false;

            OnUnavailableCondition?.Invoke (one);
        }

        public void OnEndDrag () {
            if (actionPanelContainer.activeSelf == true || tutor == true) {
                //  ActionPanelTrigger?.Invoke (false);
                actionPanelContainer.SetActive (false);
            }
        }
        public void OnDrag (int direction) {

            bool l = direction == -1;
            if (left != l) {
                left = l;
                UpdateSide ();
            }
            if (actionPanelContainer.activeSelf == false && tutor == false) {
                actionPanelContainer.SetActive (true);
                // ActionPanelTrigger?.Invoke (true);
            }

        }

        // Update is called once per frame
        void Update () {

            /* if (swipe != null && (swipe.state == SwipeState.DRAG || swipe.state == SwipeState.RETURN) && Mathf.Abs (swipe.deviation) > 0.05) {

                 // bool changeSide = swipe.deviation < 0;
                 //left = swipe.deviation < 0;

                 if (left != swipe.deviation < 0) {
                     left = swipe.deviation < 0;
                     UpdateSide ();
                 }
                 Debug.Log (swipe.deviation < 0);
                 //if (changeSide) {

                 //   UpdateSide ();
                 // }
                 if (actionPanelContainer.activeSelf == false) {
                     actionPanelContainer.SetActive (true);
                     ActionPanelTrigger?.Invoke (true);
                 }
                 //canvasGroup.alpha = Mathf.Min (Mathf.Abs (swipe.deviation * 8), 1f);
             } else {

                 if (actionPanelContainer.activeSelf == true) {
                     ActionPanelTrigger?.Invoke (false);
                     actionPanelContainer.SetActive (false);
                 }
                 //canvasGroup.alpha = 0;
             }*/
        }
    }
}