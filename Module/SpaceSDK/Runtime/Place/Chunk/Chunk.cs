using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public enum Status
    {
        RENDER_STATUS = 0,
        IN_BOUND_STATUS = 1,
        OUT_BOUND_STATUS = 2,
    }

    public interface ChunkDelegate
    {
        void UpdateGroupCondition(Chunk chunk, IPoint point);
    }

    public class ChunkKey
    {
        public int x;
        public int y;

        public ChunkKey(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return String.Format("x : {0}, y : {1}", x, y);
        }

        public override bool Equals(object obj)
        {
            var interfaceType = typeof(ChunkKey);
            return obj != null && interfaceType.IsInstanceOfType(obj) &&
                   x == ((ChunkKey)obj).x && y == ((ChunkKey)obj).y;
        }

        public override int GetHashCode()
        {
            return (x + 37).GetHashCode() + y.GetHashCode();
        }

        public static ChunkKey operator +(ChunkKey a, ChunkKey b)
            => new ChunkKey(a.x + b.x, a.y + b.y);

        public static ChunkKey operator -(ChunkKey a, ChunkKey b)
            => new ChunkKey(a.x - b.x, a.y - b.y);

        public static bool operator ==(ChunkKey a, ChunkKey b) => a.x == b.x && a.y == b.y;

        public static bool operator !=(ChunkKey a, ChunkKey b) => a.x != b.x || a.y != b.y;
    }

    public class Chunk
    {
        private ChunkKey key = new ChunkKey(0, 0);
        private readonly List<IPoint> points = new();
        private readonly List<IPoint> filterPoints = new();
        private Status chuchkStatus = Status.OUT_BOUND_STATUS;
        private ChunkDelegate chunkDelegate = null;

        public ref ChunkKey Key { get { return ref key; } }
        public long UpdateTimestamp { get; set; } = 0L;
        public int Count => points.Count;
        public bool IsPossibleRender { get; set; } = true;

        public static void MakeKey(ref Vector3 v, ChunkEnv env, out ChunkKey key)
        {
            key = new ChunkKey(0, 0);
            key.x = (int)(v.x / env.UnitDistance());
            key.y = (int)(v.z / env.UnitDistance());
        }

        public Chunk(ChunkDelegate chunkDelegate)
        {
            this.chunkDelegate = chunkDelegate;
        }

        public void SetPovVisible(bool isVisible)
        {
            UpdateStatus(isVisible ? Status.RENDER_STATUS : Status.OUT_BOUND_STATUS);
        }

        public void SetKey(ChunkKey v)
        {
            key = v;
        }

        public bool UpdateStatus(Status status)
        {
            if (chuchkStatus == status) return false;

            chuchkStatus = status;

            switch (status)
            {
                case Status.RENDER_STATUS:
                    foreach (var p in points)
                    {
                        p.OnInBounds();
                        p.OnEnable();
                    }
                    break;
                case Status.IN_BOUND_STATUS:
                    foreach (var p in points)
                    {
                        p.OnInBounds();
                    }
                    break;
                case Status.OUT_BOUND_STATUS:
                    foreach (var p in points)
                    {
                        p.OnOutBounds();
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public void GenerateFilterPoints(ChunkElementFilter filter, out List<IPoint> deactivePoints)
        {
            filterPoints.Clear();
            deactivePoints = new List<IPoint>();
            foreach (var point in points)
            {
                if (!AddToFilterPoint(point, filter))
                {
                    deactivePoints.Add(point);
                }
            }
        }

        public int DispatchRender(ChunkEnv env, bool ret, ChunkElementFilter filter = null)
        {
            if (filter == null)
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    points[i].OnRender(env, ret);
                }
                return points.Count;
            }
            else
            {
                for (int i = 0; i < filterPoints.Count; ++i)
                {
                    filterPoints[i].OnRender(env, ret);
                }
                return filterPoints.Count;
            }
        }

        public void DispatchReset()
        {
            foreach (var p in points)
            {
                p.OnOutBounds();
            }
        }

        public void DispatchHide(PointType type)
        {
            foreach (var p in points)
            {
                if (p.GetPointType() == type)
                {
                    p.OnInBounds();
                }
            }
        }

        public void DispatchOnAdded(IPoint point)
        {
            switch (chuchkStatus)
            {
                case Status.RENDER_STATUS:
                    point.OnInBounds();
                    point.OnEnable();
                    break;
                case Status.IN_BOUND_STATUS:
                    point.OnInBounds();
                    break;
                case Status.OUT_BOUND_STATUS:
                    point.OnOutBounds();
                    break;
                default:
                    break;
            }
        }

        public void Add(IPoint point, ChunkElementFilter filter)
        {
            point.SetRelationship(new WeakReference<Chunk>(this), chunkDelegate);
            points.Add(point);
            AddToFilterPoint(point, filter);
        }

        public void RemoveAll(PointType type, ChunkElementFilter filter)
        {
            points.RemoveAll(p =>
            {
                if (p.GetPointType() == type)
                {
                    p.SetRelationship(null, null);
                    p.OnDispose();
                    return true;
                }
                else
                {
                    return false;
                }
            });

            GenerateFilterPoints(filter, out _);
        }

        private bool AddToFilterPoint(IPoint point, ChunkElementFilter filter)
        {
            if (filter.UsedFilter)
            {
                if (point.IsIgnoreGroup()
                    || filter.CheckVaild(point.GetGroups()))
                {
                    filterPoints.Add(point);
                    return true;
                }
            }
            return false;
        }

        public void UpdateFilterPoint(IPoint point, ChunkElementFilter filter, out bool isRemove)
        {
            if (filter.UsedFilter)
            {
                if (point.IsIgnoreGroup()
                    || filter.CheckVaild(point.GetGroups()))
                {
                    if (!filterPoints.Contains(point))
                    {
                        filterPoints.Add(point);
                    }
                }
                else
                {
                    filterPoints.Remove(point);
                    isRemove = true;
                    return;
                }
            }
            isRemove = false;
        }
    }
}
