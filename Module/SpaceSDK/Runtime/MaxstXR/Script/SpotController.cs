using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SpotController : MonoBehaviour
{
    private List<SpotController> neighbors = new ();

    private void Start()
    {
        neighbors = transform.parent.GetComponentsInChildren<SpotController>()?.ToList() ?? new ();   
    }

    public void ProcessSelect(SpotController controller)
    {
        neighbors.ForEach(sc => sc.gameObject.SetActive(sc == controller));
    }
}

