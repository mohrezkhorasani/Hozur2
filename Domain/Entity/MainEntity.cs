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
         , Pin = 999
         , Disable = -100
         , Canceled = -101
         , Delete = -400
         , Forbidden = -403
         , Ban = -403 ,

         //USERS
        USER_ACTIVE = 500,
        USER_ACTIVATED_BY_JOINING_MEETING = 501,
        USER_ACTIVATED_BY_CREATING_MEETING = 502,
        USER_DEACTIVE = -500,
        //Certificates
        CERT_FULL = 600,
        CERT_MEDIUM = 601
    }   
    public enum MainEntity_Types
    {
        Normal = 1,

        // user from 500 
        USER_UNKNOWN = 599,
        USER_GUEST = 501,
        USER_MODERATOR = 502,
        USER_ADMINSTRATOR = 503,
        USER_ADMIN = 504,
        USER_USER = 505,
        USER_CREATOR = 506,
        USER_VIEWER = 507,
        USER_TEACHER = 508,

        //Rooms
        ROOM_IGNORE=99,
        ROOM_HIDDEN=98,
        ROOM_DELETED=-98,
        ROOM_NORMAL=1,

        //cert
        CERT_NORMAL = 600
    }
    public interface MainEntity
    {
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
