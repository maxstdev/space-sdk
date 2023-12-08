using Castle.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace MaxstXR.Place
{
    public enum SearchState
    {
        Init,
        InSearch,
        Complete
    }

    public partial class CustomerRepo
    {
		private readonly int MAX_INSEARCH_CNT = 15;
        /*
        public IEnumerator Find(string placeUniqeName, Action<Place> result)
        {
            var isComplete = false;
            var p = FindPlaceFromName(placeUniqeName);
            if (p != null)
            {
                result?.Invoke(p);
                yield break;
            }
            else
            {
                placeListReqComplete.AddObserver(this, () =>
                {
                    isComplete = true;
                });
                FetchPlaceList();
            }

            yield return new WaitUntil(() => isComplete);
            placeListReqComplete.RemoveAllObserver(this);

            p = FindPlaceFromName(placeUniqeName);
            if (p != null)
            {
                isComplete = false;
                spotListReqComplete.AddObserver(this, () =>
                {
                    isComplete = true;
                });
                FetchSpotList(p.placeId);
                yield return new WaitUntil(() => isComplete);
                spotListReqComplete.RemoveAllObserver(this);

            }

            result?.Invoke(p);
            yield break;
        }
        */

        //public Place FindPlaceFromName(string placeUniqeName, List<Place> places)
        //{
        //    foreach (var p in places ?? new List<Place>())
        //    {
        //        if (placeUniqeName.Equals(p.placeUniqueName,
        //            StringComparison.OrdinalIgnoreCase))
        //        {
        //            return p;
        //        }
        //    }
        //    return null;
        //}

        //public Spot FindSpotFromName(string vpsSpotName, List<Spot> spots)
        //{
        //    foreach (var s in spots ?? new List<Spot>())
        //    {
        //        if (vpsSpotName.Equals(s.vpsSpotName,
        //            StringComparison.OrdinalIgnoreCase))
        //        {
        //            return s;
        //        }
        //    }
        //    return null;
        //}

        private Space FindSpace(string spaceId, List<Space> spaces)
        {
            if (cacheCategorySpace != null && cacheCategorySpace.spaceId.Equals(spaceId))
            {
                return cacheCategorySpace;
            }

            foreach (var space in spaces ?? new List<Space>())
            {
                if (space.spaceId.Equals(spaceId))
                {
                    cacheCategorySpace = space;
                    return space;
                }
            }

            return null;
        }

        //private Place FindPlace(string placeId, List<Place> places)
        //{
        //    if (int.TryParse(placeId, out int pid))
        //    {
        //        if (cacheCategoryPlace != null && cacheCategoryPlace.placeId == pid)
        //        {
        //            return cacheCategoryPlace;
        //        }

        //        foreach (var place in places ?? new List<Place>())
        //        {
        //            if (place.placeId == pid)
        //            {
        //                cacheCategoryPlace = place;
        //                return place;
        //            }
        //        }
        //    }
        //    return null;
        //}

        private Spot FindSpot(string spotId, List<Spot> spots)
        {
            if (int.TryParse(spotId, out int sid))
            {
                if (cacheCategorySpot != null && cacheCategorySpot.id == sid)
                {
                    return cacheCategorySpot;
                }

                if (spots.IsEmpty())
                {
                    Debug.LogWarning("spots IsEmpty");
                    return null;
                }

                foreach (var spot in spots)
                {
                    if (spot.id == sid)
                    {
                        cacheCategorySpot = spot;
                        return spot;
                    }
                }
            }
            return null;
        }

        private BaseCategory FindCategory(long categoryId)
        {
            if (cacheCategory != null && cacheCategory.categoryId == categoryId)
            {
                return cacheCategory;
            }

            if (categoryDictionaryEvent.Value == null)
            {
                Debug.LogWarning("categoryDictionaryEvent.Value is null!!!");
                return null;
            }

            if (categoryDictionaryEvent.Value.TryGetValue(categoryId, out BaseCategory baseCategory))
            {
                cacheCategory = baseCategory;
                return baseCategory;
            }
            return null;
        }

        public List<PoiPromise> GetAllContainsKeywordAndName(string value, List<Poi> pois)
        {
            var temp_dic_poi = new Dictionary<int, PoiPromise>();
            var temp_poi = new List<PoiPromise>();

            if (!string.IsNullOrEmpty(value))
            {
                string upperValue = value.ToUpper();

                foreach (var poi in pois ?? new List<Poi>())
                {
#if false
                    if (eachPlace.ExtensionObject != null)
					{
						if (false == eachPlace.ExtensionObject.isSearchAvailable)
						{
							continue;
						}
					}
#endif
					if (!string.IsNullOrEmpty(poi.PoiName) && poi.PoiName.Contains(value))
                    {
                        temp_poi.Add(poi);
                        continue;
                    }

                    if (poi.Keyward != null)
                    {
                        foreach (string keyword in poi.Keyward)
                        {
                            string upperPlacekeywork = keyword.ToUpper();

                            if (upperPlacekeywork.Contains(upperValue))
                            {
                                temp_poi.Add(poi);
                                break;
                            }
                        }
                    }
                }
                temp_poi = temp_poi.OrderBy(x => x.PoiName).ToList();
            }
            return temp_poi;
        }

		public List<PoiPromise> GetAllMatchedNameAndCategory(string value, SearchState state, List<Poi> pois)
		{
			var poisResult = new List<PoiPromise>();

			if (string.IsNullOrEmpty(value))
			{
				return poisResult;
			}
			
			var categoryId = categoryDictionaryEvent.Value.FirstOrDefault(x => x.Value.categoryName.ko == value).Key;

			foreach (var poi in pois ?? new List<Poi>())
			{
#if false
                if (eachPlace.ExtensionObject?.isSearchAvailable == false)
				{
					continue;
				}
#endif
				if (state == SearchState.InSearch 
					&& poi.PoiName?.StartsWith(value) == true)
				{
					poisResult.Add(poi);

					if (poisResult.Count >= MAX_INSEARCH_CNT)
					{
						break;
					}
				}
				else if (state == SearchState.Complete)
				{
					if (poi.PoiName?.Contains(value) == true)
					{
						poisResult.Add(poi);
					}
					else if ((categoryId != 0) && (categoryId == poi.CategotyId()))
					{
						poisResult.Add(poi);
					}
					else if (!poi.Keyward.IsNullOrEmpty() 
						&& !poi.Keyward.Find(x => x.Contains(value)).IsNullOrEmpty())
					{
						poisResult.Add(poi);
					}
				}
			}
			
			return poisResult.OrderBy(x => x.PoiName).ToList();
		}
	}
}
