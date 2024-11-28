// See https://aka.ms/new-console-template for more information
using c2panalyze2;
using System.Text.Json;
using System.Diagnostics;
using System.IO;
using System.Collections;

using Mono.Unix;

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

void Fs_watch()
{

    try
    {
        DirectoryInfo dirlist = new DirectoryInfo(PATHWATCH);
        foreach (FileInfo file in dirlist.GetFiles("*.JPG", SearchOption.AllDirectories))
        {
            if ((!file.FullName.Contains("_signed")) && (!File.Exists(file.FullName.Replace(".JPG", "_signed.JPG"))))
            {
                //inform - html-change
                inform("do not eject<br> signing " + file.Name);

                processC2PA runc2pa = new processC2PA(file.FullName, file.FullName.Replace(".JPG", "_signed.JPG"));
                runc2pa.runSign(file.FullName.Replace(".JPG", "_signed.JPG"));

            }
        }
    }
    catch (System.Exception e) { Console.WriteLine(e.Message); }
    //inform - html-change
    inform("all done<br>you can remove card");
}



while (killme == false)
{
    Fs_watch();
    Thread.Sleep(1000);
}