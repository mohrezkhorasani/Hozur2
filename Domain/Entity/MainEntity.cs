namespace Domain.Entity
{
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
        public int Status { get; set; }
        /**Types:
         * Normal:1
         **/
        public int Type { get; set; }
        public string Explain { get; set; }
        public DateTime DateInsert { get; set; }
    }
}
