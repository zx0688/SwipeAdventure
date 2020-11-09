using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Managers;
using UnityEngine;

public static class Utils {

    /*public static void DeepCopy<T> (T copyFrom, T copyTo) {
        if (copyFrom == null || copyTo == null)
            throw new Exception ("Must not specify null parameters");

        bool copyChildren = true;
        var properties = copyFrom.GetType ().GetProperties ();

        foreach (var p in properties.Where (prop => prop.CanRead && prop.CanWrite)) {
            if (p.PropertyType.IsClass && p.PropertyType != typeof (string)) {
                if (!copyChildren) continue;

                var destinationClass = Activator.CreateInstance (p.PropertyType);
                object copyValue = p.GetValue (copyFrom);

                DeepCopy (copyValue, destinationClass);

                p.SetValue (copyTo, destinationClass);
            } else {
                object copyValue = p.GetValue (copyFrom);
                p.SetValue (copyTo, copyValue);
            }
        }
    }*/
    public static List<T> DeepCopyList<T> (List<T> origin) where T : class, new () {
        List<T> copy = new List<T>();
        origin.ForEach(r => copy.Add(DeepCopyClass(r)));
        return copy;
    }
    public static T DeepCopyClass<T> (T origin) where T : class, new () {
        T copyTo = JsonUtility.FromJson<T> (JsonUtility.ToJson (origin));
        return copyTo;
    }
    /*public static T DeepCopy<T> (T origin) where T : class, new () {
        T copy = JsonUtility.FromJson<T> (JsonUtility.ToJson (origin));
        return copy;
    }*/
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