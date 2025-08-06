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
using System.IO;

namespace Reader {
   /// <summary>
   /// Class to read characters from a text reader and replace them with a mapping.
   /// </summary>
   public class MappingTextReader : ChainableTextReader {
      #region Instance variables
      /// <summary>
      /// Mapping of characters to their replacements.
      /// </summary>
      private readonly SortedDictionary<char, char[]> _mapping;

      /// <summary>
      /// Current replacement characters.
      /// </summary>
      private char[] _currentReplacement;

      /// <summary>
      /// Index of the next character to return from the current replacement.
      /// </summary>
      private int _replacementIndex;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the <see cref="MappingTextReader"/> class.
      /// </summary>
      /// <param name="innerReader">The inner text reader to wrap.</param>
      /// <param name="mapping">Mapping of characters to their replacements.</param>
      public MappingTextReader(TextReader innerReader, SortedDictionary<char, char[]> mapping)
         : base(innerReader) {
         _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));

         _currentReplacement = null;
         _replacementIndex = 0;
      }
      #endregion


      #region Implementation of TextReader
      public override int Read() {
         // 1. Are there still replacements available from the current mapping?
         if (_currentReplacement != null &&
             _replacementIndex < _currentReplacement.Length)
            return _currentReplacement[_replacementIndex++];

         // 2. Read next character from the inner reader until a mapped character is found.
         int ch;
         while ((ch = _innerReader.Read()) != -1) {
            char c = (char)ch;

            // Is the character mapped?
            if (_mapping.TryGetValue(c, out var replacements)) {
               _currentReplacement = replacements;
               _replacementIndex = 1;
               return replacements[0];
            }
         }

         return ch;
      }

      public override int Peek() {
         // 1. Return replacement character if available.
         if (_currentReplacement != null &&
            _replacementIndex < _currentReplacement.Length)
            return _currentReplacement[_replacementIndex];

         // 2. Read next character from the inner reader until a mapped character is found.
         int ch;
         while ((ch = _innerReader.Read()) != -1) {
            char c = (char)ch;

            if (_mapping.TryGetValue(c, out var replacements)) {
               // Because we map to several characters, we need to store the first replacement as the peeked character.
               _currentReplacement = replacements;
               _replacementIndex = 0;

               return replacements[0];
            }
         }

         return ch;
      }
      #endregion


      #region Implementation of IDisposable
      protected override void Dispose(bool disposing) {
         if (!_isDisposed)
            return;

         if (disposing) {
            _currentReplacement = null;
            _replacementIndex = 0;
         }

         base.Dispose(disposing);

         _isDisposed = true;
      }
      #endregion
   }
}
