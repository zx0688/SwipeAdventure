using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePopup : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {

    }

    private void OnEnable () {
        if (Services.isInited == true) {
            Text tf = transform.Find ("Header").gameObject.GetComponent<Text> ();
            //tf.text = Service.data.isWin (0) ? "you win!" : "you lose!";
        }
    }

    void OnShow () {

    }

    // Update is called once per frame
    void Update () {

    }
}