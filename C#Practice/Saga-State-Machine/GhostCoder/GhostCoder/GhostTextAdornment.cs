using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text.Editor;

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
            _view.LayoutChanged += OnLayoutChanged;
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

            string prompt = "Analyze the following C# code context and predict what to do next. " +
                            "Only provide the next logical lines of code no need previous lines, formatted, without explanation and dont start with " +
                            "```csharp" + "and dont end with ``` :\n" + context;
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
                    Foreground = Brushes.DeepSkyBlue,
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

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            // restart debounce timer every time user types
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private string GetContextForPrediction()
        {
            var caret = _view.Caret.Position.BufferPosition;
            var snapshot = _view.TextBuffer.CurrentSnapshot;

            // Get up to 5 lines above the caret
            int startLineNumber = Math.Max(0, caret.GetContainingLine().LineNumber - 4);
            int endLineNumber = caret.GetContainingLine().LineNumber;

            var contextBuilder = new StringBuilder();
            for (int i = startLineNumber; i <= endLineNumber; i++)
            {
                contextBuilder.AppendLine(snapshot.GetLineFromLineNumber(i).GetText());
            }

            return contextBuilder.ToString();
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift && !string.IsNullOrEmpty(_currentGhostSuggestion))
            {
                // Insert the ghost text at the caret
                var caret = _view.Caret.Position.BufferPosition;
                var edit = _view.TextBuffer.CreateEdit();
                edit.Insert(caret.Position, _currentGhostSuggestion);
                edit.Apply();

                // Clear the ghost suggestion
                _currentGhostSuggestion = null;

                // Prevent default Tab behavior
                e.Handled = true;
            }
        }
    }
}
