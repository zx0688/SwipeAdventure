using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    public class CardController : MonoBehaviour, IUpdateData<QueueItem> {
        // Start is called before the first frame update

       // [SerializeField]
       // public GameObject actionText;

        [SerializeField]
        public GameObject actionPanelContainer;
        
        [SerializeField]
        public GameObject actionPanelItems;

        public CardData data;
        public bool me;
        private Swipe swipe;
        private CanvasGroup canvasGroup;
        private bool left;

        public void UpdateData (QueueItem data) {
            this.data = data.card;
            this.me = data.me;

            UpdateHUD ();
            //UpdateEffectPanel ();

            left = true;
            UpdateSide ();

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

        async void UpdateHUD () {

            foreach (Transform g in transform.GetComponentsInChildren<Transform> ()) {

                switch (g.name) {
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

            if(actionPanelContainer.activeSelf == true)
            {
                actionPanelContainer.SetActive(false);
            }
        }

        void UpdateSide () {

            ChoiseData chd = null;
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

            foreach (RewardData r in chd.reward)
                rs.Add (r);

            Transform trActionPanel = actionPanelItems.transform;
            int children = trActionPanel.childCount;
            for (int i = 0; i < children; ++i) {
                Transform t = trActionPanel.GetChild (i);
                GameObject g = t.gameObject;
                if (i < rs.Count) {
                    bool add = chd.reward.Contains(rs[i]);
                    g.GetComponent<ResourceActionController> ().UpdateDataSign (rs[i], add);
                    g.SetActive (true);
                } else {
                    g.SetActive (false);
                }
            }
        }

        void Start () {
            swipe = GetComponent<Swipe> ();
            //canvasGroup = actionPanel.GetComponent<CanvasGroup> ();
        }

        // Update is called once per frame
        void Update () {

            if (swipe != null && (swipe.state == SwipeState.DRAG || swipe.state == SwipeState.RETURN) && Mathf.Abs(swipe.deviation) > 0.05) {

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
                if(actionPanelContainer.activeSelf == false)
                    actionPanelContainer.SetActive(true);


                //canvasGroup.alpha = Mathf.Min (Mathf.Abs (swipe.deviation * 8), 1f);
            } else {
                
                if(actionPanelContainer.activeSelf == true)
                    actionPanelContainer.SetActive(false);
                //canvasGroup.alpha = 0;
            }
        }
    }
}