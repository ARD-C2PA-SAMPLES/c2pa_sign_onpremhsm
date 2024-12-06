using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Globalization;
using System.Numerics;
using System.Runtime.ConstrainedExecution;

namespace c2panalyze
{

    public class certchainelement
    {
        public string issuer { get; set; }
        public DateTime notAfter { get; set; }
        public DateTime notBefore { get; set; }
        public string cert_serial_number { get; set; }
        public string subject { get; set; }
        public string signatureAlgorithm { get; set; }
        public string thumbPrint { get; set; }
        public int version { get; set; }
    }

    public class processC2PA
    {

        string _filetoAnalyze = "";

        string _outputFolder = "";



        public processC2PA(string filetoAnalyze, string outputFolder)
        {
            _filetoAnalyze = filetoAnalyze;
            _outputFolder = outputFolder;
            
        }

        public string runassets()
        {
            if (_filetoAnalyze != "")
            {
                Process c2parunner = new Process();

                c2parunner.StartInfo.FileName = "c2patool";

                //c2parunner.StartInfo.Arguments = _filetoAnalyze + " -d";
                c2parunner.StartInfo.Arguments = _filetoAnalyze + " -f --output " + _outputFolder;
                Console.WriteLine("runC2PA runassets: 'c2parunner.StartInfo.Arguments': " + c2parunner.StartInfo.Arguments);
                c2parunner.StartInfo.CreateNoWindow = true;
                c2parunner.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                c2parunner.StartInfo.UseShellExecute = false;
                c2parunner.StartInfo.RedirectStandardError = true;
                c2parunner.StartInfo.RedirectStandardOutput = true;
                c2parunner.Start();

                if (!c2parunner.WaitForExit(20 * 1000))
                {
                    try
                    {
                        Console.WriteLine("runC2PA: 'msg': 'c2patool runassets process timed out'");
                        c2parunner.Kill();

                    }
                    catch { }
                }

                string s_runc2pa_out = "";
                string s_runc2pa_err = "";

                try
                {
                    s_runc2pa_out = c2parunner.StandardOutput.ReadToEnd().Trim();
                    s_runc2pa_err = c2parunner.StandardError.ReadToEnd().Trim();
                    c2parunner.WaitForExit();
                }
                catch
                { }

                Console.WriteLine("runC2PA: 'msg': 'c2patool runassets process finished'");

                //Console.WriteLine("runC2PA: 'msg': 'c2patool process s_runc2pa_out'" + s_runc2pa_out);

                //Console.WriteLine("runC2PA: 'msg': 'c2patool process s_runc2pa_err'" + s_runc2pa_out);

                return s_runc2pa_out;
            }
            else
            {
                return "";
            } 
        }

