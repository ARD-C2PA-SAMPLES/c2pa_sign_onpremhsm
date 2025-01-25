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
using Avalonia.Threading;

namespace UImobile_c2paSigner
{
    public partial class MainWindow : Window
    {

        //string PATHWATCH = "/Users/coadmin/Downloads/test/";
        string PATHWATCH = "/media/";
        //string PATHWATCH = "Z:\\Documents\\GitHub\\c2pa_sign_onpremhsm";

        //string previous_message = "";
        private void SetText(string text) => TextBlock1.Text = text;

        private void SetImage(string source) => Image1.SetValue(Avalonia.Controls.Image.SourceProperty, new Bitmap(source));
            
        private void SetColor(string color) => this.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(color));

        public MainWindow()
        {
 
            InitializeComponent();
    
            DataContext = this;


            _ = Task.Run(() => UpdateGUI());
        }



        private async Task UpdateGUI()
        {
            while (true)
            {
                Fs_watch(".JPG");
                Fs_watch(".MP4");
                await Task.Delay(10000);
                //inform - UI-change
                inform("all done\r\nyou can remove card", "green");
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
                    if ((!file.Name.Contains("_signed")) && (!Path.GetFileName(file.Name).StartsWith(".")))
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
                            inform("do not eject\r\nsigning " + file.Name,"red");


                            processC2PA runc2pa = new processC2PA(file.FullName);
                            runc2pa.runSign(file.FullName.Replace(extension, "_signing" + extension));
                            File.Move(file.FullName.Replace(extension, "_signing" + extension), file.FullName.Replace(extension, "_signed" + extension));
                            
                            FileInfo filecheck = new FileInfo(file.FullName.Replace(extension, "_signed" + extension));
                            Console.WriteLine("FileSize for " + filecheck.FullName + " is " + filecheck.Length);

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
	    Console.WriteLine("resizing Image " + filename);
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

        void inform(string msg, string color)
        {
            Dispatcher.UIThread.Post(() => SetText(msg));
            Dispatcher.UIThread.Post(() => SetImage(Path.Combine("certs","wdr_icon.png")));
            Dispatcher.UIThread.Post(() => SetColor(color));
        }


    }
}
