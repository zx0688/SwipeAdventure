using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameTime {
    private static int diff;
    public static void Init (int synchTime) {

        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        
        diff = synchTime - cur_time;
    }

    public static int GetTime () {
        return DateTime.Now.Second + diff;
    }

    public static bool isExpired (int time) {
        return (DateTime.Now.Second + diff) > time;
    }

    public static int Left (int time) {
        int l = time - (DateTime.Now.Second + diff);
        return l >= 0 ? l : 0;
    }
}