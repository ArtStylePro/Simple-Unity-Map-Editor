using UnityEngine;
using System.Collections;
using System;

namespace MapEditor
{
	[Serializable]
	public class IntVector2
	{
		public int X;
		public int Y;

		public IntVector2()
		{
			X = 0;
			Y = 0;
		}

		public IntVector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public IntVector2(float x, float y)
		{
			X = (int)x;
			Y = (int)y;
		}

		public static IntVector2 operator +(IntVector2 iv1, IntVector2 iv2)
		{
			return new IntVector2 (iv1.X + iv2.X, iv1.Y + iv2.Y);
		}

		public static IntVector2 operator -(IntVector2 iv1, IntVector2 iv2)
		{
			return new IntVector2(iv1.X - iv2.X, iv1.Y - iv2.Y);
		}

		public static IntVector2 Zero()
		{
			return new IntVector2(0, 0);
		}

		public override string ToString()
		{
			return String.Format("{0},{1}", X, Y);
		}
	}
}