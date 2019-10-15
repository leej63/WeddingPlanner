using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Attendee
    {
        [Key]
        public int AttendeeId {get;set;}

        public int UserId {get;set;}

        public int WeddingId{get;set;}

        public User User {get;set;}             // must be same as name of what was speicified in Id property above

        public Wedding Wedding {get;set;}       // must be same as name of what was speicified in Id property above
    }
}