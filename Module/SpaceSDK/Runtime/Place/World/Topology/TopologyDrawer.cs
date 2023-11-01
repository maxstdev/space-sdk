using Cysharp.Threading.Tasks;
using MaxstXR.Place;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaxstXR.Place
{
    public class TopologyDrawer : MonoBehaviour
    {
        public GameObject parent;
        public GameObject lineObj;
        public async UniTask DrawTopologyLine(List<LineData> lines)
        {
            await UniTask.Yield();
            foreach (LineData line in lines)
            {
                GameObject lineObj = CreateLineObject(line, parent);
                AdjustObjectPosition(lineObj);
            }
        }
        private GameObject CreateLineObject(LineData line, GameObject parent)
        {
            GameObject obj = Instantiate(lineObj, parent.transform);
            var lineRenderer = obj.GetComponent<LineRenderer>();

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, line.FromPostion.ToVector3());
            lineRenderer.SetPosition(1, line.ToPostion.ToVector3());
            return obj;
        }

        private void AdjustObjectPosition(GameObject obj)
        {
            LineRenderer lineRenderer = obj.GetComponent<LineRenderer>();

            AdjustPositionFromRayHit(lineRenderer, 0);
            AdjustPositionFromRayHit(lineRenderer, 1);
        }

        private void AdjustPositionFromRayHit(LineRenderer lineRenderer, int positionIndex)
        {
            Vector3 worldPoint = GetWorldPositionFromLineRenderer(lineRenderer, positionIndex);
            Ray ray = new Ray(worldPoint, Vector3.down);
            float yOffset = 0.2f;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 adjustedPoint = new Vector3(-hit.point.x, hit.point.z, hit.point.y + yOffset);
                lineRenderer.SetPosition(positionIndex, adjustedPoint);
            }
        }

        Vector3 GetWorldPositionFromLineRenderer(LineRenderer lineRenderer, int positionIndex)
        {
            return lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(positionIndex));
        }
    }
}
