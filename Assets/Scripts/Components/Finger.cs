using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class Finger : MonoBehaviour
{


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


    private void OnShow(bool right)
    {
        rectTransform.anchoredPosition = pivotPoint;
        gameObject.SetActive(true);
        coroutine = StartCoroutine(MoveOn(right == true ? -300 : 300));
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
