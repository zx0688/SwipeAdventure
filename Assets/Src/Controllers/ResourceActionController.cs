using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    public class ResourceActionController : MonoBehaviour, IUpdateData<RewardData> {

        public RewardData data;
        public bool add;

        private GameObject addIcon;
        private GameObject subIcon;

        [SerializeField]
        private GameObject[] icons;
        [SerializeField]
        private GameObject value;

        public void UpdateDataSign (RewardData data, bool add) {
            this.add = add;
            UpdateData (data);
        }
        public void UpdateData (RewardData data) {

            this.data = data;
            this.gameObject.SetActive (false);
            UpdateHUD ().Forget ();
        }

        async UniTask UpdateHUD () {

            if (data.category == 3) {
                addIcon.SetActive (false);
                subIcon.SetActive (false);
            } else {
                if (addIcon != null && add) {
                    addIcon.SetActive (true);
                    subIcon.SetActive (false);
                }

                if (subIcon != null && add == false) {
                    subIcon.SetActive (true);
                    addIcon.SetActive (false);
                }
            }

            Debug.Log (data.count);

            for (int i = 0; i < icons.Length; i++) {

                if (i > data.count - 1) {
                    icons[i].SetActive (false);
                    continue;
                }

                icons[i].SetActive (true);
                Image icon = icons[i].GetComponent<Image> ();
                if (data.category == 3) {
                    ActionData ad = Services.data.ActionInfo (data.id);
                    icon.sprite = await Services.assets.GetSprite ("Actions/" + ad.id + "/icon", true);
                } else {
                    ResourceData res = Services.data.ResInfo (data.id);
                    icon.sprite = await Services.assets.GetSprite ("Resources/" + res.id + "/icon", true);
                }
            }

            if (data.count > 4) {
                value.SetActive (true);
                value.GetComponent<Text> ().text = data.count.ToString (); //add ? data.count.ToString () : "-" + data.count.ToString ();
            } else {
                value.SetActive (false);
            }

            this.gameObject.SetActive (true);
        }

        void Start () {
            add = true;

            addIcon = transform.Find ("Add")?.gameObject;
            subIcon = transform.Find ("Sub")?.gameObject;

        }

        void Update () {

        }

    }
}