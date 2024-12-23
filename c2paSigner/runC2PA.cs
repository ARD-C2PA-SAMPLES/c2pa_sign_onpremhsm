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
using System.IO;
using System.Collections;

namespace UImobile_c2paSigner;



    public class processC2PA
    {

        string _filetoAnalyze = "";

        public processC2PA(string filetoAnalyze)
        {
            _filetoAnalyze = filetoAnalyze;
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

