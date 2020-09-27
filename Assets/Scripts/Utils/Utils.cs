using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Managers;
using UnityEngine;

public static class Utils {

    public static T DeepCopy<T> (T origin) where T : class, new () {
        T copy = JsonUtility.FromJson<T> (JsonUtility.ToJson (origin));
        return copy;
    }
    public static bool Intersection<T> (T[] array1, T[] array2) {
        foreach (T e1 in array1)
            foreach (T e2 in array2)
                if (e1.Equals (e2))
                    return true;
        return false;
    }
    public static bool Contains<T> (T[] array1, T element) {
        return Array.IndexOf (array1, element) != -1;
    }
}