using System.Collections.Generic;
using UnityEngine;

namespace MaxstXR.Place
{
    public static class AssetMap
    {
        private const string SDK_VERSION_PATH = "v0.11.0/";
        private const string URL = "https://xr-client-web.s3.ap-northeast-2.amazonaws.com/assetBundle/" + SDK_VERSION_PATH;
        public static string ADDRESSABLE_URL = "https://xr-client-web.s3.ap-northeast-2.amazonaws.com/placeSDK-addressable-v2/";
        public const string XR_BUNDLE_ANDROID = "xr_scene_common_android.unity3d";
        public const string XR_BUNDLE_IOS = "xr_scene_common_ios.unity3d";
        public const string XR_BUNDLE_WEBGL = "xr_scene_common_webgl.unity3d";
        public const string XR_BUNDLE_WINDOWS = "xr_scene_common_windows.unity3d";
        public const string XR_BUNDLE_OSX = "xr_scene_common_osx.unity3d";

        public const string SUB_ANDROID = "android";
        public const string SUB_IOS = "ios";
        public const string SUB_WEBGL = "webgl";
        public const string SUB_WINDOWS = "windows";
        public const string SUB_OSX = "osx";

        private static readonly Dictionary<RuntimePlatform, string> commonBundleManifest = new()
    {
        { RuntimePlatform.Android,          SUB_ANDROID },
        { RuntimePlatform.IPhonePlayer,     SUB_IOS },
        { RuntimePlatform.WebGLPlayer,      SUB_WEBGL },
        { RuntimePlatform.WindowsEditor,    SUB_WINDOWS },
        { RuntimePlatform.WindowsPlayer,    SUB_WINDOWS },
        { RuntimePlatform.OSXEditor,        SUB_OSX },
        { RuntimePlatform.OSXPlayer,        SUB_OSX },
    };

        private static readonly Dictionary<RuntimePlatform, string> commonBundleUrls = new()
    {
        { RuntimePlatform.Android,          URL + SUB_ANDROID + "/" },
        { RuntimePlatform.IPhonePlayer,     URL + SUB_IOS + "/" },
        { RuntimePlatform.WebGLPlayer,      URL + SUB_WEBGL + "/" },
        { RuntimePlatform.WindowsEditor,    URL + SUB_WINDOWS + "/" },
        { RuntimePlatform.WindowsPlayer,    URL + SUB_WINDOWS + "/" },
        { RuntimePlatform.OSXEditor,        URL + SUB_OSX + "/" },
        { RuntimePlatform.OSXPlayer,        URL + SUB_OSX + "/" },
    };

        private static readonly Dictionary<RuntimePlatform, string> commonBundleNams = new()
    {
        { RuntimePlatform.Android,          XR_BUNDLE_ANDROID },
        { RuntimePlatform.IPhonePlayer,     XR_BUNDLE_IOS },
        { RuntimePlatform.WebGLPlayer,      XR_BUNDLE_WEBGL },
        { RuntimePlatform.WindowsEditor,    XR_BUNDLE_WINDOWS },
        { RuntimePlatform.WindowsPlayer,    XR_BUNDLE_WINDOWS },
        { RuntimePlatform.OSXEditor,        XR_BUNDLE_OSX },
        { RuntimePlatform.OSXPlayer,        XR_BUNDLE_OSX },
    };

        public const string PLACE_RESOURCE = "PlaceResource.asset";
        public const string KNN_RESOURCES_SO = "KnnResourcesSO.asset";
        public const string IBR_CULL_FRONT = "IbrCullFront.asset";
        public const string IBR_CULL_BACK = "IbrCullBack.asset";
        public const string BUNDLE_CACHE = "bundleCache";

        public static string GetCommonBundleName(RuntimePlatform platform)
        {
            if (commonBundleNams.TryGetValue(platform, out string bundleName))
            {
                return bundleName;
            }
            Debug.Log(">>> Bundle name for current platform does not exist!");
            return null;
        }

        public static string GetCommonManifestName(RuntimePlatform platform)
        {
            if (commonBundleManifest.TryGetValue(platform, out string manifest))
            {
                return manifest;
            }
            Debug.Log(">>> Manifest name for current platform does not exist!");
            return null;
        }

        public static string GetCommonBundleUrl(RuntimePlatform platform)
        {
            if (commonBundleUrls.TryGetValue(platform, out string url))
            {
                return url + GetCommonBundleName(platform);
            }
            Debug.Log(">>> Bundle url for current platform does not exist!");
            return null;
        }

        public static string GetManifestBundleUrl(RuntimePlatform platform)
        {
            if (commonBundleUrls.TryGetValue(platform, out string url))
            {
                return url + GetCommonManifestName(platform);
            }
            Debug.Log(">>> Manifest Bundle url for current platform does not exist!");
            return null;
        }
    }
}