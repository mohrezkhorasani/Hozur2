using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface IWordService
    {
        string CreateCertificatePdf(string pdfPath, string fontPath, string bgImagePath, string Name,string category, string title, string date, string role, string number);
    }

    public interface IScpUploader
    {
        void UploadFile(string localFilePath);
    }
}
