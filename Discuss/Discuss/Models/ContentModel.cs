using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;


namespace Discuss.Models
{
   
    public class Post
    {
        public int id { get; set; }
        [Required]
        [Display (Name ="Title")]
        [StringLength(100)]
        public string TopicTitle {get;set;}
        [Required]
        [Display(Name = "Description")]
        public string TopicDescription{get;set;}
        public DateTime PostedDate { get; set; }
        [Required]
        [Display(Name = "Post as")]
        public String PostedBy { get; set; }
        public int PostedbyId { get; set; }
        [Required]
        [Display(Name = "Post to")]
        public string PostedTo { get; set; }
        public bool Liked { get; set; }
        public bool Resolved { get; set; }
        public List<Reply> Replies { get; set; }
        public string PostedDateNonDisplay { get; set; }
        public int GroupId { get; set; }

    }

    public class Reply
    {
        public int id { get; set; }
        public string ReplyText { get; set; }
        public DateTime PostedDate { get; set; }
        public String PostedBy { get; set; }
        public int PostedbyId { get; set; }
        public bool Liked { get; set; }

    }
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Posts")]
    public class Posts
    {   
        public List<Post> Post { get; set; }
    }
}