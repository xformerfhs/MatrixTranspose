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

using EncodingHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReadHandling {
   /// <summary>
   /// Class that implements a composable text reader.
   /// </summary>
   public class ComposableTextReader : TextReader {
      #region Instance variables
      /// <summary>
      /// The composed text reader.
      /// </summary>
      private TextReader _reader;

      /// <summary>
      /// The maximum number characters that replace a single character.
      /// </summary>
      private int _maxSubstitutionCount = 1;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed = false;
      #endregion


      #region Public attributes
      /// <summary>
      /// Gets the current character encoding used by the reader.
      /// </summary>
      public Encoding CurrentEncoding { get; }
      public bool HasBom { get; }
      #endregion


      #region Constructors
      /// <summary>
      /// Initializes a new instance of the <see cref="ComposableTextReader"/> class using the specified stream and
      /// encoding.
      /// </summary>
      /// <remarks>
      /// This constructor creates a <see cref="StreamReader"/> internally to read from the
      /// provided stream using the specified encoding. The <see cref="CurrentEncoding"/> property is initialized based
      /// on the encoding used by the <see cref="StreamReader"/>.
      /// </remarks>
      /// <param name="stream">The input <see cref="Stream"/> to read from.</param>
      /// <param name="encoding">The character encoding to use for interpreting the stream.</param>
      public ComposableTextReader(Stream stream, Encoding encoding) {
         (HasBom, CurrentEncoding) = GetBomAndEncoding(stream, encoding);

         _reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: false);
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ComposableTextReader"/> class using the specified file path and
      /// text encoding.
      /// </summary>
      /// <param name="filePath">The path to the file to be read. Must not be <see langword="null"/> or empty.</param>
      /// <param name="encoding">The character encoding to use when reading the file. Must not be <see langword="null"/>.</param>
      public ComposableTextReader(string filePath, Encoding encoding) :
         this(File.OpenRead(filePath), encoding) {
      }
      #endregion


      #region Pulic builder methods

      /// <summary>
      /// Configures the <see cref="ComposableTextReader"/> to filter input, allowing only the specified characters.
      /// </summary>
      /// <param name="allowedChars">An array of characters that are permitted in the input. All other characters will be excluded.</param>
      /// <returns>The current <see cref="ComposableTextReader"/> instance with filtering applied, enabling method chaining.</returns>
      public ComposableTextReader WithFiltering(in char[] allowedChars) {
         _reader = new FilteringTextReader(_reader, allowedChars);
         return this;
      }

      /// <summary>
      /// Configures the <see cref="ComposableTextReader"/> to normalize line endings in the underlying text.
      /// </summary>
      /// <remarks>
      /// This method wraps the current text reader with a <see
      /// cref="LineEndingNormalizingTextReader"/>,  ensuring that all line endings are normalized to a consistent
      /// format.  Subsequent operations on the <see cref="ComposableTextReader"/> will use the normalized
      /// text.
      /// </remarks>
      /// <returns>The current <see cref="ComposableTextReader"/> instance with line ending normalization applied.</returns>
      public ComposableTextReader WithNormalizedLineEnding() {
         _reader = new LineEndingNormalizingTextReader(_reader);
         return this;
      }

      /// <summary>
      /// Configures the <see cref="ComposableTextReader"/> to use the specified character mapping for text
      /// substitution.
      /// </summary>
      /// <remarks>
      /// The provided mapping determines how characters in the input text are substituted during
      /// reading. The maximum substitution length is calculated based on the longest replacement array in the
      /// mapping.
      /// </remarks>
      /// <param name="mapping">A sorted dictionary where each key represents a character to be replaced, and the corresponding value is an
      /// array of characters that will replace the key during text processing.</param>
      /// <returns>The current instance of <see cref="ComposableTextReader"/>, allowing for method chaining.</returns>
      public ComposableTextReader WithMapping(in SortedDictionary<char, char[]> mapping) {
         _reader = new MappingTextReader(_reader, mapping);
         _maxSubstitutionCount = mapping.Values
            .Select(replacement => replacement.Length)
            .Max();
         return this;
      }

      /// <summary>
      /// Applies a character transformation function to the text being read.
      /// </summary>
      /// <remarks>
      /// The specified transformation function is applied to each character read from the
      /// underlying text source. This method modifies the behavior of the reader by wrapping it with a transformation
      /// layer.
      /// </remarks>
      /// <param name="transformer">A function that takes a character as input and returns the transformed character.</param>
      /// <returns>The current <see cref="ComposableTextReader"/> instance with the transformation applied.</returns>
      public ComposableTextReader WithTransformation(in Func<char, char> transformer) {
         _reader = new TransformingTextReader(_reader, transformer);
         return this;
      }
      #endregion


      #region Public helper methods
      /// <summary>
      /// Reads the content of the <see cref="ComposableTextReader"/> up to the <paramref name="streamSizeInBytes"/>.
      /// </summary>
      /// <param name="streamSizeInBytes">Maximum number of bytes to read.</param>
      /// <returns>The read data and the read length.</returns>
      public (char[], int) ReadData(long streamSizeInBytes) {
         char[] result = new char[(streamSizeInBytes / CurrentEncoding.BytesPerCharacter()) * _maxSubstitutionCount];

         int actIndex = 0;
         int c;
         while ((c = _reader.Read()) != -1)
            result[actIndex++] = (char)c;

         return (result, actIndex);
      }
      #endregion


      #region Private methods
      /// <summary>
      /// Get the BOM and encoding from the stream.
      /// </summary>
      /// <param name="stream">Stream to check.</param>
      /// <param name="encoding"><see cref="Encoding"/> to return if there is no BOM in the stream.</param>
      /// <returns>
      /// 1. return value: <see langword="true"/>, if a BOM is found; <see langword="false"/>, if not.
      /// 2. return value: The <see cref="Encoding"/> corresponding to the found BOM;
      /// <paramref name="encoding"/>, if no BOM is found.
      /// </returns>
      private static (bool, Encoding) GetBomAndEncoding(Stream stream, Encoding encoding) {
         bool hasBom;
         Encoding usedEncoding;
         (hasBom, usedEncoding) = EncodingHelper.DetectBomEncoding(stream);
         if (usedEncoding == null)
            usedEncoding = encoding;

         return (hasBom, usedEncoding);
      }
      #endregion


      #region Implementation of TextReader
      public override int Read() {
         return _reader.Read();
      }
      #endregion


      #region Implementation of IDisposable
      protected override void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing && _reader != null) {
            _reader.Dispose();
            _reader = null;
         }

         _isDisposed = true;

         base.Dispose(disposing);
      }
      #endregion
   }
}
