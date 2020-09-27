using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers {
    public class EnemyManager : PlayerManager {

        public State state;

        public enum State {
            IDLE,
            WAIT_MY_TURN,
            TURN
        }
        void Start () {

        }

        // Update is called once per frame
        void Update () {

        }

        public IEnumerator Turning () {
            state = State.TURN;
            yield return null; //

            currentChoise = 1;

            state = State.WAIT_MY_TURN;
        }

    }
}