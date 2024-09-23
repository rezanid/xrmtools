namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/// <summary>
/// Interaction logic for EnvironmentSelectorDialog.xaml
/// </summary>
internal partial class EnvironmentSelectorDialog : DialogWindow
{
    internal EnvironmentSelectorDialog(ISettingsProvider settingsProvider, SolutionItem solutionItem, bool userMode)
    {
        DataContext = new EnvironmentSelectorViewModel(solutionItem, settingsProvider, userMode);
        InitializeComponent();
    }
}
