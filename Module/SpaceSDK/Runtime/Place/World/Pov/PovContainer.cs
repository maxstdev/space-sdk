using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public class PovContainer : ChunkContainer<PovEnv>
    {
        [DI(DIScope.component, DIComponent.place)] private PovEvent PovEvent { get; }

        private readonly List<Chunk> chuckList = new();
        protected override void Start()
        {
            base.Start();
        }
        private void OnEnable()
        {
            PovEvent.receivePoints.AddObserver(this, OnReceivePoints);
            PovEvent.removeAllPointType.AddObserver(this, OnRemoveAllPointType);
            PovEvent.povVisible.AddObserver(this, OnPovVisible);
        }

        private void OnDisable()
        {
            PovEvent.receivePoints.RemoveAllObserver(this);
            PovEvent.removeAllPointType.RemoveAllObserver(this);
            PovEvent.povVisible.RemoveAllObserver(this);
        }

        protected override void ClearChunkBox()
        {
            foreach (var cb in chuckList)
            {
                cb.DispatchReset();
            }
            chuckList.Clear();
        }

        protected override IEnumerable<Chunk> GetEnumerableChunk()
        {
            for (int i = 0; i < chuckList.Count; ++i)
            {
                yield return chuckList[i];
            }
        }

        private void OnPovVisible(bool isVisible)
        {
            chuckList.ForEach(chuck => { chuck.SetPovVisible(isVisible); });
        }

        protected override void ProcessChunkStatus(Chunk chunk, bool isConfig = false)
        {
            if (chunk == null) return;
            var diff = chunk.Key - currentKey;
            var status = MeasurePositionAndStatus(diff);
            //Debug.Log($"ProcessChunkStatus : {diff}/{status}/{isConfig}");
            chunk.UpdateStatus(status);

            if (isConfig)
            {
                if (status < Status.OUT_BOUND_STATUS)
                {
                    chuckList.Add(chunk);
                }
                else
                {
                    chunk.DispatchReset();
                }
            }
        }

        protected override void ResetChunkBox(int x, int y)
        {
            chuckList.RemoveAll(p =>
            {
                return p.Key.x == x && p.Key.y == y;
            });
        }

        protected override void UpdateChunkBox()
        {
            foreach (var cb in chuckList)
            {
                ProcessChunkStatus(cb, false);
            }
        }

        protected override void UpdateChunkMap()
        {
            int start_x = currentKey.x - env.InboundArea();
            int end_x = currentKey.x + env.InboundArea();
            int start_y = currentKey.y - env.InboundArea();
            int end_y = currentKey.y + env.InboundArea();

            for (int y = start_y; y < end_y; ++y)
            {
                for (int x = start_x; x < end_x; ++x)
                {
                    var chunk = GetChunk(x, y);
                    ProcessChunkStatus(chunk, true);
                    if (chunk == null)
                    {
                        var key = new ChunkKey(x, y);
                        ResetChunkBox(key.x, key.y);
                    }
                }
            }
        }
        private void OnReceivePoints(List<IPoint> points)
        {
            //Debug.Log("PovContainer OnReceivePoints : " + points.Count);
            foreach (var point in points)
            {
                DistributePoint("Temp", point);
            }
        }
        private void OnRemoveAllPointType(PointType pointType)
        {
            RemoveAllPointType("Temp", pointType);
        }

        private Status MeasurePositionAndStatus(ChunkKey diff)
        {
            //diff.x = Math.Abs(diff.x);
            //diff.y = Math.Abs(diff.y);
            diff.x = diff.x <= int.MinValue ? int.MaxValue : Math.Abs(diff.x);
            diff.y = diff.y <= int.MinValue ? int.MaxValue : Math.Abs(diff.y);

            if (diff.x > env.InboundArea()
                || diff.y > env.InboundArea())
            {
                return Status.OUT_BOUND_STATUS;
            }

            if (diff.x > env.RenderArea()
                || diff.y > env.RenderArea())
            {
                return Status.IN_BOUND_STATUS;
            }

            return Status.RENDER_STATUS;
        }
    }
}
