using System;
using System.IO;
using System.Windows;

namespace Heice.Controls
{
    public partial class SelectFolderControl
    {
        #region - DependencyProperties -

        public static readonly DependencyProperty SelectedFolderPathProperty = 
            DependencyProperty.Register(
                nameof(SelectedFolderPath),
                typeof(string),
                typeof(SelectFolderControl));

        #endregion
        
        #region [ctor]
        public SelectFolderControl()
        {
            InitializeComponent();

            TbSelectedPath.Text = "-";
            
            BtnSelectFolder.Click += BtnSelectFolderOnClick;
            IsEnabledChanged += OnIsEnabledChanged;
        }
        #endregion
        
        #region - properties -
        
        public string SelectedFolderPath
        {
            get => (string)GetValue(SelectedFolderPathProperty);
            set => SetValue(SelectedFolderPathProperty, value);
        }
        
        #endregion
        
        #region - eventhandlers -
        
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            BtnSelectFolder.IsEnabled = IsEnabled;
        }

        private void BtnSelectFolderOnClick(object sender, RoutedEventArgs e)
        {
            
            if (!IsEnabled)
                return;
            
            var initPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);

            if (Directory.Exists(SelectedFolderPath))
                initPath = SelectedFolderPath;

            SelectedFolderPath = SelectFolder(initPath);
            TbSelectedPath.Text = SelectedFolderPath;
        }
        
        #endregion

        private string SelectFolder(string initialDirectory)
        {
            // Create a "Save As" dialog for selecting a directory (HACK)
            var dialog = new Microsoft.Win32.SaveFileDialog();
            if (!string.IsNullOrEmpty(initialDirectory))
                dialog.InitialDirectory = initialDirectory; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"

            if (dialog.ShowDialog() != true)
                return null;

            if (!IsEnabled)
                return null;
            
            var path = dialog.FileName;
            
            // Remove fake filename from resulting path
            path = path
                .Replace("\\select.this.directory", "")
                .Replace(".this.directory", "");
            
            // If user has changed the filename, create the new directory
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            // Our final value is in path
            return path;
        }
    }
}