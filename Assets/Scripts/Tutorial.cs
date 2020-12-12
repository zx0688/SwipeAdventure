using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {

    [SerializeField]
    public GameObject enemyHP;

    [SerializeField]
    public GameObject myHP;
    [SerializeField]
    public GameObject actionPanel;
    [SerializeField]
    public GameObject head;
    [SerializeField]
    public GameObject unitPanel;
    [SerializeField]
    public GameObject myEnergy;

    [SerializeField]
    public GameObject tutorText;
    // Start is called before the first frame update
    void Start () {

    }

    public void ChangeCard (CardItem q) {

        if (Services.data.tutorStep != 0)
            return;

        tutorText.GetComponent<Text> ().text = q.me == true ? "ATTACK" : "DEFEND";

        tutorText.GetComponent<Text> ().color = q.me == true ? new Color32 (26, 174, 159, 255) : new Color32 (232, 108, 96, 255);

        q.data = Utils.DeepCopyClass<CardData> (q.data);

        /* if (Services.data.swipeCount == 0 || Services.data.swipeCount == 1) {
             q.data.right.cost = new List<RewardData> ();
             q.data.right.reward[0].count = 1;
             q.data.eRight = q.data.right;

             q.data.left.cost = new List<RewardData> ();
             q.data.left.reward = new List<RewardData> ();
             q.data.left.reward.Add (new RewardData ());
             q.data.left.reward[0].count = 1;
             q.data.left.reward[0].id = 2;
             q.data.left.reward[0].category = GameDataManager.RESOURCE_ID;
             q.data.left.reward[0].conditions = new List<ConditionData>();
             q.data.left.conditions = new List<ConditionData> ();

             q.data.eLeft = q.data.left;

         } else if (Services.data.swipeCount == 2) {
             q.data.right.cost = new List<RewardData> ();
             q.data.right.reward[0].count = 1;
             q.data.right.reward[0].category = 1;
             q.data.right.conditions = new List<ConditionData> ();
             q.data.left.conditions = q.data.right.conditions;
             q.data.eRight = q.data.right;
             q.data.left = Utils.DeepCopyClass<ChoiceData> (q.data.right);
             q.data.eLeft = q.data.left;
         } else if (Services.data.swipeCount == 4) {
             q.data.right.cost = new List<RewardData> ();
             q.data.right.reward[0].count = 3;
             q.data.right.reward[0].category = GameDataManager.ACTION_ID;
             q.data.eRight = q.data.right;
             q.data.left = Utils.DeepCopyClass<ChoiceData> (q.data.right);
             q.data.eLeft = q.data.left;
         }*/

    }

    public async UniTask<int> CheckCurrentTutor () {

        if (Services.data.tutorStep != 0)
            return 1;

        int swipeStep = Services.data.swipeCount;
        head.SetActive (swipeStep >= 0);
        actionPanel.SetActive (false);
        enemyHP.SetActive (swipeStep > 0);
        myHP.SetActive (swipeStep > 1);
        myEnergy.SetActive (swipeStep > 0);
        unitPanel.SetActive (swipeStep >= 3);
        tutorText.SetActive (swipeStep < 2);
        


        if (swipeStep == 0) {

            myEnergy.SetActive (false);
            myHP.SetActive (false);
            tutorText.SetActive (false);

            head.GetComponent<EnemyHead> ().SetCustomEmotion ("emotion1");

            RectTransform rectTransform = head.GetComponent<RectTransform> ();
            float _y = rectTransform.anchoredPosition.y;
            rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x, 400);
            await rectTransform.DOAnchorPosY (_y, 0.8f, false).AsyncWaitForCompletion ();
            await UniTask.Delay (500);
            head.GetComponent<EnemyHead> ().SetCustomEmotion ("emotion4");
            await rectTransform.DOShakeAnchorPos (1f, new Vector3 (0, 22, 0), 13, 89, true, false).AsyncWaitForCompletion ();
            rectTransform.DOKill (false);

            head.GetComponent<EnemyHead> ().SetCustomEmotion ("emotion1");
            enemyHP.SetActive (true);
            tutorText.SetActive (true);
            tutorText.GetComponent<CanvasGroup>().alpha = 0;

            CanvasGroup group = enemyHP.GetComponent<CanvasGroup> ();
            group.alpha = 0;
            await group.DOFade (1f, 0.7f).AsyncWaitForCompletion ();
            group.DOKill ();

        } else if (swipeStep == 1) {
            //enemyHP.SetActive (false);
            myHP.SetActive (true);

            CanvasGroup group = myHP.GetComponent<CanvasGroup> ();
            group.alpha = 0;
            await group.DOFade (1f, 0.7f).AsyncWaitForCompletion ();
            group.DOKill ();

        }

        return 1;
    }

    public async UniTask<int> PostCardAnimation () {

        int swipeStep = Services.data.swipeCount;
        unitPanel.SetActive (swipeStep >= 2);

        if (swipeStep == 0) {

            myEnergy.SetActive (true);
            CanvasGroup group = myEnergy.GetComponent<CanvasGroup> ();
            group.alpha = 0;
            await group.DOFade (1f, 0.7f).AsyncWaitForCompletion ();
            group.DOKill ();

        } else if (swipeStep == 2) {
            unitPanel.SetActive (true);
            CanvasGroup group = unitPanel.GetComponent<CanvasGroup> ();
            group.alpha = 0;
            await group.DOFade (1f, 0.7f).AsyncWaitForCompletion ();
            group.DOKill ();
        }

        return 1;
    }
}