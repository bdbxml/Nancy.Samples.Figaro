using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// code based on Phillip Wagner's File Uploads with Nancy at 
// http://bytefish.de/blog/file_upload_nancy/
namespace Nancy.Demos.Figaro
{
    /// <summary>
    /// Use our FileUploadHandler to upload the data into the database.
    /// </summary>
    class FileUploadHandler : IFileUploadHandler
    {
        private readonly string root;
        private readonly FigaroDataContext context;
        public FileUploadHandler(FigaroDataContext dataContext, IRootPathProvider provider)
        {
            context = dataContext;
            root = provider.GetRootPath();
        }
        public void HandleUpload(string fileName, Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            Console.WriteLine($"uploading {fileName}...");
            long l = 0;
            var t = Path.GetFileNameWithoutExtension(fileName);
            Console.WriteLine($"writing data for category {t}...");
            var settings = new XmlReaderSettings()
            { CheckCharacters = true, CloseInput = true, ConformanceLevel = ConformanceLevel.Fragment };
            var reader = XmlReader.Create(ms, settings);
            //{
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
                            l++;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0} exception caught writing to db: {1}.  retrying...", DateTime.Now, ex.Message);
                        }
                    }
                }
                reader.Dispose();
            //}
            //flush writes to disk
            context.Environment.SetEnvironmentTransactionCheckpoint(true,0,0);
            context.BeerDb.Sync();
            Console.WriteLine($"{l} records written from {fileName}.");
        }

    }


    public interface IFileUploadHandler
    {
        void HandleUpload(string fileName, Stream stream);
    }
    public class FileUploadResult
    {
        public string Identifier { get; set; }
    }
}
