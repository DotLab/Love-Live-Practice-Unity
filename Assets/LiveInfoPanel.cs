﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;

public class LiveInfoPanel : MonoBehaviour {
	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	public EasedHidable bgHidable;

	public TextSwapable titleText, uploaderText, songInfoText, mapInfoText, playerInfoText;

	public void ChangeLive(Texture2D texture, LiveListItem liveListItem) {
		StartCoroutine(ChangeLiveHandler(texture, liveListItem));
	}

	IEnumerator ChangeLiveHandler(Texture2D texture, LiveListItem liveListItem) {
		bgHidable.Hide();

		yield return new WaitForSeconds(bgHidable.TransitionDuration + 0.1f);

		titleText.Swap(liveListItem.artist + " - " + liveListItem.live_name + " [LEVEL" + liveListItem.level.ToString("N0") + "]");
		uploaderText.Swap("Mapped by " + liveListItem.upload_user.username);

		bgUiRawImage.texture = texture;
		bgFitter.aspectRatio = (float)texture.width / texture.height;
	
		bgHidable.Show();

		var www = new WWW(UrlBuilder.GetLiveUrl(liveListItem.live_id));

		yield return www;
		if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);

		var response = JsonUtility.FromJson<LiveResponse>(www.text);
		var live = response.content;

		songInfoText.Swap(string.Format("Category: {0} Likes: {1:N0} Clicks: {2:N0}", live.category.name, live.like_count, live.click_count));
		playerInfoText.Swap(live.live_info);

		www = new WWW(UrlBuilder.GetCachedUploadUrl(live.map_path));
		yield return www;
		if (!string.IsNullOrEmpty(www.error)) Debug.LogError(www.error);
		System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + live.map_path, www.bytes);

		Debug.Log(www.text);
	}
}
