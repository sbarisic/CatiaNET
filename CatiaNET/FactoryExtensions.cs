using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using MECMOD;

namespace CatiaNET {
	public static class FactoryExtensions {
		static Dictionary<Vector, Point2D> LookupTable = new Dictionary<Vector, Point2D>();

		public static Vector GetPos(this Point2D P) {
			object[] Pos = new object[2];
			P.GetCoordinates(Pos);
			return new Vector((double)Pos[0], (double)Pos[1]);
		}

		public static Point2D DrawPoint(this Factory2D F, Vector P) {
			return F.CreatePoint(P.X, P.Y);
		}

		static Line2D CreateLine(this Factory2D F, Vector Start, Vector End) {
			return F.CreateLine(Start.X, Start.Y, End.X, End.Y);
		}

		public static Line2D DrawLine(this Factory2D F, Line L) {
			return F.DrawLine(L.Start, L.End);
		}

		public static Line2D DrawLine(this Factory2D F, Vector Start, Vector End) {
			Line2D L = F.CreateLine(Start, End);
			if (LookupTable.ContainsKey(Start))
				L.StartPoint = LookupTable[Start];
			else {
				L.StartPoint = L.StartPoint;
				LookupTable.Add(Start, L.StartPoint);
			}
			if (LookupTable.ContainsKey(End))
				L.EndPoint = LookupTable[End];
			else {
				L.EndPoint = L.EndPoint;
				LookupTable.Add(End, L.EndPoint);
			}
			return L;
		}

		public static Line2D DrawLine(this Factory2D F, Point2D Start, Point2D End) {
			Line2D L = F.CreateLine(Start.GetPos(), End.GetPos());
			L.StartPoint = Start;
			L.EndPoint = End;
			return L;
		}

		public static Line2D DrawLine(this Factory2D F, Point2D Start, Vector End) {
			Line2D L = F.CreateLine(Start.GetPos(), End);
			L.StartPoint = Start;
			if (LookupTable.ContainsKey(End))
				L.EndPoint = LookupTable[End];
			else {
				L.EndPoint = L.EndPoint;
				LookupTable.Add(End, L.EndPoint);
			}
			return L;
		}

		public static Line2D DrawLine(this Factory2D F, Vector Start, Point2D End) {
			Line2D L = F.CreateLine(Start, End.GetPos());
			if (LookupTable.ContainsKey(Start))
				L.StartPoint = LookupTable[Start];
			else {
				L.StartPoint = L.StartPoint;
				LookupTable.Add(Start, L.StartPoint);
			}
			L.EndPoint = End;
			return L;
		}
	}
}