using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;

namespace DayTomato.Droid
{
	public static class PictureUtil
	{
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
	}
}
