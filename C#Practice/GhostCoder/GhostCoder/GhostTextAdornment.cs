using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GhostCoder
{
    internal class GhostTextAdornment
    {
        private readonly IWpfTextView _view;
        private readonly HuggingFaceChatClient _hf;
        private readonly DispatcherTimer _debounceTimer;
        private string _currentLineText;
        private string _currentGhostSuggestion;
        private HuggingFaceChatClient HF => _hf;
        public GhostTextAdornment(IWpfTextView view)
        {
            _view = view;
            _view.Caret.PositionChanged += OnCaretPositionChanged;
            _view.VisualElement.PreviewKeyDown += OnPreviewKeyDown;


            _hf = new HuggingFaceChatClient("Secret");

            _debounceTimer = new DispatcherTimer();
            _debounceTimer.Interval = TimeSpan.FromSeconds(1); // wait 1 second after typing stops
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop();
                await QueryHuggingFaceAndRender(_currentLineText);
            };
        }

    

        private async Task QueryHuggingFaceAndRender(string _)
        {

            string context = GetContextForPrediction();
            if (string.IsNullOrWhiteSpace(context))
                return;

            string prompt =
                "You are an AI code completion assistant. " +
                "The following is a C# code snippet with a caret marker `// <caret>`. " +
                "Predict only the most likely next lines of code that the developer would type at the caret. " +
                "Do not repeat the surrounding code, do not explain, and do not include ``` fences. " +
                "Only return valid language code continuation:\n\n" + context;

            // Take code near the caret as prompt
            string suggestion = await HF.SendMessageAsync(prompt + context);
            _currentGhostSuggestion = suggestion; // store for Tab insertion

            // If result isn’t empty, show faded suggestion
            if (!string.IsNullOrEmpty(suggestion))
            {
                var tb = new TextBlock
                {
                    Text = suggestion,
                    Opacity = 0.4,
                    Foreground = Brushes.AliceBlue,
                    FontStyle = FontStyles.Normal,
                    FontSize = 14
                };

                var caretPos = _view.Caret;
                Canvas.SetLeft(tb, caretPos.Left);
                Canvas.SetTop(tb, caretPos.Top);

                var layer = _view.GetAdornmentLayer("GhostAdornment");

                layer.RemoveAdornmentsByTag("ghost");

                layer.AddAdornment(
                    Microsoft.VisualStudio.Text.Editor.AdornmentPositioningBehavior.TextRelative,
                    _view.Caret.Position.BufferPosition.GetContainingLine().Extent,
                    "ghost",
                    tb,
                    null
                );
            }
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // restart debounce timer every time user types
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private string GetContextForPrediction()
        {
            var caret = _view.Caret.Position.BufferPosition;
            var snapshot = _view.TextBuffer.CurrentSnapshot;
            var caretLine = caret.GetContainingLine().LineNumber;

            // Expand window: 15 lines before, 5 lines after
            int startLineNumber = Math.Max(0, caretLine - 15);
            int endLineNumber = Math.Min(snapshot.LineCount - 1, caretLine + 5);

            var contextBuilder = new StringBuilder();

            for (int i = startLineNumber; i <= endLineNumber; i++)
            {
                if (i == caretLine)
                {
                    // Insert caret marker so model knows where prediction should happen
                    contextBuilder.AppendLine("// <caret>");
                }
                contextBuilder.AppendLine(snapshot.GetLineFromLineNumber(i).GetText());
            }

            return contextBuilder.ToString();
        }


        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var layer = _view.GetAdornmentLayer("GhostAdornment");
            if (e.Key == System.Windows.Input.Key.RightCtrl && !string.IsNullOrEmpty(_currentGhostSuggestion))
            {
                InsertGhostSuggestion();
                e.Handled = true;

            }
            else if(e.Key == System.Windows.Input.Key.Escape)
            {
                // Just remove the faded suggestion
                _currentGhostSuggestion = null;
                layer.RemoveAdornmentsByTag("ghost");
                e.Handled = true;
            }
        }

        private void InsertGhostSuggestion()
        {
            if (string.IsNullOrEmpty(_currentGhostSuggestion))
                return;

            var caret = _view.Caret.Position.BufferPosition;
            var caretLine = caret.GetContainingLine();
            var lineText = caretLine.GetText();

            // Determine leading whitespace of the current line
            string leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);

            // Prepare indented suggestion
            var indentedSuggestion = string.Join(Environment.NewLine,
                _currentGhostSuggestion
                    .Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Split('\n')
                    .Select(line => leadingWhitespace + line)
            );

            var edit = _view.TextBuffer.CreateEdit();

            if (!string.IsNullOrWhiteSpace(lineText))
            {
                // Current line has text → insert on the next line
                edit.Insert(caretLine.End.Position, Environment.NewLine + indentedSuggestion);
            }
            else
            {
                // Current line empty → insert at caret
                edit.Insert(caret.Position, indentedSuggestion);
            }

            edit.Apply();

            // Clear suggestion and remove faded text
            _currentGhostSuggestion = null;
            var layer = _view.GetAdornmentLayer("GhostAdornment");
            layer.RemoveAdornmentsByTag("ghost");
        }
    }
}
