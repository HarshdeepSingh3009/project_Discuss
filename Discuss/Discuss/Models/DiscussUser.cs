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
    using Discuss.Helpers;
    using Discuss.Models;
    using System.Web.Mvc;

    [Table("DiscussUser")]
    public partial class DiscussUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DiscussUser()
        {
            this.Groups = new HashSet<Group>();
            this.Memberships = new HashSet<Membership>();
            UniversityList = GenericHelper.GetUniversityList();
            DepartmentList = GenericHelper.GetDepartmentList();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        [Required]
        [Display (Name ="First Name")]
        [StringLength(200)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(200)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Alternate E-mail")]
        [StringLength(200)]
        public string email { get; set; }

        [Required]
        [Display(Name = "University")]
        [StringLength(50)]
        public string university { get; set; }

        [StringLength(30)]
        [Display(Name = "Department")]
        public string department { get; set; }
        [Required]
        
        [StringLength(20)]
        [Display(Name = "Join As")]
        public string userrole { get; set; }
    
        public virtual UserAccount UserAccount { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Group> Groups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Membership> Memberships { get; set; }
    }
    public partial class DiscussUser
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> UniversityList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> DepartmentList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> RolesList
        {
            get
            {
                return new[]
                {
            new SelectListItem { Value = "", Text = "", Selected=true },
            new SelectListItem { Value = "Student", Text = "Student" },
            new SelectListItem { Value = "Instructor", Text = "Instructor" },
        };
            }
        }
    }
}
