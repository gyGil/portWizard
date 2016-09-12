/// \file PortWizardFromCtoCS.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace portwizard
{
    //===============================[ CLASS DEFINITIONS ]===============================//

    /// <summary>
    /// It converts c code to c-sharp code.
    /// This class provides keywordsTable to connect keywords with proper methods to convert proper C# code
    /// </summary>
    class PortWizardFromCtoCS
    {
        //==============================[ CONSTANTS ]==============================//

        const int MAX_HEADER_FILES = 20;    ///< The maximum number which keyowrdsTable is allowed to have header files
        const int MAX_KEYWORDS = 50;        ///< The maximum number which keyowrdsTable is allowed to have keywords for a header file.

        //===============================[ MEMBERS ]===============================//

        private string[,] keywordsTable;     /// 1d is header file names (ex. keywordsTable[....][0] = "file name")and 
                                             /// 2d is keywords corresponds to 1d header file included in c source file
                                             /// (ex. file name= "stdlib.h" and keywords "atof" ,"atoi"  
                                             /// ==> keywordsTable[1][0] = "stdlib.h" keywordsTable[1][1] = "atof" keywordsTable[1][2] ="atoi"

        private string strNamespace;            ///< namespace to be used for C# code

        //===============================[ METHODS ]===============================//


        public PortWizardFromCtoCS() { }
        /// <summary>
        /// register the namespace to be used in C# code and register the basic keywords
        /// </summary>
        public PortWizardFromCtoCS(string strNamespace)
        {
            this.strNamespace = strNamespace;

            this.keywordsTable = new string[PortWizardFromCtoCS.MAX_HEADER_FILES, PortWizardFromCtoCS.MAX_KEYWORDS];
            this.keywordsTable[0, 0] = "generic";
            this.keywordsTable[0, 1] = "main";
            this.keywordsTable[0, 2] = "#include";
            this.keywordsTable[0, 3] = "int";
            this.keywordsTable[0, 4] = "float";
            this.keywordsTable[0, 5] = "double";
            this.keywordsTable[0, 6] = "char";
            this.keywordsTable[0, 7] = "void";
            this.keywordsTable[0, 8] = "NULL";

        }

        /// <summary>
        /// identify keywords for the C to convert to C# 
        /// </summary>
        /// <param name="line"> a line from source file (reference)</param>
        /// <returns>true = detect main(), false = not detect main()</returns>
        public bool KeywordIdentifier(ref string line)
        {
            bool isDetectMainEntry = false; // check whether main() is detected or not
            int max1dIndex = this.GetLastIndex_keywordsTable(1, -1) + 1;

            // looping the all header files keywords to find matched keywords
            for (int curIndexHeader = 0; curIndexHeader < max1dIndex; ++curIndexHeader)
            {
                // looping to find the matched keywords
                int max2dIndex = this.GetLastIndex_keywordsTable(2, curIndexHeader) + 1;
                for (int curIndexKeywords = 1; curIndexKeywords < max2dIndex; ++curIndexKeywords)
                {
                    // if keywords is matched, call the methods to convert into C# code
                    Match match = Regex.Match(line, @"(^|[^\w])(" + this.keywordsTable[curIndexHeader, curIndexKeywords] + @")([^\w]|$)");
                    if (match.Success)
                    {
                        if (this.keywordsTable[curIndexHeader, 0] == "generic")
                            this.Convertor_generic('c', ref line, curIndexKeywords);
                        else if (this.keywordsTable[curIndexHeader, 0] == "stdio.h")
                            this.Convertor_stdio_h('c', ref line, curIndexKeywords);
                        else if (this.keywordsTable[curIndexHeader, 0] == "stdlib.h")
                            this.Convertor_stdlib_h('c', ref line, curIndexKeywords);
                        else if (this.keywordsTable[curIndexHeader, 0] == "string.h")
                            this.Convertor_string_h('c', ref line, curIndexKeywords);

                        if (this.keywordsTable[curIndexHeader, curIndexKeywords] == "main")
                            isDetectMainEntry = true;
                    }
                }
            }// end of for

            return isDetectMainEntry;
        }

        /// <summary>f
        /// It register keywords or convert to c# code for generic type (ex. #include, main)
        /// </summary>
        /// <param name="mode"> 'r' = for register keywords, 'c' = for convert c to c#</param>
        /// <param name="line"> the source code line to be converted to c# </param>
        /// <param name="matchKeywordNum"> which keyword is matched </param>
        /// <returns>string the converted string or null if type is 'r'</returns>
        private void Convertor_generic(char mode, ref string line, int matchKeywordNum)
        {
            // 'r' is for register keywords, 'c' is for converting keywords
            switch (mode)
            {
                case 'r':   // register keywords

                    int fileIndex = this.GetLastIndex_keywordsTable(1, -1) + 1;

                    this.keywordsTable[fileIndex, 0] = "generic";
                    this.keywordsTable[fileIndex, 1] = "main";
                    this.keywordsTable[fileIndex, 2] = "#include";
                    this.keywordsTable[fileIndex, 3] = "int";
                    this.keywordsTable[fileIndex, 4] = "float";
                    this.keywordsTable[fileIndex, 5] = "double";
                    this.keywordsTable[fileIndex, 6] = "char";
                    this.keywordsTable[fileIndex, 7] = "void";
                    this.keywordsTable[fileIndex, 8] = "NULL";
                    break;

                case 'c':   // convert c keyword to c# code

                    // check which keyword is found in a line
                    switch (matchKeywordNum)
                    {
                        case 1: // main

                            // build string for c# main entry point
                            string patternMain = @"(^|[^\w])([a-zA-Z]+)(\s+)(main)(.*)";
                            string main = "";
                            if (Regex.IsMatch(line, patternMain))
                            {
                                main = Regex.Replace(line, patternMain, m => "\t\tstatic " + m.Groups[1].Value +
                                                     m.Groups[2].Value + " Main(string[] args)");
                            }

                            line = "namespace " + this.strNamespace + System.Environment.NewLine;
                            line += "{" + System.Environment.NewLine;
                            line += "\tclass Program" + System.Environment.NewLine;
                            line += "\t{" + System.Environment.NewLine;
                            line += main;

                            break;

                        case 2: // #include

                            // stdio.h => register keywords and replace to proper CS code
                            Match match = Regex.Match(line, @"(^|[^\w])<stdio.h>([^\w]|$)");
                            if (match.Success)
                            {
                                this.Convertor_stdio_h('r', ref line, 0);   // register keywords for stdio.h

                                line = @"using System;" + System.Environment.NewLine;
                                line += @"using System.Collections.Generic;" + System.Environment.NewLine;
                                line += @"using System.Text;";
                            }

                            // stdlib.h => register keywords and delete a line
                            match = Regex.Match(line, @"(^|[^\w])<stdlib.h>([^\w]|$)");
                            if (match.Success)
                            {
                                this.Convertor_stdlib_h('r', ref line, 0);
                                line = "";
                            }

                            // string.h skip
                            match = Regex.Match(line, @"(^|[^\w])<string.h>([^\w]|$)");
                            if (match.Success)
                            {
                                this.Convertor_string_h('r', ref line, 0);
                                line = "";
                            }

                            break;

                        case 6: // char

                            string patternCharArray = @"(^|[^\w])(char)(\s+)(\w+)(\[\d+\])"; // pattern for char array (ex. char buffer[100])
                            if (Regex.IsMatch(line, patternCharArray))
                            {
                                line = Regex.Replace(line, patternCharArray, m => m.Groups[1].Value + "string" + m.Groups[3].Value + m.Groups[4].Value);
                            }
                            break;

                        case 8: // NULL

                            string patternNull = @"(^|[^\w])(NULL)([^\w]|$)"; // pattern for char array (ex. char buffer[100])
                            if (Regex.IsMatch(line, patternNull))
                            {
                                line = Regex.Replace(line, patternNull, m => m.Groups[1].Value + "null" + m.Groups[3].Value);
                            }
                            break;

                    }// end of switch(matchKeywordNum)

                    break;
            }// end of switch(type)

        }

        /// <summary>
        /// It register keywords or convert to c# code for keywords in stdio.h header (ex. printf)
        /// </summary>
        /// <param name="mode"> 'r' = for register keywords, 'c' = for convert c to c#</param>
        /// <param name="line"> the source code line to be converted to c# </param>
        /// <param name="matchKeywordNum"> which keyword is matched </param>
        private void Convertor_stdio_h(char mode, ref string line, int matchKeywordNum)
        {
            // 'r' is for register keywords, 'c' is for converting keywords
            switch (mode)
            {
                case 'r':   // register keywords

                    int fileIndex = this.GetLastIndex_keywordsTable(1, -1) + 1;

                    this.keywordsTable[fileIndex, 0] = "stdio.h";
                    this.keywordsTable[fileIndex, 1] = "printf";
                    this.keywordsTable[fileIndex, 2] = "gets";
                    this.keywordsTable[fileIndex, 3] = "FILE";
                    this.keywordsTable[fileIndex, 4] = "fopen";
                    this.keywordsTable[fileIndex, 5] = "fgets";
                    this.keywordsTable[fileIndex, 6] = "fclose";
                    this.keywordsTable[fileIndex, 7] = "fprintf";
                    break;

                case 'c':   // convert c keyword to c# code

                    switch (matchKeywordNum)    // check which keyword is found in a line
                    {
                        case 1: // keyword: printf 

                            string patternPrintf = @"(^|[^\w])(printf)([^\w])";
                            Regex rNewLine = new Regex(@"(\\n)(\s*"")");      // Regex pattern for \n at the end of string.

                            if (rNewLine.IsMatch(line)) // including new line char in string
                            {
                                line = Regex.Replace(line, patternPrintf, m => m.Groups[1].Value + @"System.Console.WriteLine" + m.Groups[3].Value);
                                line = rNewLine.Replace(line, m => "" + m.Groups[2].Value);
                            }
                            else                        // no new line char
                            {
                                line = Regex.Replace(line, patternPrintf, m => m.Groups[1].Value + @"System.Console.Write" + m.Groups[3].Value);
                            }

                            string patternFormat = @"(""[^""^%]*)(%(\d)*([dDfFsSuUcC]))";
                            Match matchFormat = Regex.Match(line, patternFormat);

                            int i = 0;
                            while (matchFormat.Success)
                            {
                                if (matchFormat.Groups[3].Success)
                                    line = Regex.Replace(line, patternFormat, m => m.Groups[1].Value + "{" + i++ +
                                    "," + matchFormat.Groups[3].Value + ":" + matchFormat.Groups[4].Value + "}");
                                else
                                    line = Regex.Replace(line, patternFormat, m => m.Groups[1].Value + "{" + i++ +
                                    ":" + matchFormat.Groups[4].Value + "}");
                                matchFormat = Regex.Match(line, patternFormat);
                            }
                                

                            break;

                        case 2: // keyword: gets

                            string patternGets = @"(^|[^\w])(gets)(\s*)(\()(\w+)(\))";
                            if (Regex.IsMatch(line, patternGets))
                            {
                                line = Regex.Replace(line, patternGets, m => m.Groups[1].Value + m.Groups[5].Value + " = " +
                                        @"System.Console.ReadLine()");
                            }

                            break;

                        case 3: // keyword: FILE

                            string patternFopen = @"(^|[^\w])(fopen)([^\w])";
                            if (Regex.IsMatch(line, patternFopen))  // ex. FILE fp = fopen(..) ==> fp = fopen(..)
                            {
                                string patternFile = @"(^|[^\w])(FILE)([^\w])";
                                line = Regex.Replace(line, patternFile, m => "");
                            }
                            else  // ex. FILE fp  ==> delete all.
                            {
                                line = "";
                            }

                            break;

                        case 4: // keyword: fopen

                            string patternFopen1 = @"(^|[^\w])(\w+)(\s*=\s*)(fopen)(\s*\([^,]*)(,\s*"")([a-zA-Z]+)(""\s*\))";
                            Match match = Regex.Match(line, patternFopen1);
                            if (match.Success)
                            {
                                if (match.Groups[7].Value == "w")   // FILE for write
                                {
                                    line = Regex.Replace(line, patternFopen1, m => m.Groups[1].Value + "System.IO.StreamWriter " + m.Groups[2].Value +
                                             m.Groups[3].Value + "new System.IO.StreamWriter" + m.Groups[5].Value + ")");
                                }

                                else if (match.Groups[7].Value == "r")  // FILE for read
                                {
                                    line = Regex.Replace(line, patternFopen1, m => m.Groups[1].Value + "System.IO.StreamReader " + m.Groups[2].Value +
                                             m.Groups[3].Value + "new System.IO.StreamReader" + m.Groups[5].Value + ")");
                                }

                            }

                            break;

                        case 5: // keyword: fgets

                            string patternFgets = @"(^|[^\w])(fgets)(\s*\()(\w+)(\s*,\s*[^,]+,\s*)(\w+)(\s*\)\s*)((!=)?(==)?)";
                            Match matchFgets = Regex.Match(line, patternFgets);
                            if (matchFgets.Success)
                            {
                                if (matchFgets.Groups[8].Success)
                                {
                                    line = Regex.Replace(line, patternFgets, m => "(" + m.Groups[1].Value + m.Groups[4].Value +
                                            " = " + m.Groups[6].Value + ".ReadLine()" + ") " + m.Groups[8].Value);
                                }
                                else
                                {
                                    line = Regex.Replace(line, patternFgets, m => m.Groups[1].Value + m.Groups[4].Value +
                                            " = " + m.Groups[6].Value + ".ReadLine()");
                                }
                            }

                            break;

                        case 6: // keyword: fclose

                            string patternFclose = @"(^|[^\w])(fclose)(\s*\()(\w+)(\s*\))";
                            if (Regex.IsMatch(line, patternFclose))
                            {
                                line = Regex.Replace(line, patternFclose, m => m.Groups[1].Value + m.Groups[4].Value +
                                                     ".Close()");
                            }

                            break;

                        case 7: // keyword: fprintf

                            string patternFprintf = @"(^|[^\w])(fprintf)(\s*\()(\w+)(,\s*)([^\)]*)";
                            Regex rNewLine1 = new Regex(@"(\\n)(\s*"")");      // Regex pattern for \n at the end of string.

                            if (rNewLine1.IsMatch(line))
                            {
                                line = Regex.Replace(line, patternFprintf, m => m.Groups[1].Value + m.Groups[4].Value +
                                             ".WriteLine(" + m.Groups[6].Value);
                                line = rNewLine1.Replace(line, m => "" + m.Groups[2].Value);
                            }
                            else
                            {
                                line = Regex.Replace(line, patternFprintf, m => m.Groups[1].Value + m.Groups[4].Value +
                                             ".Write(" + m.Groups[6].Value + ")");
                            }


                            break;

                    }// end of switch(matchKeywordNum)


                    break;
            }// end of switch(type)
        }

        /// <summary>
        /// It register keywords or convert to c# code for keywords in stdlib.h header 
        /// </summary>
        /// <param name="mode"> 'r' = for register keywords, 'c' = for convert c to c#</param>
        /// <param name="line"> the source code line to be converted to c# </param>
        /// <param name="matchKeywordNum"> which keyword is matched </param>
        private void Convertor_stdlib_h(char mode, ref string line, int matchKeywordNum)
        {
            // 'r' is for register keywords, 'c' is for converting keywords
            switch (mode)
            {
                case 'r':   // register keywords

                    int fileIndex = this.GetLastIndex_keywordsTable(1, -1) + 1;

                    this.keywordsTable[fileIndex, 0] = "stdlib.h";
                    this.keywordsTable[fileIndex, 1] = "atoi";
                    break;

                case 'c':   // convert c keyword to c# code

                    switch (matchKeywordNum)    // check which keyword is found in a line
                    {
                        case 1: // keyword: atoi 

                            string patternAtoi = @"(^|[^\w])(atoi)([^\w])";

                            if (Regex.IsMatch(line, patternAtoi))
                            {
                                line = Regex.Replace(line, patternAtoi, m => m.Groups[1].Value + "Int32.Parse" + m.Groups[3].Value);
                            }

                            break;

                    }// end of switch(matchKeywordNum)


                    break;
            }// end of switch(type)
        }

        /// <summary>
        /// It register keywords or convert to c# code for keywords in string.h header (ex. printf)
        /// </summary>
        /// <param name="mode"> 'r' = for register keywords, 'c' = for convert c to c#</param>
        /// <param name="line"> the source code line to be converted to c# </param>
        /// <param name="matchKeywordNum"> which keyword is matched </param>
        private void Convertor_string_h(char mode, ref string line, int matchKeywordNum)
        {
            // 'r' is for register keywords, 'c' is for converting keywords
            switch (mode)
            {
                case 'r':   // register keywords

                    int fileIndex = this.GetLastIndex_keywordsTable(1, -1) + 1;

                    this.keywordsTable[fileIndex, 0] = "string.h";
                    this.keywordsTable[fileIndex, 1] = "strlen";
                    break;

                case 'c':   // convert c keyword to c# code

                    switch (matchKeywordNum)    // check which keyword is found in a line
                    {
                        case 1: // keyword: strlen 

                            string patternStrlen = @"(^|[^\w])(strlen\s*\(\s*)([^)|^\s]+)(\s*\))";

                            if (Regex.IsMatch(line, patternStrlen))
                            {
                                line = Regex.Replace(line, patternStrlen, m => m.Groups[1].Value + m.Groups[3].Value + ".Length");
                            }

                            break;

                    }// end of switch(matchKeywordNum)


                    break;
            }// end of switch(type)
        }

        /// <summary>
        /// get the last index for keywordsTable[1d][2d]
        /// </summary>
        /// <param name="whichDimention">indicate which dim want (1 or 2 dimension)</param>
        /// <param name="which1Dim">if want 2 dimention, indicatie the index of 1 dimention</param>
        /// <returns>the last index which want, if fails, returns -1</returns>
        private int GetLastIndex_keywordsTable(int whichDimention, int which1Dim)
        {
            int i = 0;
            switch (whichDimention)
            {
                case 1:     // find the next 1d-index after the last 1d-index;

                    while (this.keywordsTable[i, 0] != null)
                    {
                        ++i;
                        if (i >= PortWizardFromCtoCS.MAX_HEADER_FILES) return -1;   // if index go over the boundary, return error
                    }
                    break;

                case 2:     // find the next 2d-index after the last index 2d-index in which1Dim(1-dimention)

                    while (this.keywordsTable[which1Dim, i] != null)
                    {
                        ++i;
                        if (i >= PortWizardFromCtoCS.MAX_KEYWORDS) return -1;   // if index go over the boundary, return error
                    }
                    break;
            }

            return i - 1;
        }
    }// end of class PortWizardFromCtoCS
}
