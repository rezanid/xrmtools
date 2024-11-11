namespace XrmTools.UI.Controls
{
    using Community.VisualStudio.Toolkit;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for EnvironmentSelector.xaml
    /// </summary>
    public partial class EnvironmentSelector : UserControl
    {
        #region SolutionItem
        public SolutionItem SolutionItem
        {
            get { return (SolutionItem)GetValue(SolutionItemProperty); }
            set { SetValue(SolutionItemProperty, value); }
        }
        public static readonly DependencyProperty SolutionItemProperty =
            DependencyProperty.Register("SolutionItem", typeof(SolutionItem), typeof(EnvironmentSelector));
        #endregion
        #region Environment
        public DataverseEnvironment Environment
        {
            get { return (DataverseEnvironment)GetValue(EnvironmentProperty); }
            set { SetValue(EnvironmentProperty, value); }
        }
        public static readonly DependencyProperty EnvironmentProperty =
            DependencyProperty.Register("Environment", typeof(DataverseEnvironment), typeof(EnvironmentSelector));
        #endregion
        #region Environments
        public IEnumerable<DataverseEnvironment> Environments
        {
            get { return (IEnumerable<DataverseEnvironment>)GetValue(EnvironmentsProperty); }
            set { SetValue(EnvironmentsProperty, value); }
        }
        public static readonly DependencyProperty EnvironmentsProperty =
            DependencyProperty.Register("Environments", typeof(IEnumerable<DataverseEnvironment>), typeof(EnvironmentSelector));
        #endregion
        #region Commands
        public ICommand Select
        {
            get { return (ICommand)GetValue(SelectProperty); }
            set { SetValue(SelectProperty, value); }
        }
        public static readonly DependencyProperty SelectProperty =
            DependencyProperty.Register("Select", typeof(ICommand), typeof(EnvironmentSelector));

        public ICommand Test
        {
            get { return (ICommand)GetValue(TestProperty); }
            set { SetValue(TestProperty, value); }
        }
        public static readonly DependencyProperty TestProperty =
            DependencyProperty.Register("Test", typeof(ICommand), typeof(EnvironmentSelector));

        public ICommand Cancel
        {
            get { return (ICommand)GetValue(CancelProperty); }
            set { SetValue(CancelProperty, value); }
        }
        public static readonly DependencyProperty CancelProperty =
            DependencyProperty.Register("Cancel", typeof(ICommand), typeof(EnvironmentSelector));
        #endregion

        public EnvironmentSelector()
        {
            InitializeComponent();
        }
    }
}
