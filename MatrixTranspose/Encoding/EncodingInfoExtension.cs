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

using StringHandling;
using System;
using System.Text;

namespace EncodingHandling {
   /// <summary>
   /// Extension class for <see cref="EncodingInfo"/>.
   /// </summary>
   public static class EncodingInfoExtension {
      /// <summary>
      /// Reports, whether the encoding is an EBCDIC encoding.
      /// </summary>
      /// <returns><c>true</c>, if the encoding is an EBCDIC encoding; otherwise <c>false</c>.</returns>
      public static bool IsEbcdic(this EncodingInfo encodingInfo) {
         return encodingInfo.DisplayName.Contains("EBCDIC", StringComparison.InvariantCultureIgnoreCase);
      }
   }
}
