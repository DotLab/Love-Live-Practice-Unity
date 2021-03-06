﻿using UnityEngine;

using Uif;

public class LiveScroll : InfiniteScroll<LiveScrollItem> {
	public float ItemMinWidth = 360, ItemMaxWidth = 480;
	public EasingType EasingType = EasingType.Cubic;
	public EasingPhase EasingPhase = EasingPhase.Out;

	public float HorizontalDragLimit = 0.2f;

	public bool Dirty;

	public LiveInfoPanel liveInfoPanel;
	public EasedHidable hidable;

	public void Start() {
		uiScroll.onValueChanged.AddListener(value => Dirty = true);

		ScrollTop = rectTrans.rect.yMax - (ItemHeight / 2);
		ScrollBottom = rectTrans.rect.yMin - (ItemHeight / 2);

		RebuildItems();
	}

	public override void RebuildContent() {
		ItemCount = Game.ActiveLives.Count;

		base.RebuildContent();
	
		hidable.Show();
	}

	public void Update() {
		if (Dirty) {
			Dirty = false;

			if (SwipeControl.Swipe == SwipeControl.SwipeDirection.Left) {
				Debug.Log("Next Page");
				MenuScheduler.NextPage();
			} else if (SwipeControl.Swipe == SwipeControl.SwipeDirection.Right) {
				Debug.Log("Previous Page");
				MenuScheduler.PreviousPage();
			}

			UpdateScroll();

			foreach (var item in items) {
				float step = 1 - (Mathf.Abs(item.Index - contentTop / itemSkip) / visibleItemCount * 2);
				step = Easing.Ease(EasingType, EasingPhase, step);
				item.Width = ItemMinWidth + step * (ItemMaxWidth - ItemMinWidth);
			}
		}
	}

	public override void InitItem(LiveScrollItem item, int index) {
		base.InitItem(item, index);

		item.Init(Game.ActiveLives[index]);
	}

	public override void OnItemClick(LiveScrollItem item) {
		if (Game.ActiveLive == Game.ActiveLives[item.Index]) {  // Select agin
			item.particleSystem.Stop();
			MenuScheduler.PlayLive();
		} else {
			foreach (var i in items) if (Game.ActiveLives[i.Index] == Game.ActiveLive) i.Deselect(Game.ActiveLives[i.Index]);
			Game.ActiveLive = Game.ActiveLives[item.Index];
			item.Select();

			MenuScheduler.ChangeLive();
		}
	}
}
