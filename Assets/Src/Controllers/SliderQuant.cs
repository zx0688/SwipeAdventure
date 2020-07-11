using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderQuant : MonoBehaviour {
    [SerializeField]
    public int defaultValue = 0;

    [SerializeField]
    public Sprite disable;
    [SerializeField]
    public Sprite enable;
    [SerializeField]
    public List<GameObject> items;

    private int currentValue;

    // Start is called before the first frame update
    void Start () {
        for (int i = 0; i < maxValue; i++) {
            items[i].SetActive (true);
        }

        SetValue (defaultValue);
    }

    private void UpdateValue () {
        for (int i = 0; i < maxValue; i++) {
            Image image = items[i].GetComponent<Image> ();
            if (i < currentValue) {
                image.sprite = enable;
            } else {
                image.sprite = disable;
            }
        }
    }

    public int maxValue {
        get { return items.Count; }
    }

    public int GetValue () {
        return currentValue;
    }

    public void SetValue (int value) {
        currentValue = value;
        UpdateValue ();
    }
    //private int value;

    // Update is called once per frame
    void Update () {

    }
}