using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity
{
    public enum MainEntity_Status
    {
        Deactive = 0,
        Default = 101,
        IsWaiting = 98,
        Succedd = 100,
        Enabled = 100
         , Special = 110

         , MEETING_STARTED_LATER = 250 // started meeting but not configured[title,teacher]
         , MEETING_LIVE = 251 // started meeting and configured 
         , MEETING_END = 252
         , MEETING_END_BY_CRON_JOB = 253
         , MEETING_END_BY_HOOK = 254
         , MEETING_END_BY_HANDY = 255


         , Pin = 999
         , Disable = -100
         , Canceled = -101
         , Delete = -400
         , Forbidden = -403
         , Ban = -403 ,

        LOG_NORMAL = 300,
        LOG_REMOVED = -300,
        LOG_KILLED =301,// masalan 10 ta join poshte ham nayad

        PAYMENT_PAID=200,
        PAYMENT_WAITING=201,
        PAYMENT_DENIED=0,


        //USERS
        USER_ACTIVE = 500,
        USER_ACTIVATED_BY_JOINING_MEETING = 501,
        USER_ACTIVATED_BY_CREATING_MEETING = 502,
        USER_ACTIVATED_BY_SIGN_UP_SITE = 503,
        USER_ACTIVATED_BY_REQUEST_JOIN_MEET = 503,
        USER_DEACTIVE = -500,
        //Certificates
        CERT_FULL = 600,
        CERT_MEDIUM = 601,
        //Questions
        

        QUESTION_ANSWERED=610,
        QUESTION_NOT_ANSWERED=611,

        CONTACTUS_SUBMITED=620,
        CONTACTUS_SEEN=621,
    
        FILE_INSERTED=630,
        FILE_DELETED=631,

        USERMEET_REQUESTED=640,
        USERMEET_JOINED=641,

    }
    public enum MainEntity_Types
    {
        Normal = 1,

        //Rooms
        ROOM_IGNORE = 99,
        ROOM_HIDDEN = 98,
        ROOM_DELETED = -98,
        ROOM_NORMAL = 1,
        ROOM_CREATED_BY_ADMIN = 2,

        //Plan
        PLAN_NORMAL=14,
        PLAN_WITH_CERT = 15,

        //Meetings
        MEET_IGNORE = -99,
        MEET_GREEN_LIGHT = 100,
        MEET_API = 101,

        MEETLOG_DOWNLOADABLE=102,
        MEETLOG_DOWNLOAD_FORBIDDEN=103,
        MEETLOG_NORMAL=1,
        META_INT = 140,    // عدد صحیح
        META_STRING = 141,    // رشته متنی
        META_JSON = 142,    // JSON / آرایه
        META_BOOL = 143,    // بولین (True/False)
        META_FLOAT = 144,    // عدد اعشاری
        META_DATE = 145,    // تاریخ
        META_DATETIME = 146,    // تاریخ و زمان
        META_GUID = 147,    // شناسه یکتا (UUID/GUID)
        META_BYTES = 148,    // داده باینری
        META_ENUM = 149,    // مقادیر شمارشی
        META_ARRAY = 150,    // آرایه (لیست)
        META_OBJECT = 151,    // شیء تودرتو
        META_NULL = 152,    // مقدار تهی
        META_EMAIL = 153,    // ایمیل
        META_PHONE = 154,    // شماره تلفن
        META_URL = 155,    // آدرس وب

        // user from 500 
        USER_UNKNOWN = 599,
        USER_GUEST = 501,
        USER_MODERATOR = 502,
        USER_ADMINSTRATOR = 503,
        USER_ADMIN = 504,
        USER_ADMIN2 = 510,
        USER_USER = 505,
        USER_CREATOR = 506,
        USER_VIEWER = 507,
        USER_TEACHER = 508,
        USER_DABIR = 509,
        USER_HOZURI = 511,


        //
        USERMEETLOG_HOOK = 520,

        //ROLE
        ROLE_MANAGER = 500,
        ROLE_FORROOM = 501,

        ROOMTOUSER_SIGNEDUP =550,
        ROOMTOUSER_ADMIN =551,
        ROOMTOUSER_MEMBER =552,
        ROOMTOUSER_SUBSCRIBER =553,

        //cert
        CERT_NORMAL = 600,


        COMMENT_ANONYMOUS = 610,
        COMMENT_NORMAL=609,
        
        CONTACTUS_WEBSITE=620,

        //quiz
        QUIZ_OPTIONAL = 625,
        QUIZ_INPUT = 626,
        QUIZ_OPTIONAL_NOSCORE = 627,

    }
    public interface MainEntity
    {
        //[NotMapped]

        public long ID { get; set; }
        /** Statuses: >0 => ok 
         * Enable,Succedd:100
         * IsWaiting: 98
         * Default: 101
         * Pin: 999
         * Special: 110
         * Disable:-100
         * Delete : -400
         * Forbidden:-403
         * Canceled: -101
         * Ban: -403
        **/
        public MainEntity_Status Status { get; set; }
        /**Types:
         * Normal:1
         **/
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
    }
}
