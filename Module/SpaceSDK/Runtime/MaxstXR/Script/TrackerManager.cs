/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace maxstAR
{
	/// <summary>
	/// Control AR Engine (Singletone)
	/// </summary>
	public class TrackerManager
	{
		private static TrackerManager instance = null;
		//private TrackingState trackingState = null;
		private byte[] timeBytes = new byte[1000];
		private byte[] accessTokenBytes = new byte[1000];

		/// <summary>
		/// Get TrackerManager instance
		/// </summary>
		/// <returns></returns>
		public static TrackerManager GetInstance()
		{
			if (instance == null)
			{
				instance = new TrackerManager();

			}

			return instance;
		}

		private TrackerManager()
		{

		}


		/// <summary>Start Tracker.</summary>
		[Obsolete]
		public void StartTracker()
		{
			NativeAPI.maxst_TrackerManager_startTracker();
		}

        public void StartTrackerAtSpace(string spaceId)
        {
            NativeAPI.maxst_TrackerManager_startTracker();
            AddTrackerData("{\"vps_placeid\":" + spaceId + "}");
        }

  //      public void StartTrackerAtPlace(int placeId)
		//{
		//	NativeAPI.maxst_TrackerManager_startTracker();
		//	AddTrackerData("{\"vps_placeid\":" + placeId + "}");
		//}

		public void SetAccessToken(string token)
		{
			NativeAPI.maxst_TrackerManager_setAccessToken(token);
		}

		public void SetSpaceId(string spaceId)
        {	
			XRAPI.Instance.SetSpaceId(spaceId);
		}

		/// <summary>Stop Tracker.</summary>
		public void StopTracker()
		{
			NativeAPI.maxst_TrackerManager_stopTracker();
		}

		/// <summary>Destroy Tracker.</summary>
		public void DestroyTracker()
		{
			NativeAPI.maxst_TrackerManager_destroyTracker();
		}

		/// <summary>Refresh Tracker.</summary>
		public void RefreshTracker()
		{
			NativeAPI.maxst_TrackerManager_refreshTracker();
		}

		/// <summary>Add the Trackable data to the Map List.</summary>
		/// <param name="trackingFileName">File path of map for map addition.</param>
		/// <param name="isAndroidAssetFile">Map file position for addition. True is in Asset folder.</param>
		public void AddTrackerData(string trackingFileName, bool isAndroidAssetFile = false)
		{
			NativeAPI.maxst_TrackerManager_addTrackerData(trackingFileName, isAndroidAssetFile);
		}

		/// <summary>Delete the Trackable data from the Map List.</summary>
		/// <param name="trackingFileName">trackingFileName map file name. 
		/// This name should be same which added. If set "" (empty) file list will be cleared</param>
		public void RemoveTrackerData(string trackingFileName = "")
		{
			NativeAPI.maxst_TrackerManager_removeTrackerData(trackingFileName);
		}

		public void ReplaceServerIP(string serverIP)
		{
			NativeAPI.maxst_TrackerManager_replaceServerIP(serverIP);
		}

		/// <summary>Load the Trackable data.</summary>
		public void LoadTrackerData()
		{
			NativeAPI.maxst_TrackerManager_loadTrackerData();
		}

		/// <summary>Request ARCore apk.</summary>
		public void RequestARCoreApk()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
            AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if(currentActivity != null) 
            {
                AndroidJavaClass trackerManagerClass = new AndroidJavaClass("com.maxst.ar.TrackerManager");
                AndroidJavaObject trackerManager = trackerManagerClass?.CallStatic<AndroidJavaObject>("getInstance");
                var message = trackerManager?.Call<string>("requestARCoreApk", currentActivity);
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.LogWarning($"RequestARCoreApk {message}");
                }
            }
#endif
		}

		public bool VerificateARCoreApk()
		{
			bool result = true;
#if !UNITY_EDITOR && UNITY_ANDROID
             AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if(currentActivity != null) 
            {
                AndroidJavaClass arCoreApkClass = new AndroidJavaClass("com.google.ar.core.ArCoreApk");
                AndroidJavaObject arCoreApk = arCoreApkClass?.CallStatic<AndroidJavaObject>("getInstance");
                var availability = arCoreApk?.Call<AndroidJavaObject>("checkAvailability", currentActivity);
				result = availability.Call<bool>("isSupported");
			}
#endif
			return result;
		}

		/// <summary>Update ARFrame.</summary>
		public void UpdateFrame(bool isTexture)
		{
			NativeAPI.maxst_TrackerManager_updateFrame(isTexture);
		}

		/// <summary>Get ARFrame. must be call after UpdateFrame function.</summary>
		/// <returns>ARFrame</returns>
		public ARFrame GetARFrame()
		{
			ulong arFrameCPtr = 0;
			arFrameCPtr = NativeAPI.maxst_TrackerManager_getARFrame();

			return new ARFrame(arFrameCPtr);
		}

		/// <summary>Get Server State.</summary>
		/// <returns>Server Information</returns>
		public string GetServerQueryTime()
		{
			Array.Clear(timeBytes, 0, timeBytes.Length);
			NativeAPI.maxst_TrackerManager_getServerQueryTime(timeBytes);

			return Encoding.UTF8.GetString(timeBytes).TrimEnd('\0');
		}

		public string GetAccessToken()
		{
			return XRAPI.Instance.GetAccessToken();
		}
	}
}
