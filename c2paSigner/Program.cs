// See https://aka.ms/new-console-template for more information
using c2panalyze;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;

using Mono.Unix;
using SixLabors.ImageSharp.Processing;

bool killme = false;

var PATHWATCH = "/media/";

try
{
    PATHWATCH = Environment.GetEnvironmentVariable("PATHWATCH").Trim().ToLower();
}
catch
{
}

string previous_message = "";

Console.WriteLine("PATHWATCH " + PATHWATCH);

void inform(string msg)
{
    if (msg == previous_message) return;
    Console.WriteLine(msg);
    string html = File.ReadAllText("html_status/template.html");
    html = html.Replace("@info@", msg);
    File.WriteAllText("html_status/_status.html", html);
    
    var fileInfo = new UnixFileInfo("html_status/_status.html");

    // Set owner and group
    fileInfo.SetOwner("ubuntu", "ubuntu");

    // Set permissions
    fileInfo.FileAccessPermissions = FileAccessPermissions.UserRead | FileAccessPermissions.UserWrite;

    
    File.Move("html_status/_status.html", "html_status/status.html", true);
    previous_message = msg;
}
//*.JPG
void Fs_watch(string extension)
{

    try
    {
        DirectoryInfo dirlist = new DirectoryInfo(PATHWATCH);
        foreach (FileInfo file in dirlist.GetFiles("*" + extension, SearchOption.AllDirectories))
        {
            if ((!file.FullName.Contains("_signed")) && (!File.Exists(file.FullName.Replace(extension, "_signed" + extension))))
            {
                if(extension == ".JPG")
                {
                    resizeImage(file.FullName);
                    //inform - html-change
                    inform("do not eject<br> signing " + file.Name);

                    processC2PA runc2pa = new processC2PA(file.FullName, file.FullName.Replace(extension, "_signed" + extension));
                    runc2pa.runSign(file.FullName.Replace(extension, "_signed" + extension));
                }
                else
                {
                    File.Copy("certs/thumbnail.jpg", file.FullName.Replace(extension, "_signed" + extension));
                    //inform - html-change
                    inform("do not eject<br> signing " + file.Name);

                    processC2PA runc2pa = new processC2PA(file.FullName, file.FullName.Replace(extension, "_signed" + extension));
                    runc2pa.runSign(file.FullName.Replace(extension, "_signed" + extension));
                }
                

                

            }
        }
    }
    catch (System.Exception e) { Console.WriteLine(e.Message); }
    //inform - html-change
    inform("all done<br>you can remove card");
}

string resizeImage(string filename)
{
    using (Image image = Image.Load(filename))
        {
            // Resize the image
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 300), // Set the desired dimensions
                Mode = ResizeMode.Max     // Preserve aspect ratio
            }));

            // Save the image as JPEG with quality settings
            image.Save(Path.Combine(Directory.GetCurrentDirectory(),"certs","thumbnail.jpg"), new JpegEncoder
            {
                Quality = 85 // Set JPEG quality (1-100)
            });


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

    return "s";
}

while (killme == false)
{
    Fs_watch(".JPG");
    Fs_watch(".MP4");
    Thread.Sleep(1000);
}