﻿using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Managers;
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

    [SerializeField]
    public GameObject gameScripts;

    private int currentValue;

    private bool showAction;

    private CardItem cardItem;

    // Start is called before the first frame update
    void Start () {

        showAction = false;
        currentValue = maxValue;

        for (int i = 0; i < maxValue; i++) {
            items[i].SetActive (true);
        }

        Swipe.OnStartSwipe += OnStartShake;
        Swipe.OnEndSwipe += OnStopShake;
        Swipe.OnDrop += OnStopShake;
        Swipe.OnChangeDirection += OnChangeDirection;

        SetValue (defaultValue);
    }

    private void OnChangeDirection (SwipeDirection direction) {

        //if (showAction == false)
        //    return;

        bool enemy = GetComponent<ResourceStateController> ().enemy;
        int resId = GetComponent<ResourceStateController> ().resourceId;

        ChoiceData choiseData = null;
        if (cardItem.me == true)
            choiseData = direction == SwipeDirection.LEFT ? cardItem.data.left : cardItem.data.right;
        else
            choiseData = direction == SwipeDirection.LEFT ? cardItem.data.eLeft : cardItem.data.eRight;
            
        List<RewardData> result = new List<RewardData>();
        Services.data.GetResourceReward(result, choiseData.reward, choiseData.cost, 0);
        
        RewardData rc = result.Find (r => r.count < 0 && r.id == resId && r.enemy == enemy && r.category == GameDataManager.RESOURCE_ID);
        RewardData rr = result.Find (r => r.count > 0 && r.id == resId && r.enemy == enemy && r.category == GameDataManager.RESOURCE_ID);

        for (int i = 0; i < maxValue; i++) {
            items[i].GetComponent<ShakeComponent> ().shake = false;
            if (rc != null && i >= (currentValue + rc.count) && i < currentValue) {
                items[i].GetComponent<ShakeComponent> ().shake = true;
            } else if (rr != null && i < currentValue + rr.count && i >= currentValue) {
                items[i].GetComponent<ShakeComponent> ().shake = true;
            }
        }
    }

    private void OnStopShake () {
        // showAction = false;

        for (int i = 0; i < maxValue; i++) {
            items[i].GetComponent<ShakeComponent> ().shake = false;
        }
    }

    private void OnStartShake () {
        //showAction = true;
        cardItem = gameScripts.GetComponent<GameLoop> ().cardItem;
    }

    private void UpdateValue () {
        for (int i = 0; i < maxValue; i++) {
            Image image = items[i].GetComponent<Image> ();
            if (i < currentValue) {
                image.sprite = enable;
                items[i].GetComponent<Animator> ().SetBool ("on", true);
            } else {
                image.sprite = disable;
                items[i].GetComponent<Animator> ().SetBool ("on", false);
            }
            items[i].GetComponent<ShakeComponent> ().shake = false;
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