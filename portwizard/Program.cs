/// \file Program.cs
///
/// \mainpage SQ2 - Assignment #3 (Automated Porting of Code from C to C#)
///
/// \section intro Program Introduction
/// - This Program is for porting C code to C# automatically
/// - The main algorithm is that it register keywords included in C header files(stdio.h/stdlib.h...) to the loop-up table,
///    go through the C code, and if it find the matched keyword, it replace with proper C# code from C code.
/// - Conversioning is proceeded a line by line.
///
/// \section version Current version of this program
/// <ul>
/// <li>\author         GuenYoung Gil, Marcus Rankin</li>
/// <li>\version        1.00.00</li>
/// <li>\date           2016.02.25</li>
/// <li>\copyright      GuenYoung Gil, Marcus Rankin</li>
/// <ul>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace portwizard
{
    /// <summary>
    /// This class provides entry point for porting program.
    /// It validates the commend lines and read from(write to) files to port codes.
    /// It instantiates the PortWizardFromCtoCS class to port codes.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //==================================[ LOCAL VAR ]==================================//

            string outFileNamespace = "";       // namespace for output file(c#)
            int outputFileIndex = 0;            // argument number to have a string for output file (c#)
            int srcFileIndex = 0;               // argument number to have a string for source file (c)
            System.IO.StreamReader srcFile;     // StreamReader for source file (c)
            System.IO.StreamWriter outputFile;  // SteramWriter for output file (c#)
            string srcLine = "";                // a line read from source file
            Match match;                        // match for ReGex

            //===============================[ PROGRAM ENTRY ]===============================//

            // arguments validation
            if ((args.Length != 4) ||
                (args[0] != "-i" && args[0] != "-o") ||
                ((args[0] == "-i" && args[2] != "-o") && (args[0] == "-o" && args[2] != "-i")))
            {
                System.Console.WriteLine("Invalid command line!");
                System.Console.WriteLine("portwizard -i C_source_file -o C#_output_file");
                return;
            }

            // find the index in arguments for output file.                
            if (args[2] == "-o")
            {
                outputFileIndex = 3;
                srcFileIndex = 1;
            }
            else
            {
                outputFileIndex = 1;
                srcFileIndex = 3;
            }


            // extract the source file name without extension and set as namespace 
            match = Regex.Match(args[outputFileIndex], @"(\w+).cs");
            if (match.Success)
            {
                outFileNamespace = match.Groups[1].Value;
            }
            else
            {
                System.Console.WriteLine(@"Output file name is invalid or not a ""cs"" file!");
                return;
            }

            
            // open StreamReader from source file to read
            try
            {
                srcFile = new System.IO.StreamReader(args[srcFileIndex]);
                outputFile = new System.IO.StreamWriter(args[outputFileIndex]);

                PortWizardFromCtoCS pwCtoCs = new PortWizardFromCtoCS(outFileNamespace);
                bool isDetectMainEntry = false; // indicate whether detect main() or not

                srcLine = String.Empty;
                // Looping to read line by line until get the end of file
                while ((srcLine = srcFile.ReadLine()) != null)
                {
                    srcLine.TrimEnd(); // remove spaces at the end of string

                    if(pwCtoCs.KeywordIdentifier(ref srcLine))  // detect main() now
                    {
                        outputFile.WriteLine(srcLine);
                        isDetectMainEntry = true;
                    }
                    else
                    {
                        if(isDetectMainEntry)   // after detect main(), indents all code lines
                            outputFile.WriteLine("\t\t" + srcLine);
                        else
                            outputFile.WriteLine(srcLine);  // before detecting main()
                    }                  

                    srcLine = String.Empty;
                }

                // closing curly brakets if main() is detected
                if (isDetectMainEntry)  
                {
                    outputFile.WriteLine("\t}");
                    outputFile.WriteLine("}");
                }

                // closing file streamer
                srcFile.Close();
                outputFile.Close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);               
            }            
            
            return;
        }// End of void Main
    }// End of class Program
}