        public string runCerts()
        {
            if (_filetoAnalyze != "")
            {
                try
                {
                    Process c2parunner2 = new Process();

                    c2parunner2.StartInfo.FileName = Path.Combine("Users", "coadmin", ".cargo", "bin", "c2patool");
                    string s_runc2pa_out2 = "";
                    string s_runc2pa_err2 = "";

                    //c2parunner1.StartInfo.Arguments = _filetoAnalyze + " -d";
                    c2parunner2.StartInfo.Arguments = _filetoAnalyze + " --certs";
                    Console.WriteLine("runC2PA runCerts " + c2parunner2.StartInfo.Arguments);
                    c2parunner2.StartInfo.CreateNoWindow = true;
                    c2parunner2.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                    c2parunner2.StartInfo.UseShellExecute = false;
                    c2parunner2.StartInfo.RedirectStandardError = true;
                    c2parunner2.StartInfo.RedirectStandardOutput = true;
                    c2parunner2.Start();

                    if (!c2parunner2.WaitForExit(20 * 1000))
                    {
                        try
                        {
                            Console.WriteLine("runC2PA: 'msg': 'c2patool runCerts process timed out'");
                            c2parunner2.Kill();

                        }
                        catch { }
                    }

                    try
                    {
                        s_runc2pa_out2 = c2parunner2.StandardOutput.ReadToEnd().Trim();
                        s_runc2pa_err2 = c2parunner2.StandardError.ReadToEnd().Trim();
                        c2parunner2.WaitForExit();
                    }
                    catch
                    { }

                    string[] certs = s_runc2pa_out2.Split("-----END CERTIFICATE-----");

                    //Console.WriteLine("runC2PA 2: 's_runc2pa_out2': " + s_runc2pa_out2);

                    List<certchainelement> certchain = new List<certchainelement>();

                    foreach (string currcert in certs)
                    {
                        if (currcert.Contains("-----BEGIN CERTIFICATE-----"))
                        {

                            //Console.WriteLine("runC2PA 2: 'currcert': " + currcert + "-----END CERTIFICATE-----\r\n");
                            byte[] bytes = Encoding.ASCII.GetBytes(currcert + "-----END CERTIFICATE-----\r\n");
                            try
                            {
                                certchainelement currchainelement = new certchainelement();
                                var cert = new X509Certificate2(bytes);
                                currchainelement.issuer = cert.Issuer;
                                //currchainelement.cert_serial_number = cert.SerialNumber;
                                currchainelement.cert_serial_number = BigInteger.Parse(cert.SerialNumber, NumberStyles.HexNumber).ToString();
                                currchainelement.subject = cert.Subject;
                                currchainelement.thumbPrint = cert.Thumbprint;
                                currchainelement.notBefore = cert.NotBefore;
                                currchainelement.notAfter = cert.NotAfter;
                                currchainelement.signatureAlgorithm = cert.SignatureAlgorithm.FriendlyName.ToString();
                                currchainelement.version = cert.Version;
                                certchain.Add(currchainelement);
                            }
                            catch (System.Exception e)
                            {
                                Console.WriteLine("runC2PA: 'runCerts interim ': " + e.StackTrace + " @ " + e.Message);
                            }


                        }
                    }
                    string jsonString = JsonSerializer.Serialize(certchain);

                    File.WriteAllText(Path.Combine(_outputFolder, "certchain.json"), jsonString);

                    Console.WriteLine("runC2PA 2: 'msg': 'c2patool runCerts process finished'");

                    //Console.WriteLine("runC2PA 2: 'msg': 'c2patool process s_runc2pa_out'" + s_runc2pa_out2);

                    //Console.WriteLine("runC2PA 2: 'msg': 'c2patool process s_runc2pa_err'" + s_runc2pa_out2);

                    return s_runc2pa_out2;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("runC2PA: 'runCerts': " + e.StackTrace + " @ " + e.Message);
                    return "";
                }

            }
            else
            {
                return "";
            }
        }

        public string runSign(string outputFile)
        {
            if (_filetoAnalyze != "")
            {
                Process c2parunner3 = new Process();

                try
                {
                    File.Delete(outputFile);
                }
                catch{}

                c2parunner3.StartInfo.FileName = "c2patool";

                //c2parunner3.StartInfo.Arguments = _filetoAnalyze + " -d";
                c2parunner3.StartInfo.Arguments = "\""+ _filetoAnalyze + "\"" + " -m " + "\"" + Path.Combine(Directory.GetCurrentDirectory(),"certs/test_wdr2.json") + "\"" +  " --signer-path ./hsm_signer -f -o " + "\""  +  outputFile + "\"";
                Console.WriteLine("runC2PA 3 " + c2parunner3.StartInfo.Arguments);
                c2parunner3.StartInfo.CreateNoWindow = true;
                c2parunner3.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                c2parunner3.StartInfo.UseShellExecute = false;
                c2parunner3.StartInfo.RedirectStandardError = true;
                c2parunner3.StartInfo.RedirectStandardOutput = true;
                c2parunner3.Start();

                if (!c2parunner3.WaitForExit(60 * 1000))
                {
                    try
                    {
                        Console.WriteLine("runC2PA 3: 'msg': 'c2patool runSign process timed out'");
                        c2parunner3.Kill();

                    }
                    catch { }
                }

                string s_runc2pa_out1 = "";
                string s_runc2pa_err1 = "";

                try
                {
                    s_runc2pa_out1 = c2parunner3.StandardOutput.ReadToEnd().Trim();
                    s_runc2pa_err1 = c2parunner3.StandardError.ReadToEnd().Trim();
                    c2parunner3.WaitForExit();
                }
                catch
                { }

                Console.WriteLine("runC2PA 3: 'msg': 'c2patool runSign process finished'");

                Console.WriteLine("runC2PA 3: 'msg': 'c2patool process s_runc2pa_out'" + s_runc2pa_out1);

                Console.WriteLine("runC2PA 3: 'msg': 'c2patool process s_runc2pa_err'" + s_runc2pa_out1);

                return s_runc2pa_out1;
            }
            else
            {
                return "";
            }
        }
    }
}
