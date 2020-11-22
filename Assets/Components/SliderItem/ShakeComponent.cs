using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShakeComponent : MonoBehaviour {
    // Start is called before the first frame update
    private RectTransform rectTransform;

    // [SerializeField]
    private float shake_speed = 8f;
    // [SerializeField]
    private float radius = 1000f;

    private Vector2 nextPosition;
    private Vector2 originPosition;
    public bool shake = false;

    private bool ret = false;

    void Start () {
        rectTransform = this.GetComponent<RectTransform> ();
        originPosition = rectTransform.anchoredPosition;

        //float angle = Random.Range (0f, 6.28f);
        //nextPosition = new Vector2 (originPosition.x, Random.Range (0f, 10f));
    }

    public void Shake()
    {
        rectTransform.DOKill(false);
        rectTransform.anchoredPosition = new Vector2(originPosition.x, originPosition.y);
        
        shake = true;
        rectTransform.DOShakeAnchorPos(2f, new Vector3(0, 22, 0), 13, 89, true, false).SetLoops(-1);
        
    }

    public void StopShake()
    {
        shake = false;
        rectTransform.DOKill(false);
        rectTransform.DOAnchorPos(originPosition, 0.5f, true).SetAutoKill();
    }

    // Update is called once per frame
    void Update () {


       /*  if (shake == false) {
           
            rectTransform.anchoredPosition = Vector2.MoveTowards (rectTransform.anchoredPosition, originPosition, shake_speed);
            return;
        } */
       
    }

}