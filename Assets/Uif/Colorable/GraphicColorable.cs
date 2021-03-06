﻿using UnityEngine;
using UnityEngine.UI;

namespace Uif {
	[AddComponentMenu("Uif/Colorable/Graphic Colorable")]
	[RequireComponent(typeof(Graphic))]
	public class GraphicColorable : Colorable {
		public Graphic graphic;


		public void OnValidate() {
			graphic = GetComponent<Graphic>();
		}

		public override Color GetColor() {
			return graphic.color;
		}

		public override void SetColor(Color newColor) {
			graphic.color = newColor;
		}
	}
}