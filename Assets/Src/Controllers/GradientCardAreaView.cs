using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradientCardAreaView : MonoBehaviour{
    // Start is called before the first frame update
    private Swipe swipe;
    public float speed_fadeout;
    private UIGradient gradient;
    private Image image;

    public void CardUpdate (GameObject card) {
        swipe = card.GetComponent<Swipe> ();

    }

    void Start () {
        gradient = GetComponent<UIGradient> ();
        image = GetComponent<Image> ();
    }

    // Update is called once per frame
    void Update () {
        var tempColor = image.color;
        if (swipe != null && (swipe.state == SwipeState.DRAG || swipe.state == SwipeState.RETURN)) {
            if (swipe.swipeParams.leftAvailable && swipe.swipeParams.rightAvailable) {
                Mathf.MoveTowards(tempColor.a, 0f, speed_fadeout);
            } else {
                tempColor.a = Mathf.MoveTowards(tempColor.a, swipe.deviation, speed_fadeout * 2);
                image.color = tempColor;
                gradient.m_angle = Vector2.Angle (new Vector2 (0f, 1f), swipe.vector);
            }
        } else {
            tempColor.a = Mathf.MoveTowards(tempColor.a, 0f, speed_fadeout);
        }
        image.color = tempColor;
    }

}