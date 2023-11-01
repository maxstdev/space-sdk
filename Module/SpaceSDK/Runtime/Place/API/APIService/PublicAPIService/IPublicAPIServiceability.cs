using Retrofit.Methods;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;

namespace MaxstXR.Place
{
	public interface IPublicAPIServiceability
	{
		[Get("/v1/public/place/{place_id}")]
		IObservable<Place> GetPlaceFromId(
			[Path("place_id")] long placeId);

		[Get("/v1/public/place/{place_id}/spot-list")]
		IObservable<List<Spot>> ReqSpotList(
				[Path("place_id")] long placeId);
	}
}