using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using UnityEngine;

public class Finger : MonoBehaviour
{
    private static bool[] directions = new bool[]{false, true, true};

    private RectTransform rectTransform;
    private Vector2 pivotPoint;
    private Coroutine coroutine;
    
    void Awake () {

        if (Services.isInited)
            Init ();
        else
            Services.OnInited += Init;
    }
    private void Init () {

        gameObject.SetActive(false);

        rectTransform = GetComponent<RectTransform>();

        pivotPoint = rectTransform.anchoredPosition;
        
        Services.OnInited -= Init;
        Services.player.ShowFinger += OnShow;

        Swipe.OnFirstSwipeCard += OnHide;


    }

    private void OnHide()
    {
        rectTransform.DOKill(false);
        gameObject.SetActive(false);
    }
    private void OnShow()
    {
        rectTransform.DOKill(false);
        rectTransform.anchoredPosition = pivotPoint;
        
        gameObject.SetActive(true);

        bool right = Services.data.fingerStep < directions.Length ? directions[Services.data.fingerStep] : true;

        Services.data.fingerStep++;

        float to = right ? -350 : 350;

        rectTransform.DOAnchorPosX(to, 1.1f, false).SetLoops(-1);
     
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
