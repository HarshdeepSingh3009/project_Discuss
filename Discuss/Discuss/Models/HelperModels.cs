using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Discuss.Helpers;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Discuss.Models
{
    public class HelperModels
    {
    }
    public class SearchGroup
    {
        public SearchGroup()
        {
            GrpList = new List<Group>();
            UniversityList = GenericHelper.GetUniversityList();
            DepartmentList = GenericHelper.GetDepartmentList();
            CourseList = GenericHelper.GetCourseList();

        }
        [Display(Name = "Select your University")]
        public string University { get; set; }
        [Display(Name = "Select your Department")]
        public string Department { get; set; }
        [Display(Name = "Select a Course")]
        public string Course { get; set; }
        [Display(Name = "Select a Term")]
        public string Term { get; set; }
        public IEnumerable<Group> GrpList { get; set; }
        public int grpResultCount { get; set;}
        public IEnumerable<SelectListItem> UniversityList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> CourseList { get; set; }
        public IEnumerable<SelectListItem> TermList {
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


    public class HomeModel {
        // if get is called 
        // load page with group tab only with all the groups user member is and blank scrren for now
        //select a group show two tabspost and private msgs and a drop down for groups and new post and private message functionality
        //click on a post it shows title and escnext div with ioption to reply.
        public List<Group> grpList { get;set;}
        public int selectedGroup { get; set; } // for on click function
        public string changeGroupTo { get; set; } // for group dropdown
        public List<SelectListItem> groupNames { get; set; }
        public List<Post> postList { get; set; }
        public int User { get; set; }
        public bool showDropDown { get; set; }
        public bool showGroupTabOnly { get; set; }
        public bool showTwoTabs { get; set; }
        /// may be message list

    }

   
}