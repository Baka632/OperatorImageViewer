#nullable disable

using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Composition.SystemBackdrops;
using OperatorImageViewer.Models;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OperatorImageViewer.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public readonly MainWindowViewModel ViewModel = new();

        public MainWindow()
        {
            this.InitializeComponent();
            SetTitleBar(AppTitleBar);
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop();
            }
            else
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
            ExtendsContentIntoTitleBar = true;
            Title = "Operator Image Viewer";
        }

        private async void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is not null)
            {
                OperatorCodenameInfo codenameInfo = (OperatorCodenameInfo)args.ChosenSuggestion;
                sender.Text = codenameInfo.Codename;
                await ViewModel.SetOperatorImageAsync((OperatorCodenameInfo)args.ChosenSuggestion);
            }
            else if (ViewModel.UseCodename != true)
            {
                string text = (from pair in ViewModel.OperatorImageMapping where pair.Value == args.QueryText select pair.Key).FirstOrDefault();
                await ViewModel.SetOperatorImageAsync(text);
            }
            else
            {
                await ViewModel.SetOperatorImageAsync(args.QueryText);
            }
        }

        private void OnTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                ViewModel.CurrentOperatorType = ViewModel.OperatorTypes[comboBox.SelectedIndex];
            }
        }

        private void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = ViewModel.FindOperatorCodename(sender.Text);
            }
        }

        private void OnSkinAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string skinText)
            {
                ViewModel.SkinCodename = skinText;
            }
        }

        private void OnSkinAutoSuggestBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is AutoSuggestBox box)
            {
                box.ItemsSource = ViewModel.FindOperatorSkinCodename(AutoSuggestBox.Text);
            }
        }
    }
}
