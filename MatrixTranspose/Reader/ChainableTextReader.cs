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
 *    2025-08-11: V1.1.0: Add protected constant "NoMoreCharacters". fhs
 */

using System;
using System.IO;

namespace ReadHandling {
   /// <summary>
   /// Base class for a chainable text reader.
   /// </summary>
   public abstract class ChainableTextReader : TextReader {
      #region Protected constants
      /// <summary>
      /// "No More Characters" (NMC) character.
      /// </summary>
      protected const int NoMoreCharacters = -1;
      #endregion


      #region Instance variables
      /// <summary>
      /// The inner text reader that this chainable text reader wraps.
      /// </summary>
      protected TextReader _innerReader;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the <see cref="ChainableTextReader"/> class.
      /// </summary>
      /// <param name="innerReader">The inner text reader to wrap.</param>
      protected ChainableTextReader(TextReader innerReader) {
         _innerReader = innerReader ?? throw new ArgumentNullException(nameof(innerReader));
      }
      #endregion


      #region Implementation of IDisposable
      protected override void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing) {
            _innerReader.Dispose();
            _innerReader = null;
         }

         base.Dispose(disposing);

         _isDisposed = true;
      }
      #endregion
   }
}
