using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CatiaNET;

namespace TSBLogo {
	class Program {
		static string Prompt(string P) {
			Console.Write(P);
			return Console.ReadLine();
		}

		static int PromptInt(string P) {
			return int.Parse(Prompt(P));
		}

		[STAThread]
		static void Main(string[] args) {
			Console.Title = "TSB Logo";

			//Prompt("Press enter to generate maze ... ");
			Catia.GenerateMaze(300 / 2, 500 / 2, 15, 25);
			//Catia.GenerateMaze(PromptInt("Width: "), PromptInt("Height: "), PromptInt("Grid X: "), PromptInt("Grid Y: "));
			/*Prompt("Press enter to generate logo ... ");
			Catia.GenerateLogo(90, 250, 100, 16);*/

			Console.WriteLine("Done!");
			//Console.ReadLine();
		}
	}
}