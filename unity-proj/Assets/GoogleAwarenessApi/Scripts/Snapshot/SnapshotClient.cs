﻿using System;
using System.Collections;
using System.Collections.Generic;
using DeadMosquito.GoogleMapsView.Internal;
using GoogleAwarenessApi.Scripts.Internal;
using JetBrains.Annotations;
using NinevaStudios.AwarenessApi.Internal;
using UnityEngine;

namespace NinevaStudios.AwarenessApi
{
	// TODO permissions


	/// <summary>
	/// Main class to interact with snapshot API
	///
	/// See https://developers.google.com/android/reference/com/google/android/gms/awareness/SnapshotClient
	/// </summary>
	[PublicAPI]
	public static class SnapshotClient
	{
		const string SnapshotClientClass = "com.google.android.gms.awareness.SnapshotClient";

		static AndroidJavaObject _client;

		/// <summary>
		/// Gets the current information about nearby beacons. Note that beacon snapshots are only available on API level 18 or higher.
		/// </summary>
		/// <param name="beaconTypes"></param>
		/// <param name="onSuccess"></param>
		/// <param name="onFailure"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void GetBeaconState([NotNull] List<BeaconState.TypeFilter> beaconTypes, Action<BeaconState> onSuccess, Action<string> onFailure)
		{
			if (beaconTypes == null)
			{
				throw new ArgumentNullException("beaconTypes");
			}

			if (beaconTypes.Count == 0)
			{
				throw new ArgumentException("beaconTypes must not be empty");
			}

			if (JniToolkitUtils.IsNotAndroidRuntime)
			{
				return;
			}

			CreateClientLazy();

			var onSuccessListenerProxy = new OnSuccessListenerProxy<BeaconState>(onSuccess, ajo => BeaconState.FromAJO(ajo.CallAJO("getBeaconState")));
			_client.CallAJO("getBeaconState", beaconTypes.ToJavaList(x => x.AJO))
				.CallAJO("addOnSuccessListener", onSuccessListenerProxy)
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		public static void GetDetectedActivity(Action<ActivityRecognitionResult> onSuccess, Action<string> onFailure)
		{
			if (CheckPreconditions())
			{
				return;
			}

			_client.CallAJO("getDetectedActivity")
				.CallAJO("addOnSuccessListener", new OnSuccessListenerProxy<ActivityRecognitionResult>(onSuccess,
					ajo => ActivityRecognitionResult.FromAJO(ajo.CallAJO("getActivityRecognitionResult"))))
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		/// <summary>
		/// Reports whether headphones are plugged into the device.
		/// </summary>
		/// <param name="onSuccess">Invoked with result if success</param>
		/// <param name="onFailure">Invoked with error message if failed</param>
		public static void GetHeadphoneState(Action<HeadphoneState> onSuccess, Action<string> onFailure)
		{
			if (CheckPreconditions())
			{
				return;
			}

			_client.CallAJO("getHeadphoneState")
				.CallAJO("addOnSuccessListener", new OnSuccessListenerProxy<HeadphoneState>(onSuccess, ajo => (HeadphoneState) ajo.CallAJO("getHeadphoneState").CallInt("getState")))
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		public static void GetLocation(Action<Location> onSuccess, Action<string> onFailure)
		{
			if (CheckPreconditions())
			{
				return;
			}

			_client.CallAJO("getLocation")
				.CallAJO("addOnSuccessListener", new OnSuccessListenerProxy<Location>(onSuccess, ajo => Location.FromAJO(ajo.CallAJO("getLocation"))))
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		public static void GetPlaces(Action<List<PlaceLikelihood>> onSuccess, Action<string> onFailure)
		{
			if (CheckPreconditions())
			{
				return;
			}

			_client.CallAJO("getPlaces")
				.CallAJO("addOnSuccessListener", new OnSuccessListenerProxy<List<PlaceLikelihood>>(onSuccess,
					ajo => ajo.CallAJO("getPlaceLikelihoods").FromJavaList(PlaceLikelihood.FromAJO)))
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		public static void GetTimeIntervals(Action<TimeIntervals> onSuccess, Action<string> onFailure)
		{
			if (CheckPreconditions())
			{
				return;
			}

			_client.CallAJO("getTimeIntervals")
				.CallAJO("addOnSuccessListener", new OnSuccessListenerProxy<TimeIntervals>(onSuccess, ajo =>
				{
					var intervals = ajo.CallAJO("getTimeIntervals").Call<int[]>("getTimeIntervals");
					return new TimeIntervals(Array.ConvertAll(intervals, i => (TimeInterval) i));
				}))
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		public static void GetWeather(Action<Weather> onSuccess, Action<string> onFailure)
		{
			if (CheckPreconditions())
			{
				return;
			}

			_client.CallAJO("getWeather")
				.CallAJO("addOnSuccessListener", new OnSuccessListenerProxy<Weather>(onSuccess, ajo =>
				{
					var weatherAJO = ajo.CallAJO("getWeather");
					return weatherAJO.IsJavaNull() ? null : new Weather(weatherAJO);
				}))
				.CallAJO("addOnFailureListener", new OnFailureListenerProxy(onFailure));
		}

		static bool CheckPreconditions()
		{
			if (JniToolkitUtils.IsNotAndroidRuntime)
			{
				return true;
			}

			CreateClientLazy();
			return false;
		}

		static void CreateClientLazy()
		{
			if (_client != null)
			{
				return;
			}

			_client = AwarenessUtils.AwarenessClass.AJCCallStaticOnceAJO("getSnapshotClient", JniToolkitUtils.Activity);
			AwarenessSceneHelper.Init();
		}
	}
}