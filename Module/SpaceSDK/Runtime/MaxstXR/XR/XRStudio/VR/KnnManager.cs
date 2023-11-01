using KNN;
using KNN.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public partial class KnnManager : MonoBehaviour
{
    public GameObject FindNearest(Vector3 position)
    {
        return FindNearest(transform, position);
    }

    public GameObject FindNearest(Transform scope, Vector3 position)
    {
        return FindNearest(scope, new float3(position));
    }

    public GameObject FindNearest(Transform scope, float3 queryPosition)
    {
        var knnPoints = scope.GetComponentsInChildren<KnnPoint>(true);
        return FindNearest(knnPoints, queryPosition);
    }

    public GameObject FindNearest(KnnPoint[] knnPoints, float3 queryPosition)
    {
        return FindNearestK(knnPoints, queryPosition, 1).First();
    }

    public GameObject[] FindNearestK(Vector3 position, int k)
    {
        return FindNearestK(transform, position, k);
    }

    public GameObject[] FindNearestK(Transform scope, Vector3 position, int k)
    {
        return FindNearestK(scope, new float3(position), k);
    }

    public GameObject[] FindNearestK(Transform scope, float3 queryPosition, int k)
    {
        var knnPoints = scope.GetComponentsInChildren<KnnPoint>();
        return FindNearestK(knnPoints, queryPosition, k);
    }

    public GameObject[] FindNearestK(KnnPoint[] knnPoints, float3 queryPosition, int k)
    {
        // Create a native array for output indices
        var result = new NativeArray<int>(k, Allocator.TempJob);

        // Create a native array for input points
        var points = new NativeArray<float3>(knnPoints.Length, Allocator.TempJob);
        for (var i = 0; i < points.Length; ++i)
            points[i] = knnPoints[i].Point;

        // Create a container, i.e., a K-d tree
        var container = new KnnContainer(points, false, Allocator.TempJob);
        new KnnRebuildJob(container).Schedule().Complete();

        new QueryKNearestJob(container, queryPosition, result).Schedule().Complete();

        container.Dispose();
        points.Dispose();

        var gameObjects = knnPoints.Where((p, i) => result.Contains(i))
            .Select(p => p.gameObject).ToArray();

        // Cleanup
        result.Dispose();

        return gameObjects;
    }

    public GameObject[] FindWithinRange(Vector3 position, float r)
    {
        return FindWithinRange(transform, position, r);
    }

    public GameObject[] FindWithinRange(Transform scope, Vector3 position, float r)
    {
        return FindWithinRange(scope, new float3(position), r);
    }

    public GameObject[] FindWithinRange(Transform scope, float3 queryPosition, float r)
    {
        var knnPoints = scope.GetComponentsInChildren<KnnPoint>();
        return FindWithinRange(knnPoints, queryPosition, r);
    }

    public GameObject[] FindWithinRange(KnnPoint[] knnPoints, float3 queryPosition, float r)
    {
        // Create a native array for input points
        var points = new NativeArray<float3>(knnPoints.Length, Allocator.TempJob);
        for (var i = 0; i < points.Length; ++i)
            points[i] = knnPoints[i].Point;

        // Create a container, i.e., a K-d tree
        var container = new KnnContainer(points, false, Allocator.TempJob);
        new KnnRebuildJob(container).Schedule().Complete();

        // Create a native list for output indices
        var result = new NativeList<int>(Allocator.TempJob);

        new QueryRangeJob(container, queryPosition, r, result).Schedule().Complete();

        // Cleanup
        container.Dispose();
        points.Dispose();

        var gameObjects = knnPoints.Where((p, i) => result.Contains(i))
            .Select(p => p.gameObject).ToArray();

        // Cleanup
        result.Dispose();

        return gameObjects;
    }

    public IList<GameObject[]> FindWithinRangeBatch(IEnumerable<Vector3> positions, float r)
    {
        return FindWithinRangeBatch(transform, positions.Select(p => new float3(p)), r);
    }

    public IList<GameObject[]> FindWithinRangeBatch(IEnumerable<float3> positions, float r, int maxK = 5)
    {
        return FindWithinRangeBatch(transform, positions, r, maxK);
    }

    public IList<GameObject[]> FindWithinRangeBatch(Transform scope, IEnumerable<float3> positions, float r, int maxK = 5)
    {
        var queryPositions = new NativeArray<float3>(positions.ToArray(), Allocator.TempJob);

        // Create a native array for input points
        var knnPoints = scope.GetComponentsInChildren<KnnPoint>();
        var l = knnPoints.Length;
        var points = new NativeArray<float3>(l, Allocator.TempJob);
        for (var i = 0; i < points.Length; ++i)
            points[i] = knnPoints[i].Point;

        // Create a container, i.e., a K-d tree
        var container = new KnnContainer(points, false, Allocator.TempJob);
        new KnnRebuildJob(container).Schedule().Complete();

        // Create a native array for output results
        var results = new NativeArray<RangeQueryResult>(queryPositions.Length, Allocator.TempJob);
        for (var i = 0; i < results.Length; ++i)
            results[i] = new RangeQueryResult(maxK, Allocator.TempJob);

        new QueryRangeBatchJob(container, queryPositions, r, results)
            .ScheduleBatch(queryPositions.Length, queryPositions.Length / 32)
            .Complete();

        HashSet<int> selector(RangeQueryResult result)
        {
            return new HashSet<int>(Enumerable.Range(0, result.Length).Select(i => result[i]));
        }

        var gameObjectsList = results.Select(selector)
            .Select(result => knnPoints.Where((p, i) => result.Contains(i))
            .Select(p => p.gameObject).ToArray()).ToList();

        // Cleanup
        foreach (var result in results) { result.Dispose(); }
        results.Dispose();
        container.Dispose();
        points.Dispose();
        queryPositions.Dispose();

        return gameObjectsList;
    }
}