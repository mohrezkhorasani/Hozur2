using System;

namespace Application.DTO.Generals
{
    public enum OrderBy
    {
        DATE_DESC=100,
        DATE = 101,
        NAME_DESC = 102,
        NAME = 103,
        SCORE = 104,
        SCORE_DESC = 105,

    }
    public class BaseParentRequest
    {
        public string Token { get; set; }
        public IGeneralHeaders GeneralHeaders { get; set; }
        public int? Page { get; set; }
        public int? Count { get; set; }

    }
    public class BaseParentResponce
    {
        public int Status { set; get; }
        public string Message { set; get; }

    }
}
