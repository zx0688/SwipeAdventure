﻿using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

public class UI_AchivementTooltip : UIInventoryTooltip, ITick
{

    private Image image;
    private Text description;
    private Text _name;

    private ItemMeta _item;

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    public void ShowTooltip(ItemMeta item)
    {

        return;
        gameObject.SetActive(true);
        _item = item;

        _name.text = item.Name;
        description.text = item.Desc;

    }


    void Awake()
    {
        image = transform.Find("Image").GetComponent<Image>();

        _name = transform.Find("Name").GetComponent<Text>();
        description = transform.Find("Description").GetComponent<Text>();

        /// gameObject.GetComponent<Button> ().onClick.AddListener (HideTooltip);
    }

    /*void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HideTooltip();
        }
    }*/

    public void Tick(int timestamp)
    {

    }

    public bool IsTickble()
    {
        return false;
    }
}