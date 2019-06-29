using System;
using System.Collections.Generic;
using System.Text;

namespace LogTrack
{
	public struct TextFormat
	{
		public ConsoleColor Foreground { get; set; }
		public ConsoleColor BackGround { get; set; }

		public TextFormat(ConsoleColor foreground)
			: this(foreground, ConsoleColor.Black)
		{

		}

		public TextFormat(ConsoleColor foreground, ConsoleColor backGround)
		{
			Foreground = foreground;
			BackGround = backGround;
		}

		public void Apply()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = BackGround;
		}

		public void Reset()
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
		}
	}
}
