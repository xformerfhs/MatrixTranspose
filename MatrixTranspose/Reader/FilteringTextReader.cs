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

using System.Collections.Generic;
using System.IO;

namespace Reader {
   /// <summary>
   /// Class to filter characters read from a text reader.
   /// </summary>
   public class FilteringTextReader : ChainableTextReader {
      // ******** Instance Variables ********

      /// <summary>
      /// Allowed characters for this reader.
      /// </summary>
      private readonly SortedSet<char> _allowedChars;

      /// <summary>
      /// Peeked character, if any.
      /// </summary>
      private int? _peekedChar;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;


      // ******** Constructors ********

      /// <summary>
      /// Creates a new instance of the <see cref="FilteringTextReader"/> class.
      /// </summary>
      /// <param name="innerReader">The inner text reader to wrap.</param>
      /// <param name="allowedChars">Array of characters that are allowed to be read.</param>
      public FilteringTextReader(TextReader innerReader, char[] allowedChars)
          : base(innerReader) {
         this._allowedChars = new SortedSet<char>(allowedChars);
      }


      // ******** Implementation of TextReader ********

      public override int Read() {
         // 1. Check if we have a peeked character and return it if available.
         if (_peekedChar.HasValue) {
            int result = _peekedChar.Value;
            _peekedChar = null;
            return result;
         }

         // 2. Read characters from the inner reader until we find an allowed character or reach the end.
         int ch;
         while ((ch = _innerReader.Read()) != -1) {
            if (_allowedChars.Contains((char)ch))
               return ch;
         }

         return ch;
      }

      public override int Peek() {
         // 1. Check if we have a peeked character and return it if available.
         if (_peekedChar.HasValue)
            return _peekedChar.Value;

         // 2. Read characters from the inner reader until we find an allowed character or reach the end.
         //    Remember it for Read or Peek calls.
         int ch;
         while ((ch = _innerReader.Read()) != -1) {
            if (_allowedChars.Contains((char)ch)) {
               _peekedChar = ch;
               return ch;
            }
         }

         return -1;
      }


      // ******** Implementation of IDisposable ********

      protected override void Dispose(bool disposing) {
         if (!_isDisposed) {
            if (disposing) {
               if (_peekedChar != null) {
                  _peekedChar = 0;
                  _peekedChar = null;
               }

               _allowedChars.Clear();
            }

            _isDisposed = true;
         }

         base.Dispose(disposing);
      }

   }
}
