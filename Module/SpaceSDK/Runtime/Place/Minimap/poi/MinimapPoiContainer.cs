using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public class MinimapPoiContainer : ChunkContainer<MinimapPoiEnv>
    {
        [DI(DIScope.component, DIComponent.minimap)] private MinimapPoiEvent MinimapPoiEvent { get; }
        [DI(DIScope.component, DIComponent.minimap)] private MinimapViewModel MinimapViewModel { get; }
        [DI(DIScope.component, DIComponent.place)] private SceneViewModel SceneViewModel { get; }

        private readonly List<Chunk> chuckList = new();

        protected override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            MinimapPoiEvent.OnReceivePoints.AddObserver(this, OnReceivePoints);
            MinimapPoiEvent.OnRemoveAllPointType.AddObserver(this, OnRemoveAllPointType);
            MinimapPoiEvent.OnReceiveChunkGroups.AddObserver(this, OnReceiveChunkGroups);
            MinimapViewModel.RenderSize.AddObserver(this, OnRenderSize);
            MinimapViewModel.VisibleSize.AddObserver(this, OnVisibleSize);
        }

        private void OnDisable()
        {
            MinimapPoiEvent.OnReceivePoints.RemoveAllObserver(this);
            MinimapPoiEvent.OnRemoveAllPointType.RemoveAllObserver(this);
            MinimapPoiEvent.OnReceiveChunkGroups.RemoveAllObserver(this);
            MinimapViewModel.RenderSize.RemoveAllObserver(this);
            MinimapViewModel.VisibleSize.RemoveAllObserver(this);
        }

        protected override IEnumerable<Chunk> GetEnumerableChunk()
        {
            for (int i = 0; i < chuckList.Count; ++i)
            {
                yield return chuckList[i];
            }
        }

        protected override void ClearChunkBox()
        {
            foreach (var cb in chuckList)
            {
                cb.DispatchReset();
            }
            chuckList.Clear();
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

        protected override void ResetChunkBox(int x, int y)
        {
            chuckList.RemoveAll(p =>
            {
                return p.Key.x == x && p.Key.y == y;
            });
        }

        protected override void ProcessChunkStatus(Chunk chunk, bool isConfig = false)
        {
            if (chunk == null) return;
            var diff = chunk.Key - currentKey;
            var status = MeasurePositionAndStatus(diff);
            chunk.UpdateStatus(status);

            if (isConfig && status < Status.OUT_BOUND_STATUS)
            {
                chuckList.Add(chunk);
            }
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


        private void OnReceivePoints(string navigationLocation, List<IPoint> points)
        {
            foreach (var point in points)
            {
                DistributePoint(navigationLocation, point);
            }
        }

        private void OnRemoveAllPointType(string navigationLocation, PointType pointType)
        {
            RemoveAllPointType(navigationLocation, pointType);
        }

        private void OnRenderSize(float renderWidth, float renderHeight)
        {
            env.UpdateRenderSize(renderWidth, renderHeight);
            if (SceneViewModel.PlaceScriptableObjects)
            {
                var groups = SceneViewModel.PlaceScriptableObjects.AvailableZoomLevel(Math.Max(renderWidth, renderHeight));
                OnReceiveChunkGroups(groups);
            }
        }

        private void OnVisibleSize(float visibleWidth, float visibleHeight)
        {
            env.UpdateVisibleSize(visibleWidth, visibleHeight);
        }

        private void OnReceiveChunkGroups(AbstractGroup[] chunkGroups)
        {
            if (env.GetElementFilter() is MinimapElementFilter filter)
            {
                filter.UpdateFilterType(chunkGroups);
            }
        }

		protected override void UpdatedChunk()
		{
			for (int i = 0; i < chuckList.Count; ++i)
			{
				chuckList[i].IsPossibleRender = true;
			}
		}

		protected override void UpdatedChunk(Chunk chunk)
		{
			chunk.IsPossibleRender = true;
		}

		protected override void PostRenderChunk(Chunk chunk)
		{
			chunk.IsPossibleRender = false;
		}
	}
}
