using System;
using System.Collections.Generic;

namespace SourceParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //SourcesHelper.SourcesHelperUtility.printAllSourcesFilePaths("F:\\2019UR\\scdpm\\private\\product\\");
            //SourcesHelper.SourcesHelperUtility.findSourcesForFile("F:\\2016UR\\SCDPM\\private\\product\\agent\\dsm\\DsmFS\\dll\\CompareIterator.cpp");
            Console.WriteLine("Enter Path to create Graph.. \n eg: F:\\2019UR\\scdpm\\private\\product\\");
            String graphCreationPath = Console.ReadLine();
            //Utility.SourcesHelperUtility.createGraph("F:\\2019UR\\scdpm\\private\\product\\");
            Utility.SourcesHelperUtility.createGraph(graphCreationPath);

            Console.WriteLine("Graph Created!! \n\nEnter dll/lib name to find dependency chain for\neg: LibraryAbs.lib");
            String BinaryName = Console.ReadLine();
            //Utility.SourcesHelperUtility.getBinaryName("$(DPM_TARGET_BIN)\\nativemethods.dll; \\");


            System.Collections.Generic.HashSet<String> binariesUsingThisBinary = Utility.SourcesHelperUtility.usageChain(BinaryName);
            // print the list
            Utility.PrintUtility.Print(binariesUsingThisBinary, "binaries using "+ BinaryName);

            // find binaries impacted by this source file

            HashSet<String> b = Utility.SourcesHelperUtility.findBinariesImpactedThroughASource("F:\\2019UR\\scdpm\\private\\product\\agent\\DpmFsFilter\\Wrapper\\dll\\sources");


            Utility.PrintUtility.Print(b, "binaries impacted by sourceFile");

            Console.ReadLine();
        }
    }
}
