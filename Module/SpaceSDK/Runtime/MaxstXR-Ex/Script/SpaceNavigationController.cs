using JsonFx.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MaxstXR.Place
{
    public class SpaceNavigationController : MonoBehaviour
    {
        static public void FindPath(MonoBehaviour monoBehaviour, Dictionary<string, string> headers,
            string startLocation, Vector3 startPosition,
            string endLocation, Vector3 endPosition,
            float distance,
            Action<Dictionary<string, List<PathModel>>> success, Action fail,
            string placeUnamePiece = "", bool isOptimize = true)
        {
            string startPositionString = startPosition.x + "," + startPosition.z + "," + startPosition.y;
            string endPositionString = endPosition.x + "," + endPosition.z + "," + endPosition.y;
            var parameters = new Dictionary<string, string>()
        {
            { "start_location", startLocation },
            { "start_position", startPositionString},
            { "end_location", endLocation },
            { "end_position", endPositionString},
            { "placeUnamePiece", placeUnamePiece}
        };

#if DEBUG_DETAIL
        Debug.Log($"FindPath start_location : {startLocation}");
        Debug.Log($"FindPath start_position : {startPositionString}");
        Debug.Log($"FindPath end_location : {endLocation}");
        Debug.Log($"FindPath end_position : {endPositionString}");
        Debug.Log($"FindPath placeUnamePiece : {placeUnamePiece}");
#endif
            monoBehaviour.StartCoroutine(GetNaviData(parameters, distance, success, fail, isOptimize));
        }

        static private IEnumerator GetNaviData(Dictionary<string, string> body, float distance,
            Action<Dictionary<string, List<PathModel>>> success, Action fail, bool isOptimize = true)
        {
            Debug.Log("<color=green> Space Navi FindPath </color>");

            var authorization = XRServiceManager.authorization;
            var contentType = XRServiceManager.contentType;

            var ob = XRService.Instance.PostNaviData(
                authorization,
                contentType,
                body
            );

            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                   .ObserveOn(Scheduler.MainThread)
                   .Timeout(TimeSpan.FromSeconds(5))
                   .Subscribe(resultString =>
                   {
                       if (!string.IsNullOrEmpty(resultString))
                       {
                           try
                           {
                               var paths = JsonReader.Deserialize<PathModel[]>(resultString);
                               var pathDictionary = new Dictionary<string, List<PathModel>>();
                               foreach (PathModel eachPathModel in paths)
                               {
                                   if (!pathDictionary.ContainsKey(eachPathModel.location))
                                   {
                                       pathDictionary[eachPathModel.location] = new List<PathModel>();
                                   }
                                   List<PathModel> pathList = pathDictionary[eachPathModel.location];
                                   if (pathList == null)
                                   {
                                       pathList = new List<PathModel>();
                                   }
                                   pathList.Add(eachPathModel);
                               }

                               var returnPathDictionary = new Dictionary<string, List<PathModel>>();
                               foreach (string eachLocation in pathDictionary.Keys)
                               {
                                   List<PathModel> eachPaths = pathDictionary[eachLocation];
                                   if (isOptimize)
                                   {
                                       var optimization = MakeOptimizePath(eachPaths.ToArray(), distance);
                                       returnPathDictionary[eachLocation] = optimization;
                                   }
                                   else
                                   {
                                       var path = MakePath(eachPaths.ToArray());
                                       returnPathDictionary[eachLocation] = path;
                                   }
                               }
                               success(returnPathDictionary);
                           }
                           catch (Exception e)
                           {
                               Debug.Log($"FindPath Error {e}");
                               fail();
                           }
                       }
                       else
                       {
                           fail();
                       }
                   },
                   error =>
                   {
                       Debug.LogWarning(error);
                   },
                   () =>
                   {

                   });

            yield return ob;
        }

        static private List<PathModel> MakePath(PathModel[] paths)
        {
            List<PathModel> returnPathModel = new List<PathModel>();
            List<Vector3> vectors = new List<Vector3>();
            foreach (PathModel eachModel in paths)
            {
                vectors.Add(new Vector3(eachModel.x, eachModel.y, eachModel.z));
            }

            List<Vector3> convertVectorPath = new List<Vector3>();
            for (int i = 0; i < vectors.Count; i++)
            {
                Vector3 eachPath = vectors[i];
                Vector3 pathPoint = new Vector3(eachPath.x, eachPath.z, eachPath.y);
                convertVectorPath.Add(pathPoint);
            }

            for (int i = 1; i < convertVectorPath.Count - 2; i++)
            {
                Vector3 first = convertVectorPath[i];
                Vector3 second = convertVectorPath[i + 1];

                Vector3 vec = first - second;
                vec.Normalize();
                Quaternion q = Quaternion.LookRotation(vec);

                PathModel pathModel = new PathModel();
                pathModel.position = convertVectorPath[i];
                pathModel.rotation = q;
                returnPathModel.Add(pathModel);
            }

            return returnPathModel;
        }

        static private List<PathModel> MakeOptimizePath(PathModel[] paths, float distance)
        {
            List<PathModel> returnPathModel = new List<PathModel>();
            List<Vector3> vectors = new List<Vector3>();
            foreach (PathModel eachModel in paths)
            {
                vectors.Add(new Vector3(eachModel.x, eachModel.y, eachModel.z));
            }

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> directions = new List<Vector3>();
            CalculateMilestones(vectors, positions, directions, distance, true);

            List<Vector3> convertVectorPath = new List<Vector3>();
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 eachPath = positions[i];
                Vector3 pathPoint = new Vector3(eachPath.x, eachPath.z, eachPath.y);
                convertVectorPath.Add(pathPoint);
            }

            for (int i = 1; i < convertVectorPath.Count - 2; i++)
            {
                Vector3 first = convertVectorPath[i];
                Vector3 second = convertVectorPath[i + 1];

                Vector3 vec = first - second;
                vec.Normalize();
                Quaternion q = Quaternion.LookRotation(vec);

                PathModel pathModel = new PathModel();
                pathModel.position = convertVectorPath[i];
                pathModel.rotation = q;
                returnPathModel.Add(pathModel);
            }

            return returnPathModel;
        }

        static private Vector3 DivideBetweenTwoPoints(in Vector3 from, in Vector3 to, double ratio)
        {
            Vector3 res = new Vector3(0, 0, 0);
            if (ratio < 0.0 || ratio > 1.0)
                return res;

            res = from * (float)(1.0 - ratio) + to * (float)ratio;
            return res;
        }

        static private void InterpolateByBezier(out Vector3 p, out Vector3 d, in Vector3 p0, in Vector3 p1, in Vector3 p2, double t)
        {
            double one_minus_t = 1.0 - t;
            p = (float)(one_minus_t * one_minus_t) * p0 + (float)(2.0 * t * one_minus_t) * p1 + (float)(t * t) * p2;
            d = (float)(-2.0 * one_minus_t) * p0 + (float)(2.0 * one_minus_t - 2.0 * t) * p1 + (float)(2.0 * t) * p2;
            d.Normalize();
        }

        static private void CalculateMilestones(List<Vector3> path, in List<Vector3> pos, in List<Vector3> dir, double interval, bool useBezier)
        {
            double totalDist = 0.0;
            List<double> dists = new List<double>();
            dists.Add(0.0);
            int finalIndex = path.Count - 1;
            for (int i = 0; i < finalIndex; i++)
            {
                Vector3 cur = path[i];
                Vector3 next = path[i + 1];
                Vector3 diff = next - cur;
                totalDist += diff.magnitude;
                dists.Add(totalDist);
            }

            pos.Clear();
            dir.Clear();

            for (double d = 0.0; d <= totalDist; d += interval)
            {
                int next = 1;
                while (next < dists.Count && d > dists[next])
                    next++;

                int cur = next - 1;
                Vector3 nextPoi = path[next];
                Vector3 curPoi = path[cur];

                double len = d - dists[cur];
                double overallLen = dists[next] - dists[cur];
                double ratio = len / overallLen;

                Vector3 po = DivideBetweenTwoPoints(curPoi, nextPoi, ratio);
                Vector3 di = nextPoi - curPoi;
                di.Normalize();

                pos.Add(po);
                dir.Add(di);
            }

            if (useBezier)
            {
                List<Vector3> newPos = pos;
                List<Vector3> newDir = dir;
                for (int i = 1; i < pos.Count - 1; i++)
                {
                    int prev = i - 1;
                    int cur = i;
                    int next = i + 1;

                    Vector3 outNewPos = Vector3.zero;
                    Vector3 outNewDir = Vector3.zero;
                    InterpolateByBezier(out outNewPos, out outNewDir, pos[prev], pos[cur], pos[next], 0.5);
                    newPos[i] = outNewPos;
                    newDir[i] = outNewDir;
                }
            }
        }
    }
}
