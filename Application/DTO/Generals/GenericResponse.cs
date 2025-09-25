using System.Collections.Generic;

namespace Application.DTO.Generals
{
    public class GenericResponse<T> : UseCaseResponseMessage
    {
        public T data { get; }
        public IEnumerable<Error> erros { get; }

        public GenericResponse(IEnumerable<Error> erros, bool success = false, string msg = null) : base(success, msg)
        {
            this.erros = erros;
        }
        public GenericResponse(T data, bool success = true, string msg = "") : base(success, msg)
        {
            this.data = data;
        }
        public GenericResponse(bool success = false, string msg = "") : base(success, msg)
        {
        }

    }
    public sealed class Error
    {
        public string code { get; set; }
        public string description { get; set; }
        public string stack { get; set; }
        public Error(string code, string description, string stack = "")
        {
            this.code = code;
            this.description = description;
#if Debug
			this.stack = stack;
#endif
        }
    }


    public abstract class UseCaseResponseMessage
    {
        public bool success { get; set; }
        public string msg { get; set; }

        protected UseCaseResponseMessage(bool success, string msg)
        {
            this.success = success;
            this.msg = msg;
        }
    }
}
