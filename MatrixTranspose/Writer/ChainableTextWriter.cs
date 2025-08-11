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
using System.IO;
using System.Text;

namespace WriteHandling {
   /// <summary>
   /// Base class for text writers that can be chained together.
   /// </summary>
   public abstract class ChainableTextWriter : TextWriter {
      #region Instance variables
      /// <summary>
      /// Text writer to which this writer writes.
      /// </summary>
      protected TextWriter _innerWriter;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of <see cref="ChainableTextWriter"/>.
      /// </summary>
      protected ChainableTextWriter(TextWriter innerWriter) {
         _innerWriter = innerWriter ?? throw new ArgumentNullException(nameof(innerWriter));
      }
      #endregion


      #region Public Attributes
      /// <summary>
      /// Encoding used by the inner writer.
      /// </summary>
      public override Encoding Encoding => _innerWriter.Encoding;
      #endregion


      #region Implementation of IDisposable
      protected override void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing) {
            _innerWriter?.Dispose();
            _innerWriter = null;
         }

         base.Dispose(disposing);

         _isDisposed = true;
      }
      #endregion
   }
}
