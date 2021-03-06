﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using Texture2D = UnityEngine.Texture2D;

namespace ColorThiefDotNet {
	public partial class ColorThief {
		public class ColorThiefJob : LocalStorage.LoadJob<List<QuantizedColor>> {
			UnityEngine.Color32[] bmp;

			Thread thread;
			List<QuantizedColor> colors;

			public ColorThiefJob(
				string key,
				Texture2D sourceImage, 
				Action<LocalStorage.ILoadJob<List<QuantizedColor>>> callback = null, 
				int colorCount = DefaultColorCount, 
				int quality = DefaultQuality, 
				bool ignoreWhite = DefaultIgnoreWhite) : base(key, callback) {

				if (quality < 1) quality = DefaultQuality;

				bmp = sourceImage.GetPixels32();
			
				thread = new Thread(() => {
					var pixels = GetIntFromColors(bmp);
					var pixelCount = bmp.Length;
					var pixelArray = ConvertPixels(pixels, pixelCount, quality, ignoreWhite);
					var cmap = GetColorMap(pixelArray, colorCount);
					if (cmap != null) colors = cmap.GeneratePalette();
					colors.Sort((a, b) => a.Population.CompareTo(b.Population));
				});
			}

			public void Start() {
				thread.Start();
			}

			public override float GetProgress() {
				return 0;
			}

			public override bool IsFinished() {
				return thread.ThreadState == ThreadState.Stopped;
			}

			public override List<QuantizedColor> GetData() {
				return colors;
			}
		}

		public static QuantizedColor GetColorFromPalette(List<QuantizedColor> palette) {
			return new QuantizedColor(new Color {
				A = Convert.ToByte(palette.Average(a => a.Color.A)),
				R = Convert.ToByte(palette.Average(a => a.Color.R)),
				G = Convert.ToByte(palette.Average(a => a.Color.G)),
				B = Convert.ToByte(palette.Average(a => a.Color.B))
			}, Convert.ToInt32(palette.Average(a => a.Population)));
		}

		public static UnityEngine.Color[] GetUnityColorsFromPalette(List<QuantizedColor> palette) {
			return palette.Select(qColor => qColor.Color.ToUnityColor()).ToArray();
		}

		/// <summary>
		///     Use the median cut algorithm to cluster similar colors and return the base color from the largest cluster.
		/// </summary>
		/// <param name="sourceImage">The source image.</param>
		/// <param name="quality">
		///     1 is the highest quality settings. 10 is the default. There is
		///     a trade-off between quality and speed. The bigger the number,
		///     the faster a color will be returned but the greater the
		///     likelihood that it will not be the visually most dominant color.
		/// </param>
		/// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
		/// <returns></returns>
		public QuantizedColor GetColor(Texture2D sourceImage, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite) {
			var palette = GetPalette(sourceImage, DefaultColorCount, quality, ignoreWhite);

			return GetColorFromPalette(palette);
		}

		/// <summary>
		///     Use the median cut algorithm to cluster similar colors.
		/// </summary>
		/// <param name="sourceImage">The source image.</param>
		/// <param name="colorCount">The color count.</param>
		/// <param name="quality">
		///     1 is the highest quality settings. 10 is the default. There is
		///     a trade-off between quality and speed. The bigger the number,
		///     the faster a color will be returned but the greater the
		///     likelihood that it will not be the visually most dominant color.
		/// </param>
		/// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
		/// <returns></returns>
		/// <code>true</code>
		public List<QuantizedColor> GetPalette(Texture2D sourceImage, int colorCount = DefaultColorCount, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite) {
			var pixelArray = GetPixelsFast(sourceImage, quality, ignoreWhite);

			var cmap = GetColorMap(pixelArray, colorCount);
			if (cmap != null) {
				var colors = cmap.GeneratePalette();
				return colors;
			}
			return new List<QuantizedColor>();
		}

		byte[][] GetPixelsFast(Texture2D sourceImage, int quality, bool ignoreWhite) {
			if (quality < 1) {
				quality = DefaultQuality;
			}

			var pixels = GetIntFromPixel(sourceImage);
			var pixelCount = sourceImage.width * sourceImage.height;

			return ConvertPixels(pixels, pixelCount, quality, ignoreWhite);
		}

		static byte[] GetIntFromPixel(Texture2D bmp) {
			var pixelList = new byte[bmp.width * bmp.height * 4];
			int count = 0;

			foreach (var clr in bmp.GetPixels32()) {
				pixelList[count] = clr.b;
				count++;

				pixelList[count] = clr.g;
				count++;

				pixelList[count] = clr.r;
				count++;

				pixelList[count] = clr.a;
				count++;
			}

			return pixelList;
		}

		static byte[] GetIntFromColors(UnityEngine.Color32[] bmp) {
			var pixelList = new byte[bmp.Length * 4];
			int count = 0;

			foreach (var clr in bmp) {
				pixelList[count] = clr.b;
				count++;

				pixelList[count] = clr.g;
				count++;

				pixelList[count] = clr.r;
				count++;

				pixelList[count] = clr.a;
				count++;
			}

			return pixelList;
		}
	}
}
