using System.Runtime.InteropServices; // DllImport
using System;

namespace VUTPP
{
	/// <summary>
	/// ToolWindowUtil에 대한 요약 설명입니다.
	/// </summary>
	public struct ToolWindowUtil
	{
		static public object LoadTabIcon( string strFilename, string runtimeVersion )
		{
			string addinPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			System.Drawing.Bitmap appIcon = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(addinPath+"\\"+strFilename));

			switch( runtimeVersion )
			{
				case "7.10":
				case "9.0":
				case "10.0":
					return GetTransparentIPictureDispFromBitmapHandle( appIcon.GetHbitmap() );

				case "8.0":
					return appIcon.GetHbitmap();

				default:
					throw new ArgumentOutOfRangeException("runtimeVersion", string.Format(Constants.FrameworkNotSupported, runtimeVersion));
			}
		}

		private struct PICTDESC
		{
			public int SizeOfStruct;
			public int PicType;
			public IntPtr Hbitmap;
			public IntPtr Hpal;
			public int Padding;
			public PICTDESC( IntPtr hBmp )
			{
				SizeOfStruct = 0;
				PicType = 1;
				Hbitmap = hBmp;
				Hpal = IntPtr.Zero;
				Padding = 0;
			}
		};

		[DllImport("olepro32.dll")]
		static extern int OleCreatePictureIndirect( ref PICTDESC pPictDesc, ref Guid riid, int fOwn, out stdole.IPictureDisp ppvObj );

		static private object GetTransparentIPictureDispFromBitmapHandle( IntPtr hIntPtr )
		{
			int iResult;
			Guid objGuid = new Guid("00020400-0000-0000-C000-000000000046");
			PICTDESC tPICTDESC = new PICTDESC(hIntPtr);
			tPICTDESC.SizeOfStruct = Marshal.SizeOf(tPICTDESC);

			stdole.IPictureDisp objPicture;
			iResult = OleCreatePictureIndirect(ref tPICTDESC, ref objGuid, 1, out objPicture);

			return objPicture;
		}

	}
}
