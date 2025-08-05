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

namespace Alphabet {
   /// <summary>
   /// Class that describes the characters that can be matrix encoded.
   /// </summary>
   public class AlphabetDescription {
      // ******** Private Constants ********

      /// <summary>
      /// Format string for exceptions when a parameter is invalid.
      /// </summary>
      private const string FormatInvalidValue = "Number of {0} must not be 0 or larger than {1}";

      /// <summary>
      /// Maximum value for places and characters.
      /// </summary>
      private const byte MaxValue = 9;


      // ******** Public Properties ********

      /// <summary>
      /// Number of places for one encoding.
      /// </summary>
      public byte NumPlaces { get; }

      /// <summary>
      /// Number of characters in the encoding.
      /// </summary>
      public byte NumCharacters { get; }

      /// <summary>
      /// AlphabetSet used for encoding.
      /// </summary>
      public SortedSet<char> AlphabetSet { get; }

      /// <summary>
      /// <c>true</c> if 'J' has to be treated as 'I', <c>false</c> otherwise.
      /// </summary>
      public bool TreatJAsI { get; }

      /// <summary>
      /// <c>true</c> if all characters have to be converted to upper case, <c>false</c> otherwise.
      /// </summary>
      public bool ToUpper { get; }


      // ******** Constructor ********

      /// <summary>
      /// Creates a new instance of <see cref="AlphabetDescription"/>.
      /// </summary> 
      /// <exception cref="ArgumentException">Thrown when a parameter is invalid.</exception>"
      public AlphabetDescription(in byte numPlaces, in byte numCharacters, in string alphabet) {
         if (numPlaces == 0 || numPlaces > MaxValue)
            throw new ArgumentOutOfRangeException(string.Format(FormatInvalidValue, @"places", MaxValue));

         if (numCharacters == 0 || numCharacters > MaxValue)
            throw new ArgumentOutOfRangeException(string.Format(FormatInvalidValue, @"characters", MaxValue));

         if (string.IsNullOrEmpty(alphabet))
            throw new ArgumentException(@"Alphabet must not be null or empty");

         int combinationCount = (int)Math.Pow((double)numCharacters, (double)numPlaces);
         if (combinationCount < alphabet.Length)
            throw new ArgumentException($"{numPlaces} places of {numCharacters} characters generate {combinationCount} combinations which is less than the alphabet length {alphabet.Length}");

         NumPlaces = numPlaces;
         NumCharacters = numCharacters;

         var alphabetSet = new SortedSet<char>(alphabet);
         AlphabetSet = alphabetSet;

         TreatJAsI = !alphabetSet.Contains('J');
         ToUpper = !alphabetSet.Contains('a');
      }
   }
}
