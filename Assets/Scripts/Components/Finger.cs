using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class Finger : MonoBehaviour
{
    private static bool[] directions = new bool[]{false, false, true};

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

        
    }

    IEnumerator MoveOn (int to) {

    
        while ((rectTransform.anchoredPosition.x > to && to < 0) || (rectTransform.anchoredPosition.x < to && to > 0)) {

            rectTransform.anchoredPosition = Vector2.MoveTowards (rectTransform.anchoredPosition, new Vector2(to, rectTransform.anchoredPosition.y), 5f);
            yield return null;
        }

        gameObject.SetActive(false);
    }


    private void OnShow()
    {
        rectTransform.anchoredPosition = pivotPoint;
        gameObject.SetActive(true);

        bool right = Services.data.fingerStep < directions.Length ? directions[Services.data.fingerStep] : true;

        Services.data.fingerStep++;

        coroutine = StartCoroutine(MoveOn(right ? -300 : 300));
        coroutine = null;
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
