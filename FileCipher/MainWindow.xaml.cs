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
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using PassLibrary;

namespace FileCipher
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        CollectionViewSource view;
        ObservableCollection<ImageList> imageItems;
        private static string encDirPath = @"\enc\";
        private static string decDirPath = @"\dec\";
        private static string pass = "habelsdfkjmsafaljngsad";
        

        public MainWindow()
        {
            InitializeComponent();
            SelectList.AllowDrop = true;

            imageItems = new ObservableCollection<ImageList>();
            view = new CollectionViewSource();
            view.Source = imageItems;
            SelectList.DataContext = view;

            //encの確認

            FileStream fs = returnFileStream(FileMode.Create, encDirPath,@".");
            MessageBox.Show(fs.Name);
        }
        //encまたはdecのフォルダ(なかったら作る)のFileStreamを返す
        private FileStream returnFileStream(FileMode fileMode,string file,string currentDir)
        {
            string dir = file.Split('\\')[1];
            String cPath = Directory.GetCurrentDirectory();
            if (Directory.GetDirectories(currentDir, dir, SearchOption.AllDirectories).Count() == 0)
            {
                Directory.CreateDirectory(dir);
            }
            return new FileStream(Directory.GetCurrentDirectory()+"\\"+dir, fileMode);
        }

        //参照元１：http://dobon.net/vb/dotnet/control/draganddrop.html
        //参照元２：https://code.msdn.microsoft.com/windowsdesktop/XAMLCVB-WPF-Windows-WPF-a1c048ae
        //参照元３：http://www.atmarkit.co.jp/fdotnet/csharptips/003dragdrop/003dragdrop.html
        private void SelectList_MouseDown(object sender, MouseEventArgs e)
        {
            DragDrop.DoDragDrop((ListView)sender, Content.ToString(), DragDropEffects.Copy);
        }

        private void SelectList_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);

            if(e.Handled) e.Effects = DragDropEffects.Move;
            else e.Effects = DragDropEffects.None;
        }

        private void SelectList_Drop(object sender, DragEventArgs e)
        {
            //()を使った場合はもしその型にキャスト出来なかった場合はInvalidCastExceptionが投げられる。
            //as を使用した場合は変換できなかった時にnullが返ってくる。
            //正直Nullの方がいいので今後はasを多様していく。
            if((string[])e.Data.GetData(DataFormats.FileDrop) != null)
            {
                //itemにはフルパスが入る。
                foreach (string item in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    Image img = new Image();
                    if (!Directory.Exists(item))
                    {
                        System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(item);

                        img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            appIcon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                    }
                    else
                    {
                        BitmapImage folIcon = new BitmapImage();
                        MemoryStream data = new MemoryStream(File.ReadAllBytes(@"../../img/folder.png"));
                        WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                        data.Close();
                        img.Source = wbmp;
                    }

                    ImageList im = new ImageList(item ,img.Source);

                    imageItems.Add(im);

                    
                }
                
            }
        }

        private void SelectList_PreviewDragOver(object sender, DragEventArgs e)
        {
            // ファイルをドロップされた場合のみ e.Handled を True にする
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void Button_Encrypt_Click(object sender, RoutedEventArgs e)
        {
            foreach (ImageList item in SelectList.SelectedItems)
                PassLibrary.PasswordManager.EncryptFile(item.fullPath, encDirPath + item.MyImageName.Split('.')[0] + ".enc", pass);
        }

        private void Button_Decrypt_Click(object sender, RoutedEventArgs e)
        {

        }
        //一応ダブルクリックでエクスプローラで確認可能にする。　右クリックのアイテムでも可能にはしたい
        private void SelectList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (ImageList item in SelectList.SelectedItems)
                System.Diagnostics.Process.Start("EXPLORER.EXE", item.fullPath);
        }
    }
}
