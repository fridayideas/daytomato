﻿using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;

namespace DayTomato.Droid
{
	public static class PictureUtil
	{
		public static Bitmap CircleBitmap(Bitmap bmp, int radius)
		{
			Bitmap sbmp;

			if (bmp.Width != radius || bmp.Height != radius)
			{
				float smallest = Math.Min(bmp.Width, bmp.Height);
				float factor = smallest / radius;
				sbmp = Bitmap.CreateScaledBitmap(bmp, (int)(bmp.Width / factor), (int)(bmp.Height / factor), false);
			}
			else {
				sbmp = bmp;
			}

			Bitmap output = Bitmap.CreateBitmap(radius, radius, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(output);

			const uint color = 0xffa19774;
			Paint paint = new Paint();
			Rect rect = new Rect(0, 0, radius, radius);

			paint.AntiAlias = true;
			paint.FilterBitmap = true;
			paint.Dither = true;
			canvas.DrawARGB(0, 0, 0, 0);
			paint.Color = Color.ParseColor("#BAB399");
			canvas.DrawCircle(radius / 2 + 0.7f,
							  radius / 2 + 0.7f, 
			                  radius / 2 + 0.1f, paint);
			paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
			canvas.DrawBitmap(sbmp, rect, rect, paint);

			return output;
		}

		public static async Task<Bitmap> DecodeByteArrayAsync(string fileName, int requiredWidth, int requiredHeight)
		{
			byte[] imageBytes = File.ReadAllBytes(fileName);
			var options = new BitmapFactory.Options { InJustDecodeBounds = true };
			await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length, options);

			options.InSampleSize = CalculateInSampleSize(options, requiredWidth, requiredHeight);
			options.InJustDecodeBounds = false;

			Bitmap resizedBitmap = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length, options);

			// Images are being saved in landscape, so rotate them back to portrait if they were taken in portrait
			Matrix mtx = new Matrix();
			ExifInterface exif = new ExifInterface(fileName);
			string orientation = exif.GetAttribute(ExifInterface.TagOrientation);

			switch (orientation)
			{
				case "6": // portrait
					mtx.PreRotate(90);
					resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					mtx.Dispose();
					mtx = null;
					break;
				case "1": // landscape
					break;
			}

			return resizedBitmap;
		}

		public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			// Raw height and width of image
			float height = options.OutHeight;
			float width = options.OutWidth;
			double inSampleSize = 1D;

			if (height > reqHeight || width > reqWidth)
			{
				int halfHeight = (int)(height / 2);
				int halfWidth = (int)(width / 2);

				// Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
				while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth)
				{
					inSampleSize *= 2;
				}
			}

			return (int)inSampleSize;
		}

		public static Bitmap StitchImages(Bitmap[] bitmaps)
		{
			const int offset = 20;
			Bitmap final = null;
			int width = 0;
			int height = 0;
			for (int i = 0; i < bitmaps.Length; ++i)
			{
				width += bitmaps[i].Width;
				height = bitmaps[i].Height;
			}
			final = Bitmap.CreateBitmap((width + bitmaps.Length * offset) * 2, 
			                            height * 2, 
			                            Bitmap.Config.Argb8888);

			Canvas stitched = new Canvas(final);
			for (int i = 0; i < bitmaps.Length; ++i)
			{
				Bitmap copy = Bitmap.CreateScaledBitmap(bitmaps[i], bitmaps[i].Width * 2, bitmaps[i].Height * 2, true);
				stitched.DrawBitmap(copy, (copy.Width * i) + (i * offset), 0, null);
			}

			return final;
		}
	}
}
