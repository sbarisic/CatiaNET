using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Windows;

namespace CatiaNET {
	public static class Utils {
		public static string TypeName(this object O) {
			return Information.TypeName(O);
		}

		public static T[] Concat<T>(this T[] A, T[] B) {
			T[] New = new T[A.Length + B.Length];
			Array.Copy(A, New, A.Length);
			Array.Copy(B, 0, New, A.Length, B.Length);
			return New;
		}

		public static double Dot(Vector A, Vector B) {
			return A.X * B.X + A.Y * B.Y;
		}

		public static double Fract(double D) {
			return D - Math.Truncate(D);
		}

		public static double ToAng(this double Rad) {
			return Rad * 180.0 / Math.PI;
		}

		public static double Rand(double X, double Y) {
			return Fract(Math.Sin(Dot(new Vector(X, Y), new Vector(12.9898, 78.233))) * 43758.5453);
		}

		public static Vector HashOffset(this Vector V, double Amp = 5) {
			return new Vector(V.X + Rand(V.X, V.Y) * Amp, V.Y + Rand(V.X, V.Y) * Amp);
		}
	}
}