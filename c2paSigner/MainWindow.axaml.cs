using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Avalonia.Media.Imaging;

namespace UImobile_c2paSigner
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string message;

        Bitmap imageData;

        static Thread t;


        //string PATHWATCH = "/Users/coadmin/Downloads/test/";
        string PATHWATCH = "/media/";

        //string previous_message = "";

        public string MessageUpdate
        {
            get { return message; }
            set { NotifyPropertyChanged(); }
        }

        public Bitmap ImageUpdate
        {
            get { return imageData; }
            set { NotifyPropertyChanged(); }
        }

        public MainWindow()
        {
            message = "";
            imageData = new Bitmap("certs/wdr_icon.png");
            InitializeComponent();
            Loaded += MainWindow_Loaded;    
            DataContext = this;
            
        }

        private void MainWindow_Loaded(object sender, System.EventArgs e)
        {
            t = new Thread(new ThreadStart(async () => await UpdateGUI()));
            t.Start();
        }

        private async Task UpdateGUI()
        {
            while (true)
            {
                Fs_watch(".JPG");
                Fs_watch(".MP4");
                await Task.Delay(1000);
                //inform - UI-change
                inform("all done\r\nyou can remove card");
                Console.WriteLine("check");
            }
        }


        //*.JPG
        void Fs_watch(string extension)
        {

            try
            {
                DirectoryInfo dirlist = new DirectoryInfo(PATHWATCH);
                foreach (FileInfo file in dirlist.GetFiles("*" + extension, SearchOption.AllDirectories))
                {
                    if (!file.Name.Contains("_signed"))
                    {
                        if (!File.Exists(file.FullName.Replace(extension, "_signed" + extension)))
                        {


                            if (extension == ".JPG")
                            {
                                resizeImage(file.FullName);
                            }
                            else
                            {
                                File.Copy("certs/save_wdr_icon.png", "certs/wdr_icon.png", true);
                            }

                            //inform - html-change
                            inform("do not eject\r\nsigning " + file.Name);

                            processC2PA runc2pa = new processC2PA(file.FullName);
                            runc2pa.runSign(file.FullName.Replace(extension, "_signed" + extension));

                        }

                    }

                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        string resizeImage(string filename)
        {
            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(filename))
            {
                // Resize the image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(400, 300), // Set the desired dimensions
                    Mode = ResizeMode.Max     // Preserve aspect ratio
                }));

                // Save the image as JPEG with quality settings
                image.Save(Path.Combine(Directory.GetCurrentDirectory(), "certs", "wdr_icon.png"), new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression, // Highest compression
                    ColorType = PngColorType.RgbWithAlpha,

                });


                try
                {
                    // Access metadata
                    ImageMetadata metadata = image.Metadata;

                    // Extract EXIF data
                    if (metadata.ExifProfile != null)
                    {
                        Console.WriteLine("EXIF Data:");
                        foreach (var exifValue in metadata.ExifProfile.Values)
                        {
                            Console.WriteLine($"{exifValue.Tag}: {exifValue.GetValue()}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No EXIF data found.");
                    }

                    // Extract IPTC data
                    if (metadata.IptcProfile != null)
                    {
                        Console.WriteLine("\nIPTC Data:");
                        foreach (var iptcValue in metadata.IptcProfile.Values)
                        {
                            Console.WriteLine($"{iptcValue.Tag}: {iptcValue.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No IPTC data found.");
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message); }

            }

            return "s";
        }

        void inform(string msg)
        {
            //Console.WriteLine(msg);
            //if (msg == previous_message) return;
            MessageUpdate = string.Empty;
            ImageUpdate = new Bitmap("certs/wdr_icon.png");
            imageData = new Bitmap("certs/wdr_icon.png");
            message = msg;
            //previous_message = msg;
        }


    }
}
