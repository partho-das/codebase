using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LinqQuiz.Library
{
    public static class Quiz
    {
        /// <summary>
        /// Returns all even numbers between 1 and the specified upper limit.
        /// </summary>
        /// <param name="exclusiveUpperLimit">Upper limit (exclusive)</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="exclusiveUpperLimit"/> is lower than 1.
        /// </exception>
        public static int[] GetEvenNumbers(int exclusiveUpperLimit)
        {
            if (exclusiveUpperLimit <= 0) throw new ArgumentOutOfRangeException();
            return GenerateNumber(exclusiveUpperLimit).Where(x => x % 2 == 0).ToArray();
        }

        public static IEnumerable<int> GenerateNumber(int x)
        {
            for (int i = 2; i < x; i++) yield return i;
        }

        /// <summary>
        /// Returns the squares of the numbers between 1 and the specified upper limit 
        /// that can be divided by 7 without a remainder (see also remarks).
        /// </summary>
        /// <param name="exclusiveUpperLimit">Upper limit (exclusive)</param>
        /// <exception cref="OverflowException">
        ///     Thrown if the calculating the square results in an overflow for type <see cref="System.Int32"/>.
        /// </exception>
        /// <remarks>
        /// The result is an empty array if <paramref name="exclusiveUpperLimit"/> is lower than 1.
        /// The result is in descending order.
        /// </remarks>
        public static int[] GetSquares(int exclusiveUpperLimit)
        {
                var result = GenerateNumber(exclusiveUpperLimit).OrderByDescending(x => x).Where(x => x % 7 == 0).Select(x =>
                {
                    checked
                    {
                        try
                        {
                            return x * x;
                        }
                        catch (OverflowException)
                        {
                            throw new OverflowException();
                        }
                    }
                }).ToArray();
                return result;

            }

        /// <summary>
        /// Returns a statistic about families.
        /// </summary>
        /// <param name="families">Families to analyze</param>
        /// <returns>
        /// Returns one statistic entry per family in <paramref name="families"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="families"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// <see cref="FamilySummary.AverageAge"/> is set to 0 if <see cref="IFamily.Persons"/>
        /// in <paramref name="families"/> is empty.
        /// </remarks>
        public static FamilySummary[] GetFamilyStatistic(IReadOnlyCollection<IFamily> families)
        {
            if(families == null) throw new ArgumentNullException();
            var result = families.Select(x => new FamilySummary(){
                FamilyID = x.ID,
                NumberOfFamilyMembers = x.Persons.Count,
                AverageAge = x.Persons.Count == 0 ? 0 : x.Persons.Sum(p => p.Age) / x.Persons.Count
            }).ToArray();
            return result;
        }

        /// <summary>
        /// Returns a statistic about the number of occurrences of letters in a text.
        /// </summary>
        /// <param name="text">Text to analyze</param>
        /// <returns>
        /// Collection containing the number of occurrences of each letter (see also remarks).
        /// </returns>
        /// <remarks>
        /// Casing is ignored (e.g. 'a' is treated as 'A'). Only letters between A and Z are counted;
        /// special characters, numbers, whitespaces, etc. are ignored. The result only contains
        /// letters that are contained in <paramref name="text"/> (i.e. there must not be a collection element
        /// with number of occurrences equal to zero.
        /// </remarks>
        public static (char letter, int numberOfOccurrences)[] GetLetterStatistic(string text)
        {
            return text.GroupBy(ch =>
            
                ch switch
                {
                    'A' or 'a' => 'A',
                    'B' or 'b' => 'B',
                    'C' or 'c' => 'C',
                    'D' or 'd' => 'D',
                    'E' or 'e' => 'E',
                    'F' or 'f' => 'F',
                    'G' or 'g' => 'G',
                    'H' or 'h' => 'H',
                    'I' or 'i' => 'I',
                    'J' or 'j' => 'J',
                    'K' or 'k' => 'K',
                    'L' or 'l' => 'L',
                    'M' or 'm' => 'M',
                    'N' or 'n' => 'N',
                    'O' or 'o' => 'O',
                    'P' or 'p' => 'P',
                    'Q' or 'q' => 'Q',
                    'R' or 'r' => 'R',
                    'S' or 's' => 'S',
                    'T' or 'T' => 'T',
                    'U' or 'U' => 'U',
                    'V' or 'v' => 'V',
                    'W' or 'w' => 'W',
                    'X' or 'x' => 'X',
                    'Y' or 'y' => 'Y',
                    'Z' or 'z' => 'Z',
                    _ => '#'
                })
                .Where(g => g.Key >= 'A' && g.Key <= 'Z')
                .Select(g => (letter: g.Key, numberOfOccurrences: g.Count())).ToArray();
        }
    }
}
