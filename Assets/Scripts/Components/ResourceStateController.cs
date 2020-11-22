using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Controllers {
    public class ResourceStateController : MonoBehaviour {
        [SerializeField]
        public int resourceId;

        [SerializeField]
        public bool enemy;

        // [SerializeField]
        //private Sprite customSprite;

        protected PlayerManager player;

        protected GameObject icon;
        protected GameObject image;

        private Text valueText;
        private Slider slider;
        private SliderQuant sliderQuant;

        void Awake () {

            if (Services.isInited)
                Init ();
            else
                Services.OnInited += Init;
        }

        public void ChangeID (int id) {
            resourceId = id;
            InitHUD ().Forget ();
            OnUpdateCountP ();
        }

        void Start () {

        }

        private void Init () {
            Services.OnInited -= Init;

            if (!isAvailable ())
                return;

            player = enemy == true ? Services.enemy : Services.player;
            player.OnProfileUpdated += OnUpdateCountP;
            player.OnResourceUpdated += OnUpdateCount;

            slider = gameObject.GetComponent<Slider> ();
            sliderQuant = gameObject.GetComponent<SliderQuant> ();

            valueText = transform.Find ("Value")?.gameObject.GetComponent<Text> ();
            icon = transform.Find ("Icon")?.gameObject;
            image = transform.Find ("Image")?.gameObject;

            InitHUD ().Forget ();
            OnUpdateCountP ();
        }

        public void OnUpdateCountP () {
            OnUpdateCount (resourceId, 0);
        }

        async UniTaskVoid InitHUD () {
            ResourceData resinfo = Services.data.ResInfo (resourceId);
            Text t = transform.Find ("Name")?.gameObject.GetComponent<Text> ();
            if (t != null) {
                t.text = resinfo.name;
            }

            Text d = transform.Find ("Description")?.gameObject.GetComponent<Text> ();
            if (d != null) {
                d.text = resinfo.name;
            }

            if (icon != null) {
                Image ic = icon.GetComponent<Image> ();
                ic.sprite = await Services.assets.GetSprite ("Resources/" + resourceId + "/icon", true);
            }

            if (image != null) {
                Image im = image.GetComponent<Image> ();
                im.sprite = await Services.assets.GetSprite ("Resources/" + resourceId + "/icon", true);
            }
        }

        void OnDisable () {
            if (Services.isInited && isAvailable ()) {
                player.OnProfileUpdated -= OnUpdateCountP;
                player.OnResourceUpdated -= OnUpdateCount;
            }
        }

        void OnDestroy () {
            if (Services.isInited && isAvailable ()) {
                player.OnProfileUpdated -= OnUpdateCountP;
                player.OnResourceUpdated -= OnUpdateCount;
            }
        }
        void OnEnable () {

            if (Services.isInited && isAvailable ()) {
                player.OnProfileUpdated += OnUpdateCountP;
                player.OnResourceUpdated += OnUpdateCount;
            }

        }

        public virtual bool isAvailable () {
            return resourceId > 0;
        }

        public virtual void OnUpdateCount (int id, int count) {

            if (!isAvailable () || resourceId != id)
                return;

            int _value = player.AvailableResource (resourceId);
            int maxValue = player.MaxResourceValue (resourceId);

            if (sliderQuant != null) {

                sliderQuant.maxValue = maxValue;
                sliderQuant.SetValue (_value);

            } else if (slider != null) {

                slider.minValue = 0;
                slider.maxValue = maxValue;
                slider.value = _value;
            }

            if (valueText != null)
                valueText.text = _value.ToString ();
        }

        // Update is called once per frame
        void Update () {

        }
    }
}