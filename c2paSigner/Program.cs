// See https://aka.ms/new-console-template for more information
using c2panalyze2;
using System.Text.Json;
using System.Diagnostics;
using System.IO;
using System.Collections;

bool killme = false;

var PATHWATCH = "/media/";

try
{
    PATHWATCH = Environment.GetEnvironmentVariable("PATHWATCH").Trim().ToLower();
}
catch
{
}


Console.WriteLine("PATHWATCH " + PATHWATCH);



void Fs_watch()
{
    string html = "";
    try
    {
        DirectoryInfo dirlist = new DirectoryInfo(PATHWATCH);
        foreach (FileInfo file in dirlist.GetFiles("*.JPG", SearchOption.AllDirectories))
        {
            if ((!file.FullName.Contains("_signed")) && (!File.Exists(file.FullName.Replace(".JPG", "_signed.JPG"))))
            {
                //inform - html-change
                html = File.ReadAllText("html_status/template.html");
                html = html.Replace("@info@", "do not eject<br> signing " + file.Name);
                File.WriteAllText("html_status/status.html", html);
                File.SetUnixFileMode("html_status/status.html", UnixFileMode.UserRead | UnixFileMode.UserWrite);


                processC2PA runc2pa = new processC2PA(file.FullName, file.FullName.Replace(".JPG", "_signed.JPG"));
                runc2pa.runSign(file.FullName.Replace(".JPG", "_signed.JPG"));

            }
        }
    }
    catch (System.Exception e) { Console.WriteLine(e.Message); }
    //inform - html-change
    html = File.ReadAllText("html_status/template.html");
    html = html.Replace("@info@", "all done<br>you can remove card");
    File.WriteAllText("html_status/status.html", html);
    File.SetUnixFileMode("html_status/status.html", UnixFileMode.UserRead | UnixFileMode.UserWrite);

}



while (killme == false)
{
    Fs_watch();
    Thread.Sleep(1000);
}




