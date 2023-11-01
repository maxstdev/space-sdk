using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public abstract class ChunkContainer<T> : InjectorBehaviour, ChunkDelegate where T : ChunkEnv
    {
        private string currentLocalizerAlias = "";
        private bool isDirty = false;
        private bool isRunning = true;
        protected int renderCount = 0;

        protected Dictionary<ChunkKey, Chunk> chunkMap = new Dictionary<ChunkKey, Chunk>();
        protected Dictionary<string, Dictionary<ChunkKey, Chunk>> chunkSection = new Dictionary<string, Dictionary<ChunkKey, Chunk>>();
        protected ChunkKey currentKey = new ChunkKey(int.MaxValue, int.MaxValue);

        [DI(DIScope.singleton)] protected T env { get; }

        protected Action<ChunkEnv> OnPrevRenderAction = null;
        protected Action<ChunkEnv> OnPostRenderAction = null;
		
		public ChunkContainer() : base()
        {

        }

        protected virtual void Start()
        {
            env.GetElementFilter().FilterValueInit();
            StartCoroutine(ProcessUpdate());
        }

        public IEnumerator ProcessUpdate()
        {
            while (isRunning)
            {
                if (!env.IsInitialize())
                {
                    yield return null;
                    continue;
                }

                if (UpdateMap())
                {
                    ClearChunkBox();
                    yield return null;
                }
                else if (UpdateCurrentKey(ref env.CurrentPosition()))
                {
                    UpdateChunkBox();
                    UpdateChunkMap();
					UpdatedChunk();
					env.UpdateMapComplete();
                    yield return null;
                }
                else
                {
                    renderCount = 0;
                    OnPrevRenderAction?.Invoke(env);
                    foreach (var chunk in GetEnumerableChunk())
                    {
                        yield return RenderToChunk(chunk);
                        if (CheckRenderLimit())
                        {
                            yield return null;
                        }
                    }
                    OnPostRenderAction?.Invoke(env);
                    yield return null;
                }
            }
        }

        public void DistributePoint(string localizerAlias, IPoint point)
        {
            Dictionary<ChunkKey, Chunk> findMap = FindOrCreateMap(localizerAlias);
            Chunk.MakeKey(ref point.GetPosition(), env, out ChunkKey key);
            Chunk findChunk = FindOrCreateChunk(findMap, key);
            findChunk.Add(point, env.GetElementFilter());
            findChunk.DispatchOnAdded(point);
            //Debug.Log("ChunkContainer DistributePoint : " + key + ", localizerAlias : " + localizerAlias);
        }

        /*
        public void DistributePointList(string localizerAlias, List<IPoint> list) 
        {
            Dictionary<ChunkKey, Chunk> findMap = FindOrCreateMap(localizerAlias);
            Chunk findChunk = null;
            for (int i = 0; i < list.Count; ++i)
            {
                Chunk.MakeKey(ref list[i].GetPosition(), out ChunkKey key);
                if (findChunk == null || findChunk.Key != key)
                {
                    findChunk = FindOrCreateChunk(findMap, key);
                }
                findChunk.Points.Add(list[i]);
                findChunk.DispatchOnAdded(list[i]);
            }
        }
        
        public void RemovePointList(string localizerAlias, List<IPoint> list)
        {
            Dictionary<ChunkKey, Chunk> findMap = FindOrCreateMap(localizerAlias);
            Chunk findChunk = null;
            for (int i = 0; i < list.Count; ++i)
            {
                Chunk.MakeKey(ref list[i].GetPosition(), out ChunkKey key);
                if (findChunk == null || findChunk.Key != key)
                {
                    findChunk = FindOrCreateChunk(findMap, key);
                }
                findChunk.Points.Remove(list[i]);
                list[i].OnDispose();
            }
        }
        */
        public void RemoveAllPointType(string localizerAlias, PointType type)
        {
            Dictionary<ChunkKey, Chunk> findMap = FindOrCreateMap(localizerAlias);
            var removeKeys = new List<ChunkKey>();
            foreach (var entry in findMap)
            {
                entry.Value.RemoveAll(type, env.GetElementFilter());

                if (entry.Value.Count == 0)
                {
                    removeKeys.Add(entry.Key);
                }
            }

            for (int i = 0; i < removeKeys.Count; ++i)
            {
                var key = removeKeys[i];
                findMap.Remove(key);
                if (localizerAlias == currentLocalizerAlias)
                {
                    ResetChunkBox(key.x, key.y);
                }
            }
        }

        public void HideAllPointType(string localizerAlias, PointType type)
        {
            Dictionary<ChunkKey, Chunk> findMap = FindOrCreateMap(localizerAlias);

            foreach (var entry in findMap)
            {
                entry.Value.DispatchHide(type);
            }

            Debug.Log("HideAllPointType");
        }

        public void DebugPrint()
        {
            Debug.LogFormat("ChunkContainer chunkSection size {0}", chunkSection.Count);
            foreach (var sectionEntry in chunkSection)
            {
                Debug.LogFormat("ChunkContainer {0} chunkMap size {1}", sectionEntry.Key, sectionEntry.Value.Count);
                foreach (var mapEnrtry in sectionEntry.Value)
                {
                    Debug.LogFormat("ChunkContainer {0} chunk point size {1}", mapEnrtry.Value.Key, mapEnrtry.Value.Count);
                }
            }
        }

        private Dictionary<ChunkKey, Chunk> FindOrCreateMap(string localizerAlias)
        {
            if (chunkSection.TryGetValue(localizerAlias, out Dictionary<ChunkKey, Chunk> map))
            {
                return map;
            }
            else
            {
                var newMap = new Dictionary<ChunkKey, Chunk>();
                chunkSection.Add(localizerAlias, newMap);
                return newMap;
            }
        }

        private Chunk FindOrCreateChunk(Dictionary<ChunkKey, Chunk> map, ChunkKey key)
        {
            if (map.TryGetValue(key, out Chunk chunk))
            {
                return chunk;
            }
            else
            {
                var newChunk = new Chunk(this);
                newChunk.SetKey(key);
                map.Add(key, newChunk);
                ProcessChunkStatus(newChunk, true);
                //Debug.Log("ChunkContainer FindOrCreateChunk : " + key + ", name :" + Location.LocalizerAlias);
                return newChunk;
            }
        }

        protected bool UpdateMap()
        {
            if (currentLocalizerAlias != env.VisibleNavigationLocation())
            {
                currentLocalizerAlias = env.VisibleNavigationLocation();
                if (chunkSection.TryGetValue(currentLocalizerAlias, out Dictionary<ChunkKey, Chunk> map))
                {
                    chunkMap = map;
                }
                else
                {
                    chunkMap = new Dictionary<ChunkKey, Chunk>();
                    chunkSection.Add(currentLocalizerAlias, chunkMap);
                }
                isDirty = true;
                return true;
            }
            isDirty = false;
            return false;
        }

        protected Chunk GetChunk(int x, int y)
        {
            if (chunkMap.TryGetValue(new ChunkKey(x, y), out Chunk chunk))
            {
                return chunk;
            }
            return null;
        }

        protected bool UpdateCurrentKey(ref Vector3 cameraPosition)
        {
            var key = new ChunkKey(
                (int)(cameraPosition.x / env.UnitDistance()),
                (int)(cameraPosition.z / env.UnitDistance()));
            if (env.IsCheckKey() && key == currentKey) return isDirty;
            currentKey = key;
            isDirty = true;
            return isDirty;
        }

		protected virtual IEnumerator RenderToChunk(Chunk chunk)
        {
            var filter = env.GetElementFilter();
            if (filter.UsedFilter)
            {
                if (chunk.UpdateTimestamp != filter.UpdateTimestamp)
                {
                    chunk.GenerateFilterPoints(filter, out List<IPoint> deactivePoints);
                    chunk.UpdateTimestamp = filter.UpdateTimestamp;
                    
                    for (int i = 0; i < deactivePoints.Count; ++i)
                    {
                        deactivePoints[i].OnRender(env, false);
                    }
					renderCount += chunk.Count;
					UpdatedChunk(chunk);
				}

				if (CheckRenderLimit())
				{
					yield return null;
				}

				if (chunk.IsPossibleRender)
				{
					renderCount += chunk.DispatchRender(env, true, filter);
					PostRenderChunk(chunk);
				}
            }
            else
            {
				if (chunk.IsPossibleRender)
				{
					renderCount += chunk.DispatchRender(env, true);
					PostRenderChunk(chunk);
				}
            }
			yield break;
        }

        public void UpdateGroupCondition(Chunk chunk, IPoint point)
        {
            var filter = env.GetElementFilter();
            chunk.UpdateFilterPoint(point, filter, out bool isRemove);
            if (isRemove) point.OnRender(env, false);
        }

		private bool CheckRenderLimit()
		{
			if (renderCount > env.renderLimitPerFrame)
			{
				renderCount = 0;
				return true;
			}
			return false;
		}

        // implements render box
        protected abstract IEnumerable<Chunk> GetEnumerableChunk();
        protected abstract void ClearChunkBox();
        protected abstract void UpdateChunkBox();
        protected abstract void UpdateChunkMap();
        protected abstract void ResetChunkBox(int x, int y);
        protected abstract void ProcessChunkStatus(Chunk chunk, bool isConfig = false);

		protected virtual void UpdatedChunk()
		{

		}

		protected virtual void UpdatedChunk(Chunk chunk)
		{

		}

		protected virtual void PostRenderChunk(Chunk chunk)
		{

		}
	}
}
