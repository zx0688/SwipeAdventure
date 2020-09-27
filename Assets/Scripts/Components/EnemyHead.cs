﻿using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class EnemyHead : MonoBehaviour {

    [SerializeField]
    private int resourceId;

    [SerializeField]
    private string headId;

    [SerializeField]
    private SpriteAtlas atlas;
    private Image emotion;
    private GameObject damage;

    [SerializeField]
    private GameObject gameScripts;
    // Start is called before the first frame update
    void Awake () {

        Transform d = transform.Find ("Damage");
        damage = d.gameObject;

        if (Services.isInited)
            Init ();
        else
            Services.OnInited += Init;
    }

    void Start () {
        emotion = transform.Find ("Emotion").GetComponent<Image> ();

    }

    public void UpdateHead (string head) {

        headId = head;

        Transform g = transform.Find ("Back");
        g.GetComponent<Image> ().sprite = atlas.GetSprite (head + "_Head");

        

        damage.GetComponent<Image>().sprite = atlas.GetSprite (head + "_damage0");
        damage.SetActive (false);

        OnSetDefaultFace ();
        OnUpdateHit ();
    }

    private void Init () {

        Services.OnInited -= Init;
        Services.enemy.OnProfileUpdated += OnUpdateHit;
        Services.enemy.OnResourceUpdated += OnDamage;

        Swipe.OnDrop += OnSetDefaultFace;
        Swipe.OnChangeDirection += OnChangeDirection;

        //Swipe.OnChangeDirection += OnChangeDirection;
        //UpdateHead("Orc");
        OnUpdateHit ();
    }

    IEnumerator WaitAndSetDefault () {
        emotion.sprite = atlas.GetSprite (headId + "_emotion3");
        yield return new WaitForSeconds (1);
        OnSetDefaultFace ();
    }

    private void OnDamage (int id, int value) {
        if (id != 1)
            return;

        StopCoroutine (WaitAndSetDefault ());
        Coroutine c = StartCoroutine (WaitAndSetDefault ());
        c = null;
    }

    private void OnChangeDirection (SwipeDirection value) {
        if (Swipe.state != SwipeState.DRAG) {
            OnSetDefaultFace ();
            return;
        }

        emotion.sprite = atlas.GetSprite (headId + "_emotion2");
    }

    private void OnSetDefaultFace () {
        GameLoop gl = gameScripts.GetComponent<GameLoop> ();

        if(gl.cardItem == null)
        {
            emotion.sprite = atlas.GetSprite (headId + "_emotion4");
            return; 
        }
        emotion.sprite = gl?.cardItem.me == false ? atlas.GetSprite (headId + "_emotion4") : atlas.GetSprite (headId + "_emotion1");
    }

    private void OnUpdateHit () {

        int current = Services.enemy.AvailableResource (resourceId);

        if (current == 0) {
            emotion.sprite = atlas.GetSprite (headId + "_emotion5");
            return;
        }

        int max = Services.enemy.MaxResourceValue (resourceId);

        if(max == current)
        {
            damage.SetActive(false);
            return;
        }

        damage.SetActive(true);

        int step = 2 - Mathf.FloorToInt (3f * current / (max));

        damage.GetComponent<Image> ().sprite = atlas.GetSprite (headId + "_damage" + step);
    }

    // Update is called once per frame
    void Update () {

    }
}