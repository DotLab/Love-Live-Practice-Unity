﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;
using LoveLivePractice.Api;

public class MenuScheduler : MonoBehaviour {
	public static MenuScheduler Instance;

	public EasedHidable maskHidable, flashHidable;
	public LiveScroll liveScroll;
	public LiveInfoPanel liveInfoPanel;
	public InputField pageNumberInput;

	public void Start() {
		Instance = this;

		pageNumberInput.onEndEdit.AddListener(value => {
			int newPage;
			if (int.TryParse(value, out newPage)) {
				ChangePage(newPage);
			} else pageNumberInput.text = currentPage.ToString();
		});

		StartCoroutine(StartHandler());
	}
		
	IEnumerator StartHandler() {
#if UNITY_EDITOR
		if (Game.LiveDict.Keys.Count == 0) {
			Game.LoadGameData();
//
//			Game.AvailableLiveCount = Game.CachedLives.Count;
//			Game.IsOffline = true;

			Game.IsOffline = false;

			var www = new WWW(UrlBuilder.GetLiveListUrl(0, UrlBuilder.ApiLimit));
			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
			var response = JsonUtility.FromJson<ApiLiveListResponse>(www.text);
			Game.ActivateApiLiveList(response.content.items);
		}
#endif

		yield return new WaitForSeconds(1);

		maskHidable.Hide();
		yield return Wait(maskHidable.TransitionDuration);

		liveScroll.RebuildContent();
	}

	public void ChangePage(int newPage) {
		if (newPage < 0 || (Game.IsOffline && newPage >= Game.CachedLives.Count / UrlBuilder.ApiLimit) || (!Game.IsOffline && newPage >= Game.AvailableLiveCount / UrlBuilder.ApiLimit)) {
			pageNumberInput.text = currentPage.ToString();
			return;
		}

		currentPage = newPage;
		pageNumberInput.text = currentPage.ToString();

		StopAllCoroutines();
		StartCoroutine(ChangePageHandler());
	}

	IEnumerator ChangePageHandler() {
		liveScroll.hidable.Hide();

		yield return Wait(liveScroll.hidable.TransitionDuration + 0.1f);

		if (Game.IsOffline) {
			Game.ActivateCachedLives(UrlBuilder.ApiLimit * currentPage, UrlBuilder.ApiLimit);
		} else {
			var www = new WWW(UrlBuilder.GetLiveListUrl(UrlBuilder.ApiLimit * currentPage, UrlBuilder.ApiLimit));
			yield return www;
			if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
			
			var apiLiveList = JsonUtility.FromJson<ApiLiveListResponse>(www.text).content;
			Game.ActivateApiLiveList(apiLiveList.items);
		}

		liveScroll.RebuildContent();

		liveScroll.hidable.Show();
	}

	[ContextMenu("Flash")]
	public void Flash() {
		flashHidable.ForceShow();
		flashHidable.Hide();
	}

	static WaitForSeconds Wait(float t) {
		return new WaitForSeconds(t);
	}

	public static void ChangeLive() {
		Instance.Flash();
		Instance.liveInfoPanel.ChangeLive();
	}

	static int currentPage, maxPage;

	public static void NextPage() {
		Instance.ChangePage(currentPage + 1);
	}

	public static void PreviousPage() {
		Instance.ChangePage(currentPage - 1);
	}
}