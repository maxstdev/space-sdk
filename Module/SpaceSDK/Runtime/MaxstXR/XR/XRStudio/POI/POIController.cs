using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using maxstAR;
using JsonFx.Json;
using System;

public class POIController : MonoBehaviour
{
    public static void GetPOI(MonoBehaviour monoBehaviour, Dictionary<string, string> headers, string applicationKey, int placeId, Action<POIData[]> success, Action fail)
    {
		monoBehaviour.StartCoroutine(APIController.GET(XRAPI.apiURL + "/v1/api/poi/place/" + placeId, headers, 10, (resultString) =>
		{
			//Debug.Log(resultString);
            if (resultString != "")
            {
                POIData[] pois = JsonReader.Deserialize<POIData[]>(resultString);
                success(pois);
                XRAPI.Instance.SendLog(XRAPI.Operation.POI, true);
            }
            else
            {
                fail();
                XRAPI.Instance.SendLog(XRAPI.Operation.POI, false);
            }
        }, error =>
        {
            fail();
            XRAPI.Instance.SendLog(XRAPI.Operation.POI, false);
        }));
    }
}
