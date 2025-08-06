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
 *    2025-07-24: V1.0.0: Created. fhs
 */

using System;
using System.Text;

namespace MatrixTranspose {
   /// <summary>
   /// Class representing an item in a ComboBox for Encoding selection.
   /// </summary>
   internal class EncodingComboBoxItem {
      #region Public properties
      /// <summary>
      /// The name of the encoding in uppercase.
      /// </summary>
      public string NameUpper { get; }

      /// <summary>
      /// The display name of the encoding.
      /// </summary>
      public string DisplayName { get; }

      /// <summary>
      /// The code page of the encoding.
      /// </summary>
      public int CodePage { get; }
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of <see cref="EncodingComboBoxItem"/>.
      /// </summary>
      /// <param name="encodingInfo">The <see cref="EncodingInfo"/> from which the ComboBox data are derived.</param>
      public EncodingComboBoxItem(in EncodingInfo encodingInfo) {
         if (encodingInfo == null)
            throw new ArgumentNullException(nameof(encodingInfo));

         NameUpper = encodingInfo.Name.ToUpperInvariant();
         DisplayName = encodingInfo.DisplayName;
         CodePage = encodingInfo.CodePage;
      }
      #endregion
   }
}
