using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    public class CardIconController : MonoBehaviour, IUpdateData<CardItem> {

        public CardItem data;

        public Sprite sme;
        public Sprite senemy;

        private Animator animator;

        public async UniTask FadeOut () {
            if (gameObject.activeInHierarchy) {
                animator.SetTrigger ("fadeout");
                while (animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
                    await UniTask.Yield ();
                }
            }
        }

        public void UpdateData (CardItem data) {
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
                        icon.sprite = await Services.assets.GetSprite ("Cards/" + data.data.id + "/icon/icon", true);

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

            animator = transform.Find ("Container").gameObject.GetComponent<Animator> ();

        }

        void Update () {

        }

    }
}