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

namespace Writer {
   /// <summary>
   /// Class that replaces characters in the output based on a mapping.
   /// </summary>
   public class MappingTextWriter : ChainableTextWriter {
      // ******** Instance Variables ********

      /// <summary>
      /// The mapping from characters to their replacements.
      /// </summary>
      private readonly SortedDictionary<char, char[]> _mapping;


      // ******** Constructors ********

      /// <summary>
      /// Creates a new instance of <see cref="MappingTextWriter"/>.
      /// </summary>
      /// <param name="innerWriter">Inner text writer to which this writer writes.</param>
      /// <param name="mapping">Mapping from a character to a character array.</param>
      public MappingTextWriter(TextWriter innerWriter, SortedDictionary<char, char[]> mapping)
          : base(innerWriter) {
         this._mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
      }


      // ******** Implementation of TextWriter ********

      public override void Write(char value) {
         if (_mapping.TryGetValue(value, out char[] replacement))
            _innerWriter.Write(replacement);
         else
            _innerWriter.Write(value);
      }
   }
}
