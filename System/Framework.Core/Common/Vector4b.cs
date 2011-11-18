using System;

namespace Framework.Core.Common
{
	public struct Vector4b
	{
		public byte X;
		public byte Y;
		public byte Z;
		public byte W;

		public Vector4b(byte x, byte y, byte z, byte w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}

		public Vector4b(byte x, byte y, byte z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = 1;
		}

		public Vector4b(byte value)
		{
			this.X = this.Y = this.Z = value;
			this.W = 1;
		}

		#region Operators

		public static bool operator ==(Vector4b left, Vector4b right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Vector4b left, Vector4b right)
		{
			return !left.Equals(right);
		}

		public static Vector4b operator +(Vector4b a, Vector4b b)
		{
			Vector4b result;

			result.X = (byte)(a.X + b.X);
			result.Y = (byte)(a.Y + b.Y);
			result.Z = (byte)(a.Z + b.Z);
			result.W = 1;

			return result;
		}

		public static Vector4b operator -(Vector4b a, Vector4b b)
		{
			Vector4b result;

			result.X = (byte)(a.X - b.X);
			result.Y = (byte)(a.Y - b.Y);
			result.Z = (byte)(a.Z - b.Z);
			result.W = 1;

			return result;
		}

		public static Vector4b operator *(Vector4b a, Vector4b b)
		{
			Vector4b result;

			result.X = (byte)(a.X * b.X);
			result.Y = (byte)(a.Y * b.Y);
			result.Z = (byte)(a.Z * b.Z);
			result.W = 1;

			return result;
		}

		public static Vector4b operator /(Vector4b a, Vector4b b)
		{
			Vector4b result;

			result.X = (byte)(a.X / b.X);
			result.Y = (byte)(a.Y / b.Y);
			result.Z = (byte)(a.Z / b.Z);
			result.W = 1;

			return result;
		}

		#endregion

		public bool Equals(Vector4b other)
		{
			return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z) && (this.W == other.W));
		}

		public override bool Equals(object obj)
		{
			bool flag = false;

			if (obj is Vector4b)
			{
				flag = this.Equals((Vector4b)obj);
			}

			return flag;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode() + this.W.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2},{3}", X, Y, Z, W);
		}

		public static string ConvertToString(byte x, byte y, byte z, byte w)
		{
			return string.Format("{0},{1},{2},{3}", x, y, z, w);
		}
	}
}