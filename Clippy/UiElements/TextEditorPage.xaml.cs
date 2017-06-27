using Clippy.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaction logic for TextEditorPage.xaml
    /// </summary>
    public partial class TextEditorPage : Page
    {
        private const int sMinFontSize = 8;
        private const int sMaxFontSize = 400;

        private string m_FontSizeTextBoxOldValue;
        private double m_LastValidFontSize;
        private double m_DefaultFontSize;

        public TextEditorPage()
        {
            InitializeComponent();
            m_FontSizeTextBoxOldValue = null;
            m_LastValidFontSize = 12;
            m_DefaultFontSize = 12;

            int fontIndex = FontComboBox.Items.IndexOf(MainTextbox.FontFamily);
            if (fontIndex > -1)
            {
                FontComboBox.SelectedIndex = fontIndex;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="TextEditorPage"/> with the given text
        /// </summary>
        public TextEditorPage(string text, double defaultFontSize = 11) : this()
        {
            MainTextbox.Text = text;
            m_DefaultFontSize = defaultFontSize;
        }

        /// <summary>
        /// Gets the line count of the current text
        /// </summary>
        public int GetLineCount()
        {
            if (string.IsNullOrEmpty(MainTextbox.Text)) { return 0; }

            return MainTextbox.LineCount;
        }

        /// <summary>
        /// Sets the given text. If it is NULL an empty string will be set
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (text == null)
            {
                MainTextbox.Text = string.Empty;
            }

            MainTextbox.Text = text;
        }

        /// <summary>
        /// Returns the actual text
        /// </summary>
        public string GetText()
        {
            return MainTextbox.Text;
        }

        /// <summary>
        /// Clears the current text
        /// </summary>
        public void ClearText()
        {
            MainTextbox.Text = string.Empty;
        }

        public void SetFontSize(double fontSize)
        {
            if (fontSize < sMinFontSize || fontSize > sMaxFontSize)
            {
                FontSizeTextBox.Text = m_LastValidFontSize.ToString();
                return;
            }

            m_LastValidFontSize = fontSize;
            MainTextbox.FontSize = fontSize;
            FontSizeTextBox.Text = fontSize.ToString();
        }

        #region event handler
        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            FontFamily selectedFontFamily= FontComboBox.SelectedItem as FontFamily;
            if (MainTextbox != null && selectedFontFamily != null && MainTextbox.FontFamily != selectedFontFamily)
            {
                MainTextbox.FontFamily = selectedFontFamily;
            }
        }

        private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_FontSizeTextBoxOldValue == null)
            {
                return;
            }

            string currentText = FontSizeTextBox.Text;
            if (currentText == m_FontSizeTextBoxOldValue)
            {
                return;
            }
          
            if (!StaticHelper.IsNumeric(currentText))
            {
                FontSizeTextBox.Text = m_FontSizeTextBoxOldValue;
                return;
            }
        }

        private void FontSizeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            m_FontSizeTextBoxOldValue = FontSizeTextBox.Text;

            if (e.Key == Key.Enter)
            {
                double fontSize = Convert.ToDouble(FontSizeTextBox.Text);
                SetFontSize(fontSize);
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            FontBigger();
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {

            FontSmaller();
        }

        private void MainTextbox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta < 0)
                {
                    FontSmaller();
                }

                else if (e.Delta > 0)
                {
                    FontBigger();
                }
            }
        }

        private void MainTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineCountAndCaret();
        }

        private void MainTextbox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateLineCountAndCaret();
        }

        private void MainTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Keyboard.IsKeyDown(Key.D0) || Keyboard.IsKeyDown(Key.NumPad0))
                {
                    FontDefault(); 
                }

                if (Keyboard.IsKeyDown(Key.Add) || Keyboard.IsKeyDown(Key.OemPlus))
                {
                    FontBigger();
                }

                if (Keyboard.IsKeyDown(Key.Subtract) || Keyboard.IsKeyDown(Key.OemMinus))
                {
                    FontSmaller();
                }
            }
        }

        #endregion

        private void UpdateLineCountAndCaret()
        {
            if (MainTextbox == null) { return; }
            if (LineCountLabel != null)
            {
                LineCountLabel.Content = MainTextbox.LineCount;
            }

            if (CaretLabel != null && CurrentLineLabel != null)
            {
                int caretIndex = MainTextbox.CaretIndex;
                int currentline = MainTextbox.GetLineIndexFromCharacterIndex(caretIndex);

                CurrentLineLabel.Content = currentline;

                if (currentline == 0)
                {
                    CaretLabel.Content = caretIndex;
                }
                else if (currentline > 0)
                {
                    CaretLabel.Content = caretIndex - MainTextbox.GetCharacterIndexFromLineIndex(currentline);
                }
            }
        }

        private void FontBigger()
        {
            double fontSize = Convert.ToDouble(FontSizeTextBox.Text);
            if (fontSize < sMaxFontSize)
            {
                fontSize++;
                FontSizeTextBox.Text = fontSize.ToString();
                m_LastValidFontSize = fontSize;
                MainTextbox.FontSize = fontSize;
            }
        }

        private void FontSmaller()
        {
            double fontSize = Convert.ToDouble(FontSizeTextBox.Text);
            if (fontSize > sMinFontSize)
            {
                fontSize--;
                FontSizeTextBox.Text = fontSize.ToString();
                m_LastValidFontSize = fontSize;
                MainTextbox.FontSize = fontSize;
            }
        }

        private void FontDefault()
        {
            if (MainTextbox.FontSize != m_DefaultFontSize)
            {
                MainTextbox.FontSize = m_DefaultFontSize;
                FontSizeTextBox.Text = m_DefaultFontSize.ToString();
            }
        }

    }
}
