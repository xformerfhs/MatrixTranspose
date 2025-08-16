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
 *    2025-08-15: V1.0.0: Created. fhs
 */

using LineEndingHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WriteHandling {
   /// <summary>
   /// Class that implements a composable text writer.
   /// </summary>
   public class ComposableTextWriter : TextWriter {
      #region Instance variables
      /// <summary>
      /// The composed text writer.
      /// </summary>
      private TextWriter _writer;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed = false;
      #endregion


      #region Public properties
      public override Encoding Encoding => _writer.Encoding;
      #endregion


      #region Public constructors
      /// <summary>
      /// Creates a new instance of <see cref="ComposableTextWriter"/> with the specified stream and encoding.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to wrap.</param>
      /// <param name="encoding">The <see cref="Encoding"/> to use in the writer.</param>
      /// <exception cref="ArgumentNullException">Thrown, if any parameter is <see langword="null"/>.</exception>
      /// <exception cref="ArgumentException">Thrown, if <paramref name="stream"/> is not writable.</exception>
      public ComposableTextWriter(Stream stream, Encoding encoding) {
         if (stream == null)
            throw new ArgumentNullException(nameof(stream));

         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

         if (!stream.CanWrite)
            throw new ArgumentException("Stream is not writable.", nameof(stream));

         _writer = new StreamWriter(stream, encoding);
      }

      /// <summary>
      /// Creates a new instance of <see cref="ComposableTextWriter"/> with a writable 
      /// <see cref="FileStream"/> of the specified file path and encoding.
      /// </summary>
      /// <param name="filePath">File path of the file to write.</param>
      /// <param name="encoding">The <see cref="Encoding"/> to use in the writer.</param>
      /// <exception cref="ArgumentNullException">Thrown, if any parameter is <see langword="null"/>.</exception>
      /// <exception cref="ArgumentException">Thrown, if <paramref name="stream"/> is not writable.</exception>
      public ComposableTextWriter(string filePath, Encoding encoding) :
         this(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read), encoding) { 
      }
      #endregion

      #region Pulic builder methods
      /// <summary>
      /// Adds a <see cref="LineEndingTextWriter"/> to the composition that normalizes line endings to the specified option.
      /// </summary>
      /// <param name="lineEndingOption">The <see cref="LineEndingHandler.Option"/> to use for lin ending normalization.</param>
      /// <returns>This <see cref="ComposableTextWriter"/>.</returns>
      public ComposableTextWriter WithNormalizedLineEndings(LineEndingHandler.Option lineEndingOption) {
         _writer = new LineEndingTextWriter(_writer, lineEndingOption);
         return this;
      }

      /// <summary>
      /// Adds a <see cref="GroupingWriter"/> to the composition that groups characters and limits line length.
      /// </summary>
      /// <param name="groupSize">Size of groups to write. 0 means no grouping.</param>
      /// <param name="maxLineLength">Maximum line length. 0 means no maximum line length.</param>
      /// <returns>This <see cref="ComposableTextWriter"/>.</returns>
      public ComposableTextWriter WithGroupedOutput(int groupSize, int maxLineLength) {
         _writer = new GroupingWriter(_writer, groupSize, maxLineLength);
         return this;
      }

      /// <summary>
      /// Adds a <see cref="UnmappingTextWriter"/> to the composition that replaces character sequences with their mapped characters.
      /// </summary>
      /// <param name="unsubstitutionMap">Mapping from character sequences to their replacement characters.</param>
      /// <returns>This <see cref="ComposableTextWriter"/>.</returns>
      public ComposableTextWriter WithUnsubstitution(in SortedDictionary<char[], char> unsubstitutionMap) {
         _writer = new UnmappingTextWriter(_writer, unsubstitutionMap);
         return this;
      }
      #endregion


      #region Public helper methods
      /// <summary>
      /// Writes the characters in the specified array to the composed text writer character by character.
      /// </summary>
      /// <param name="data">The data to write.</param>
      /// <param name="count">The number of characters to write.</param>
      public void WriteData(in char[] data, int count) {
         for (int i = 0; i < count; i++)
            _writer.Write(data[i]);
      }
      #endregion


      #region Implementation of TextWriter
      public override void Write(char value) {
         _writer.Write(value);
      }
      public override void Flush() {
         _writer.Flush();
      }
      #endregion


      #region Implementation of IDisposable
      protected override void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing && _writer != null) {
            _writer.Dispose();
            _writer = null;
         }

         _isDisposed = true;

         base.Dispose(disposing);
      }
      #endregion
   }
}
