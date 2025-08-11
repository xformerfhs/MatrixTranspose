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

using System.Collections.Generic;
using System.Text;

namespace EncodingHandling {
   /// <summary>
   /// Class that implements helper methods for <see cref="EncodingInfo"/> handling.
   /// </summary>
   public static class EncodingInfoHelper {
      #region Public methods
      /// <summary>
      /// Gets the set of all code pages that use EBCDIC encoding.
      /// </summary>
      /// <param name="encodingInfos">Array of <see cref="EncodingInfo"/>.</param>
      /// <returns>A <see cref="SortedSet{int}"/> of the code pages that use EBCDIC encoding.</returns>
      public static SortedSet<int> GetEbcdicCodePages(in EncodingInfo[] encodingInfos) {
         SortedSet<int> result = new SortedSet<int>();

         foreach (var encodingInfo in encodingInfos)
            if (encodingInfo.IsEbcdic())
               result.Add(encodingInfo.CodePage);

         return result;
      }
      #endregion
   }
}
