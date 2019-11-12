using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    class SourcesHelperUtility
    {
        // Look for sources till depth
        static int sourceInquiryDepth = 2;

        // Dictionary for undefined variables
        static IDictionary<String, String> Variables = new Dictionary<String, String>();

        // Dictionary having mapping from [ source file path -> binary file that it creates (dll or lib or exe) ]
        static IDictionary<String, List<String>> BinCreatedInSource = new Dictionary<String, List<String>>();

        // Dictionary having mapping from [ binary used in a source file (dll or lib or exe) -> source file path ]
        static IDictionary<String, List<String>> SourceUsingTheBin = new Dictionary<String, List<String>>();
        public static List<String> GetAllSourcesFilePaths(string rootPath)
        {
            return new List<String>( Directory.GetFiles(rootPath, "sources", SearchOption.AllDirectories));// directory.GetFiles("*.sources"); //Getting Text files
        }

        // Create Source dependency Graph (binary to source file they are used in, and source to binary they create)
        public static void createGraph(String rootPath)
        {
            String[] AllSourceFilesFiles = Directory.GetFiles(rootPath, "sources", SearchOption.AllDirectories);
            foreach (String file in AllSourceFilesFiles)
            {
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    string fileContents;
                    String targetName = "";
                    Boolean hasTargetType = false;
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        char[] spearator = { '\n'};
                        fileContents = reader.ReadToEnd();
                        String contentLabel = "";
                        String[] strlist = fileContents.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
                        for(int i=0;i<strlist.Length;i++)
                        {
                            // Control logic ------------------------------------------------------------------------------------------
                            // TargetLibs tag has destination of dependencies for current dll/exe/lib
                            if ((strlist[i].Contains("TARGETLIBS") || strlist[i].Contains("REFERENCES")) && strlist[i].Contains("="))
                                contentLabel = "TARGETLIBS";
                            
                            else if (strlist[i].Contains("INCLUDES") && strlist[i].Contains("="))
                                contentLabel = "INCLUDES";
                            
                            else if (strlist[i].Contains("SOURCES") && strlist[i].Contains("="))
                                contentLabel = "SOURCES";
                            
                            // checking if binaries are placed apart from Target
                            else if (strlist[i].Contains("BINPLACE") && strlist[i].Contains("="))
                            {
                                if (strlist[i].Contains("lib"))
                                {
                                    contentLabel = "BINPLACE_LIB";
                                }
                                if (strlist[i].Contains("dll"))
                                {
                                    contentLabel = "BINPLACE_DLL";
                                }
                            }
                            else if (strlist[i].Contains("TARGETNAME") && strlist[i].Contains("="))
                            {
                                targetName = getTargetName(strlist[i]);
                                hasTargetType = false;
                            }
                            else if (strlist[i].Contains("TARGETTYPE") && strlist[i].Contains("="))
                            {
                                String targetNameTemp = targetName;
                                if (strlist[i].Contains("DYNLINK"))
                                    targetNameTemp = targetNameTemp + ".dll";
                                else if (strlist[i].Contains("PROGRAM"))
                                    targetNameTemp = targetNameTemp + ".exe";
                                else if (strlist[i].Contains("LIBRARY"))
                                    targetNameTemp = targetNameTemp + ".lib";
                                    addToGraph(BinCreatedInSource, file, targetNameTemp);
                                hasTargetType = true;
                            }



                            switch (contentLabel)
                            {
                                case "TARGETLIBS":
                                    // read the target files and update SourceUsingTheBin map
                                    if(strlist[i].Contains("lib") || strlist[i].Contains("exe") || strlist[i].Contains("dll"))
                                    {
                                        String binaryName = getBinaryName(strlist[i]);
                                        addToGraph(SourceUsingTheBin, binaryName, file);
                                    }
                                    break;
                                case "BINPLACE_LIB":
                                    addToGraph(BinCreatedInSource, file, (targetName + ".lib"));
                                    contentLabel = "";
                                    break;
                                case "BINPLACE_DLL":
                                    addToGraph(BinCreatedInSource, file, (targetName + ".dll"));
                                    contentLabel = "";
                                    break;
                            }
                        }
                        if(!hasTargetType && targetName!="")
                        {
                            addToGraph(BinCreatedInSource, file, (targetName + ".lib"));
                        }
                    }
                }
            }
        }

        public static void addToGraph(IDictionary<String, List<String>> graph, String key, String value)
        {
            key = key.Trim();
            value = value.Trim();
            if (graph.ContainsKey(key))
            {
                graph[key].Add(value);
            }
            else
            {
                List<String> newNode = new List<String>();
                newNode.Add(value);
                graph.Add(key, newNode);
            }
        }
        public static string getTargetName(String path)
        {
            char[] spearator = { '\\', '=',' ','\r'};
            String[] strlist = path.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < strlist.Length; i++)
            {
                // String followed by = is the target
                int a = path.IndexOf(strlist[i]);
                int b = path.IndexOf('=');
                if (strlist[i].CompareTo("TARGETNAME")!=0)
                    return strlist[i];
            }
            return "";
        }

        // Get the binary's name from the line (binary can be .lib, or .dll or .exe)
        public static string getBinaryName(String path)
        {
            char[] spearator = { '\\',';' };
            String[] strlist = path.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            for (int i=0;i< strlist.Length;i++)
            {
                if ((strlist[i].Contains("dll") || strlist[i].Contains("exe") || strlist[i].Contains("lib")) && strlist[i].Contains('.'))
                    return strlist[i];
            }
            return "";
        }

        // Get the path of the folder sourceInquiryDepth level below filePath
        private static String getFolderPathBelow(int sourceInquiryDepth, String filePath)
        {
            String path = filePath;
            for(int i = 0;i <= sourceInquiryDepth;i++)
                path = Directory.GetParent(path).FullName;
            return path;
        }

        //Print usage chain for a file
        public static HashSet<String> usageChain(String binary)
        {
            HashSet<String> binariesUsingThisBinary = new HashSet<String>();
            List<String> LinkPath = new List<String>();
            Queue<String> binaryQueue = new Queue<String>();
            // add first binary to the queue
            binaryQueue.Enqueue(binary);
            binaryQueue.Enqueue("***");
            String binaryCurrent = "";
            int level = 0;
            if(!SourceUsingTheBin.ContainsKey(binary))
            {
                Console.WriteLine("This binary "+ binary + " is not consumed in any of the source files");
                if(SourceUsingTheBin.ContainsKey(binary))
                {
                    Console.WriteLine("However, this binary is used in this source file - %s", SourceUsingTheBin[binary]);
                }
                return binariesUsingThisBinary;
            }

            // 
            while (binaryQueue.Count!=0)
            {
                try
                {
                    binaryCurrent = binaryQueue.Dequeue();

                    // if we dequeue "***" the it means we need to change the level
                    if(binaryCurrent.CompareTo("***")==0)
                    {
                        level++;
                        binaryQueue.Enqueue("***");
                        binaryCurrent = binaryQueue.Dequeue();
                    }

                    // insert in the linked list at the "level" index and remove everything above it
                    if (LinkPath.Count == level)
                    {
                        LinkPath.Add(binaryCurrent);
                    }
                    else if(LinkPath.Count > level)
                    {
                        LinkPath.Insert(level, binaryCurrent);
                        for (int i = level + 1; i < LinkPath.Count; i++)
                        {
                            LinkPath.RemoveAt(i);
                        }
                    }
                    else
                    {
                        // this should be an error
                        throw new Exception("Trying to insert above permissible height");
                    }

                    if (binaryCurrent.Contains("exe") || binaryCurrent.Contains("dll"))
                    {
                        binariesUsingThisBinary.Add(binaryCurrent);

                        // for logging!! List the complete path for how the two binaries are connected
                        PrintUtility.PrintChain(LinkPath);
                    }
                    else
                    {
                        if (binaryCurrent != "")
                        {
                            // enqueue all source files for the binary
                            // if sources use this binary then find binaries created in those sources, else carry on
                            if (SourceUsingTheBin.ContainsKey(binaryCurrent))
                            {
                                List<String> sourcesUsingThisBinary = SourceUsingTheBin[binaryCurrent];
                                for (int i = 0; i < sourcesUsingThisBinary.Count; i++)
                                {
                                    List<String> binariesCreatedInThisSource = BinCreatedInSource[sourcesUsingThisBinary[i]];
                                    for (int j = 0; j < binariesCreatedInThisSource.Count; j++)
                                    {
                                        // for all the sources found, find the binaries
                                        if (binariesCreatedInThisSource[j] != "")
                                            binaryQueue.Enqueue(binariesCreatedInThisSource[j]);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }

            return binariesUsingThisBinary;
        }

        // under progress
      public static List<String> findSourcesForFile(String filePath)
        {
            /* filePath eg -> a/b/c/xyz.cpp
             * if sourceInquiryDepth = 2;
             * look for sources in all folder under - a/
             * a/b/c/ - depth 0
             * a/b/   - depth 1
             * a/     - depth 2
            */
            List<String> tentativeSourceList = GetAllSourcesFilePaths(getFolderPathBelow(sourceInquiryDepth, filePath));
            List<String> finalSourceList;
            // find, out of these tentative files, which one has the file of interest in either - INCLUDES or SOURCES
            finalSourceList = findSourcesMentioningCurrentFile(filePath, tentativeSourceList);

            return null;
        }
   
        // find binaries impacted through this source file
        public static HashSet<String> findBinariesImpactedThroughASource(String sourceFile)
        {
            List<String> binCreatedInThisSource = null;
            HashSet<String> binariesImpacted = new HashSet<String>();
            if (BinCreatedInSource.ContainsKey(sourceFile))
            {
                binCreatedInThisSource = BinCreatedInSource[sourceFile];
                for (int i = 0; i < binCreatedInThisSource.Count; i++)
                {
                    if(binCreatedInThisSource[i].Contains("exe") || binCreatedInThisSource[i].Contains("dll"))
                        binariesImpacted.Add(binCreatedInThisSource[i]);

                    binariesImpacted.UnionWith(usageChain(binCreatedInThisSource[i]));
                }
            }
            return binariesImpacted;
        }

        public static List<String> findSourcesMentioningCurrentFile(String filePath, List<String> tentativeSourceList)
        {

            return null;
        }
    }
}
