using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
	public class PoIContentGroup : AbstractGroup
	{
		private PoiPromise poi;

		public PoIContentGroup(PoiPromise poi)
		{
			this.poi = poi;
		}

		public override long GetGroupId()
		{
			return poi.CategotyId();
		}
	}
	public class PoIContent : IPoint
	{
		private Vector3 pos = Vector3.zero;
		private readonly PoiPromise poi;
		private PoICardList poi3DCardList;
		private readonly List<PoiPromise> poiList;
		private readonly List<PoiPromise> filteredPoiList = new();
		private readonly List<AbstractGroup> groupsList;

		private GameObject extensionObj = null;
		private GameObject obj = null;
		private GameObject parent = null;
		private CanvasGroup canvasGroup = null;
		private WeakReference<Chunk> wpChunk = null;
		private ChunkDelegate chunkDelegate = null;
		private bool isDest = false;
		private long filterUpdateTimestamp = -1;

		public PoIContent(List<PoiPromise> poiList, GameObject parent)
			: base()
		{
			this.poi = poiList[0];
			this.poiList = poiList;
			groupsList = poiList.ConvertAll(p => new PoIContentGroup(p) as AbstractGroup);

			this.parent = parent;
			pos = poiList[0].GetVpsPosition();

			foreach (var poi in poiList)
			{
				poi.OnDestination.AddObserver(this, OnDestination);
			}
		}

		~PoIContent()
		{
			foreach (var poi in poiList)
			{
				poi.OnDestination.RemoveAllObserver(this);
			}
		}


		public PointType GetPointType()
		{
			return PointType.SIGN_3D_TYPE;
		}

		public List<AbstractGroup> GetGroups()
		{
			return groupsList;
		}

		public ref Vector3 GetPosition()
		{
			return ref pos;
		}

		public bool IsIgnoreGroup()
		{
			return isDest;
		}

		public void SetRelationship(WeakReference<Chunk> wpChunk, ChunkDelegate chunkDelegate)
		{
			this.wpChunk = wpChunk;
			this.chunkDelegate = chunkDelegate;
		}

		public bool OnDispose()
		{
			ReleaseObject();
			return true;
		}

		public bool OnEnable()
		{
			CreateObject();
			//CreateExtensionObject();
			obj?.SetActive(false);
			extensionObj?.SetActive(false);
			return true;
		}

		public bool OnInBounds()
		{
			CreateObject();
			//CreateExtensionObject();
			obj?.SetActive(false);
			extensionObj?.SetActive(false);
			return true;
		}

		public bool OnOutBounds()
		{
			ReleaseObject();
			return true;
		}

		public bool OnRender(ChunkEnv env, bool isRenderStatus)
		{
			if (obj != null)
			{
				obj.SetActive(isRenderStatus && filteredPoiList.Count != 0);
				obj.transform.localPosition.Normalize2D(env.CurrentPosition(), out Vector3 direction);
				obj.transform.localRotation = Quaternion.LookRotation(direction);
				var distance = obj.transform.localPosition.Distance2D(env.CurrentPosition());
				UpdateCanvasAlpha(distance);
				UpdatePoiUIByDistance(distance);
				ConfigPoiCardList(env);
			}
			extensionObj?.SetActive(isRenderStatus);
			return true;
		}

		private void ConfigPoiCardList(ChunkEnv env)
		{
			if (poi3DCardList == null) return;

			if (filterUpdateTimestamp != env.GetElementFilter().UpdateTimestamp)
			{
				filterUpdateTimestamp = env.GetElementFilter().UpdateTimestamp;
				poi3DCardList.Config(filteredPoiList);
			}
		}

		public void OnChangeFilterType(HashSet<AbstractGroup> filterTypeList)
		{
			UpdateFilteredList(filterTypeList);
		}

		private void UpdateFilteredList(HashSet<AbstractGroup> filterTypeList)
		{
			filteredPoiList.Clear();
			if (filterTypeList == null)
			{
				filteredPoiList.AddRange(poiList);
			}
			else
			{
				filteredPoiList.AddRange(
					poiList.FindAll(p =>
						{
							return p.OnDestination.Value || filterTypeList.Contains(new ChunkGroup(p.CategotyId()));
						}
					)
				);
			}
		}

		private void CreateObject()
		{
			if (obj != null) return;

			if (poi.PoiName.Contains("안내"))
			{
				obj = GameObject.Instantiate(PlaceResources.Instance(parent).InfoDeskPrefab, parent.transform);
			}
			else if (poi.PoiName.Contains("화장실"))
			{
				obj = GameObject.Instantiate(PlaceResources.Instance(parent).ToiletPrefab, parent.transform);
			}
			else
			{

				obj = GameObject.Instantiate(PlaceResources.Instance(parent).PoICardListPrefab, parent.transform);
				poi3DCardList = obj.GetComponent<PoICardList>();
				UpdateFilteredList(null);
				poi3DCardList.Config(filteredPoiList);
			}
			canvasGroup = obj.GetComponent<CanvasGroup>();
			var p = poi.GetVpsPosition();
			obj.transform.localPosition = new Vector3(p.x, p.y + 1.5f, p.z);
			obj.SetActive(false);
			obj.name = poi.PoiName;
		}
#if false
		private void CreateExtensionObject()
		{
			if (poi.ExtensionObject == null) return;
			if (extensionObj != null) return;

			extensionObj = new GameObject(poi.PoiName);
			extensionObj.transform.parent = parent.transform;

			foreach (var component in poi.ExtensionObject.components)
			{
				switch (component.type)
				{
					case ComponentType.Video:
						if (component is Video video)
						{
							var extendObj = GameObject.Instantiate(PlacePrefabScriptableObjects.videoScreenPrefab, extensionObj.transform);
							extendObj.GetComponent<VideoContentsDataManager>().Config(video);
						}
						break;
					case ComponentType.Board:
						if (component is Board board)
						{
							var extendObj = GameObject.Instantiate(PlacePrefabScriptableObjects.boardPrefab, extensionObj.transform);
							extendObj.GetComponent<BoardContentsDataManager>().Config(board);
						}
						break;
					case ComponentType.Model:
						if (component is Model model)
						{
							var io = modelObjectRegistry.Find(model.modelType);
							if (io != null)
							{
								var extendObj = GameObject.Instantiate(io.gameObject, extensionObj.transform);
								extendObj.GetComponent<ModelContentsDataManager>().Config(model);
							}
						}
						break;
					default:
						break;
				}
			}
			var p = poi.GetVpsPosition();
			extensionObj.transform.localPosition = new Vector3(p.x, p.y, p.z);
			extensionObj.SetActive(false);
			extensionObj.name = poi.PoiName + "-Extension";
			return;
		}
#endif

		private void ReleaseObject()
		{
			if (obj != null)
			{
				GameObject.Destroy(obj);
				obj = null;
				canvasGroup = null;
				poi3DCardList = null;
			}

			if (extensionObj != null)
			{
				GameObject.Destroy(extensionObj);
				extensionObj = null;
			}
		}

		private void UpdateCanvasAlpha(float distance)
		{
			if (canvasGroup == null) return;
			canvasGroup.alpha = distance switch
			{
				float d when d < 3F => 0.8F,
				float d when d < 10F => 0.75F,
				float d when d < 20F => 0.7F,
				_ => 0.65F,
			};
		}

		private void UpdatePoiUIByDistance(float distance)
		{
			if (poi3DCardList == null) return;

			switch (distance)
			{
				case <= 10f:
					poi3DCardList.ExpendPoi();
					break;
				default:
					poi3DCardList.CollapsePoi();
					break;
			}
		}

		private void UpdateSprite(Sprite sprite)
		{
			//if (obj != null && spriteRenderer != null)
			//{
			//    spriteRenderer.sprite = sprite;
			//}
		}

		private void OnDestination(bool isDest)
		{
			//UpdateContent(isDest);
			if (this.isDest != isDest)
			{
				this.isDest = isDest;
				NotifyGroupCondition();
			}
		}

		private void NotifyGroupCondition()
		{
			Chunk chunk = null;
            if (wpChunk?.TryGetTarget(out chunk) ?? false)
			{
				chunkDelegate?.UpdateGroupCondition(chunk, this);
			}
		}
	}
}