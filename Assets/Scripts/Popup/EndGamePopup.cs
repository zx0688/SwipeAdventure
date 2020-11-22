using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePopup : MonoBehaviour {
    // Start is called before the first frame update
    private Image headerImgFail;
    private Image headerImgWin;

    private Image back;

    private Transform glowContainer;
    private Transform glowBack;
    private GameObject glowIcon;
    private Image icon;
    private Text buttonTf;

    void Start () {

        if (Services.isInited)
            OnEnable ();
    }

    void OnEnable () {

        if (!Services.isInited)
            return;

        bool isWin = Services.data.isWin (1);
        headerImgFail.gameObject.SetActive (false);
        headerImgWin.gameObject.SetActive (false);

        Image headerImg = isWin ? headerImgWin : headerImgFail;
        headerImg.gameObject.SetActive (true);

        buttonTf.text = isWin ? "FIGHT" : "TRY AGAIN";

        RectTransform rect = headerImg.gameObject.GetComponent<RectTransform> ();
        rect.anchoredPosition = new Vector2 (rect.anchoredPosition.x, 1420f);
        rect.DOAnchorPosY (576f, 1f).SetEase (Ease.OutCirc).SetAutoKill ();

        Color color = back.color;
        color.a = 0f;
        back.color = color;

        back.DOFade(0.6f, 1f);

        UpdateIcon ().Forget ();

        glowContainer.gameObject.SetActive (isWin);

        if (isWin) {
            glowContainer.localScale = new Vector3 (0f, 0f, 0f);
            glowContainer.DOScale (new Vector3 (1f, 1f, 1f), 1f);
        }
    }

    async UniTaskVoid UpdateIcon () {

        if (Services.isInited == false) {
            icon.sprite = await Services.assets.GetSprite ("WinSword", true);
        } else if (Services.data.tutorStep >= Services.data.game.config.tutorial.Count) {
            icon.sprite = await Services.assets.GetSprite ("WinSword", true);
        } else {
            int step = Services.data.tutorStep + 1;
            int id = Services.data.game.config.tutorial[step];
            icon.sprite = await Services.assets.GetSprite ("Cards/" + id + "/" + "me", true);
        }
    }

    void Awake () {

        headerImgFail = transform.Find ("HeaderFail").gameObject.GetComponent<Image> ();
        headerImgWin = transform.Find ("HeaderWin").gameObject.GetComponent<Image> ();

        buttonTf = transform.Find ("ButtonText").GetComponent<Text> ();

        glowContainer = transform.Find ("GlowContainer");
        glowBack = glowContainer.Find ("GlowBack");
        icon = glowContainer.Find ("Icon").GetComponent<Image> ();

        back = GetComponent<Image> ();
    }

    // Update is called once per frame
    void Update () {
        glowBack.Rotate (0f, 0f, 0.2f, Space.Self);
    }
}