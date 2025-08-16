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
 *    2025-08-03: V1.0.0: Created. fhs
 */

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MatrixTranspose.UserControls {
   /// <summary>
   /// Class to implement a spin button.
   /// </summary>
   public partial class NumericUpDown : UserControl {
      #region Dependency Properties
      /// <summary>
      /// Registers the "Value" property of the NumericUpDown control.
      /// </summary>
      public static readonly DependencyProperty ValueProperty =
          DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericUpDown),
              new PropertyMetadata(1, OnValueChanged, CoerceValue));

      /// <summary>
      /// Registers the "Minimum" property of the NumericUpDown control.
      /// </summary>
      public static readonly DependencyProperty MinimumProperty =
          DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(NumericUpDown),
              new PropertyMetadata(1, OnMinMaxChanged));

      /// <summary>
      /// Registers the "Maximum" property of the NumericUpDown control.
      /// </summary>
      public static readonly DependencyProperty MaximumProperty =
          DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(NumericUpDown),
              new PropertyMetadata(20, OnMinMaxChanged));
      #endregion


      #region Public properties
      /// <summary>
      /// Value of the NumericUpDown control.
      /// </summary>
      public int Value {
         get => (int)GetValue(ValueProperty);
         set => SetValue(ValueProperty, value);
      }

      /// <summary>
      /// Minimum allowed value of the NumericUpDown control.
      /// </summary>
      public int Minimum {
         get => (int)GetValue(MinimumProperty);
         set => SetValue(MinimumProperty, value);
      }

      /// <summary>
      /// Maximum allowed value of the NumericUpDown control.
      /// </summary>
      public int Maximum {
         get => (int)GetValue(MaximumProperty);
         set => SetValue(MaximumProperty, value);
      }
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the NumericUpDown control.
      /// </summary>
      public NumericUpDown() {
         InitializeComponent();
      }
      #endregion


      #region Callbacks
      /// <summary>
      /// Coerces the value of the "Value" property to ensure it is within the specified range.
      /// </summary>
      /// <param name="d">Sender control.</param>
      /// <param name="baseValue">New value.</param>
      /// <returns>Validated new value.</returns>
      private static object CoerceValue(DependencyObject d, object baseValue) {
         var control = (NumericUpDown)d;
         var value = (int)baseValue;

         if (value < control.Minimum)
            return control.Minimum;
         
         if (value > control.Maximum)
            return control.Maximum;

         return value;
      }

      /// <summary>
      /// Handles changes to the "Minimum" or "Maximum" property.
      /// </summary>
      /// <param name="d">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
         var control = (NumericUpDown)d;
         // Validate the current value against the new minimum or maximum.
         control.CoerceValue(ValueProperty);
      }

      /// <summary>
      /// Handles changes to the "Value" property.
      /// </summary>
      /// <remarks>
      /// This method is empty because the validation is done in the "CoerceValue" callback to avoid
      /// recursive calls when the value is set.
      /// </remarks>
      /// <param name="d">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
         // This method is intentionally left empty.
      }
      #endregion


      #region Event Handlers
      /// <summary>
      /// Handles clicks on the "down" button to decrement the value.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void DownButton_Click(object sender, RoutedEventArgs e) {
         if (Value > Minimum) 
            Value--;
      }

      /// <summary>
      /// Handles the PreviewKeyDown event of the TextBox to allow incrementing and decrementing with arrow keys.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextBoxValue_PreviewKeyDown(object sender, KeyEventArgs e) {
         // Arrow keys for incrementing and decrementing the value.
         switch (e.Key) {
            case Key.Up:
               if (Value < Maximum)
                  Value++;

               e.Handled = true;
               break;

            case Key.Down:
               if (Value > Minimum)
                  Value--;

               e.Handled = true;
               break;

            case Key.Space:
               e.Handled = true; // Do not allow space key.
               break;
         }
      }

      /// <summary>
      /// Handles the PreviewTextInput event of the TextBox to restrict input to digits only.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextBoxValue_PreviewTextInput(object sender, TextCompositionEventArgs e) {
         // Allow only digits.
         e.Handled = !IsOnlyDigits(e.Text);
      }

      /// <summary>
      /// Handles the TextChanged event of the TextBox to update the Value property based on user input.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextBoxValue_TextChanged(object sender, TextChangedEventArgs e) {
         if (!(sender is TextBox textBox) ||
             string.IsNullOrEmpty(textBox.Text))
            return;
         
         if (int.TryParse(textBox.Text, out int newValue)) {
            if (newValue >= Minimum && newValue <= Maximum) {
               Value = newValue;
            } else {
               // Do not allow values outside the range.
               textBox.Text = Value.ToString();
               textBox.CaretIndex = textBox.Text.Length;
            }
         } else {
            // Do not allow values outside the range.
            textBox.Text = Value.ToString();
            textBox.CaretIndex = textBox.Text.Length;
         }
      }

      /// <summary>
      /// Handles clicks on the "up" button to increment the value.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void UpButton_Click(object sender, RoutedEventArgs e) {
         if (Value < Maximum)
            Value++;
      }
      #endregion


      #region Private helper methods
      /// <summary>
      /// Checks if the given text contains only digits.
      /// </summary>
      /// <param name="text">Text to check.</param>
      /// <returns>
      /// <see langword="true"/>, if the text contains only digits;
      /// <see langword="false"/>, if not.
      /// </returns>
      private static bool IsOnlyDigits(string text) {
         return text.All(c => c >= '0' && c <= '9'); // Only allow digits.
      }
      #endregion
   }
}