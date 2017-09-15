using System;



////////////////////////////////////////////////////////////////////////////
//	Copyright 2005-2014  , 2017 : Vladimir Novick    https://www.linkedin.com/in/vladimirnovick/  
//
//         https://github.com/Vladimir-Novick/Dir2XML
//
//    NO WARRANTIES ARE EXTENDED. USE AT YOUR OWN RISK. 
//
// To contact the author with suggestions or comments, use  :vlad.novick@gmail.com
//
////////////////////////////////////////////////////////////////////////////


namespace SGcombo.Dir2XML
{
    class Program
    {
        static void Main(string[] args)
        {
            String fileName;
            Dir2XMLWriter writer = new Dir2XMLWriter();
            String directory = ".";
            String title;
            if (args.Length < 3 ) {
                Console.WriteLine(
@"Invalid arguments :
         Dir2XML {drive:}{path} {drive:}{path}{xmlFileName} {DocumentTitle} ");
            } else {
                try
                {
                    fileName = args[1];
                    directory = args[0];
                    title = args[2];

                    writer.CheckDirectory(directory);
                    writer.WriteXML(fileName, title);
                } catch ( Exception ex ){
                    Console.WriteLine("Dir2XML error " + ex.Message );
                }
            }

        }
    }
}
