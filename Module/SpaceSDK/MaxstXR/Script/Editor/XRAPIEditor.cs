using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using maxstAR;

[CustomEditor(typeof(XRAPI))]
public class XRAPIEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        XRAPI xrAPI = (XRAPI)target;

        if(xrAPI.spaceId == "")
        {
            if(xrAPI.GetComponentInChildren<PovManager>() != null)
            {
                GameObject trackable = xrAPI.GetComponentInChildren<PovManager>().Trackable;
                VPSTrackable vPSTrackable = trackable.GetComponent<VPSTrackable>();
                xrAPI.spaceId = vPSTrackable.spaceId;
            }
        }

        if (GUI.changed)
            EditorUtility.SetDirty(xrAPI);
    }
}
