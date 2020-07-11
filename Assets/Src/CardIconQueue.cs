using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class CardIconQueue : MonoBehaviour {
    public static readonly int UNITS_LIMIT = 6;
    [SerializeField]
    private List<GameObject> cardIcons;
    [SerializeField]
    private float fMoving = 1f;
    [SerializeField]
    private float spacing = 20;

    private GameLoop gameloop;

    void Start () {

        gameloop = GetComponent<GameLoop> ();

        foreach (GameObject i in cardIcons) {
            i.SetActive (false);
        }

        //await Loop();
    }

    public QueueItem GetFirstItem () {
        return cardIcons[0].GetComponent<CardIconController> ().data;
    }

    public void Add (QueueItem item) {
        for (int i = 0; i < cardIcons.Count; i++) {
            GameObject icon = cardIcons[i];
            if (cardIcons[i].activeInHierarchy == true)
                continue;

            cardIcons[i].SetActive (true);
            cardIcons[i].GetComponent<CardIconController> ().UpdateData (item);
            PlaceToPosition (cardIcons[i], i);
            break;
        }
    }
    public void CreateQueue () {
        bool me = UnityEngine.Random.Range (0, 1f) > 0.5;

        for (int i = 0; i < cardIcons.Count; i++) {
            QueueItem q = Services.data.GetNewCard (default (CardData), me, 0);
            me = !me;
            Add (q);
        }
    }
    public async UniTask Shift () {

        GameObject first = cardIcons[0];

        for (int i = 0; i < cardIcons.Count - 1; i++) {
            cardIcons[i] = cardIcons[i + 1];
            cardIcons[i].SetActive (true);
        }

        cardIcons[cardIcons.Count - 1] = first;
        cardIcons[cardIcons.Count - 1].SetActive (false);
        PlaceToPosition (cardIcons[cardIcons.Count - 1], cardIcons.Count);

        Vector3 fpivot = GetPositionByIndex (cardIcons[0], 0);
        RectTransform frt = cardIcons[0].GetComponent<RectTransform> ();
        do {
            for (int i = 0; i < cardIcons.Count; i++) {
                MoveTowards (cardIcons[i], i - 1);
            }
            await UniTask.Yield ();

        } while (frt.anchoredPosition.x > fpivot.x);

        for (int i = 0; i < cardIcons.Count; i++) {
            PlaceToPosition (cardIcons[i], i);
        }
    }

    /* public async UniTask RemoveIcon (QueueItem item) {
        GameObject cardIcon = cardIcons.Find (i => i.GetComponent<CardIconData> ().item == item);
        if (cardIcon == null) { return; }
        cardIcon.GetComponent<CardIconData> ().item = null;
        await cardIcon.GetComponent<CardIconData> ().FadeOut ();
        cardIcon.SetActive (false);
    }*/

    /*public async UniTask CreateNewIcon (QueueItem item) {
        GameObject cardIcon = cardIcons.Find (i => i.GetComponent<CardIconData> ().item == null);
        if (cardIcon == null) { return; }
        cardIcon.SetActive (true);
        cardIcon.GetComponent<CardIconData> ().UpdateHUD (item);
        PlaceToPosition (cardIcon, cardIcons.Count);

        await cardIcon.GetComponent<CardIconData> ().FadeIn ();
        //await MoveToPivot (cardIcon, cardIcons.Count);
    }*/

    /*public async UniTask UpdatePosition (QueueItem item, int index) {
        GameObject cardIcon = cardIcons.Find (i => i.GetComponent<CardIconData> ().item == item);
        if (cardIcon == null) { return; }
        await MoveToPivot (cardIcon, index);
    }
    public async UniTask Execute (QueueItem item, int choise) {

        GameObject cardIcon = cardIcons.Find (i => i.GetComponent<CardIconData> ().item == item);
        if (cardIcon == null) { return; }

        await cardIcon.GetComponent<CardIconData> ().Execute (choise);
        await RemoveIcon (item);
    }*/

    private void PlaceToPosition (GameObject icon, int index) {
        RectTransform rt = icon.GetComponent<RectTransform> ();
        rt.anchoredPosition = GetPositionByIndex (icon, index);
    }

    private void MoveTowards (GameObject icon, int index) {
        Vector3 pivot = GetPositionByIndex (icon, index);
        RectTransform rt = icon.GetComponent<RectTransform> ();
        rt.anchoredPosition = Vector3.MoveTowards (rt.anchoredPosition, pivot, fMoving);
    }

    private Vector3 GetPositionByIndex (GameObject icon, int index) {
        RectTransform rt = icon.GetComponent<RectTransform> ();
        return new Vector3 (index * (spacing + rt.rect.width) + rt.rect.width / 2, rt.anchoredPosition.y, 0);
    }

    void Update () {

    }
}