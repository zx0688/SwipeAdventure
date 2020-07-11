using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public struct SwipeParams {
    public bool leftAvailable;
    public bool rightAvailable;

};
public enum SwipeState {
    DISABLE,
    RETURN,
    DRAG,
    IDLE
}

public class Swipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {

    public static readonly int LEFT = 0;
    public static readonly int RIGHT = 1;
    public static readonly int ANY = -1;

    [System.Serializable] public class mEvent : UnityEvent { }

    [SerializeField] public Canvas _parent;
    [SerializeField] private float fMovingSpeed = 15;
    [Range (0f, 1000f)] public float swipeDetectionLimit_LR = 1f;
    [SerializeField] private float fRotation = 0;
    [SerializeField] private float fScale = 1f;

    [SerializeField] private Vector2 pivotPoint;

    public int currentChoise;

    // [SerializeField] private GameObject cardObject;

    // [SerializeField] private mEvent OntriggerLeft;
    //  [SerializeField] private mEvent OntriggerRight;
    [SerializeField] public event Action<float> MovingDispather;
    [SerializeField] public static event Action<int> OnTrigger;

    public float deviation;
    public Vector2 vector;

    //private DragDropChoise choise;

    public SwipeState state { get; private set; }

    public SwipeParams swipeParams { get; private set; }
    // private Image image;

    private RectTransform rectTransform;

    void Awake () {
        state = SwipeState.DISABLE;
        deviation = 0;
        rectTransform = this.GetComponent<RectTransform> ();
        pivotPoint = new Vector2 (rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y);
    }

    public void ConstructNewSwipe (SwipeParams swipeParams) {
        this.swipeParams = swipeParams;
        deviation = 0;
        vector = Vector2.zero;
        currentChoise = -1;
        StopAllCoroutines ();
        rectTransform.anchoredPosition = pivotPoint;
        rectTransform.rotation = Quaternion.Euler (0, 0, 0);
        rectTransform.localScale = new Vector3 (1f, 1f, 1f);
    }
    public void StartSwipe () {
        state = SwipeState.IDLE;

    }

    public bool isDisabled () {
        return state == SwipeState.DISABLE;
    }

    void Start () {

    }

    void Update () {

        switch (state) {
            case SwipeState.IDLE:

                break;
            case SwipeState.RETURN:

                rectTransform.anchoredPosition = Vector2.MoveTowards (rectTransform.anchoredPosition, pivotPoint, fMovingSpeed);
                rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (1, 1, 1), 0.1f);

                rectTransform.rotation = Quaternion.Euler (0, 0, (rectTransform.anchoredPosition.x - pivotPoint.x) * fRotation);
                MovingDispatcher ();

                break;
            case SwipeState.DRAG:
                //rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (fScale, fScale, 0), 0.1f);
                break;

            default:
                break;
        }

    }

    private void MovingDispatcher () {
        Vector2 distance = rectTransform.anchoredPosition - pivotPoint;
        float proc = distance.magnitude / swipeDetectionLimit_LR;
        vector = distance.normalized;

        if (rectTransform.anchoredPosition.x > pivotPoint.x) {

            //MovingDispather?.Invoke (proc);
            deviation = proc;

        } else if (rectTransform.anchoredPosition.x < pivotPoint.x) {

            //MovingDispather?.Invoke (-proc);
            deviation = -proc;

        } else {
            //MovingDispather?.Invoke (0f);
            deviation = 0;
        }

    }

    private void TriggerLeft () {

        currentChoise = LEFT;
        OnTrigger?.Invoke (LEFT);

    }

    private void TriggerRight () {

        currentChoise = RIGHT;
        OnTrigger?.Invoke (RIGHT);

    }

    public void OnPointerDown (PointerEventData eventData) {

    }

    public void OnPointerUp (PointerEventData eventData) {

    }

    public void OnBeginDrag (PointerEventData eventData) {

        if (state == SwipeState.DISABLE) {
            return;
        }

        state = SwipeState.DRAG;
    }

    public void OnEndDrag (PointerEventData eventData) {

        if (state == SwipeState.DISABLE) {
            return;
        }

        Vector2 distance = rectTransform.anchoredPosition - pivotPoint;

        if (distance.magnitude >= swipeDetectionLimit_LR) {

            if (rectTransform.anchoredPosition.x < pivotPoint.x) {
                TriggerLeft ();
            } else {
                TriggerRight ();
            }
            state = SwipeState.DISABLE;
            eventData.pointerDrag = null;

            if (isActiveAndEnabled) {
                Coroutine _coroutine = StartCoroutine (OnFideOut ());
                _coroutine = null;
            }

        } else {
            state = SwipeState.RETURN;
        }
    }

    public void OnDrag (PointerEventData eventData) {

        if (state == SwipeState.DISABLE) {
            return;
        }

        state = SwipeState.DRAG;

        //if (rectTransform.anchoredPosition.x < pivotPoint.x && !swipeParams.leftAvailable) {
        //   rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (fScale, fScale, 0), 0.1f);
        //} else 
        rectTransform.anchoredPosition += new Vector2 (eventData.delta.x, 0) / _parent.scaleFactor;
        rectTransform.rotation = Quaternion.Euler (0, 0, (rectTransform.anchoredPosition.x - pivotPoint.x) * fRotation);

        if (!swipeParams.rightAvailable && rectTransform.anchoredPosition.x >= pivotPoint.x) {
            float scale = 0.93f;
            rectTransform.anchoredPosition = pivotPoint;
            rectTransform.rotation = Quaternion.Euler (0, 0, 0);
            rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (scale, scale, 0), 0.02f);

        } else if (!swipeParams.leftAvailable && rectTransform.anchoredPosition.x <= pivotPoint.x) {
            float scale = 0.93f;
            rectTransform.anchoredPosition = pivotPoint;
            rectTransform.rotation = Quaternion.Euler (0, 0, 0);
            rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (scale, scale, 0), 0.02f);

        } else {

        }

        MovingDispatcher ();

    }

    private bool CheckOnCamera () {
        Camera cam = Camera.main;

        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners (objectCorners);

        bool isVisible = false;
        foreach (Vector3 _corner in objectCorners) {
            Vector3 corner = cam.WorldToViewportPoint (_corner);
            if ((corner.x >= 0 && corner.x <= 1 && corner.y >= 0 && corner.y <= 1)) {
                isVisible = true;
                break;
            }
        }
        return isVisible;
    }

    private IEnumerator OnFideOut () {

        Vector2 v = (rectTransform.anchoredPosition - pivotPoint);
        v.Normalize ();
        v *= fMovingSpeed;

        while (CheckOnCamera ()) {

            rectTransform.anchoredPosition += v;
            yield return null;
        }
        rectTransform.anchoredPosition += 5 * v;

    }

}