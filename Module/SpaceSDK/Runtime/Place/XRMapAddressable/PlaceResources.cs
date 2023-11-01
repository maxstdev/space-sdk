using MaxstUtils;
using MaxstXR.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaxstXR.Place
{
    public class PlaceResources : InSceneUniqueBehaviour
    {
        public static PlaceResources Instance(GameObject go) => Instance<PlaceResources>(go);

        [Tooltip("PoI Resources")]
        [field: SerializeField] public GameObject PoICardListPrefab { get; private set; }
        [field: SerializeField] public GameObject InfoDeskPrefab { get; private set; }
        [field: SerializeField] public GameObject ToiletPrefab { get; private set; }

        [Tooltip("Minimap Resources")]
        [field: SerializeField] public GameObject MinimapPoI { get; private set; }
        [field: SerializeField, ColorHtmlProperty] public Color NormalColor { get; private set; }
        [field: SerializeField, ColorHtmlProperty] public Color DestColor { get; private set; }
        [field: SerializeField] public Material MinimapBillboardMaterial { get; private set; }

        [Tooltip("Place layers")]
        [field: SerializeField, Layer] public int StructureLayer { get; private set; }
        [field: SerializeField, Layer] public int GroundLayer { get; private set; }
        [field: SerializeField, Layer] public int MinimapLayer { get; private set; }
    }
}
