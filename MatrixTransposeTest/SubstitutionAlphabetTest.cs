using Alphabet;
using ArrayHelper;
using System.Collections;

namespace MatrixTransposeTest {
   [TestClass]
   public sealed class SubstitutionAlphabetTest {
      [TestMethod]
      public void SimpleTest() {
         var alphabet = AlphabetFactory.GetAlphabet(2, 5);
         var substitution = new SubstitutionAlphabet(alphabet, @"OSCARFUELLTDASXYLOPHONMITQUARK", "ADFGV".ToCharArray());
         SortedDictionary<char, char[]> expectedCharToSubstitution = new() {
            { 'A', new[] { 'A', 'G' } },
            { 'B', new[] { 'V', 'G' } },
            { 'C', new[] { 'A', 'F' } },
            { 'D', new[] { 'F', 'A' } },
            { 'E', new[] { 'D', 'F' } },
            { 'F', new[] { 'D', 'A' } },
            { 'G', new[] { 'V', 'V' } },
            { 'H', new[] { 'F', 'V' } },
            { 'I', new[] { 'G', 'F' } },
            { 'K', new[] { 'G', 'V' } },
            { 'L', new[] { 'D', 'G' } },
            { 'M', new[] { 'G', 'D' } },
            { 'N', new[] { 'G', 'A' } },
            { 'O', new[] { 'A', 'A' } },
            { 'P', new[] { 'F', 'G' } },
            { 'Q', new[] { 'G', 'G' } },
            { 'R', new[] { 'A', 'V' } },
            { 'S', new[] { 'A', 'D' } },
            { 'T', new[] { 'D', 'V' } },
            { 'U', new[] { 'D', 'D' } },
            { 'V', new[] { 'V', 'A' } },
            { 'W', new[] { 'V', 'D' } },
            { 'X', new[] { 'F', 'D' } },
            { 'Y', new[] { 'F', 'F' } },
            { 'Z', new[] { 'V', 'F' } },
         };
         CompareCharToCharArray(expectedCharToSubstitution, substitution.CharToCombination);
         SortedDictionary<char[], char> expectedCombinationToChar = InvertSortedDictionary(expectedCharToSubstitution);
         CompareCharArrayToChar(expectedCombinationToChar, substitution.CombinationToChar);
      }


      // ******** Private Helper Methods ********

      /// <summary>
      /// Inverts a sorted dictionary where the keys are arrays and the values are single items.
      /// </summary>
      /// <typeparam name="T">Type of <see cref="SortedDictionary{TKey, TValue}"/> data.</typeparam>
      /// <param name="dictionary"><see cref="SortedDictionary{TKey, TValue}"> to invert.</param>
      /// <returns>Inverted <paramref name="dictionary"/>.</returns>
      private static SortedDictionary<T[], T> InvertSortedDictionary<T>(SortedDictionary<T, T[]> dictionary) {
         var inverted = new SortedDictionary<T[], T>(new ArrayComparer<T>());
         foreach (var kvp in dictionary)
            inverted[kvp.Value] = kvp.Key;

         return inverted;
      }

      /// <summary>
      /// Compares two dictionaries where the keys are characters and the values are character arrays.
      /// </summary>
      /// <remarks>
      /// Fails assertion if the dictionaries differ in count or if any key-value pair does not match.
      /// </remarks>
      /// <param name="expected">Expected dictionary.</param>
      /// <param name="actual">Actual dictionary.</param>
      private static void CompareCharToCharArray(SortedDictionary<char, char[]> expected, SortedDictionary<char, char[]> actual) {
         CheckCount(expected, actual);

         var comparer = new ArrayComparer<char>();

         foreach (var kvp in expected) {
            if (!actual.TryGetValue(kvp.Key, out var actualValue))
               Assert.Fail($"Key '{kvp.Key}' not found in actual dictionary.");

            if (comparer.Compare(kvp.Value, actualValue) != 0)
               Assert.Fail($"Values for key '{kvp.Key}' differ. Expected: {new string(kvp.Value)}, Actual: {new string(actualValue)}.");
         }
      }

      /// <summary>
      /// Compares two dictionaries where the keys are character arrays and the values are characters.
      /// </summary>
      /// <remarks>
      /// Fails assertion if the dictionaries differ in count or if any key-value pair does not match.
      /// </remarks>
      /// <param name="expected">Expected dictionary.</param>
      /// <param name="actual">Actual dictionary.</param>
      private static void CompareCharArrayToChar(SortedDictionary<char[], char> expected, SortedDictionary<char[], char> actual) {
         CheckCount(expected, actual);

         foreach (var kvp in expected) {
            if (!actual.TryGetValue(kvp.Key, out var actualValue))
               Assert.Fail($"Key '{new string(kvp.Key)}' not found in actual dictionary.");

            if (kvp.Value != actualValue)
               Assert.Fail($"Values for key '{new string(kvp.Key)}' differ. Expected: {kvp.Value}, Actual: {actualValue}.");
         }
      }

      /// <summary>
      /// Checks if the counts of two dictionaries are equal.
      /// </summary>
      /// <param name="expected">Expected dictionary.</param>
      /// <param name="actual">Actual dictionary.</param>
      private static void CheckCount(IDictionary expected, IDictionary actual) {
         if (expected.Count != actual.Count)
            Assert.Fail($"Dictionary counts differ. Expected: {expected.Count}, Actual: {actual.Count}.");
      }
   }
}
