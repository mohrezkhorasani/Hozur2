using Application.DTO.Generals;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Presenter
{
    public sealed class ApiPresenter<T> : IOutPutPort<GenericResponse<T>>
    {
        public JsonContentResult jsonContentResult { get; }

        public ApiPresenter(JsonContentResult jsonContentResult)
        {
            this.jsonContentResult = jsonContentResult;
        }

        public void Handle(GenericResponse<T> resp)
        {
            try
            {

                jsonContentResult.StatusCode = (int)(resp.success ?
                    HttpStatusCode.OK : HttpStatusCode.BadRequest);
                jsonContentResult.Content = jsonContentResult.Serialize
                    (resp.success ? (object)resp.data : (object)resp.erros);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
