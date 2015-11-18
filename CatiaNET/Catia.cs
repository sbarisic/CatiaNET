using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;

using INFITF;
using PARTITF;
using MECMOD;

namespace CatiaNET {
	public class Catia {
		public static Application CatiaInstance;

		static Catia() {
			CatiaInstance = Marshal.GetActiveObject("Catia.Application") as Application;
			if (CatiaInstance == null)
				throw new Exception("Could not find Catia interface");
		}

		public static PartDocument GetCurrentPartDocument() {
			return (PartDocument)CatiaInstance.ActiveDocument;
		}

		public static Sketch GetCurrentSketch() {
			return GetCurrentPartDocument().Part.InWorkObject as Sketch;
		}

		public static void EditSketch(Sketch S, Action<Factory2D> A) {
			if (S == null)
				throw new ArgumentException("Sketch is null", "S");
			Factory2D F = S.OpenEdition();
			if (F == null)
				throw new Exception("Could not open edition");
			try {
				A(F);
			} finally {
				S.CloseEdition();

			}
		}

		static void Logo(Factory2D F, float X, float Y, float Height, float Fatness) {
			const double AngleOffset = 0.09;

			Ellipse2D LeftUpper = F.CreateEllipse(X, Y - Fatness / 2 + Height,
				0, 0, Height / 2, Height, -3 * Math.PI / 2, -Math.PI / 2);

			Ellipse2D RightUpperUpper = F.CreateEllipse(X, Y - Fatness / 2 + Height,
				0, 0, Height / 2 - Fatness, Height - Fatness, Math.PI / 2, Math.PI - AngleOffset);

			Ellipse2D RightUpperLower = F.CreateEllipse(X, Y - Fatness / 2 + Height,
				0, 0, Height / 2 - Fatness, Height - Fatness, Math.PI + AngleOffset, 3 * Math.PI / 2);

			Ellipse2D LeftLower = F.CreateEllipse(X, Y + Fatness / 2 - Height,
				0, 0, Height / 2 - Fatness, Height - Fatness, 3 * Math.PI / 2, Math.PI / 2);
			Ellipse2D RightLower = F.CreateEllipse(X, Y + Fatness / 2 - Height,
				0, 0, Height / 2, Height, 3 * Math.PI / 2, Math.PI / 2);

			LeftUpper.EndPoint = LeftLower.EndPoint;
			RightUpperLower.EndPoint = RightLower.EndPoint;

			Circle2D UpperCircle = F.CreateCircle(X, (Y + Height * 2) - Fatness, Fatness / 2, -Math.PI / 2, Math.PI / 2);
			UpperCircle.EndPoint = LeftUpper.StartPoint;
			UpperCircle.StartPoint = RightUpperUpper.StartPoint;

			Circle2D LowerCircle = F.CreateCircle(X, (Y - Height * 2) + Fatness, Fatness / 2, Math.PI / 2, -Math.PI / 2);
			LowerCircle.StartPoint = LeftLower.StartPoint;
			LowerCircle.EndPoint = RightLower.StartPoint;

			Point2D BarLowerStart = RightUpperLower.StartPoint;
			Line2D BarLower = F.CreateLine(BarLowerStart.GetPos().X, BarLowerStart.GetPos().Y,
				BarLowerStart.GetPos().X + Height / 2, BarLowerStart.GetPos().Y);
			BarLower.StartPoint = BarLowerStart;

			Point2D BarUpperStart = RightUpperUpper.EndPoint;
			Line2D BarUpper = F.CreateLine(BarUpperStart.GetPos().X, BarUpperStart.GetPos().Y,
				BarUpperStart.GetPos().X + Height / 2, BarUpperStart.GetPos().Y);
			BarUpper.StartPoint = BarUpperStart;

			double UpperX = BarUpper.EndPoint.GetPos().X;
			double UpperY = BarUpper.EndPoint.GetPos().Y;
			double LowerY = BarLower.EndPoint.GetPos().Y;
			double R = (UpperY - LowerY) / 2;
			Circle2D BarCircle = F.CreateCircle(UpperX, LowerY + R, R, -Math.PI / 2, Math.PI / 2);
			BarCircle.EndPoint = BarUpper.EndPoint;
			BarCircle.StartPoint = BarLower.EndPoint;
		}

		public static void GenerateMaze(int W, int H, int GridX, int GridY) {
			List<Tuple<Vector, Point2D>> LookupTable = new List<Tuple<Vector, Point2D>>();
			Func<Vector, Point2D> TableGet = (A) => {
				for (int i = 0; i < LookupTable.Count; i++)
					if (LookupTable[i].Item1 == A)
						return LookupTable[i].Item2;
				return null;
			};

			Maze M = new Maze(GridX, GridY);
			Line[] Lines = M.Generate((double)W / (double)GridX, (double)H / (double)GridY);

			EditSketch(GetCurrentSketch(), (F) => {
				for (int i = 0; i < Lines.Length; i++)
					F.DrawLine(Lines[i]);

				/*Line2D L = F.DrawLine(new Vector(20, 20), new Vector(100, 100));
				F.DrawLine(L.EndPoint, new Vector(30, 20));*/
			});
			GetCurrentPartDocument().Part.Update();
		}

		public static void GenerateLogo(float X, float Y, float Height, float Fatness) {
			EditSketch(GetCurrentSketch(), (F) => Logo(F, X, Y, Height, Fatness));
			GetCurrentPartDocument().Part.Update();
		}
	}
}