using System;
using Microsoft.Diagnostics.Runtime;
using System.IO;

namespace clrmd
{
    public class Clrmdhelper
    {
        private string path;
        private string mscrdwaksloc;
        private string symbolserverEnv;
        private ClrRuntime runtime;

        public Clrmdhelper()
        {
            Console.WriteLine("************************");
            Console.WriteLine("*     CLRMD HELPER     *");
            Console.WriteLine("*     DINOR GELER      *");
            Console.WriteLine("************************");
            Console.WriteLine("Loaded ..........");
            Console.WriteLine("digeler@microsoft.com");
            Console.WriteLine("please load the diagnostics module");
            string a = "[System.Reflection.Assembly]::LoadFrom('C:\\clrmd\\clrmd\\packages\\Microsoft.Diagnostics.Runtime.1.1.61812\\lib\\net45\\Microsoft.Diagnostics.Runtime.dll')";
            Console.WriteLine("please use the following command {0}", a);
            string sym = "https://msdl.microsoft.com/download/symbols";
            Console.WriteLine("using default env sympbol {0}", sym);
            Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", sym);
            Console.WriteLine("Enter Dump Path");
            Path = Console.ReadLine();
           
            Console.WriteLine("Enter Dac Location,If Unknown Leave Blank");
            Mscrdwaksloc = Console.ReadLine();
            Console.WriteLine("Enter Symbol Server Location ,default is public");
            SymbolserverEnv = Console.ReadLine();
            if (SymbolserverEnv == null)
            {
                SymbolserverEnv = sym;  
            }
            Setenv();
            Dumpinfo();
            ClrRuntime runtime = Runtime();



        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        

        public string Mscrdwaksloc
        {
            get { return mscrdwaksloc; }
            set { mscrdwaksloc = value; }

        }

        public string SymbolserverEnv
        {

            get { return symbolserverEnv; }
            set { symbolserverEnv = value; }

        }

        public void Dumpinfo()
        {
            using (DataTarget dataTarget = DataTarget.LoadCrashDump(path))
            {
                if (File.Exists(path))
                {

                    Console.WriteLine("Loaded Dumpfile name {0}", path);
                }
                else
                {
                    throw new FileLoadException();
                }

                foreach (ClrInfo version in dataTarget.ClrVersions)
                {
                    Console.WriteLine("Found CLR Version: " + version.Version);

                    // This is the data needed to request the dac from the symbol server:
                    ModuleInfo dacInfo = version.DacInfo;
                    Console.WriteLine("Filesize:  {0:X}", dacInfo.FileSize);
                    Console.WriteLine("Timestamp: {0:X}", dacInfo.TimeStamp);
                    Console.WriteLine("Dac File:  {0}", dacInfo.FileName);
                    ClrInfo runtimeInfo = dataTarget.ClrVersions[0];


                    // If we just happen to have the correct dac file installed on the machine,
                    // the "LocalMatchingDac" property will return its location on disk:
                    string dacLocation = version.LocalMatchingDac;
                    if (!string.IsNullOrEmpty(dacLocation))
                        Console.WriteLine("Local dac location: " + dacLocation);

                }




            }

        }



        public void Setenv()

        {
            Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", symbolserverEnv);
            var enval = Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH");
            Console.WriteLine("Symbol server is set to {0}", enval);



        }

        public ClrRuntime Runtime()
        {

            if (File.Exists(mscrdwaksloc))
            {
                Console.WriteLine("using {0} for dac loc", mscrdwaksloc);
                DataTarget dataTarget = DataTarget.LoadCrashDump(path);
                ClrInfo runtimeInfo = dataTarget.ClrVersions[0];
                ClrRuntime runtime = runtimeInfo.CreateRuntime(mscrdwaksloc);
                return runtime;

            }



          if (!File.Exists(mscrdwaksloc))
            {
                DataTarget dataTarget = DataTarget.LoadCrashDump(path);
                ClrInfo runtimeInfo = dataTarget.ClrVersions[0];
                ClrRuntime runtime = runtimeInfo.CreateRuntime();
                return runtime;
            }


            return null;
        }


        public void Getappdomains()
        {
            foreach (ClrAppDomain domain in runtime.AppDomains)
            {
                Console.WriteLine("ID:      {0}", domain.Id);
                Console.WriteLine("Name:    {0}", domain.Name);
                Console.WriteLine("Address: {0}", domain.Address);
            }
        }

        }
    }

