using System.Collections;
using UnityEngine;

namespace MaxstXR.Place
{
	public class AssetReloader : InjectorBehaviour
	{
		[DI(DIScope.component, DIComponent.place)] private CustomerRepo CustomerRepo { get; }

		private void Start()
		{
			StartCoroutine(PricessPlaceSelected());
		}

		private IEnumerator PricessPlaceSelected()
		{
			yield return new WaitForSeconds(0.5f);
			//CustomerRepo.placeListEvent.Post(CustomerRepo.placeListEvent.Value);
		}
	}
}
