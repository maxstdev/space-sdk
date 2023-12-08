using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using maxstAR;
using JsonFx.Json;
using System;

public class POIController : MonoBehaviour
{
    static string serverURL = XRAPI.spaceUrl;
    public static void  GetPOI(MonoBehaviour monoBehaviour, string accessToken, string spaceId, Action<POIData[]> success, Action fail)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            { "Authorization", "Bearer " + accessToken},
            { "Content-Type", "application/json"}
        };

        monoBehaviour.StartCoroutine(APIController.GET(serverURL + spaceId + "/pois", headers, 10, 
            (resultString) =>
            {
                //Debug.Log(resultString);
                if (resultString != "")
                {
                    POIData[] pois = JsonReader.Deserialize<POIData[]>(resultString);
                    success(pois);
                }
                else
                {
                    fail();
                }
            },
            (error) =>
            {
                fail();
            }));
    }
}
