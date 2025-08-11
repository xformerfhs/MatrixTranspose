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
 *    2025-08-10: V1.0.0: Created. fhs
 */

using System;
using System.IO;

namespace WriteHandling {
   /// <summary>
   /// Class to read characters from a text reader and transform them using a specified function.
   /// </summary>
   public class TransformingTextWriter : ChainableTextWriter {
      #region Instance variables
      /// <summary>
      /// Transforming function to apply to each character read.
      /// </summary>
      private readonly Func<char, char> _transformer;
      #endregion


      #region Public static properties
      /// <summary>
      /// Function to convert an Line Feed to a Next Line, which is used in EBCDIC texts.
      /// </summary>
      public static Func<char, char> LfToNlTransformer => c => c == (char)0x0A ? (char)0x85 : c;

      /// <summary>
      /// Function to convert a Line Feed to a Carriage Return.
      /// </summary>
      public static Func<char, char> LfToCrTransformer => c => c == (char)0x0A ? (char)0x0D : c;

      /// <summary>
      /// Function to transform characters to uppercase.
      /// </summary>
      public static Func<char, char> ToUpperTransformer => char.ToUpper;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the <see cref="TransformingTextWriter"/> class.
      /// </summary>
      public TransformingTextWriter(TextWriter innerWriter, Func<char, char> transformer)
          : base(innerWriter) {
         _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
      }
      #endregion


      #region Implementation of TextWriter
      public override void Write(char value) {
         _innerWriter.Write(_transformer(value));
      }
      #endregion
   }
}