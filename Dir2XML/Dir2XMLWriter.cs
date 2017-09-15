using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections.Generic;

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

    public class Dir2XMLWriter
    {

        private string startDirectory;
        private ArrayList listFilesTop = new ArrayList();

        public void CheckDirectory(String StartDirectory)
        {
            startDirectory = StartDirectory;
            DirectoryInfo dirnfo = new DirectoryInfo(StartDirectory);
            startDirectory = dirnfo.FullName;
            GetFileList(startDirectory);

        }

        string MD5hash(string filename)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] hash;
                using (FileStream s = File.OpenRead(filename))
                {
                    hash = md5.ComputeHash(s);
                }
                string r = BitConverter.ToString(hash);
                return r;
            }
        }

        private void GetFileList(string strDirectory)
        {

            DirectoryInfo MyRoot = new DirectoryInfo(strDirectory);

            FileInfo[] MyFiles = MyRoot.GetFiles();

            foreach (FileInfo F in MyFiles)
            {
                listFilesTop.Add(F.FullName);
            }

            DirectoryInfo[] dirList = MyRoot.GetDirectories();

            foreach (DirectoryInfo dir in dirList)
            {
                GetFileList(dir.FullName);
            }

        }

        private Assembly AssemblyLoader(String path, String filename)
        {
            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = path;
            domainSetup.PrivateBinPath = path;
            AppDomain myDomain = AppDomain.CreateDomain("AssemblyLoader", null, domainSetup);
            AssemblyName assName = AssemblyName.GetAssemblyName(path + "\\" + filename);
            myDomain.Load(assName);
            AssemblyName assName2 = AssemblyName.GetAssemblyName(path + "\\" + filename);
            Assembly assembly = myDomain.Load(assName2);
            AppDomain.Unload(myDomain);
            return assembly;
        }

        private List<Assembly> GetDependentAssemblies(AppDomain analyzedAssembly, Assembly baseAssemble)
        {

            List<Assembly> assemle = new List<Assembly>();
            Assembly[] asseblyList = analyzedAssembly.GetAssemblies();
            foreach (Assembly ass in asseblyList)
            {
                if (ass != baseAssemble)
                {
                    assemle.Add(ass);
                }
            }

            return assemle;

        }

        public void WriteXML(String fileName, String title)
        {
            FileInfo info = new FileInfo(fileName);
            String fullName = info.FullName;
            Console.WriteLine("Dir2XML> is started ");
            String blockFileName = info.Name;
            info = null;

            using (XmlTextWriter writer = new XmlTextWriter(fullName, null) { Formatting = Formatting.Indented })
            {
                writer.WriteStartDocument();
                writer.WriteComment(@"
                        ===================================================================
                                             Automatic Update System 
            
                                             Author: Vladimir Novick. 
                                      http://www.linkedin.com/in/vladimirnovick
                        ===================================================================
                                      (c) Vladimir Novick 2005-2014   
                                AUTOMATICALLY GENERATED FILE. DO NOT MODIFY ! 
                        ===================================================================");
                //Write the root element
                writer.WriteStartElement("SGcombo.Updates");
                writer.WriteAttributeString("FormatVersion", "3.0");
                writer.WriteStartElement("Information");
                writer.WriteAttributeString("Created", DateTime.Now.ToString());
                writer.WriteElementString("title", title);
                writer.WriteEndElement();
                writer.WriteStartElement("FilesInfo");
                AppDomain ad2 = null;
                foreach (object itemObj in listFilesTop)
                {
                    string item = itemObj as String;
                    FileInfo fi = new FileInfo(item);
                    if (blockFileName != fi.Name)
                        if (fi.FullName.IndexOf(".svn") < 0)
                        {
                            FileAttributes attributes = fi.Attributes;

                            if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                            {
                                Guid guid = Guid.NewGuid();
                                string md5 = MD5hash(fi.FullName);
                                writer.WriteStartElement("File");
                                writer.WriteAttributeString("Name", fi.Name);
                                writer.WriteAttributeString("Guid", String.Format("{{{0}}}", guid));
                                writer.WriteAttributeString("UpdateFile", item.Substring(startDirectory.Length));
                                writer.WriteAttributeString("CreationTime", fi.CreationTimeUtc.ToString());
                                writer.WriteAttributeString("LastAccessTime", fi.LastWriteTimeUtc.ToString());
                                writer.WriteAttributeString("CID", fi.CreationTimeUtc.ToBinary().ToString());
                                writer.WriteAttributeString("LID", fi.LastWriteTimeUtc.ToBinary().ToString());
                                writer.WriteAttributeString("Length", fi.Length.ToString());
                                writer.WriteAttributeString("Keypad", String.Format("{{{0}}}", md5));
                                String NetName = "";
                                if (fi.Extension.ToLower() == ".dll")
                                {
                                    //    writer.WriteStartElement("Dependencies");
                                    try
                                    {
                                        String patch = Path.GetDirectoryName(fi.FullName);
                                        Directory.SetCurrentDirectory(patch);

                                        AssemblyName assName = AssemblyName.GetAssemblyName(fi.FullName);

                                        NetName = assName.FullName.ToString();

                                        AppDomainSetup setup = new AppDomainSetup();

                                        setup.ApplicationBase = fi.Directory.FullName;

                                        setup.PrivateBinPath = fi.Directory.FullName;
                                        setup.DisallowBindingRedirects = false;
                                        setup.DisallowCodeDownload = true;

                                        setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                                        AppDomain domain = AppDomain.CreateDomain(

                                          "SecondAD", AppDomain.CurrentDomain.Evidence, setup);

                                        Assembly assembly = domain.Load(fi.FullName);

                                        List<Assembly> dependencies = GetDependentAssemblies(domain, assembly);
                                        writer.WriteStartElement("Dependencies");

                                        foreach (Assembly a in dependencies)
                                        {
                                            //         writer.WriteStartElement("Dependency");
                                            //         try
                                            //         {
                                            //             writer.WriteAttributeString("Net", a.FullName.ToString());
                                            //             writer.WriteAttributeString("Location", a.Location);
                                            //         }
                                            //         catch (Exception) { }
                                            //         writer.WriteEndElement();
                                        }

                                        AppDomain.Unload(domain);

                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    finally
                                    {
                                        //       writer.WriteEndElement();
                                    }

                                    writer.WriteAttributeString("NET", NetName.ToString());

                                }

                                writer.WriteEndElement();
                            }
                        }
                }
                writer.WriteFullEndElement();
                writer.Close();
            }

            Console.WriteLine(String.Format("Dir2XML> File: {0} created", fullName));

        }

    }
}
