using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class RoomTBLPlanTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public RoomTBL RoomTBL { get; set; }
        public long IDRoomTBL { get; set; }
        public string? Title{ get; set; }
        public string? Teachers{ get; set; }
        public string? Dabir{ get; set; }
        public bool RequiredToEnterName{get;set;} =false;
        public string? PresentationFileURL { get; set; }
        public DateTime? StartDate { get; set; }
        public ICollection<MeetingTBL> MeetingTBLs{get;set;}
        public ICollection<MeetingReservationTBL> MeetingReservationTBLs { get;set;}

        public static (string Name, string? Position) ParsePersonInfo(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return ("", null);
            var trimmed = raw.Trim();
            var match = Regex.Match(trimmed, @"^(.*?)\s*[\(（](.*?)[\)）]\s*$");
            if (match.Success)
            {
                return (match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
            }
            return (trimmed, null);
        }

        public List<(string Name, string? Position)> GetParsedTeachers()
        {
            if (string.IsNullOrWhiteSpace(Teachers)) return new List<(string, string?)>();
            var parts = Teachers.Split(new[] { "،", ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            return parts
                .Select(p => ParsePersonInfo(p))
                .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                .ToList();
        }

        public (string Name, string? Position) GetParsedDabir()
        {
            return ParsePersonInfo(Dabir);
        }
    }
}
