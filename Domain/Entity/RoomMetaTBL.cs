using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public static class MetaConfigKeys
    {
        public const string ImageCover = "image_cover";
    }
    public class RoomMetaTBL : MainEntity
    {
        public long ID { get; set; }
        public string FriendlyID { get; set; } = Guid.NewGuid().ToString("N");
        public MainEntity_Status Status { get; set; } = MainEntity_Status.Enabled;
        public MainEntity_Types Type { get; set; } = MainEntity_Types.Normal;
        public string? Explain { get; set; }
        public MainEntity_Types DataType { get; set; } = MainEntity_Types.META_STRING;
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public DateTime DateInsert { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RoomTBL")]
        public long IDRoomTBL { get; set; }
        public RoomTBL RoomTBL { get; set; }
    }
}
