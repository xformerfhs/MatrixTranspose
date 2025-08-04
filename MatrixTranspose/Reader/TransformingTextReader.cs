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

namespace Reader {
   /// <summary>
   /// Class to read characters from a text reader and transform them using a specified function.
   /// </summary>
   public class TransformingTextReader : ChainableTextReader {
      // ******** Instance Variables ********

      /// <summary>
      /// Transforming function to apply to each character read.
      /// </summary>
      private readonly Func<char, char> _transformer;


      // ******** Public Static Properties ********

      /// <summary>
      /// Function to transform characters to uppercase.
      /// </summary>
      public static Func<char, char> ToUpperTransformer => char.ToUpper;

      /// <summary>
      /// Function to replace 'J' with 'I' and 'j' with 'i'.
      /// </summary>s
      public static Func<char, char> JToITransformer => c => c == 'J' ? 'I' : (c == 'j' ? 'i' : c);


      // ******** Constructors ********

      /// <summary>
      /// Creates a new instance of the <see cref="TransformingTextReader"/> class.
      /// </summary>
      public TransformingTextReader(TextReader innerReader, Func<char, char> transformer)
          : base(innerReader) {
         _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
      }


      // ******** Implementation of TextReader ********

      public override int Read() {
         int ch = _innerReader.Read();
         if (ch == -1)
            return ch;

         return _transformer((char)ch);
      }

      public override int Peek() {
         int ch = _innerReader.Peek();
         if (ch == -1)
            return ch;

         return _transformer((char)ch);
      }
   }
}