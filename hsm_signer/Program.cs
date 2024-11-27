using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

MemoryStream input = new MemoryStream();

try
{
    using (Stream stdin = Console.OpenStandardInput())
    {
        byte[] buffer = new byte[2048];
        int bytes;
        while ((bytes = stdin.Read(buffer, 0, buffer.Length)) > 0)
        {
            input.Write(buffer, 0, bytes);
        }
    }
    input.Position = 0;

    SHA256 sha256 = SHA256.Create();
    byte[] hashData = sha256.ComputeHash(input);

    File.WriteAllBytes("input.sha256", hashData);

    Process hsmrunner = new Process();

    hsmrunner.StartInfo.FileName = "pkcs11-tool";
    hsmrunner.StartInfo.Arguments = " --id 01 -s -p 648219 -m ECDSA  --mgf MGF1-SHA256 --input-file input.sha256 --output-file output.sig";
    hsmrunner.StartInfo.CreateNoWindow = true;
    hsmrunner.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
    hsmrunner.StartInfo.UseShellExecute = false;
    hsmrunner.StartInfo.RedirectStandardError = true;
    hsmrunner.StartInfo.RedirectStandardOutput = true;
    hsmrunner.Start();
    if (!hsmrunner.WaitForExit(20 * 1000))
    {
        try
        {
            Console.WriteLine("runC2PA: 'msg': 'c2patool runassets process timed out'");
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

    //byte[] output = File.ReadAllBytes("output.sig");

    MemoryStream outputMem = new System.IO.MemoryStream(File.ReadAllBytes("output.sig"));

    outputMem.CopyTo(Console.OpenStandardOutput());


}
catch (System.Exception e)
{ File.WriteAllText("error.txt",e.Message); }
