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

using AlphabetHandling;
using EncodingHandling;
using LineEndingHandling;
using MathHandling;
using Microsoft.Win32;
using StringHandling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TranspositionHandling;

namespace MatrixTranspose {
   /// <summary>
   /// Interaction logic for MainWindow.xaml.
   /// </summary>
   public partial class MainWindow : Window {
      #region Public enums
      /// <summary>
      /// Options for the BOM handling in the output file.
      /// </summary>
      public enum BomOption {
         SameAsInput,
         NoBom,
         WithBom
      }
      #endregion

      #region Public properties
      /// <summary>
      /// List of passwords for transposition.
      /// </summary>
      public ObservableCollection<string> Passwords { get; set; } = new ObservableCollection<string>();
      #endregion


      #region Instance variables
      /// <summary>
      /// Description of the chosen substitution alphabet.
      /// </summary>
      private AlphabetDescription _alphabet;

      /// <summary>
      /// Helper variable to store the currently dragged item in the ListBox.
      /// </summary>
      private string _draggedItem;

      /// <summary>
      /// Set of code pages that use EBCDIC encoding.
      /// </summary>
      private SortedSet<int> _ebcdicCodePages;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of <see cref="MainWindow"/>.
      /// </summary>
      public MainWindow() {
         InitializeComponent();

         SetTextAlphabetText(_alphabet.NumCharacters);

         this.DataContext = this;

         InitEncodingLists();
         InitEnumComboBoxes();
      }
      #endregion


      #region Event Handlers
      /// <summary>
      /// Sets the TextAlphabet TextBox to a random substitution alphabet when the "Alphabet" button is clicked.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ButtonAlphabet_Click(object sender, RoutedEventArgs e) {
         if (_alphabet != null)
            SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Handles the click event of the "Exit" button to close the application.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ButtonExit_Click(object sender, RoutedEventArgs e) {
         this.Close();
      }

      /// <summary>
      /// Handles the click event of the "Go" button to start the encryption or decryption process.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private async void ButtonGo_Click(object sender, RoutedEventArgs e) {
         ButtonGo.IsEnabled = false;   // Do not allow multiple clicks.

         if (!CheckParameters() ||
             !ContinueWithFile(TextDestinationFile.Text)) {
            // Some parameter was wrong or destination file should not be overwritten. Do nothing.
            ButtonGo.IsEnabled = true; // Re-enable the button.
            return;
         }

         var savedCursor = Cursor; // Save the current cursor.
         try {
            Cursor = Cursors.Wait; // Change cursor to wait.

            if (RadioEncrypt.IsChecked ?? false)
               await WriteEncryptedFileAsync();
            else
               await WriteDecryptedFileAsync();

            MessageBox.Show("Operation completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         } finally {
            ButtonGo.IsEnabled = true; // Re-enable the button.
            Cursor = savedCursor;      // Change cursor back.
         }
      }

      /// <summary>
      /// Handles removing a password from the ListBox when the "X" button is clicked on a password entry.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ButtonRemovePassword_Click(object sender, RoutedEventArgs e) {
         if (sender is Button btn && btn.Tag is string password)
            Passwords.Remove(password);
      }

      /// <summary>
      /// Handles the choosing of a source file.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ButtonSourceFile_Click(object sender, RoutedEventArgs e) {
         OpenFileDialog openFileDialog = new OpenFileDialog {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Open Text File"
         };

