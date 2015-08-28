﻿
using System;
using UnityEngine;
using KSP.IO;

namespace CraftImport
{
	public class UICLASS: MonoBehaviour
	{
		private bool uiVisible = true;

		public UICLASS ()
		{
			Log.Info ("New instance of UICLASS: UICLASS constructor");

		}
		public void Start ()
		{
	
			Log.Info ("UICLASS: Start");
			DontDestroyOnLoad (this);
		}

		public void Awake ()
		{
			Log.Info ("UICLASS Awake");
			GameEvents.onShowUI.Add(onShowUI);
			GameEvents.onHideUI.Add(onHideUI);
		}

		private void onShowUI ()
		{
			Log.Info ("UICLASS onShowUI");
			uiVisible = true;
		}

		private void onHideUI ()
		{
			Log.Info ("UICLASS onHideUI");
			uiVisible = false;
		}

		public void OnDestroy ()
		{
			GameEvents.onShowUI.Remove(onShowUI);
			GameEvents.onHideUI.Remove(onHideUI);
		}

		public bool isVisible()
		{
			return uiVisible;
		}

		public void setVisible(bool b)
		{
			uiVisible = b;
		}
	}
}

