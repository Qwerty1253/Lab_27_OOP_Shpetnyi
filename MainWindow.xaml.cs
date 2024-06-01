using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lab_27
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadDrives();
        }

        // Загрузка доступных дисков в ComboBox
        private void LoadDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                ComboBoxDrives.Items.Add(drive.Name);
            }
        }

        // Обработчик события выхода из приложения
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Обработчик события показа информации о программе
        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
        }

        // Обработчик изменения выбранного диска
        private void ComboBoxDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDrives.SelectedItem != null)
            {
                string selectedDrive = ComboBoxDrives.SelectedItem.ToString();
                LoadDirectoriesAndFiles(selectedDrive);
            }
        }

        // Загрузка директорий и файлов выбранного диска
        private void LoadDirectoriesAndFiles(string path)
        {
            TreeViewFileSystem.Items.Clear();
            var rootDirectory = new DirectoryInfo(path);
            var rootNode = CreateDirectoryNode(rootDirectory);
            TreeViewFileSystem.Items.Add(rootNode);
        }

        // Создание узла для директории
        private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name, Tag = directoryInfo };
            directoryNode.Expanded += DirectoryNode_Expanded;

            try
            {
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    var subDirectoryNode = new TreeViewItem { Header = directory.Name, Tag = directory };
                    subDirectoryNode.Expanded += DirectoryNode_Expanded;
                    subDirectoryNode.Items.Add(null);  // Placeholder item
                    directoryNode.Items.Add(subDirectoryNode);
                }

                foreach (var file in directoryInfo.GetFiles())
                {
                    var fileNode = new TreeViewItem { Header = file.Name, Tag = file };
                    directoryNode.Items.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                directoryNode.Items.Add(new TreeViewItem { Header = "Access Denied", IsEnabled = false });
            }
            catch (Exception ex)
            {
                directoryNode.Items.Add(new TreeViewItem { Header = $"Error: {ex.Message}", IsEnabled = false });
            }

            return directoryNode;
        }

        // Обработчик развертывания узла директории
        private async void DirectoryNode_Expanded(object sender, RoutedEventArgs e)
        {
            var expandedNode = sender as TreeViewItem;
            if (expandedNode.Items.Count == 1 && expandedNode.Items[0] == null)
            {
                expandedNode.Items.Clear();
                var directoryInfo = expandedNode.Tag as DirectoryInfo;

                await Task.Run(() =>
                {
                    try
                    {
                        foreach (var directory in directoryInfo.GetDirectories())
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var subDirectoryNode = new TreeViewItem { Header = directory.Name, Tag = directory };
                                subDirectoryNode.Expanded += DirectoryNode_Expanded;
                                subDirectoryNode.Items.Add(null);  // Placeholder item
                                expandedNode.Items.Add(subDirectoryNode);
                            });
                        }

                        foreach (var file in directoryInfo.GetFiles())
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var fileNode = new TreeViewItem { Header = file.Name, Tag = file };
                                expandedNode.Items.Add(fileNode);
                            });
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            expandedNode.Items.Add(new TreeViewItem { Header = "Access Denied", IsEnabled = false });
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            expandedNode.Items.Add(new TreeViewItem { Header = $"Error: {ex.Message}", IsEnabled = false });
                        });
                    }
                });

                UpdateHighlighting();
            }
        }

        // Обновление подсветки элементов
        private void UpdateHighlighting()
        {
            string filterText = TextBoxFilter.Text.ToLower();

            if (string.IsNullOrWhiteSpace(filterText))
            {
                foreach (TreeViewItem item in TreeViewFileSystem.Items)
                {
                    ClearHighlightingRecursive(item);
                }
                return;
            }

            foreach (TreeViewItem item in TreeViewFileSystem.Items)
            {
                UpdateHighlightingRecursive(item, filterText);
            }
        }

        // Рекурсивное обновление подсветки | SL
        private void UpdateHighlightingRecursive(TreeViewItem item, string filterText)
        {
            if (item.Tag is DirectoryInfo directoryInfo)
            {
                item.Background = directoryInfo.Name.ToLower().Contains(filterText) ? new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)) : Brushes.Transparent;
            }
            else if (item.Tag is FileInfo fileInfo)
            {
                item.Background = fileInfo.Name.ToLower().Contains(filterText) ? new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)) : Brushes.Transparent;
            }

            foreach (TreeViewItem subItem in item.Items.OfType<TreeViewItem>())
            {
                UpdateHighlightingRecursive(subItem, filterText);
            }
        }

        // Рекурсивное очищение подсветки
        private void ClearHighlightingRecursive(TreeViewItem item)
        {
            item.Background = Brushes.Transparent;

            foreach (TreeViewItem subItem in item.Items.OfType<TreeViewItem>())
            {
                ClearHighlightingRecursive(subItem);
            }
        }

        // Обработчик события нажатия кнопки фильтрации
        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateHighlighting();
        }

        // Обработчик изменения выбранного элемента в TreeView
        private void TreeViewFileSystem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = TreeViewFileSystem.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                DisplayProperties(selectedItem.Tag);
                DisplayFileContent(selectedItem.Tag);
            }
        }

        // Отображение свойств выбранного элемента
        private void DisplayProperties(object item)
        {
            ListBoxProperties.Items.Clear();
            if (item is DriveInfo driveInfo)
            {
                ListBoxProperties.Items.Add($"Name: {driveInfo.Name}");
                ListBoxProperties.Items.Add($"Type: {driveInfo.DriveType}");
                ListBoxProperties.Items.Add($"Total Size: {driveInfo.TotalSize}");
                ListBoxProperties.Items.Add($"Available Free Space: {driveInfo.AvailableFreeSpace}");
            }
            else if (item is DirectoryInfo directoryInfo)
            {
                ListBoxProperties.Items.Add($"Name: {directoryInfo.Name}");
                ListBoxProperties.Items.Add($"Full Name: {directoryInfo.FullName}");
                ListBoxProperties.Items.Add($"Creation Time: {directoryInfo.CreationTime}");
                ListBoxProperties.Items.Add($"Last Access Time: {directoryInfo.LastAccessTime}");
            }
            else if (item is FileInfo fileInfo)
            {
                ListBoxProperties.Items.Add($"Name: {fileInfo.Name}");
                ListBoxProperties.Items.Add($"Full Name: {fileInfo.FullName}");
                ListBoxProperties.Items.Add($"Size: {fileInfo.Length}");
                ListBoxProperties.Items.Add($"Creation Time: {fileInfo.CreationTime}");
                ListBoxProperties.Items.Add($"Last Access Time: {fileInfo.LastAccessTime}");
            }
        }

        // Обработчик события нажатия кнопки "Просмотр файла"
        private async void ButtonViewFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = TreeViewFileSystem.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag is FileInfo fileInfo)
            {
                try
                {
                    if (fileInfo.Extension.ToLower() == ".txt")
                    {
                        TextBoxFileContent.Text = await Task.Run(() => File.ReadAllText(fileInfo.FullName));
                        ImageFileContent.Source = null;
                    }
                    else if (fileInfo.Extension.ToLower() == ".png" || fileInfo.Extension.ToLower() == ".jpg")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fileInfo.FullName);
                            bitmap.EndInit();
                            ImageFileContent.Source = bitmap;
                        });
                        TextBoxFileContent.Text = string.Empty;
                    }

                    // Переключение на вкладку "Content"
                    var tabControl = FindName("TabControl") as TabControl;
                    if (tabControl != null)
                    {
                        tabControl.SelectedIndex = 1; // Переключаемся на вкладку "Content"
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access to the file is denied.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Отображение содержимого файла (пока пустая логика, можно расширить в будущем)
        private void DisplayFileContent(object item)
        {
            // Логика для отображения содержимого файла
        }
    }
}