/*
 * SPDX-FileCopyrightText: 2025 Frank Schwab
 *
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * You may not use this file except in compliance with the License.
 *
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Author: Frank Schwab
 *
 * Change history:
 *    2025-08-02: V1.0.0: Created. fhs
 */

using ArrayHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alphabet {
   /// <summary>
   /// Class to handle a substitution alphabet.
   /// </summary>
   public class SubstitutionAlphabet {
      // ******** Public Properties ********

      /// <summary>
      /// Character to combination mapping.
      /// </summary>
      public SortedDictionary<char, char[]> CharToCombination { get; private set; }

      /// <summary>
      /// Combination to character mapping.
      /// </summary>
      public SortedDictionary<char[], char> CombinationToChar { get; private set; }

      /// <summary>
      /// Number of places in the substitution.
      /// </summary>
      public int NumPlaces { get; private set; }


      // ******** Constructors ********

      /// <summary>
      /// Creates a new instance of the SubstitutionAlphabet class.
      /// </summary>
      /// <param name="alphabetDescription">The <see cref="AlphabetDescription"/> to construct the alphabet from.</param>
      /// <param name="password">The password to use for the substitution maps.</param>
      /// <param name="substitutionChars">The substitution characters.</param>
      /// <exception cref="ArgumentException">Thrown, if the <paramref name="alphabetDescription"/> does not match <paramref name="substitutionChars"/>.</exception>
      public SubstitutionAlphabet(
         in AlphabetDescription alphabetDescription,
         in string password,
         in char[] substitutionChars) {
         byte numCharacters = alphabetDescription.NumCharacters;

         if (numCharacters != substitutionChars.Length)
            throw new ArgumentException($"Number of substitution characters ({substitutionChars.Length}) does not match alphabet description ({numCharacters})");

         byte numPlaces = alphabetDescription.NumPlaces;

         CharToCombination = new SortedDictionary<char, char[]>();
         CombinationToChar = new SortedDictionary<char[], char>(new ArrayComparer<char>());

         int i = 0;
         foreach (var c in KeyFromPassword(password, numPlaces, numCharacters, alphabetDescription.AlphabetSet.ToArray())) {
            char[] combination = new char[numPlaces];
            int n = i;
            for (int j = numPlaces - 1; j >= 0; j--) {
               int d = n / numCharacters;
               int r = n - (d * numCharacters);
               n = d;
               combination[j] = substitutionChars[r];
            }

            CharToCombination[c] = combination;
            CombinationToChar[combination] = c;
            i++;
         }

         NumPlaces = numPlaces;
      }

      // ******** Public Methods ********

      /// <summary>
      /// Creates a random set of substitution characters.
      /// </summary>
      /// <param name="numCharacters">Number of characters to generate.</param>
      /// <returns>Character array with <paramref name="numCharacters"/> random elements.</returns>
      public static char[] GetRandomSubstitutionCharacters(byte numCharacters) {
         if (numCharacters == 0)
            throw new ArgumentException("Number of characters must not be 0", nameof(numCharacters));

         SortedSet<char> alphabetSet = new SortedSet<char>();
         Random rnd = new Random();
         while ((byte)alphabetSet.Count < numCharacters) {
            char c = (char)('A' + rnd.Next(0, 26));
            // No need to check if character is already in set, as the Add method
            // simply will not add it, if it is already present.
            alphabetSet.Add(c);
         }

         return alphabetSet.ToArray();
      }


      // ******** Private Methods ********

      /// <summary>
      /// Builds a character array from the given password and alphabet description that contains
      /// first the characters from the password and then the remaining characters from the alphabet.
      /// This array is then used to build the substitution matrix.
      /// </summary>
      private static char[] KeyFromPassword(in string password, byte numPlaces, byte numCharacters, in char[] alphabetCharacters) {
         if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password must not be null or empty.", nameof(password));

         int count = (int)Math.Pow(numCharacters, numPlaces);

         char[] result = new char[count];
         int resultIndex = 0;
         char lastChar = '\0';
         SortedSet<char> isPresent = new SortedSet<char>();
         foreach (char c in password) {
            if (isPresent.Contains(c))
               continue; // Skip duplicates

            result[resultIndex++] = c;
            isPresent.Add(c);
            lastChar = c;
         }

         int i = GetStartIndex(lastChar, isPresent, alphabetCharacters);

         // If we have already placed all characters, we can return the result as it is.
         if (i < 0)
            return result;

         // Fill the rest of the result with the unused characters from the alphabet.
         while (resultIndex < count) {
            char c = alphabetCharacters[i++];

            if (i >= alphabetCharacters.Length)
               i = 0; // Wrap around to the start of the alphabet

            if (isPresent.Contains(c))
               continue; // Skip duplicates

            result[resultIndex++] = c;
         }

         return result;
      }

      /// <summary>
      /// Gets the start index for filling the result array with the remaining characters from the alphabet.
      /// </summary>
      /// <param name="lastChar">Last used character from password.</param>
      /// <param name="isPresent">List of characters that have already been filled in.</param>
      /// <param name="alphabetCharacters">The characters of the alphabet.</param>
      /// <returns>The start index of the first character in <paramref name="alphabetCharacters"/> 
      /// that has a higher value than <paramref name="lastChar"/>. <c>-1</c>, if the password already
      /// contained all characters in <paramref name="alphabetCharacters"/>.</returns>
      private static int GetStartIndex(char lastChar, in SortedSet<char> isPresent, in char[] alphabetCharacters) {
         for (int i = 0; i < alphabetCharacters.Length; i++) {
            char c = alphabetCharacters[i];

            if (isPresent.Contains(c))
               continue; // Skip duplicates

            if (c > lastChar)
               return i;

            // No need to remember characters in "isPresent". It will not be used after this method.
         }

         return -1; // This means that the password contained all valid characters.
      }
   }
}
