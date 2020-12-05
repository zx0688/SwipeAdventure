using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;
public class Tutorial : MonoBehaviour {

    [SerializeField]
    public GameObject enemyHP;

    [SerializeField]
    public GameObject bottomPanel;
    [SerializeField]
    public GameObject actionPanel;
    [SerializeField]
    public GameObject head;
    [SerializeField]
    public GameObject unitPanel;
    // Start is called before the first frame update
    void Start () {

    }

    public void ChangeCard (CardItem q) {

        if (Services.data.tutorStep != 0)
            return;

        q.data = Utils.DeepCopyClass<CardData> (q.data);

        if (Services.data.swipeCount == 0 || Services.data.swipeCount == 1) {
            q.data.right.cost = new List<RewardData> ();
            q.data.right.reward[0].count = 1;
            q.data.eRight = q.data.right;
            q.data.left = Utils.DeepCopyClass<ChoiceData> (q.data.right);
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
        }

    }

    public async UniTask<int> CheckCurrentTutor () {
        
        if (Services.data.tutorStep != 0)
            return 1;

        int swipeStep = Services.data.swipeCount;
        head.SetActive (swipeStep >= 0);
        actionPanel.SetActive (false);
        enemyHP.SetActive (swipeStep > 0);
        bottomPanel.SetActive (swipeStep > 1);
        unitPanel.SetActive (swipeStep > 2);

        if (swipeStep == 0) {

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

        } else if (swipeStep == 1) {
            enemyHP.SetActive (false);
            bottomPanel.SetActive (true);

            /*RectTransform rectTransform = head.GetComponent<RectTransform> ();
            head.GetComponent<EnemyHead> ().SetCustomEmotion ("emotion4");
            await rectTransform.DOShakeAnchorPos (1f, new Vector3 (0, 22, 0), 13, 89, true, false).AsyncWaitForCompletion ();
            rectTransform.DOKill (false);

            head.GetComponent<EnemyHead> ().SetCustomEmotion ("emotion1");
            */
            //await UniTask.Delay (5000);
        }

        return 1;
    }
}