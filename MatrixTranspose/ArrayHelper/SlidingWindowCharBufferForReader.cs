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

namespace ArrayHelper {
   /// <summary>
   /// Class to manage a sliding window character buffer for a <see cref="TextReader"/>.
   /// </summary>
   public class SlidingWindowCharBufferForReader : IDisposable {
      #region Instance variables
      /// <summary>
      /// Buffer to hold the characters.
      /// </summary>
      private readonly char[] _buffer;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Public properties
      /// <summary>
      /// Length of the current content in the buffer.
      /// </summary>
      public int Length { get; private set; }

      /// <summary>
      /// The raw character buffer for operations with the buffer.
      /// </summary>
      public char[] RawBuffer => _buffer;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of <see cref="SlidingWindowCharBufferForReader"/> with the specified capacity.
      /// </summary>
      /// <param name="capacity">Number of characters in the window.</param>
      /// <exception cref="ArgumentOutOfRangeException">Thrown, if the capacity is less or equal 0.</exception>
      public SlidingWindowCharBufferForReader(int capacity) {
         if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be > 0");

         _buffer = new char[capacity];
         Length = 0;
      }
      #endregion


      #region Public methods
      /// <summary>
      /// Fills the buffer.
      /// </summary>
      /// <param name="reader">TextReader to read from.</param>
      public int FillFrom(TextReader reader) {
         Length = reader.Read(_buffer, 0, _buffer.Length);
         return Length;
      }

      /// <summary>
      /// Shifts the content to the left by <paramref name="count"/> and reads <paramref name="count"/> characters from <paramref name="reader"/>.
      /// </summary>
      /// <param name="count">Number of characters to shift and fill.</param>
      /// <param name="reader">TextReader to read from.</param>
      public bool ShiftAndFill(int count, TextReader reader) {
         if (Length <= count) {
            Length = 0;
            return false;
         }

         Array.Copy(_buffer, count, _buffer, 0, Length - count);

         int read = reader.Read(_buffer, Length - count, count);
         Length = Length - count + read;

         return Length > 0;
      }
      #endregion


      #region Implementation of IDisposable
      protected virtual void Dispose(bool disposing) {
         if (_isDisposed)
            return;
         
         if (disposing) {
            Array.Clear(_buffer, 0, _buffer.Length);
         }

         _isDisposed = true;
      }

      // // Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~SlidingWindowCharBufferForReader()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose() {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
      #endregion
   }
}
