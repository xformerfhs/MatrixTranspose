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

using System;
using System.Collections.Generic;

namespace AlphabetHandling {
   /// <summary>
   /// Class that generates <see cref="AlphabetDescription"/> objects based on the number of places and characters.
   /// </summary>
   public static class AlphabetFactory {
      #region Private constants
      /// <summary>
      /// Minimum allowed number of places.
      /// </summary>
      private const byte MinNumPlaces = 2;

      /// <summary>
      /// Maximum allowed number of places.
      /// </summary>
      private const byte MaxNumPlaces = 4;

      /// <summary>
      /// Minimum allowed number of characters.
      /// </summary>
      private const byte MinNumCharacters = 3;

      /// <summary>
      /// Maximum allowed number of characters.
      /// </summary>
      private const byte MaxNumCharacters = 6;

      /// <summary>
      /// Format string for the exception message when a parameter is invalid.
      /// </summary>
      private const string FormatErrorInvalidValue = "Number of {0} must lie between {1} and {2}, but is {3}";

      /// <summary>
      /// List of known alphabets.
      /// </summary>
      /// <remarks>
      /// The key is the number of places times 10 plus the number of characters.
      /// </remarks>
      private static readonly SortedDictionary<byte, AlphabetDescription> alphabets = new SortedDictionary<byte, AlphabetDescription> {
         { 25, new AlphabetDescription(2, 5, @"ABCDEFGHIKLMNOPQRSTUVWXYZ") },
         { 33, new AlphabetDescription(3, 3, @" ABCDEFGHIJKLMNOPQRSTUVWXYZ") },
         { 26, new AlphabetDescription(2, 6, @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") },
         { 34, new AlphabetDescription(3, 4, @" .ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") },
         { 43, new AlphabetDescription(4, 3, " \n.,;:-!?'\"%&+#*/()ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") },
         { 36, new AlphabetDescription(3, 6, " \n.,;:-!?'\"§%&+#*/()|°^~<=>–—«»¡¿¥£¤€$¢ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÀÁÂÃÄÅĄÆÇĆČÐÞĎÈÉÊËĘĚÌÍÎÏŁÑŃŇÒÓÔÕÖØŘŚŠŤÙÚÛÜŮÝŸŹŻŽàáâãäåąæçćčðþďèéêëęěìíîïłñńňòóôõöøřśšßťùúûüůýÿźżž") }
      };
      #endregion


      #region Public methods
      /// <summary>
      /// Get an <see cref="AlphabetDescription"/> for the given number of places and characters.
      /// </summary>
      /// <param name="numPlaces">Number of places.</param>
      /// <param name="numCharacters">Number of characters.</param>
      /// <returns><see cref="AlphabetDescription"/> for the given <paramref name="numCharacters"/> and <paramref name="numPlaces"/>.</returns>
      /// <exception cref="ArgumentException">Thrown, if any parameter is invalid or there is no
      /// <see cref="AlphabetDescription"/> for this parameter combination.</exception>
      public static AlphabetDescription GetAlphabet(in byte numPlaces, in byte numCharacters) {
         if (numPlaces < MinNumPlaces || numPlaces > MaxNumPlaces)
            throw new ArgumentOutOfRangeException(String.Format(FormatErrorInvalidValue, "places", MinNumPlaces, MaxNumPlaces, numPlaces), nameof(numPlaces));

         if (numCharacters < MinNumCharacters || numCharacters > MaxNumCharacters)
            throw new ArgumentOutOfRangeException(String.Format(FormatErrorInvalidValue, "characters", MinNumCharacters, MaxNumCharacters, numCharacters), nameof(numPlaces));

         byte key = (byte)(numPlaces * 10 + numCharacters);
         if (alphabets.TryGetValue(key, out var alphabet))
            return alphabet;

         throw new ArgumentException($"No alphabet found for {numPlaces} places and {numCharacters} characters");
      }
      #endregion
   }
}
