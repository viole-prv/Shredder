using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Shredder
{
    public partial class Program
    {
        #region Auto

        public class IAuto : INotifyPropertyChanged
        {
            private Example.ProgressBar? _ProgressBar;

            public Example.ProgressBar? ProgressBar
            {
                get => _ProgressBar;
                set
                {
                    _ProgressBar = value;

                    NotifyPropertyChanged(nameof(ProgressBar));
                }
            }

            private string? _Directory;

            public string? Directory
            {
                get => _Directory;
                set
                {
                    _Directory = value;

                    NotifyPropertyChanged(nameof(Directory));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public void NotifyPropertyChanged(string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private static readonly IAuto Auto = new();

        public Program()
        {
            InitializeComponent();

            DataContext = Auto;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Button();

        private static async void Button()
        {
            if (string.IsNullOrEmpty(Auto.Directory))
            {
                var CommonOpenFileDialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (CommonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok && CommonOpenFileDialog.FileName.Length > 3) // C:\
                {
                    Auto.Directory = CommonOpenFileDialog.FileName;
                }
            }
            else
            {
                try
                {
                    await Task.Run(() => Shredder(new DirectoryInfo(Auto.Directory), true));
                }
                finally
                {
                    Auto.Directory = null;

                    if (Auto.ProgressBar is not null)
                    {
                        Auto.ProgressBar.Dispose();
                        Auto.ProgressBar = null;
                    }
                }
            }
        }

        public static void Shredder(DirectoryInfo X, bool Value = false)
        {
            if (X.Exists)
            {
                var D = X.EnumerateDirectories();

                if (D.Any())
                {
                    if (Value)
                    {
                        Auto.ProgressBar = new Example.ProgressBar
                        {
                            Maximum = D.Count(),
                            Value = 0
                        };
                    }

                    foreach (DirectoryInfo _ in D)
                    {
                        Shredder(_);

                        if (Value)
                        {
                            Auto.ProgressBar!.Value += 1;
                        }
                    }
                }

                var F = X.EnumerateFiles();

                if (F.Any())
                {
                    foreach (FileInfo _ in F)
                    {
                        _.Attributes = FileAttributes.Normal;
                        _.IsReadOnly = false;

                        _.Delete();
                    }
                }

                X.Delete();
            }
        }
    }
}
