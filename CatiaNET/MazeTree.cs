using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;

using Point = System.Windows.Point;

namespace CatiaNET {
	public static class Extensions {
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> Src, Random RNG) {
			var E = Src.ToArray();
			for (var i = E.Length - 1; i >= 0; i--) {
				var swapIndex = RNG.Next(i + 1);
				yield return E[swapIndex];
				E[swapIndex] = E[i];
			}
		}

		public static CellState OppositeWall(this CellState Orig) {
			return (CellState)(((int)Orig >> 2) | ((int)Orig << 2)) & CellState.Initial;
		}

		public static string GetStates(this CellState CS) {
			string[] Names = Enum.GetNames(typeof(CellState));
			StringBuilder SB = new StringBuilder();
			for (int i = 0; i < Names.Length; i++)
				if (CS.HasFlag((CellState)Enum.Parse(typeof(CellState), Names[i])))
					SB.AppendFormat("{0} ", Names[i]);
			return SB.ToString().Trim();
		}
	}

	[Flags]
	public enum CellState {
		Bottom = 1 << 0,
		Right = 1 << 1,
		Top = 1 << 2,
		Left = 1 << 3,
		Visited = 1 << 5,
		Initial = Bottom | Right | Top | Left,
	}

	public struct RemoveWallAction {
		public Point Neighbour;
		public CellState Wall;
	}

	public class Maze {
		private readonly CellState[,] Cells;
		private readonly int Width;
		private readonly int Height;
		private readonly Random RNG;

		public Maze(int Width, int Height) {
			this.Width = Width;
			this.Height = Height;
			Cells = new CellState[Width, Height];
			for (var x = 0; x < Width; x++)
				for (var y = 0; y < Height; y++)
					Cells[x, y] = CellState.Initial;
			RNG = new Random(42);
			VisitCell(RNG.Next(Width), RNG.Next(Height));
		}

		public CellState this[int x, int y] {
			get {
				return Cells[x, y];
			}
			set {
				Cells[x, y] = value;
			}
		}

		public IEnumerable<RemoveWallAction> GetNeighbours(Point p) {
			if (p.X > 0)
				yield return new RemoveWallAction {
					Neighbour = new Point(p.X - 1, p.Y),
					Wall = CellState.Left
				};
			if (p.Y > 0)
				yield return new RemoveWallAction {
					Neighbour = new Point(p.X, p.Y - 1),
					Wall = CellState.Bottom
				};
			if (p.X < Width - 1)
				yield return new RemoveWallAction {
					Neighbour = new Point(p.X + 1, p.Y),
					Wall = CellState.Right
				};
			if (p.Y < Height - 1)
				yield return new RemoveWallAction {
					Neighbour = new Point(p.X, p.Y + 1),
					Wall = CellState.Top
				};
		}

		public void VisitCell(int x, int y) {
			this[x, y] |= CellState.Visited;
			foreach (var p in GetNeighbours(new Point(x, y)).Shuffle(RNG).Where(z =>
				!(this[(int)z.Neighbour.X, (int)z.Neighbour.Y].HasFlag(CellState.Visited)))) {
				this[x, y] -= p.Wall;
				this[(int)p.Neighbour.X, (int)p.Neighbour.Y] -= p.Wall.OppositeWall();
				VisitCell((int)p.Neighbour.X, (int)p.Neighbour.Y);
			}
		}

		public CellState GetCell(int X, int Y) {
			if (X < 0 || X >= Cells.GetLength(0))
				return CellState.Initial;
			if (Y < 0 || Y >= Cells.GetLength(1))
				return CellState.Initial;
			return Cells[X, Y];
		}

		public bool HasFlag(int X, int Y, CellState F) {
			return GetCell(X, Y).HasFlag(F);
		}

		public Line[] GenerateWalls(int X, int Y, double ScaleX, double ScaleY) {
			List<Line> Lines = new List<Line>();
			CellState S = GetCell(X, Y);
			float OriginalOffset = 0.5f;
			float Offset = OriginalOffset;

			float XOffset = OriginalOffset;
			float YOffset = OriginalOffset;

			if (S.HasFlag(CellState.Bottom)) {
				XOffset = !HasFlag(X, Y, CellState.Right) ? 1 : Offset;

				Lines.Add(new Line(X * ScaleX, Y * ScaleY, (X + XOffset) * ScaleX, Y * ScaleY)); // Bottom
			}

			if (S.HasFlag(CellState.Left)) {
				YOffset = !HasFlag(X, Y, CellState.Top) ? 1 : Offset;

				Lines.Add(new Line((X) * ScaleX, Y * ScaleY, (X) * ScaleX, (Y + YOffset) * ScaleY)); // Left
			}

			if (S.HasFlag(CellState.Top)) {
				XOffset = !HasFlag(X, Y, CellState.Right) ? 1 : Offset;

				Lines.Add(new Line(X * ScaleX, (Y + YOffset) * ScaleY, (X + XOffset) * ScaleX, (Y + YOffset) * ScaleY)); // Top
			}

			if (S.HasFlag(CellState.Right)) {
				YOffset = !HasFlag(X, Y, CellState.Top) ? 1 : Offset;

				Lines.Add(new Line((X + XOffset) * ScaleX, Y * ScaleY, (X + XOffset) * ScaleX, (Y + YOffset) * ScaleY)); // Right
			}

			// Top missing
			if (!S.HasFlag(CellState.Bottom) && !S.HasFlag(CellState.Right) && HasFlag(X, Y - 1, CellState.Right))
				Lines.Add(new Line((X + OriginalOffset) * ScaleX, Y * ScaleY, (X + 1) * ScaleX, Y * ScaleY)); // Bottom

			// Bottom missing
			if (!S.HasFlag(CellState.Top) && !S.HasFlag(CellState.Right) && !HasFlag(X + 1, Y, CellState.Top))
				Lines.Add(new Line((X + OriginalOffset) * ScaleX, (Y + 1) * ScaleY, (X + 1) * ScaleX, (Y + 1) * ScaleY)); // Bottom

			// Left missing
			if (!S.HasFlag(CellState.Left) && HasFlag(X - 1, Y, CellState.Top) && HasFlag(X, Y + 1, CellState.Left))
				Lines.Add(new Line((X) * ScaleX, (Y + OriginalOffset) * ScaleY, (X) * ScaleX, (Y + YOffset) * ScaleY)); // Left

			// Right missing
			if (!S.HasFlag(CellState.Right) && HasFlag(X + 1, Y, CellState.Top) && HasFlag(X + 1, Y + 1, CellState.Bottom)
				&& !(HasFlag(X, Y + 1, CellState.Right) || HasFlag(X, Y + 1, CellState.Bottom)))
				Lines.Add(new Line((X + 1) * ScaleX, (Y + OriginalOffset) * ScaleY, (X + 1) * ScaleX, (Y + YOffset) * ScaleY)); // Left

			if (X == 4 && Y == 0) {
				Console.WriteLine(GetCell(X - 1, Y).GetStates());
				Console.WriteLine(S.GetStates());
				Console.WriteLine(GetCell(X, Y + 1).GetStates());
				Console.ReadLine();
			}

			//Lines.Add(new Line(X * ScaleX, Y * ScaleY, (X + 1) * ScaleX, Y * ScaleY)); // Bottom
			//Lines.Add(new Line((X) * ScaleX, Y * ScaleY, (X) * ScaleX, (Y + 1) * ScaleY)); // Left
			//Lines.Add(new Line(X * ScaleX, (Y + 1) * ScaleY, (X + 1) * ScaleX, (Y + 1) * ScaleY)); // Top
			//Lines.Add(new Line((X + 1) * ScaleX, Y * ScaleY, (X + 1) * ScaleX, (Y + 1) * ScaleY)); // Right

			// Old code
			/*if (S.HasFlag(CellState.Top)) {
				Lines.Add(new Line(new Vector(X * ScaleX, (Y) * ScaleY),
					new Vector((X + 1) * ScaleX, (Y) * ScaleY)));
			}
			if (S.HasFlag(CellState.Left)) {
				Lines.Add(new Line(new Vector(X * ScaleX, Y * ScaleY),
					new Vector(X * ScaleX, (Y + 1) * ScaleY)));
			}//*/

			return Lines.ToArray();
		}

		public Line[] Generate(double ScaleX, double ScaleY) {
			HashSet<Line> Lines = new HashSet<Line>();
			HashSet<Line> LinesProcessed = new HashSet<Line>();

			const int Size = 2;

			Bitmap Bmp = new Bitmap(Width * Size + 1, Height * Size + 1);
			Graphics BmpGraphics = Graphics.FromImage(Bmp);
			BmpGraphics.Clear(Color.White);
			BmpGraphics.Dispose();
			Color FillClr = Color.Black;

			Func<int, int, bool> GetPixel = (XX, YY) => {
				if (XX < 0 || XX >= Bmp.Width || YY < 0 || YY >= Bmp.Height)
					return true;
				Color Col = Bmp.GetPixel(XX, YY);
				if (Col.R == 0 && Col.G == 0 && Col.B == 0)
					return true;
				return false;
			};

			Action<int, int, bool> SetPixel = (XX, YY, B) => {
				if (XX < 0 || XX >= Bmp.Width || YY < 0 || YY >= Bmp.Height)
					return;
				Color Col = Color.Black;
				if (!B)
					Col = Color.White;
				Bmp.SetPixel(XX, YY, Col);
			};

			Action<int, int> PutLeft = (XX, YY) => {
				for (int h = 0; h < Size + 1; h++)
					SetPixel(XX * Size, YY * Size + h, true);
			};
			Action<int, int> PutBottom = (XX, YY) => {
				for (int h = 0; h < Size + 1; h++)
					SetPixel(XX * Size + h, YY * Size, true);
			};




			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					/*if (y + 1 == Height)
						SetPixel(x, y * Size, true);*/

					if (HasFlag(x, y, CellState.Left))
						PutLeft(x, y);
					if (HasFlag(x, y, CellState.Bottom))
						PutBottom(x, y);

					/*Line[] WallLines = GenerateWalls(x, y, ScaleX, ScaleY);
					for (int n = 0; n < WallLines.Length; n++)
						Lines.Add(WallLines[n]);*/

					/*if (x + 1 == Width)
						Lines.Add(new Line(new Vector((x + 1) * ScaleX, y * ScaleY),
							new Vector((x + 1) * ScaleX, (y + 1) * ScaleY)));
					if (y + 1 == Height)
						Lines.Add(new Line(new Vector(x * ScaleX, (y + 1) * ScaleY),
							new Vector((x + 1) * ScaleX, (y + 1) * ScaleY)));*/
				}
			}

			for (int y = 0; y < Bmp.Height; y++)
				for (int x = 0; x < Bmp.Width; x++)
					if (y + 1 == Bmp.Height || x + 1 == Bmp.Width)
						SetPixel(x, y, true);

			Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
			Bmp.Save("test.png", ImageFormat.Png);
			Environment.Exit(0);
			return Lines.ToArray();
		}
	}
}