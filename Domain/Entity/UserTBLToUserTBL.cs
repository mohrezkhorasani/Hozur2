using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity
{
    public class UserTBLToUserTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public UserTBL Follower { get; set; }
        public long IDFollower { get; set; }
        public UserTBL Following { get; set; }
        public long IDFollowing { get; set; }

    }
}
