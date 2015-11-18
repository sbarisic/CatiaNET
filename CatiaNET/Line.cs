using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CatiaNET {
	public struct Line {
		public Vector Start, End;

		public Line(double X1, double Y1, double X2, double Y2)
			: this(new Vector(X1, Y1), new Vector(X2, Y2)) {
		}

		public Line(Vector Start, Vector End) {
			this.Start = Start;
			this.End = End;
		}

		public bool IsEqualAngle(Line Line2, double Tolerance = 1) {
			double AngA = Vector.AngleBetween(Start, End).ToAng();
			double AngB = Vector.AngleBetween(Line2.Start, Line2.End).ToAng();
			return Math.Abs(AngA - AngB) < Tolerance;
		}

		public override bool Equals(object obj) {
			if (obj == null || GetType() != obj.GetType()) 
				return false;
			return ((Line)obj) == this;
		}

		public override int GetHashCode() {
			return (Start.GetHashCode() * 251) + End.GetHashCode();
		}

		public static bool operator ==(Line A, Line B) {
			return A.Start == B.Start && A.End == B.End;
		}

		public static bool operator !=(Line A, Line B) {
			return !(A == B);
		}
	}
}