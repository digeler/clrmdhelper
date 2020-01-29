using System;
using Microsoft.Diagnostics.Runtime;
using System.IO;
using System.Linq;

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
           // string sym = "https://msdl.microsoft.com/download/symbols";

           // Console.WriteLine("using default env sympbol {0}", sym);
           // Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", sym);
            //  Console.WriteLine("Enter Dump Path");
            //Path = Console.ReadLine();
            Path = "c:\\memconsume.dmp";


            Console.WriteLine("Enter Dac Location,If Unknown Leave Blank");
            Mscrdwaksloc = Console.ReadLine();
           
            Dumpinfo();
            runtime = Runtime();
            Console.WriteLine("Press enter for autoanalysis");
            string aa = Console.ReadLine();
            if (aa == "")
            {

                Getappdomains();
                Getmodules();
                Getthreads();
                Memenum();
            }
            Console.WriteLine("Assembly Loded");



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
                   // ClrInfo runtimeInfo = dataTarget.ClrVersions[0];


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
            Console.WriteLine("****Dumping AppDomains****");
            foreach (ClrAppDomain domain in runtime.AppDomains)
            {
                Console.WriteLine("ID:      {0}", domain.Id);
                Console.WriteLine("Name:    {0}", domain.Name);
                Console.WriteLine("Address: {0}", domain.Address);

            }



        }
        public void Getmodules()
        {
            Console.WriteLine("****Dumping Modules****");
            foreach (ClrAppDomain domain in runtime.AppDomains)
            {
                foreach (ClrModule module in domain.Modules)
                {
                    Console.WriteLine("Module: {0}", module.Name);
                }
            }



        }
        public void Getthreads() {
            Console.WriteLine("****Dumping threads****");
            foreach (ClrThread thread in runtime.Threads)
            {
                if (!thread.IsAlive)
                    continue;

                Console.WriteLine("Thread {0:X}:", thread.OSThreadId);

                foreach (ClrStackFrame frame in thread.StackTrace)
                    Console.WriteLine("{0,12:X} {1,12:X} {2}", frame.StackPointer.ToString(), frame.InstructionPointer.ToString(), frame.ToString());
            }
    Console.WriteLine();
}
        public void Memenum() {
            Console.WriteLine("****Dumping Mem****");

            foreach (var region in (from r in runtime.EnumerateMemoryRegions()
                                    where r.Type != ClrMemoryRegionType.ReservedGCSegment
                                    group r by r.Type into g
                                    let total = g.Sum(p => (uint)p.Size)
                                    orderby total descending
                                    select new
                                    {
                                        TotalSize = total,
                                        Count = g.Count(),
                                        Type = g.Key
                                    }))
            {
                Console.WriteLine("{0,6:n0} {1,12:n0} {2}", region.Count, region.TotalSize, region.Type);
                
            }
            Console.WriteLine("Loader Heap: contains CLR structures and the type system");
            Console.WriteLine("High Frequency Heap: statics, MethodTables, FieldDescs, interface map");
            Console.WriteLine("Low Frequency Heap: EEClass, ClassLoader and lookup tables");
            Console.WriteLine("Stub Heap: stubs for CAS, COM wrappers, PInvoke");
            Console.WriteLine("Large Object Heap: memory allocations that require more than 85k bytes");
            Console.WriteLine("GC Heap: user allocated heap memory private to the app");
            Console.WriteLine("JIT Code Heap: memory allocated by mscoreee (Execution Engine) and the JIT compiler for managed code");
            Console.WriteLine("Process/Base Heap: interop/unmanaged allocations, native memory, etc");
            Console.WriteLine("In general, only the GC Segments (and Reserve GC Segments) should really be eating much memory in your process. The total memory for each other type of heap in your process should be less than 50 megs (slightly more for very large dumps). If something is eating more than 100 megs of memory (and it's not a GC Segment) then that's a red flag for investigation.");


        }
    }
    }

