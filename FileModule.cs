using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nancy.Demos.Figaro
{
    //to upload a file: curl --verbose -F name=comments -F filedata=@Comments.xml http://localhost:3579/file

    /// <summary>
    /// Uploads the StackExchange files into the database.
    /// </summary>
    /// <seealso cref="http://bytefish.de/blog/file_upload_nancy/"/>
    public class FileModule: NancyModule
    {
        private readonly FigaroDataContext context;
        private readonly IFileUploadHandler handler;

        public FileModule(FigaroDataContext dataContext, IFileUploadHandler fileHandler)
        {
            handler = fileHandler;
            context = dataContext;
            
            Post("/file", o => {
                var file = Request.Files.FirstOrDefault();
                if (file == null) return new Response { StatusCode = HttpStatusCode.ExpectationFailed };
                handler.HandleUpload(file.Name, file.Value);

                return new Response { StatusCode = HttpStatusCode.Accepted };
            });
        }

    }
}
