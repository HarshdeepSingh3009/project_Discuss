//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Discuss.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;
    using Discuss.Helpers;

    public partial class Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Group()
        {
            this.Memberships = new HashSet<Membership>();
            UniversityList = GenericHelper.GetUniversityList();
            DepartmentList = GenericHelper.GetDepartmentList();
            CourseList = GenericHelper.GetCourseList();
        }
    
        public int id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Group Name is required Field.")]
        [Display (Name ="Group Name")]
        public string groupname { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Please select a Department")]
        public string department { get; set; }

        [Display(Name = "Enter Estimate Enrollment")]
        public int? estimateEnrollment { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Please select a University")]
        public string university { get; set; }
        [Required]
        [StringLength(30)]
        [Display(Name = "Please select a Course")]
        public string courseCode { get; set; }
        [StringLength(10)]
        [Required]
        [Display(Name = "Please select a Term")]
        public string term { get; set; }
        public int createdBy { get; set; }

        [Column(TypeName = "xml")]
        public string groupContent { get; set; }
    
        public virtual DiscussUser DiscussUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Membership> Memberships { get; set; }
    }

    public partial class Group
    {
        public bool IsMember { get; set; }
        public int SeatsLeft { get; set; }
        public string Creator { get; set; }
        public IEnumerable<SelectListItem> UniversityList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> CourseList { get; set; }
        public IEnumerable<SelectListItem> TermList
        {
            get
            {
                return new[]
                {
            new SelectListItem { Value = "Fall-16", Text = "Fall-2016", Selected=true },
            new SelectListItem { Value = "Spr-17", Text = "Spring-2017" },
            new SelectListItem { Value = "Fall-17", Text = "Fall-2017" },
        };
            }
        }
    }
}