using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Managers;
using UnityEngine;

public class DeficiencyComponent : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField]
    private GameObject deficiency;

    [SerializeField]
    private int id;

    [SerializeField]
    private bool enemy = false;

    //private int id;
    void Start () {

        ResourceStateController resourceStateController = GetComponent<ResourceStateController> ();

        if (resourceStateController != null) {
            id = resourceStateController.resourceId;
            enemy = resourceStateController.enemy;
        }

        deficiency.SetActive(false);

        CardController.OnUnavailableCondition += show;
    }

    private void show (ConditionData c) {
        if (c.category != GameDataManager.RESOURCE_ID || c.enemy != enemy || c.id != id)
            return;

        deficiency.SetActive(true);
        
        deficiency.GetComponent<Animator>().SetTrigger("fadein");
    }

    // Update is called once per frame
    void Update () {

    }
}