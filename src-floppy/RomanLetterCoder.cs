using System.Collections.Generic;
using MX.Kitt.Utils.Coding;
using MX.Kitt.Utils.Extensions;

namespace Blokx.Kitt.Wedding
{
	public class RomanLetterCoder : AbstractTextCoder
	{
		// http://csharphelper.com/blog/2016/04/convert-to-and-from-roman-numerals-in-c/

		// Maps letters to numbers.
		private static Dictionary<char, int> CharValues = null;



		public override string EncodeText(string data)
		{
			// TODO: add option to throw exception to To() extension
			var num = data.To<int>();
			var roman = ArabicToRoman(num);
			return roman;
		}

		public override string DecodeText(string data)
		{
			var num = RomanToArabic(data);
			return num.ToString();
		}


		// Convert Roman numerals to an integer.
		public static int RomanToArabic(string roman)
		{
			// Initialize the letter map.
			if (CharValues == null)
			{
				CharValues = new Dictionary<char, int>();
				CharValues.Add('I', 1);
				CharValues.Add('V', 5);
				CharValues.Add('X', 10);
				CharValues.Add('L', 50);
				CharValues.Add('C', 100);
				CharValues.Add('D', 500);
				CharValues.Add('M', 1000);
			}

			if (roman.Length == 0) return 0;
			roman = roman.ToUpper();

			// See if the number begins with (.
			if (roman[0] == '(')
			{
				// Find the closing parenthesis.
				var pos = roman.LastIndexOf(')');

				// Get the value inside the parentheses.
				var part1 = roman.Substring(1, pos - 1);
				var part2 = roman.Substring(pos + 1);
				var result = 1000 * RomanToArabic(part1) + RomanToArabic(part2);
				return result;
			}

			// The number doesn't begin with (.
			// Convert the letters' values.
			var total = 0;
			var last_value = 0;
			for (var i = roman.Length - 1; i >= 0; i--)
			{
				var new_value = CharValues[roman[i]];

				// See if we should add or subtract.
				if (new_value < last_value)
					total -= new_value;
				else
				{
					total += new_value;
					last_value = new_value;
				}
			}

			// Return the result.
			return total;
		}

		// Map digits to letters.
		private static string[] ThousandsLetters = 
			{ "", "M", "MM", "MMM" };
		private static string[] HundretsLetters =
			{ "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
		private static string[] TensLetters =
			{ "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
		private static string[] OnesLetters =
			{ "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

		// Convert Roman numerals to an integer.
		public static string ArabicToRoman(int arabic)
		{
			// See if it's >= 4000.
			if (arabic >= 4000)
			{
				// Use parentheses.
				int thou = arabic / 1000;
				arabic %= 1000;
				return "(" + ArabicToRoman(thou) + ")" +
				       ArabicToRoman(arabic);
			}

			// Otherwise process the letters.
			string result = "";

			// Pull out thousands.
			int num;
			num = arabic / 1000;
			result += ThousandsLetters[num];
			arabic %= 1000;

			// Handle hundreds.
			num = arabic / 100;
			result += HundretsLetters[num];
			arabic %= 100;

			// Handle tens.
			num = arabic / 10;
			result += TensLetters[num];
			arabic %= 10;

			// Handle ones.
			result += OnesLetters[arabic];

			return result;
		}
	}
}
