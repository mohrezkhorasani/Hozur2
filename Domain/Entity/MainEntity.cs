namespace Domain.Entity
{
    public enum MainEntity_Status
    {
        Succedd = 100,
        Enabled = 100,
        IsWaiting = 98
, Default = 101
         , Pin = 999
         , Special = 110
         , Disable = -100
         , Delete = -400
         , Forbidden = -403
         , Canceled = -101
         , Ban = -403
    }
    public enum MainEntity_Types
    {
        Normal = 1,

        // user from 500 
        USER_GUEST = 501,
        USER_MODERATOR = 502,
        USER_ADMINSTRATOR = 503,
        USER_ADMIN = 504,
        USER_USER = 505,
        USER_CREATOR = 506,
        USER_VIEWER = 507,
        USER_TEACHER=508

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
