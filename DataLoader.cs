using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace Nancy.Demos.Figaro
{
    /// <summary>
    /// The purpose of this class is to load beer.stackexchange.com data into the container 
    /// in a background thread of the Nancy app. 
    /// </summary>
    public class DataLoader
    {
        private readonly FigaroDataContext context;
        public DataLoader(FigaroDataContext dataContext)
        {
            context = dataContext;
            LoadBeerData();
        }

        /// <summary>
        /// Load all of our XML data files from the specified directory.
        /// </summary>
        /// <param name="path"></param>
        public void LoadBeerData()
        {
            do
            {
                Thread.Sleep(100);
            } while (!context.Initialized);


            if (context.GetLoadCount() > 0) return;

            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"beer.stackexchange.com");

                context.ConfigureContainerIndex();

                foreach (var file in Directory.GetFiles(path))
                {
                    int id = 0;
                    var t = Path.GetFileNameWithoutExtension(file);
                    var fs = new FileStream(file, FileMode.Open);
                    var settings = new XmlReaderSettings()
                    { CheckCharacters = true, CloseInput = true, ConformanceLevel = ConformanceLevel.Fragment };
                    using (var reader = XmlReader.Create(fs, settings))
                    {
                        while (!reader.EOF)
                        {
                            if (reader.Name != "row") reader.ReadToFollowing("row");
                            if (reader.EOF) continue;
                            var d = Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding("ISO-8859-1"),
                                Encoding.Default, Encoding.Default.GetBytes(reader.ReadOuterXml())));

                            while (true)
                            {
                                try
                                {
                                    context.InsertItem(d, t);
                                    id++;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("{0} exception caught writing to db: {1}.  retrying...",DateTime.Now,ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine($"{ex.GetType()}:{ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
        }

        //http://stackoverflow.com/questions/20928705/read-and-process-files-in-parallel-c-sharp
        //public void LoadBeerData(string path)
        //{

        //    var settings = new XmlReaderSettings()
        //    { Async = true, CheckCharacters = true, CloseInput = true, ConformanceLevel = ConformanceLevel.Fragment };

        //    var files = Directory.GetFiles(path, "*.xml");
        //    Parallel.ForEach(files, (currentfile) =>
        //    {
        //        var fs = new FileStream(currentfile, FileMode.Open);
        //        int idx = 0;
        //        var nam = Path.GetFileNameWithoutExtension(currentfile);
        //        using (var reader = XmlReader.Create(fs, settings))
        //        {
        //            while (!reader.EOF)
        //            {
        //                if (reader.Name != "row") reader.ReadToFollowing("row");
        //                if (reader.EOF) continue;
        //                context.InsertItem(reader.ReadOuterXml(), nam + idx);
        //                idx++;
        //            }
        //        }
        //    });
        //}
    }
}
