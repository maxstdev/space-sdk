using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MaxstXR.Place
{
    public class PoiContainer : ChunkContainer<PoIEnv>
    {
        private const int ARRAY_SIZE = 5;
        private const int SHIFT_SIZE = 2;
        private const int RENDER_SIZE = ARRAY_SIZE - SHIFT_SIZE;
        /**
         * 3x3 render
         * 5x5 valid
         * etc outbound
         */
        private readonly int[] renderIndex = new int[RENDER_SIZE * RENDER_SIZE] { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
        private readonly Chunk[] chuckBox = new Chunk[ARRAY_SIZE * ARRAY_SIZE];

        [DI(DIScope.component, DIComponent.place)] protected PoIEvent PoiEvent { get; }

        private List<IPoint> poiList = null;

        protected override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            PoiEvent.PrevRenderEvent.AddObserver(this, (action) => OnPrevRenderAction = action);
            PoiEvent.PostRenderEvent.AddObserver(this, (action) => OnPostRenderAction = action);
            PoiEvent.OnReceivePoints.AddObserver(this,
                (navigationLocation, list) =>
                {
                    if (list?.FirstOrDefault() is PoIContent)
                    {
                        poiList = list;
                    }
                    foreach (var arrow in list) DistributePoint(navigationLocation, arrow);
                });
            PoiEvent.OnRemoveAllPointType.AddObserver(this,
                (navigationLocation, list) => RemoveAllPointType(navigationLocation, list));
            PoiEvent.ChunkGroupEvent.AddObserver(this, OnReceiveChunkGruops);
            PoiEvent.OnHidePointType.AddObserver(this,
                (navigationLocation, list) => HideAllPointType(navigationLocation, list));
            PoiEvent.poiVisible.AddObserver(this, OnPoiVisible);
        }

        private void OnDisable()
        {
            PoiEvent.PrevRenderEvent.RemoveAllObserver(this);
            PoiEvent.PostRenderEvent.RemoveAllObserver(this);
            PoiEvent.OnReceivePoints.RemoveAllObserver(this);
            PoiEvent.OnRemoveAllPointType.RemoveAllObserver(this);
            PoiEvent.ChunkGroupEvent.RemoveAllObserver(this);
            PoiEvent.poiVisible.RemoveAllObserver(this);
            OnPrevRenderAction = null;
            OnPostRenderAction = null;
        }

        private void OnPoiVisible(bool isVisible)
        {
            foreach (var chunk in chuckBox)
            {
                chunk?.SetPovVisible(isVisible);
            }
        }

        protected override IEnumerable<Chunk> GetEnumerableChunk()
        {
            foreach (var i in renderIndex)
            {
                if (chuckBox[i] == null)
                {
                    continue;
                }
                else
                {
                    yield return chuckBox[i];
                }
            }
        }

        protected override void ClearChunkBox()
        {
            for (int i = 0; i < ARRAY_SIZE * ARRAY_SIZE; ++i)
            {
                chuckBox[i]?.DispatchReset();
                chuckBox[i] = null;
            }
        }

        protected override void UpdateChunkBox()
        {
            for (int i = 0; i < ARRAY_SIZE * ARRAY_SIZE; ++i)
            {
                ProcessChunkStatus(chuckBox[i], false);
            }
        }

        protected override void UpdateChunkMap()
        {
            int start_x = currentKey.x - SHIFT_SIZE;
            int end_x = start_x + ARRAY_SIZE;
            int start_y = currentKey.y - SHIFT_SIZE;
            int end_y = start_y + ARRAY_SIZE;

            for (int y = start_y; y < end_y; ++y)
            {
                for (int x = start_x; x < end_x; ++x)
                {
                    var chunk = GetChunk(x, y);
                    ProcessChunkStatus(chunk, true);
                    if (chunk == null)
                    {
                        ResetChunkBox(x, y);
                    }
                }
            }
        }

        protected override void ResetChunkBox(int x, int y)
        {
            var diff = new ChunkKey(x - currentKey.x, y - currentKey.y);
            var status = MeasurePositionAndStatus(diff, out int cx, out int cy);
            if (status < Status.OUT_BOUND_STATUS)
            {
                var index = ConvertPos(cx, cy);
                chuckBox[index] = null;
            }
        }

        protected override void ProcessChunkStatus(Chunk chunk, bool isConfig = false)
        {
            if (chunk == null) return;
            var diff = chunk.Key - currentKey;
            var status = MeasurePositionAndStatus(diff, out int x, out int y);
            chunk.UpdateStatus(status);
            var index = ConvertPos(x, y);

            if (isConfig && status < Status.OUT_BOUND_STATUS)
            {
                chuckBox[index] = chunk;
            }
        }

        private Status MeasurePositionAndStatus(ChunkKey diff, out int x, out int y)
        {
            x = diff.x + SHIFT_SIZE;
            y = diff.y + SHIFT_SIZE;
            if (x < 0 || x > ARRAY_SIZE - 1) return Status.OUT_BOUND_STATUS;
            if (y < 0 || y > ARRAY_SIZE - 1) return Status.OUT_BOUND_STATUS;

            if (x > 0 && x < ARRAY_SIZE - 1
                && y > 0 && y < ARRAY_SIZE - 1)
            {
                return Status.RENDER_STATUS;
            }
            return Status.IN_BOUND_STATUS;
        }

        private int ConvertPos(int x, int y)
        {
            return x + y * ARRAY_SIZE;
        }

        private void OnReceiveChunkGruops(HashSet<AbstractGroup> chunkGroups)
        {
            if (env.GetElementFilter() is PoIElementFilter filter)
            {
                filter.UpdateFilterType(chunkGroups);
            }

            if (poiList == null) return;

            foreach (var poi in poiList)
            {
                if (poi is PoIContent poi3DSign)
                {
                    poi3DSign.OnChangeFilterType(chunkGroups);
                }
            }
        }
    }
}
