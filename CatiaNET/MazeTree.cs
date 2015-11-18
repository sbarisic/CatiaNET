using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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
	}

	[Flags]
	public enum CellState {
		Bottom = 1,
		Right = 2,
		Top = 4,
		Left = 8,
		Visited = 128,
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
			float OriginalOffset = 0.7f;
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
				Lines.Add(new Line((X + OriginalOffset) * ScaleX, (Y + 1) * ScaleY, (X + 1) * ScaleX, (Y + 1) * ScaleY)); // Bottom*/

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

			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					Line[] WallLines = GenerateWalls(x, y, ScaleX, ScaleY);
					for (int n = 0; n < WallLines.Length; n++)
						Lines.Add(WallLines[n]);

					/*if (x + 1 == Width)
						Lines.Add(new Line(new Vector((x + 1) * ScaleX, y * ScaleY),
							new Vector((x + 1) * ScaleX, (y + 1) * ScaleY)));
					if (y + 1 == Height)
						Lines.Add(new Line(new Vector(x * ScaleX, (y + 1) * ScaleY),
							new Vector((x + 1) * ScaleX, (y + 1) * ScaleY)));*/
				}
			}

			/*foreach (var L in Lines)
				//LinesProcessed.Add(new Line(L.Start.HashOffset(), L.End.HashOffset()));
				LinesProcessed.Add(L);
			return LinesProcessed.ToArray();*/
			return Lines.ToArray();
		}
	}
}