         if (openFileDialog.ShowDialog() == true) {
            string sourceFilePath = openFileDialog.FileName;
            TextSourceFile.Text = sourceFilePath;
            string destinationFilePath = GetDestinationFilePath(sourceFilePath, GetOperationPrefix());

            TextDestinationFile.Text = destinationFilePath;

            int codepage;
            (_, codepage) = EncodingHelper.DetectBomCodepage(sourceFilePath);
            if (codepage != 0 &&
                codepage != (int)ComboInputEncoding.SelectedValue)
               ComboInputEncoding.SelectedValue = codepage;
         }
      }

      /// <summary>
      /// Controls output encoding based on the CheckKeepEncoding checkbox.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void CheckKeepEncoding_Click(object sender, RoutedEventArgs e) {
         bool isChecked = ((CheckBox)sender).IsChecked ?? false;
         ComboOutputEncoding.IsEnabled = !isChecked;
      }

      /// <summary>
      /// Sets the "IsEnabled" flag of the StackBOM control when the ComboInputBox value changes.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ComboInputEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e) {
         SetBomAndLineEnding();
      }

      /// <summary>
      /// Sets the selected value of the output encoding combobox to the input encoding when it is enabled.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ComboOutputEncoding_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
         ComboBox cbo = (ComboBox)sender;
         if (cbo.IsEnabled)
            cbo.SelectedValue = ComboInputEncoding.SelectedValue;
         else
            cbo.SelectedItem = null;
      }

      /// <summary>
      /// Sets the "IsEnabled" flag of the StackBOM control when the ComboOutputBox value changes.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void ComboOutputEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e) {
         SetBomAndLineEnding();
      }

      /// <summary>
      /// Copies a password from the InputBox to the ListBox when the Enter key is pressed.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void PasswordInputBox_KeyDown(object sender, KeyEventArgs e) {
         if (e.Key == Key.Enter) {
            string text = PasswordInputBox.Text.RemoveWhiteSpace().ToLower();

            if (!string.IsNullOrEmpty(text) && !Passwords.Contains(text)) {
               if (CheckPasswordLengths(GetPasswordsFromListBox(), text)) {
                  Passwords.Add(text);
                  PasswordInputBox.Clear();
               } else
                  e.Handled = true;
            }
         }
      }

      /// <summary>
      /// Handles the drag-and-drop functionality for the ListBox containing passwords.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void PasswordListBox_Drop(object sender, DragEventArgs e) {
         if (_draggedItem == null || !e.Data.GetDataPresent(typeof(string)))
            return;

         string droppedData = e.Data.GetData(typeof(string)) as string;
         Point pos = e.GetPosition(ListBoxPasswords);
         int index = GetCurrentIndex(pos);

         if (index >= 0 && index < Passwords.Count) {
            Passwords.Remove(droppedData);
            Passwords.Insert(index, droppedData);
         } else {
            // Add to end.
            Passwords.Remove(droppedData);
            Passwords.Add(droppedData);
         }

         _draggedItem = null;
      }

      /// <summary>
      /// Handles the drag-and-drop functionality for the ListBox containing passwords.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void PasswordListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         // Do not drag if a button was clicked.
         if (FindAncestor<Button>(e.OriginalSource as DependencyObject) != null)
            return;

         if (e.OriginalSource is DependencyObject dep &&
             ItemsControl.ContainerFromElement(ListBoxPasswords, dep) is ListBoxItem item) {
            _draggedItem = item.Content as string;
            DragDrop.DoDragDrop(item, _draggedItem, DragDropEffects.Move);
         }
      }

      /// <summary>
      /// Handles the drag over functionality for the ListBox containing passwords.
      /// </summary>
      private void PasswordListBox_DragOver(object sender, DragEventArgs e) {
         e.Effects = DragDropEffects.Move;
         e.Handled = true;
      }

      /// <summary>
      /// Sets the substitution alphabet when the "2x5" radio button is selected.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void Radio25_Checked(object sender, RoutedEventArgs e) {
         _alphabet = AlphabetFactory.GetAlphabet(2, 5);
         SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Sets the substitution alphabet when the "2x6" radio button is selected.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void Radio26_Checked(object sender, RoutedEventArgs e) {
         _alphabet = AlphabetFactory.GetAlphabet(2, 6);
         SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Sets the substitution alphabet when the "3x3" radio button is selected.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void Radio33_Checked(object sender, RoutedEventArgs e) {
         _alphabet = AlphabetFactory.GetAlphabet(3, 3);
         SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Sets the substitution alphabet when the "3x4" radio button is selected.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void Radio34_Checked(object sender, RoutedEventArgs e) {
         _alphabet = AlphabetFactory.GetAlphabet(3, 4);
         SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Sets the substitution alphabet when the "4x3" radio button is selected.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void Radio43_Checked(object sender, RoutedEventArgs e) {
         _alphabet = AlphabetFactory.GetAlphabet(4, 3);
         SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Sets the substitution alphabet when the "3x6" radio button is selected.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void Radio36_Checked(object sender, RoutedEventArgs e) {
         _alphabet = AlphabetFactory.GetAlphabet(3, 6);
         SetTextAlphabetText(_alphabet.NumCharacters);
      }

      /// <summary>
      /// Handles the selection of the "Decrypt" radio button.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void RadioDecrypt_Checked(object sender, RoutedEventArgs e) {
         if (TextSourceFile != null &&
             !string.IsNullOrEmpty(TextSourceFile.Text)) {
            string destinationFilePath = GetDestinationFilePath(TextSourceFile.Text, @"de");

            TextDestinationFile.Text = destinationFilePath;
         }

         // Disable the group size and max line length controls for decryption.
         if (NumberGroupSize != null)
            NumberGroupSize.IsEnabled = false;

         if (NumberMaxLineLength != null)
            NumberMaxLineLength.IsEnabled = false;
      }

      /// <summary>
      /// Handles the selection of the "Encrypt" radio button.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void RadioEncrypt_Checked(object sender, RoutedEventArgs e) {
         if (TextSourceFile != null &&
             !string.IsNullOrEmpty(TextSourceFile.Text)) {
            string destinationFilePath = GetDestinationFilePath(TextSourceFile.Text, @"en");

            TextDestinationFile.Text = destinationFilePath;
         }

         // Enable the group size and max line length controls for encryption.
         if (NumberGroupSize != null)
            NumberGroupSize.IsEnabled = true;

         if (NumberMaxLineLength != null)
            NumberMaxLineLength.IsEnabled = true;
      }

      /// <summary>
      /// Handles the preview key down event for the TextAlphabet TextBox to prevent entering spaces.
      /// </summary>
      /// <remarks>
      /// It is really a strange idea, that a blank is "control" character.
      /// </remarks>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextAlphabet_PreviewKeyDown(object sender, KeyEventArgs e) {
         if (e.Key == Key.Space)
            e.Handled = true;
      }

      /// <summary>
      /// Ensures that only uppercase letters can be entered in the TextAlphabet TextBox and that
      /// each character is unique.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextAlphabet_PreviewTextInput(object sender, TextCompositionEventArgs e) {
         // Convert the input character to uppercase.
         char inputChar = char.ToUpper(e.Text[0]);

         // Always mark the event as handled to prevent further processing.
         e.Handled = true;

         // Check if this is a letter and not already in TextAlphabet.
         if (inputChar < 'A' || inputChar > 'Z' || TextAlphabet.Text.Contains(inputChar))
            return;

         int selectionLength = TextAlphabet.SelectionLength;
         int newTextLength = TextAlphabet.Text.Length - selectionLength + 1;

         // Check MaxLength of TextAlphabet.
         if (TextAlphabet.MaxLength > 0 &&
             newTextLength > TextAlphabet.MaxLength)
            return;

         // Remove the selected newPassword if any.
         if (selectionLength > 0)
            TextAlphabet.Text = TextAlphabet.Text.Remove(TextAlphabet.SelectionStart, selectionLength);

         // Add the new character and sort alphabetically
         string newText = TextAlphabet.Text + inputChar;
         char[] sortedChars = newText.ToCharArray();
         Array.Sort(sortedChars);
         TextAlphabet.Text = new string(sortedChars);

         // Find the position of the newly inserted character for cursor positioning
         TextAlphabet.SelectionStart = Array.IndexOf(sortedChars, inputChar) + 1;
      }

      /// <summary>
      /// Handles the preview key down event for the TextMatrixPassword TextBox to prevent entering spaces
      /// if the alphabet does not contain a space character.
      /// </summary>
      /// <remarks>
      /// It is really a strange idea, that a blank is "control" character.
      /// </remarks>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextMatrixPassword_PreviewKeyDown(object sender, KeyEventArgs e) {
         if (e.Key == Key.Space &&
             !_alphabet.AlphabetSet.Contains(' '))
            e.Handled = true;
      }

      /// <summary>
      /// Handles the preview text input for the TextMatrixPassword TextBox to ensure that only valid characters from the current alphabet can be entered.
      /// </summary>
      /// <param name="sender">Sender control.</param>
      /// <param name="e">Event parameters.</param>
      private void TextMatrixPassword_PreviewTextInput(object sender, TextCompositionEventArgs e) {
         char inputChar = e.Text[0];

         // Convert to uppercase if necessary.
         if (_alphabet.ToUpper)
            inputChar = char.ToUpper(inputChar);

         // Check, if the character is valid in the current alphabet.
         if (!_alphabet.AlphabetSet.Contains(inputChar)) {
            e.Handled = true; // Do not accept character, if it is not in the alphabet.
            return;
         }

         // If the character had been converted to uppercase, replace it in the TextBox.
         if (inputChar == e.Text[0])
            return;

         e.Handled = true;

         TextBox textBox = sender as TextBox;
         int cursorPosition = textBox.SelectionStart;

         // Insert converted character at the correct cursor position.
         textBox.Text = textBox.Text.Insert(cursorPosition, inputChar.ToString());
         textBox.SelectionStart = cursorPosition + 1;
      }
      #endregion


      #region Private Helper Methods
      /// <summary>
      /// Checks all parameters for validity that are needed for processing.
      /// </summary>
      /// <remarks>
      /// This methods shows a message box if a parameter is not valid.
      /// </remarks>
      /// <returns>
      /// <see langword="true"/>, if all parameters are set and valid;
      /// <see langword="false"/>, if not.
      /// </returns>
      private bool CheckParameters() {
         if (_alphabet == null) {
            MessageBox.Show("Please select an alphabet first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TabCon.SelectedIndex = 0;
            GroupCharacterClasses.Focus();
            return false;
         }

         if (TextAlphabet.Text.Length != TextAlphabet.MaxLength) {
            MessageBox.Show($"Please enter a substitution alphabet of length {TextAlphabet.MaxLength}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TabCon.SelectedIndex = 0;
            TextAlphabet.SelectionLength = 0;
            TextAlphabet.SelectionStart = 0;
            TextAlphabet.Focus();
            return false;
         }

         if (string.IsNullOrEmpty(TextMatrixPassword.Text)) {
            MessageBox.Show("Please enter a matrix password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TabCon.SelectedIndex = 0;
            TextMatrixPassword.Focus();
            return false;
         }

         if (ListBoxPasswords.Items.Count == 0) {
            MessageBox.Show("Please add at least one password for transposition.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TabCon.SelectedIndex = 1;
            PasswordInputBox.Focus();
            return false;
         }

         if (string.IsNullOrEmpty(TextSourceFile.Text)) {
            MessageBox.Show("Please select a source file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TabCon.SelectedIndex = 2;
            ButtonSourceFile.Focus();
            return false;
         }

         if (ComboInputEncoding.SelectedValue == null) {
            MessageBox.Show("Please select an input encoding.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TabCon.SelectedIndex = 3;
            ComboInputEncoding.Focus();
            return false;
         }

         return true;
      }

      /// <summary>
      /// Checks if the lengths of the current passwords and the new newPassword have a common divisor.
      /// </summary>
      /// <remarks>
      /// This method shows a message box if the lengths have a common divisor.
      /// </remarks>
      /// <returns>
      /// <see langword="true"/>, if the password lengths do not have a common divisor;
      /// <see langword="false"/>, if they do.
      /// </returns>
      private static bool CheckPasswordLengths(string[] passwords, string newPassword) {
         int textLength = newPassword.Length;
         foreach (string password in passwords) {
            int gcd = MathHelper.Gcd(password.Length, textLength);
            if (gcd != 1) {
               MessageBox.Show($"Length of \"{newPassword}\" has common divisor {gcd} with length of \"{password}\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
               return false;
            }
         }

         return true;
      }

      /// <summary>
      /// Checks if the user wants to continue with the specified file path.
      /// </summary>
      /// <param name="filePath">Path of file to write.</param>
      /// <returns>
      /// <see langword="true"/>, if file should be written to;
      /// <see langword="false"/>, if not.
      /// </returns>
      private static bool ContinueWithFile(string filePath) {
         bool result = true;

         if (File.Exists(filePath)) {
            MessageBoxResult mbr = MessageBox.Show(
               $"The file \"{filePath}\" already exists. Do you want to overwrite it?",
               "File Exists",
               MessageBoxButton.YesNo,
               MessageBoxImage.Warning
            );

            if (mbr != MessageBoxResult.Yes)
               result = false;
         }

         return result;
      }

      /// <summary>
      /// Gets the BOM for the output file.
      /// </summary>
      /// <param name="inputHasBom">Indication, if input file has a BOM.</param>
      /// <returns>
      /// <see langword="true"/>, if output file has to write a BOM;
      /// <see langword="false"/>, if not.
      /// </returns>
      private bool GetBomForOutput(bool inputHasBom) {
         if (StackBOM.IsEnabled) {
            switch (EnumComboBoxHelper.GetSelectedEnumValue<BomOption>(ComboBom)) {
               case BomOption.SameAsInput:
                  return inputHasBom;

               case BomOption.WithBom:
                  return true;

               default:
                  return false;
            }
         }

         return inputHasBom;
      }

      /// <summary>
      /// Gets the destination index for the dragged item based on the mouse position.
      /// </summary>
      /// <param name="position">Mouse position.</param>
      /// <returns>Index of item.</returns>
      private int GetCurrentIndex(Point position) {
         for (int i = 0; i < ListBoxPasswords.Items.Count; ++i) {
            ListBoxItem item = (ListBoxItem)ListBoxPasswords.ItemContainerGenerator.ContainerFromIndex(i);

            if (item != null) {
               Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
               Point itemPos = item.TransformToAncestor(ListBoxPasswords).Transform(new Point(0, 0));
               bounds.Location = itemPos;

               if (bounds.Contains(position))
                  return i;
            }
         }

         return -1;
      }

      /// <summary>
      /// Builds the destination file path based on the source file path and a modifier.
      /// </summary>
      /// <param name="sourceFilePath">File path of the source file.</param>
      /// <param name="modifier">Modifier, that should be appended to the base file name.</param>
      /// <returns>Destination file path.</returns>
      private static string GetDestinationFilePath(in string sourceFilePath, in string modifier) {
         string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
         string extension = Path.GetExtension(sourceFilePath);

         string destinationFilePath;

         string directory = Path.GetDirectoryName(sourceFilePath);
         destinationFilePath = $"{fileName}_{modifier}crypted{extension}";
         if (!string.IsNullOrEmpty(directory))
            destinationFilePath = Path.Combine(directory, destinationFilePath);

         return destinationFilePath;
      }

      /// <summary>
      /// Gets the prefix for the operation based on the selected radio button.
      /// </summary> 
      private string GetOperationPrefix() {
         if ((bool)RadioEncrypt.IsChecked)
            return @"en"; // Encrypt operation.
         else
            return @"de"; // Decrypt operation.
      }

      /// <summary>
      /// Gets the output encoding based on the supplied code page.
      /// </summary>
      /// <param name="codePage">Code page to start with.</param>
      /// <returns>The <see cref="Encoding"/> to use for output.</returns>
      private Encoding GetOutputEncoding(int codePage) {
         bool keepEncoding = CheckKeepEncoding.IsChecked ?? false;

         if (keepEncoding)
            return Encoding.GetEncoding(codePage);
         else
            return Encoding.GetEncoding((int)ComboOutputEncoding.SelectedValue);
      }

      /// <summary>
      /// Gets the output code page based on the supplied code page.
      /// </summary>
      /// <returns>The code page to use for output.</returns>
      private int GetOutputCodePage() {
         if (CheckKeepEncoding.IsChecked ?? true)
            return (int)ComboInputEncoding.SelectedValue;
         else
            return (int)ComboOutputEncoding.SelectedValue;
      }

      /// <summary>
      /// Returns the current passwords from ListBoxPasswords as a string array.
      /// </summary>
      /// <returns>List of current passwords as a string array.</returns>
      private string[] GetPasswordsFromListBox() {
         return ListBoxPasswords.Items.OfType<string>().ToArray();
      }

      /// <summary>
      /// Finds the first ancestor of type <typeparamref name="T"/> in the visual tree.
      /// </summary>
      /// <typeparam name="T">Type of ancestor to find.</typeparam>
      /// <param name="current">Start object.</param>
      /// <returns>
      /// Ancestor of <paramref name="current"/> of type <typeparamref name="T"/>;
      /// <see langword="null"/> if none is found.
      /// </returns>
      private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject {
         while (current != null) {
            if (current is T match)
               return match;

            current = VisualTreeHelper.GetParent(current);
         }

         return null;
      }

      /// <summary>
      /// Initializes the encoding lists for input and output encodings.
      /// </summary>
      private void InitEncodingLists() {
         var encodingInfoList = Encoding.GetEncodings();
         _ebcdicCodePages = EncodingInfoHelper.GetEbcdicCodePages(encodingInfoList);
         var encodingItems = encodingInfoList
             .Where(enc => !enc.Name.StartsWith("X-", StringComparison.InvariantCultureIgnoreCase))
             .Select(enc => new EncodingComboBoxItem(enc))
             .OrderBy(ecbi => ecbi.NameUpper)
             .ToList();
         ComboInputEncoding.ItemsSource = encodingItems;
         ComboOutputEncoding.ItemsSource = encodingItems;
         ComboInputEncoding.SelectedValue = Encoding.UTF8.CodePage;
      }

      private void InitEnumComboBoxes() {
         // BOM-ComboBox
         EnumComboBoxHelper.SetupEnumComboBox<BomOption>(ComboBom, bomOption => {
            switch (bomOption) {
               case BomOption.SameAsInput:
                  return "Same as source";
               case BomOption.NoBom:
                  return "No BOM";
               case BomOption.WithBom:
                  return "With BOM";
               default:
                  return bomOption.ToString();
            }
         });

         // Line Ending-ComboBox
         EnumComboBoxHelper.SetupEnumComboBox<LineEndingHandler.Option>(ComboLineEnding, bomOption => {
            switch (bomOption) {
               case LineEndingHandler.Option.Windows:
                  return "Windows (CR/LF)";
               case LineEndingHandler.Option.Unix:
                  return "Unix (LF)";
               case LineEndingHandler.Option.OldMac:
                  return "Old Macintosh (CR)";
               case LineEndingHandler.Option.Ebcdic:
                  return "EBCDIC (NL)";
               default:
                  return bomOption.ToString();
            }
         });
      }

      /// <summary>
      /// Set BOM and line endings according to the input and output encodings.
      /// </summary>
      private void SetBomAndLineEnding() {
         int outputCodepage = GetOutputCodePage();
         StackBOM.IsEnabled = EncodingHelper.SupportsBom(outputCodepage);

         var oldLineEnding = EnumComboBoxHelper.GetSelectedEnumValue<LineEndingHandler.Option>(ComboLineEnding);
         if (oldLineEnding != null)
            if (_ebcdicCodePages.Contains(outputCodepage)) {
               if (oldLineEnding != LineEndingHandler.Option.Ebcdic)
                  ComboLineEnding.SelectedValue = LineEndingHandler.Option.Ebcdic;
            } else {
               if (oldLineEnding == LineEndingHandler.Option.Ebcdic)
                  ComboLineEnding.SelectedValue = LineEndingHandler.Option.Windows;
            }
      }

      /// <summary>
      /// Sets the text of the TextAlphabet TextBox to a random substitution alphabet of the specified length.
      /// </summary>
      /// <param name="numCharacters">Number of characters in the random alphabet.</param>
      private void SetTextAlphabetText(in byte numCharacters) {
         if (TextAlphabet == null)
            return;

         TextAlphabet.MaxLength = numCharacters;
         TextAlphabet.Text = new string(SubstitutionAlphabet.GetRandomSubstitutionCharacters(numCharacters));
      }

      /// <summary>
      /// Writes a decrypted file asynchronously based on the input parameters.
      /// </summary>
      private async Task WriteDecryptedFileAsync() {
         // 1. Copy UI variables into local variables to avoid cross-thread issues.
         var inputEncoding = Encoding.GetEncoding((int)ComboInputEncoding.SelectedValue);
         var alphabetChars = TextAlphabet.Text.ToCharArray();
         var numPlaces = _alphabet.NumPlaces;
         var sourceFile = TextSourceFile.Text;
         var destFile = TextDestinationFile.Text;
         var matrixPassword = TextMatrixPassword.Text;
         var lineEndingOption = EnumComboBoxHelper.GetSelectedEnumValue<LineEndingHandler.Option>(ComboLineEnding)
                                ?? LineEndingHandler.Option.Windows;
         var passwords = GetPasswordsFromListBox();

         // 2. Read encrypted file and substitute matrix entries with characters.
         var (substitutedText, hasBom, readLength, usedEncoding) =
             await FileHandlerAsync.ReadEncryptedTextFileAsync(
                 sourceFile,
                 inputEncoding,
                 alphabetChars,
                 numPlaces);

         // 3. Untranspose
         var transposition = new Transposition(passwords);
         var transposedText = transposition.Untranspose(substitutedText, readLength);

         // 4. Get substitution alphabet.
         var substitutionAlphabet = new SubstitutionAlphabet(_alphabet, matrixPassword, alphabetChars);

         // 5. Write decrypted file.
         await FileHandlerAsync.WriteUnsubstitutedCleartextFileAsync(
             destFile,
             GetOutputEncoding(usedEncoding.CodePage),
             GetBomForOutput(hasBom),
             lineEndingOption,
             substitutionAlphabet.CombinationToChar,
             transposedText,
             readLength);
      }

      /// <summary>
      /// Writes an encrypted file asynchronously based on the input parameters.
      /// </summary>
      private async Task WriteEncryptedFileAsync() {
         // 1. Copy UI variables into local variables to avoid cross-thread issues.
         var inputEncoding = Encoding.GetEncoding((int)ComboInputEncoding.SelectedValue);
         var alphabetChars = TextAlphabet.Text.ToCharArray();
         var numPlaces = _alphabet.NumPlaces;
         var sourceFile = TextSourceFile.Text;
         var destFile = TextDestinationFile.Text;
         var matrixPassword = TextMatrixPassword.Text;
         var passwords = GetPasswordsFromListBox();
         var groupSize = NumberGroupSize.Value;
         var maxLineLength = NumberMaxLineLength.Value;
         var treatJAsI = _alphabet.TreatJAsI;
         var toUpper = _alphabet.ToUpper;
         var lineEndingOption = EnumComboBoxHelper.GetSelectedEnumValue<LineEndingHandler.Option>(ComboLineEnding)
                                ?? LineEndingHandler.Option.Windows;

         // 2. Get substitution alphabet.
         var substitutionAlphabet = new SubstitutionAlphabet(_alphabet, matrixPassword, alphabetChars);

         // 3. Read source file and substitute characters with matrix entries.
         var (substitutedText, hasBom, readLength, usedEncoding) =
             await FileHandlerAsync.ReadSubstitutedCleartextFileAsync(
                 sourceFile,
                 inputEncoding,
                 substitutionAlphabet.CharToCombination,
                 numPlaces,
                 treatJAsI,
                 toUpper);

         // 4. Transpose
         var transposition = new Transposition(passwords);
         var transposedText = transposition.Transpose(substitutedText, readLength);

         // 5. Write encrypted file.
         await FileHandlerAsync.WriteEncryptedFileAsync(
             destFile,
             GetOutputEncoding(usedEncoding.CodePage),
             GetBomForOutput(hasBom),
             lineEndingOption,
             transposedText,
             readLength,
             groupSize,
             maxLineLength);
      }
      #endregion
   }
}
