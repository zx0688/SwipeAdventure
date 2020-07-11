using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    public class CardIconController : MonoBehaviour, IUpdateData<QueueItem> {

        public QueueItem data;

        public Sprite sme;
        public Sprite senemy;

        public void UpdateData (QueueItem data) {
            this.data = data;

            UpdateHUD ();
        }

        async void UpdateHUD () {

            foreach (Transform g in transform.GetComponentsInChildren<Transform> ()) {

                switch (g.name) {
                    case "Back":
                        Image back = g.GetComponent<Image> ();

                        back.sprite = data.me == true ? sme : senemy;

                        break;
                    case "Icon":
                        Image icon = g.GetComponent<Image> ();

                        string iconType = data.me == true ? "me" : "enemy";
                        icon.sprite = await Services.assets.GetSprite ("Cards/" + data.card.id + "/icon/icon", true);

                        // Color col;
                        //ColorUtility.TryParseHtmlString(data.me ? "#00E11E" : "#E10007", out col);
                        //icon.color = col;

                        break;
                    default:
                        break;
                }
            }
        }

        void Start () {

        }

        void Update () {

        }

    }
}