using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Controllers;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum SwipeDirection {
    LEFT,
    RIGHT
}
public enum SwipeState {
    DISABLE,
    RETURN,
    DRAG,
    IDLE
}

public class Swipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {

    public static readonly int LEFT_CHOICE = 0;
    public static readonly int RIGHT_CHOICE = 1;
    [System.Serializable] public class mEvent : UnityEvent { }

    [SerializeField] public Canvas _parent;
    [SerializeField] private float fMovingSpeed = 15;
    //[Range (0f, 1000f)] 
    public float swipeDetectionLimit_LR = 300f;
    [SerializeField] private float fRotation = 0;
    [SerializeField] private float fScale = 1f;

    [SerializeField] private Vector2 pivotPoint;

    public int currentChoise;

    [HideInInspector]
    public static event Action<float> OnChangeDeviation;
    [HideInInspector]
    public static event Action<SwipeDirection> OnChangeDirection;
    [HideInInspector]
    public static event Action OnStartSwipe;
    [HideInInspector]
    public static event Action OnFirstSwipeCard;
    [HideInInspector]
    public static event Action OnEndSwipe;
    [HideInInspector]
    public static event Action OnDrop;

    public float deviation;
    private int direction;
    public Vector2 vector;

    //private DragDropChoise choise;
    [SerializeField]
    public GameObject gameScripts;

    private CardController cardController;

    public static SwipeState state { get; private set; }

    private RectTransform rectTransform;
    private bool firstTakeCard;

    void Awake () {
        currentChoise = -1;
        state = SwipeState.DISABLE;
        deviation = 0;
        rectTransform = this.GetComponent<RectTransform> ();
        pivotPoint = new Vector2 (rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y);
        cardController = this.GetComponent<CardController> ();
    }

    public void ConstructNewSwipe () {
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
        // GetComponent<Animator> ().enabled = false;
        firstTakeCard = false;

        OnStartSwipe?.Invoke ();
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

            deviation = proc;
            OnChangeDeviation?.Invoke (deviation);

        } else if (rectTransform.anchoredPosition.x < pivotPoint.x) {

            deviation = -proc;
            OnChangeDeviation?.Invoke (deviation);

        } else {

            OnChangeDeviation?.Invoke (0f);
            deviation = 0f;

        }

    }

    private void TriggerLeft () {

        currentChoise = LEFT_CHOICE;
    }

    private void TriggerRight () {

        currentChoise = RIGHT_CHOICE;

    }

    public void OnPointerDown (PointerEventData eventData) {

    }

    public void OnPointerUp (PointerEventData eventData) {

    }

    public void OnBeginDrag (PointerEventData eventData) {

        if (state == SwipeState.DISABLE) {
            return;
        }

        direction = 0;

        if (firstTakeCard == false) {
            firstTakeCard = true;
            OnFirstSwipeCard?.Invoke ();
        }

        state = SwipeState.DRAG;
    }

    public void OnEndDrag (PointerEventData eventData) {

        if (state == SwipeState.DISABLE) {
            return;
        }

        if (cardController != null) {
            cardController.OnEndDrag ();
        }

        OnDrop?.Invoke ();

        Vector2 distance = rectTransform.anchoredPosition - pivotPoint;

        if (distance.magnitude >= swipeDetectionLimit_LR) {

            if (rectTransform.anchoredPosition.x < pivotPoint.x) {
                TriggerLeft ();
            } else {
                TriggerRight ();
            }
            state = SwipeState.DISABLE;
            eventData.pointerDrag = null;
            OnEndSwipe?.Invoke ();

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

        rectTransform.anchoredPosition += new Vector2 (eventData.delta.x, 0) / _parent.scaleFactor;
        rectTransform.rotation = Quaternion.Euler (0, 0, (rectTransform.anchoredPosition.x - pivotPoint.x) * fRotation);

        if (rectTransform.anchoredPosition.x > pivotPoint.x) {
            if (direction != 1) {
                OnChangeDirection?.Invoke (SwipeDirection.RIGHT);
            }
            direction = 1;
        } else if (rectTransform.anchoredPosition.x < pivotPoint.x) {
            if (direction != -1) {
                OnChangeDirection?.Invoke (SwipeDirection.LEFT);
            }
            direction = -1;
        }

        if (cardController != null) {

            cardController.OnDrag (direction);

            if (!cardController.rightAvailable && rectTransform.anchoredPosition.x > pivotPoint.x) {
                float scale = 0.93f;
                rectTransform.anchoredPosition = pivotPoint;

                rectTransform.rotation = Quaternion.Euler (0, 0, 0);
                rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (scale, scale, scale), 0.02f);

            } else if (!cardController.leftAvailable && rectTransform.anchoredPosition.x < pivotPoint.x) {
                float scale = 0.93f;
                rectTransform.anchoredPosition = pivotPoint;
                rectTransform.rotation = Quaternion.Euler (0, 0, 0);
                rectTransform.localScale = Vector3.MoveTowards (rectTransform.localScale, new Vector3 (scale, scale, scale), 0.02f);

            } else {

            }
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
        v *= 15f; //fMovingSpeed;

        while (CheckOnCamera ()) {

            rectTransform.anchoredPosition += v;
            yield return null;
        }
        rectTransform.anchoredPosition += 6 * v;
        gameObject.SetActive (false);
    }

}