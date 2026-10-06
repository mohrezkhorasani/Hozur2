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
        public string? Token { get; set; }

        private string? _agent;
        public string? agent
        {
            get => !string.IsNullOrEmpty(_agent) ? _agent : (Headers?.Agent ?? GeneralHeaders?.Agent);
            set => _agent = value;
        }

        private IGeneralHeaders? _headers;
        public IGeneralHeaders? Headers
        {
            get => _headers;
            set => _headers = value;
        }

        public IGeneralHeaders? GeneralHeaders
        {
            get => _headers;
            set => _headers = value;
        }

        private Guid? _identificationCode;
        public Guid IdentificationCode
        {
            get
            {
                if ((_identificationCode == null || _identificationCode == Guid.Empty) && (Headers != null || GeneralHeaders != null))
                {
                    var h = Headers ?? GeneralHeaders;
                    if (h != null && h.IdentificationCode != Guid.Empty)
                    {
                        return h.IdentificationCode;
                    }
                }
                return _identificationCode ?? (Headers ?? GeneralHeaders)?.IdentificationCode ?? Guid.Empty;
            }
            set
            {
                _identificationCode = value;
            }
        }

        private int? _count = 20;
        public int? Count
        {
            get => _count ?? 20;
            set => _count = (value == 0 || value == null) ? 20 : value;
        }

        private int? _page = 0;
        public int? Page
        {
            get => _page ?? 0;
            set => _page = (value == 0 || value == null) ? 0 : value;
        }
    }
    public class BaseParentResponce
    {
        public int Status { set; get; }
        public string Message { set; get; }

    }
}
