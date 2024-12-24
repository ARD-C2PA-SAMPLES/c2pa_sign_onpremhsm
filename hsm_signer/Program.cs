using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

MemoryStream input = new MemoryStream();

try
{
    try
    {
        File.Delete("input.txt");
    }
    catch
    {
    }
    try
    {
        File.Delete("signature.sig");
    }
    catch
    {
    }


    string HSMPASSWORD = "password";

    try
    {
        HSMPASSWORD = Environment.GetEnvironmentVariable("HSMPASSWORD").Trim().ToLower();
    }
    catch
    {
    }

    string HSMOBJECTID = "64176";

    try
    {
        HSMOBJECTID = Environment.GetEnvironmentVariable("HSMOBJECTID").Trim().ToLower();
    }
    catch
    {
    }

    using (Stream stdin = Console.OpenStandardInput())
    {
        byte[] buffer = new byte[2048];
        int bytesin;
        while ((bytesin = stdin.Read(buffer, 0, buffer.Length)) > 0)
        {
            input.Write(buffer, 0, bytesin);
        }
    }
    input.Position = 0;

    using (FileStream file = new FileStream("input.txt", FileMode.Create, System.IO.FileAccess.Write))
        input.CopyTo(file);

 
    Process hsmrunner = new Process();

    hsmrunner.StartInfo.FileName = "yubihsm-shell";
    hsmrunner.StartInfo.Arguments = "-C http://localhost:12345 -p " + HSMPASSWORD + " --action=sign-ecdsa --object-id=" + HSMOBJECTID + " --algorithm=ecdsa-sha256 --in=input.txt --out=signature.sig";
    hsmrunner.StartInfo.CreateNoWindow = true;
    hsmrunner.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
    hsmrunner.StartInfo.UseShellExecute = false;
    hsmrunner.StartInfo.RedirectStandardError = true;
    hsmrunner.StartInfo.RedirectStandardOutput = true;
    hsmrunner.Start();
    if (!hsmrunner.WaitForExit(1 * 1000))
    {
        try
        {
            Console.WriteLine("runC2PA: 'msg': 'c2patool yubikey hsm2 sign process timed out'");
            hsmrunner.Kill();

        }
        catch { }
    }

    string s_rhsm_out = "";
    string s_hsm_err = "";

    try
    {
        s_rhsm_out = hsmrunner.StandardOutput.ReadToEnd().Trim();
        s_hsm_err = hsmrunner.StandardError.ReadToEnd().Trim();
        hsmrunner.WaitForExit();
    }
    catch
    { }

    byte[] bytes = Convert.FromBase64String(File.ReadAllText("signature.sig"));

    MemoryStream outputMem = new MemoryStream(bytes);

    outputMem.CopyTo(Console.OpenStandardOutput());


}
catch (System.Exception e)
{ File.WriteAllText("error.txt", e.Message); }
