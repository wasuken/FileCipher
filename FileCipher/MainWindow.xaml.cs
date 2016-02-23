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
using System.IO.Compression;

namespace FileCipher
{
    public enum ITEM_TYPE { file, folder }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        

        CollectionViewSource encView,decView;
        ObservableCollection<ImageList> encImageItems,decImageItems;
        private static string encDirPath = @"\enc\";
        private static string decDirPath = @"\dec\";
        private static string pass = "habelsdfkjmsafaljsqfdsgdfdgdfhgfhjghngsad";
        

        public MainWindow()
        {
            InitializeComponent();
            SelectList.AllowDrop = true;

            //encrypt周り
            encImageItems = new ObservableCollection<ImageList>();
            encView = new CollectionViewSource();

            encView.Source = encImageItems;
            EncryptionList.DataContext = encView;

            //decrypt周り
            decImageItems = new ObservableCollection<ImageList>();
            decView = new CollectionViewSource();

            decView.Source = decImageItems;
            SelectList.DataContext = decView;

            //decの確認
            checkDir(decDirPath);

            //encの確認及び追加
            checkDir(encDirPath);
            viewReadItems(encDirPath, encImageItems);

            
        }

        //指定したディレクトリ下にあるそれぞれの(enc,dec)ファイルを指定したListに追加する
        private void viewReadItems(string filePath,ObservableCollection<ImageList> list)
        {
            
            string dir = filePath.Split('\\')[1];
            string[] files = Directory.GetFiles(
                @"." + filePath, "*."+dir, SearchOption.TopDirectoryOnly);
            //１つ以上暗号化ファイルが存在するなら　
            if (files.Count() > 0)
            {
                //int num = files.Count();
                //MessageBox.Show("存在する数:" + files.Count());
                //存在する暗号化ファイルのフルパスと画像を入れる
                foreach (var file in files) list.Add(returnImageList(file));
            }

        }
        //encまたはdecのフォルダ(なかったら作る)のFileStreamを返す
        private void checkDir(string filePath)
        {
            string dir = filePath.Split('\\')[1];
            String cPath = Directory.GetCurrentDirectory();
            if (Directory.GetDirectories(@".", "^"+dir+"$", SearchOption.AllDirectories).Count() == 0)
                Directory.CreateDirectory(dir);
            
        }
        //itemで指定したフルパスの先にあるファイルorフォルダのフルパスと名前と対応するアイコンを追加する。
        private ImageList returnImageList(string item)
        {
            Image img = new Image();
            ITEM_TYPE type;
            if (!Directory.Exists(item))
            {
                System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(item);

                img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    appIcon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                type = ITEM_TYPE.file;
            }
            else
            {
                BitmapImage folIcon = new BitmapImage();
                MemoryStream data = new MemoryStream(File.ReadAllBytes(@"../../img/folder.png"));
                WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                data.Close();
                img.Source = wbmp;
                type = ITEM_TYPE.folder;
            }
            return new ImageList(item, img.Source,type);
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
                    decImageItems.Add(returnImageList(item));
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
            {
                /*
                    fullPathの先にあるものがフォルダだった場合エラー吐く。
                    なのでフォルダは一度圧縮してそれを暗号化しようと思ってる。
                    それをどうしようか
                */

                MessageBox.Show(item.fullPath);
                if(item.type == ITEM_TYPE.folder)
                {
                    //zipをenc下に作って
                    ZipFile.CreateFromDirectory(item.fullPath, @"." + encDirPath + item.MyImageName + ".zip");
                    //作ったZipを暗号化して
                    PassLibrary.PasswordManager.EncryptFile(@"." + encDirPath + item.MyImageName+".zip", @"."
                    + encDirPath + item.MyImageName + ".zip.enc", pass);
                    //Zipの残骸を削除
                    File.Delete(@"." + encDirPath + item.MyImageName + ".zip");
                }
                else 
                    PassLibrary.PasswordManager.EncryptFile(@"." + encDirPath + item.MyImageName, @"."
                    + encDirPath + item.MyImageName + ".enc", pass);

                viewReadItems(encDirPath, encImageItems);
            }

        }

        private void EncryptionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            foreach (ImageList item in EncryptionList.SelectedItems)
            {
                MessageBox.Show(item.MyImageName.Substring(0,item.MyImageName.Length - 4));
                PasswordManager.DecryptFile(item.fullPath, 
                    @"." + decDirPath + item.MyImageName.Substring(0,item.MyImageName.Length-4), pass);
            }
                

            System.Diagnostics.Process.Start("EXPLORER.EXE", @".\dec");
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
