using System;

namespace Application.DTO.Generals
{
    public class BaseParentRequest
    {
        public string Token { get; set; }
        public IGeneralHeaders GeneralHeaders { get; set; }
        public int? Page { get; set; }

    }
    public class BaseParentResponce
    {
        public int Status { set; get; }
        public string Message { set; get; }

    }
}
