using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Discuss.Models;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using Discuss.Helpers;
using System.Threading.Tasks;

namespace Discuss.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        /// <summary>
        /// Loads the homepage when the user gets succesfully logged in
        /// deals with Groups list and Posts List
        /// </summary>
        /// <returns></returns>
        public ActionResult HomePage()
        {
            HomeModel model = new HomeModel();
            model.User = Convert.ToInt32(Session["id"]);
            model.grpList = new List<Group>();
            model.postList = new List<Post>();
            model.groupNames = new List<SelectListItem>();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    // fetch informaton from the databse about the groups joined by the logged-in user 
                    List<int> lstids = dbcontext.Memberships.Where(t => t.userId == model.User).Select(i => i.groupId).ToList();
                    foreach (int gid in lstids)
                    { 
                        Group grp = dbcontext.Groups.Where(t => t.id == gid).Select(i => i).FirstOrDefault();
                        model.groupNames.Add(new SelectListItem { Value = Convert.ToString(grp.id), Text = grp.courseCode.ToUpper() + " " + grp.groupname.ToUpper() });
                        if (grp != null)
                        {
                            grp.Creator = dbcontext.DiscussUsers.Where(t => t.id == grp.createdBy).Select(i => i.FirstName + " " + i.LastName).FirstOrDefault(); // adds group creator's name to the group field
                            model.grpList.Add(grp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
            }
            if (model.groupNames.Count > 0)
            {
                model.groupNames.Sort((g1, g2) => g1.Text.CompareTo(g2.Text)); // Sort the group names based on the group names
                if (Session["GroupId"] != null)
                {
                    model.changeGroupTo = Convert.ToString(Session["GroupId"]); // if the user is coming back from other page and was previously on this group.
                    Session["GroupId"] = null;
                }
                else {
                    model.changeGroupTo = model.groupNames.FirstOrDefault().Value; // otherwise select the first group the in the group dropdown as default.
                }

                // from here on we fetch all the information related to the selected group. Posts, comments etc
                string gp = model.grpList.Where(i => i.id == Convert.ToInt32(model.changeGroupTo)).Select(t => t.groupContent).FirstOrDefault();
                if (!string.IsNullOrEmpty(gp))
                {
                    model.postList.AddRange(DesrializeString(gp)); // deserialize the string fromxml format to neded format.
                }
                // remove from the list of posts all the posts that are posted to the instructor by users and are not meant to be seen by anyone else but instructor.
                // so based on user role filter those posts.
                if (Convert.ToString(Session["Role"]) != "Instructor")
                {
                    var temp = model.postList.Where(i => i.PostedTo == "Instructors" && i.PostedbyId != model.User).Select(j => j).ToList();
                    foreach (var t in temp)
                    {
                        model.postList.Remove(t);
                    }
                }
            }

            return View(model);
        }

        /// <summary>
        /// handles post request from the homepage. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult HomePage(string id)
        {
            int tempid = Convert.ToInt32(id);
            HomeModel model = new HomeModel();
            model.User = Convert.ToInt32(Session["id"]);
            model.grpList = new List<Group>();
            model.groupNames = new List<SelectListItem>();
            model.postList = new List<Post>();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    // from here on we fetch all the information related to the selected group. Posts, comments etc
                    Group grp = dbcontext.Groups.Where(t => t.id == tempid).Select(i => i).FirstOrDefault();
                    // deserialize the string fromxml format to neded format.
                    if (!string.IsNullOrEmpty(grp.groupContent))
                    {
                        model.postList.AddRange(DesrializeString(grp.groupContent));// deserialize the string fromxml format to neded format
                    }
                }
            }
            catch (Exception ex)
            { System.Console.Write(ex.StackTrace); }
            if (Convert.ToString(Session["Role"]) != "Instructor")
            {
                // remove from the list of posts ,all the posts that are posted to the instructor by users and are not meant to be seen by anyone else but instructor.
                // so based on user role filter those posts.
                var temp = model.postList.Where(i => i.PostedTo == "Instructors" && i.PostedbyId != model.User).Select(j => j).ToList();
                foreach (var t in temp)
                {
                    model.postList.Remove(t);
                }
            }
            return PartialView("_TabView", model.postList);
        }
        /// <summary>
        /// Loads a partial view with all information related to the selected post in the group(including replies). 
        /// </summary>
        /// <param name="postid"></param>
        /// <param name="grpid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPostData(string postid, string grpid)
        {
            Post post = new Post();
            int pid = Convert.ToInt32(postid);
            int gid = Convert.ToInt32(grpid);
            post = GetPostModel(pid, gid);
            return PartialView("_PostView", post);
        }
        /// <summary>
        /// Fills the POST Model with information related to the selected post. It deserializes the string stored in the databaseand uses required info. 
        /// </summary>
        /// <param name="pid">post id </param>
        /// <param name="gid">group id</param>
        /// <returns></returns>
        private Post GetPostModel(int pid, int gid)
        {
            Post post = new Post();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {

                    Group grp = dbcontext.Groups.Where(t => t.id == gid).Select(i => i).FirstOrDefault();
                    if (!string.IsNullOrEmpty(grp.groupContent))
                    {
                        var lstPost = DesrializeString(grp.groupContent);// deserialise the group contetn string to get posts. 
                        post = lstPost.Where(t => t.id == pid).Select(i => i).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            { System.Console.Write(ex.StackTrace); }
            return post;
        }

        [HttpPost]
        public ActionResult DeletePost(string postid, string grpid)
        {
            int pid = Convert.ToInt32(postid);
            int gid = Convert.ToInt32(grpid);
            Post pst = new Post();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    var grp = dbcontext.Groups.Where(t => t.id == gid).Select(i => i).FirstOrDefault();
                    if (!string.IsNullOrEmpty(grp.groupContent))
                    {
                        var lstPost = DesrializeString(grp.groupContent);
                        pst = lstPost.Where(t => t.id == pid).Select(i => i).FirstOrDefault();
                        lstPost.Remove(pst);
                        var xEle = new XElement("Posts", GetElement(lstPost));
                        var xdoc = new XDocument(xEle);
                        var xmlstring = GetXMLAsString(DocumentExtensions.ToXmlDocument(xdoc));
                        grp.groupContent = xmlstring;
                        dbcontext.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.StackTrace);
                return Json("Fail");
            }
            Session["GroupId"] = gid;
            return Json("Success");
        }


        [HttpPost]
        public ActionResult AddReplyToPost(string postid, string grpid, string replyText)
        {
            Reply reply = new Reply();
            reply.id = Guid.NewGuid().GetHashCode();
            reply.PostedBy = Convert.ToString(Session["Name"]);
            reply.PostedbyId = Convert.ToInt32(Session["id"]);
            reply.PostedDate = DateTime.Now;
            reply.ReplyText = replyText;
            Post newpost = new Post();
            int pid = Convert.ToInt32(postid);
            int gid = Convert.ToInt32(grpid);
            newpost = GetPostModel(pid, gid);
            if (newpost.Replies == null)
            {
                newpost.Replies = new List<Reply>();
                newpost.Replies.Add(reply);
            }
            else {
                newpost.Replies.Add(reply);
            }
            //Add to the database
            try
            {
                newpost = AddToDB(newpost, pid, gid);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
                TempData["ErrorMsg"] = "Oops!!!!, something went wrong. Please try again later.";

            }

            var post = GetPostModel(pid, gid);

            return PartialView("_PostView", post);
        }

        public Post AddToDB(Post post, int pid, int gid)
        {
            using (DISCUSSEntities dbcontext = new DISCUSSEntities())
            {
                Group grp = dbcontext.Groups.Where(i => i.id == gid).Select(t => t).FirstOrDefault();
                if (!string.IsNullOrEmpty(grp.groupContent))
                {
                    var lstPost = DesrializeString(grp.groupContent);
                    var oldpost = lstPost.Where(i => i.id == pid).FirstOrDefault();
                    post.Replies.OrderBy(i => i.PostedDate);
                    var modifiedPostList = lstPost.Remove(oldpost);
                    lstPost.Add(post);
                    var xEle = new XElement("Posts", GetElement(lstPost));
                    var xdoc = new XDocument(xEle);
                    var xmlstring = GetXMLAsString(DocumentExtensions.ToXmlDocument(xdoc));
                    grp.groupContent = xmlstring;
                    dbcontext.SaveChanges();
                }
            }
            return post;
        }
        [HttpPost]
        public ActionResult DeleteReply(string postid, string grpid, string replyId)
        {
            Post post = new Post();
            int pid = Convert.ToInt32(postid);
            int gid = Convert.ToInt32(grpid);
            int rid = Convert.ToInt32(replyId);
            post = GetPostModel(pid, gid);
            var reply = (from c in post.Replies
                         where c.id == rid
                         select c).FirstOrDefault();
            if (reply.PostedbyId == Convert.ToInt32(Session["id"]))
            {
                post.Replies.Remove(reply);
                try
                {
                    post = AddToDB(post, pid, gid);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.StackTrace);
                    TempData["ErrorMsg"] = "Oops!!!!, something went wrong. Please try again later.";

                }
            }
            else {
                TempData["ErrorMsg"] = "Do not have permission to delete the Messsage/Reply.";
            }

            return PartialView("_PostView", post);
        }

        [HttpGet]
        public ActionResult SearchGroups()
        {
            SearchGroup sgmodel = new SearchGroup();
            TempData["ShowDiv"] = 0;
            return View(sgmodel);
        }

        [HttpPost]
        public ActionResult SearchGroups(string university, string department, string term, string course)
        {
            SearchGroup sgmodel = new SearchGroup();
            List<Group> tempList = new List<Group>();
            List<Membership> memberlist = new List<Membership>();
            sgmodel.University = university;
            sgmodel.Department = department;
            sgmodel.Term = term;
            sgmodel.Course = course;
            int id = Convert.ToInt32(Session["id"]);
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    var model = dbcontext.Groups.Where(t => t.university == university
                               && t.department == department
                               && t.term == term && t.courseCode == course).ToList();
                    tempList = model;
                    foreach (var grp in tempList)
                    {
                        var model1 = dbcontext.Memberships.Where(t => t.groupId == grp.id &&
                                                                 t.userId == id).FirstOrDefault();
                        if (model1 != null)
                        {
                            grp.IsMember = true;
                        }
                        else { grp.IsMember = false; }

                        int modelCount = dbcontext.Memberships.Where(t => t.groupId == grp.id).ToList().Count;
                        if (grp.estimateEnrollment.HasValue)
                        {
                            grp.SeatsLeft = (int)grp.estimateEnrollment - modelCount;
                        }

                        var model3 = dbcontext.DiscussUsers.Where(t => t.id == grp.createdBy).Select(i => i.FirstName + " " + i.LastName).FirstOrDefault();
                        grp.Creator = model3;
                    }
                    sgmodel.GrpList = tempList;

                }
            }
            catch (Exception ex)
            { System.Console.Write(ex.StackTrace); }

            TempData["ShowDiv"] = 1;
            return PartialView("_SearchGrid", sgmodel.GrpList);
        }
        [HttpPost]
        public ActionResult JoinLeaveGroup(string action, int id)
        {
            SearchGroup sgmodel = new SearchGroup();
            List<Group> tempList = new List<Group>();
            int userid = Convert.ToInt32(Session["id"]);
            Membership membermodel = new Membership();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    var model = dbcontext.Groups.Where(t => t.id == id).FirstOrDefault();
                    tempList.Add(model);
                }

                foreach (var grp in tempList)
                {
                    if (action == "Join")
                    {
                        using (DISCUSSEntities dbContext = new DISCUSSEntities())
                        {

                            membermodel.groupId = id;
                            membermodel.userId = userid;
                            dbContext.Memberships.Add(membermodel);
                            dbContext.SaveChanges();
                            grp.IsMember = true;
                            int modelCount = dbContext.Memberships.Where(t => t.groupId == grp.id).ToList().Count;
                            if (grp.estimateEnrollment.HasValue)
                            {
                                grp.SeatsLeft = (int)grp.estimateEnrollment - modelCount;
                            }
                            string name = dbContext.DiscussUsers.Where(t => t.id == grp.createdBy).Select(i => i.FirstName + " " + i.LastName).FirstOrDefault();
                            grp.Creator = name;
                        }
                    }
                    else {
                        using (DISCUSSEntities dbcontext1 = new DISCUSSEntities())
                        {
                            var model = dbcontext1.Memberships.Where(t => t.groupId == id && t.userId == userid).Select(i => i).FirstOrDefault();
                            dbcontext1.Memberships.Remove(model);
                            dbcontext1.SaveChanges();


                            int modelCount = dbcontext1.Memberships.Where(t => t.groupId == grp.id).ToList().Count;
                            if (grp.estimateEnrollment.HasValue)
                            {
                                grp.SeatsLeft = (int)grp.estimateEnrollment - modelCount;
                            }
                            string name = dbcontext1.DiscussUsers.Where(t => t.id == grp.createdBy).Select(i => i.FirstName + " " + i.LastName).FirstOrDefault();
                            grp.Creator = name;
                        }
                    }
                }
                sgmodel.GrpList = tempList;

            }

            catch (Exception ex)
            { System.Console.Write(ex.StackTrace); }

            TempData["ShowDiv"] = 1;
            return PartialView("_SearchGrid", sgmodel.GrpList);
        }

        [HttpPost]
        public ActionResult DeleteGroup(string university, string department, string term, string course,string id)
        {
            SearchGroup sgmodel = new SearchGroup();
            List<Group> tempLst = new List<Group>();
            int userid = Convert.ToInt32(Session["id"]);
            int grpid = Convert.ToInt32(id);
            Membership membermodel = new Membership();
            try
            {
                using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                {
                    var model = dbcontext.Groups.Where(t => t.id == grpid).FirstOrDefault();
                    dbcontext.Groups.Remove(model);
                    var tempList = dbcontext.Memberships.Where(t => t.groupId == grpid).Select(i => i).ToList();
                    dbcontext.Memberships.RemoveRange(tempList);
                    dbcontext.SaveChanges();
                }

            } catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
               
            }

            return PartialView("_SearchGrid", sgmodel.GrpList);
        }
        [HttpGet]
        public ActionResult CreateGroup()
        {
            Group grp = new Group();
            grp.UniversityList = grp.UniversityList.OrderBy(m => m.Value);
            grp.DepartmentList = grp.DepartmentList.OrderBy(m => m.Value);
            grp.CourseList = grp.CourseList.OrderBy(m => m.Value);
            return View(grp);
        }

        [HttpPost]
        public ActionResult CreateGroup(Group model)
        {
           
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                Membership memebermodel = new Membership();

                try
                {
                    if (model != null)
                    {
                        model.createdBy = Convert.ToInt32(Session["id"]);
                        using (DISCUSSEntities dbcontext = new DISCUSSEntities())
                        {
                            var duplicateGroup = dbcontext.Groups.Where(t => t.groupname == model.groupname.Trim() && t.term ==model.term && t.university == model.university 
                                                                        && t.department == model.department && t.courseCode == model.courseCode).Select(i=>i).FirstOrDefault();
                            if (duplicateGroup == null)
                            {
                                dbcontext.Groups.Add(model);
                                dbcontext.SaveChanges();
                            }
                            else
                            {
                                TempData["ErrorMsg"] = "Group name already exists !! Please either change the group name or delete existing group.";
                                return View(model);
                            }
                        }
                        using (DISCUSSEntities dbcontext1 = new DISCUSSEntities())
                        {
                            int grpid = dbcontext1.Groups.Where(t => t.groupname == model.groupname
                                          && t.term == model.term && t.createdBy == model.createdBy && t.university == model.university
                                             && t.courseCode == model.courseCode).Select(i => i.id).FirstOrDefault();
                            memebermodel.groupId = grpid;
                            memebermodel.userId = model.createdBy;
                            dbcontext1.Memberships.Add(memebermodel);
                            dbcontext1.SaveChanges();

                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.Write(ex.StackTrace);
                }
                TempData["successMsg"] = "Group Successfully created !!";
                return View(model);
            
        }
        [HttpGet]
        public ActionResult NewPost(string grpid)
        {
            Post model = new Post();
            model.GroupId = Convert.ToInt32(grpid);
            return View(model);
        }
        [HttpPost]
        public ActionResult NewPost(string postto, string title, string desc, string postby, string groupId)
        {
            List<Post> lstpost = new List<Post>();
            Group grp = new Group();
            int grpId = Convert.ToInt32(groupId);
            Post postModel = new Post();
            postModel.id = Guid.NewGuid().GetHashCode();
            postModel.TopicTitle = title;
            postModel.TopicDescription = desc;
            postModel.PostedTo = postto;
            if (postby != "Name")
                postModel.PostedBy = "Anonymous";
            else
                postModel.PostedBy = Convert.ToString(Session["Name"]);
            postModel.PostedbyId = Convert.ToInt32(Session["id"]);
            postModel.PostedDate = DateTime.Now;
            postModel.Replies = null;
            if (string.IsNullOrEmpty(postto) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc) || string.IsNullOrEmpty(postby))
            {
                TempData["FailureMsg"] = "All the fields are compulsory.";
                return View(postModel);
            }
            lstpost.Add(postModel);
            using (DISCUSSEntities dbcontext = new DISCUSSEntities())
            {
                grp = dbcontext.Groups.Where(t => t.id == grpId).Select(i => i).FirstOrDefault();
                if (grp != null)
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(grp.groupContent))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(grp.groupContent);
                            var xEle = GetElement(lstpost);
                            XmlNode xmlnde = doc.ImportNode(XElementExtensions.ToXmlElement(xEle.FirstOrDefault()), true);
                            doc.DocumentElement.AppendChild(xmlnde);
                            var xmlstring = doc.OuterXml;
                            grp.groupContent = xmlstring;
                            dbcontext.SaveChanges();
                        }
                        else
                        {
                            var xEle = new XElement("Posts", GetElement(lstpost));
                            var xdoc = new XDocument(xEle);
                            var xmlstring = GetXMLAsString(DocumentExtensions.ToXmlDocument(xdoc));
                            grp.groupContent = xmlstring;
                            dbcontext.SaveChanges();
                        }


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return View();
                    }
                }
            }

            Session["GroupId"] = grpId;
            return Json("Success");
        }

        public string GetXMLAsString(XmlDocument myxml)
        {
            return myxml.OuterXml;
        }

        public IEnumerable<XElement> GetElement(List<Post> lstpost)
        {
            try
            {
                var xEle = from pst in lstpost
                           select new XElement("Post",
                                                   new XElement("Id", pst.id),
                                                   new XElement("Title", pst.TopicTitle),
                                                   new XElement("PostedTo", pst.PostedTo),
                                                   new XElement("PostedBy", pst.PostedBy),
                                                   new XElement("PostedById", pst.PostedbyId),
                                                   new XElement("PostedDate", pst.PostedDate.Date.ToShortDateString() + " " + pst.PostedDate.ToShortTimeString()),
                                                   new XElement("PostedDateNonDisplay", pst.PostedDate),
                                                   new XElement("Desc", pst.TopicDescription),
                                                   new XElement("Replies", GetReplyElement(pst.Replies))
                                               );

                return xEle;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.StackTrace);
            }
            return null;
        }

        public IEnumerable<XElement> GetReplyElement(List<Reply> lstReply)
        {
            if (lstReply != null && lstReply.Count > 0)
            {
                var xEle = from rply in lstReply
                           select new XElement("Reply",
                                                   new XElement("Id", rply.id),
                                                   new XElement("PostedBy", rply.PostedBy),
                                                   new XElement("PostedById", rply.PostedbyId),
                                                   new XElement("PostedDate", rply.PostedDate),
                                                   new XElement("Text", rply.ReplyText)
                                               );
                return xEle;
            }
            else {
                return null;
            }

        }

        public List<Post> DesrializeString(string content)
        {
            List<Post> lstpost = new List<Post>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodes = root.SelectNodes("Post");

                foreach (XmlNode node in nodes)
                {
                    Post pst = new Post();
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        switch (child.Name)
                        {
                            case "Id":
                                pst.id = Convert.ToInt32(child.InnerText);
                                break;
                            case "Title":
                                pst.TopicTitle = child.InnerText;
                                break;
                            case "Desc":
                                pst.TopicDescription = child.InnerText;
                                break;
                            case "PostedTo":
                                pst.PostedTo = child.InnerText;
                                break;
                            case "PostedBy":
                                pst.PostedBy = child.InnerText;
                                break;
                            case "PostedById":
                                pst.PostedbyId = Convert.ToInt32(child.InnerText);
                                break;
                            case "PostedDate":
                                pst.PostedDate = Convert.ToDateTime(child.InnerText);
                                break;
                            case "PostedDateNonDisplay":
                                pst.PostedDateNonDisplay = child.InnerText;
                                break;
                            case "Replies":
                                pst.Replies = DesrializeStringForReplies(child.SelectNodes("Reply"));
                                break;
                        }
                    }
                    lstpost.Add(pst);
                }
            }
            catch (Exception ex)
            {
                return lstpost;
            }
            return lstpost;
        }
        public List<Reply> DesrializeStringForReplies(XmlNodeList replyNodes)
        {
            List<Reply> lstrply = new List<Reply>();
            try
            {
                foreach (XmlNode node in replyNodes)
                {
                    Reply rply = new Reply();
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        switch (child.Name)
                        {
                            case "Id":
                                rply.id = Convert.ToInt32(child.InnerText);
                                break;
                            case "PostedBy":
                                rply.PostedBy = child.InnerText;
                                break;
                            case "PostedById":
                                rply.PostedbyId = Convert.ToInt32(child.InnerText);
                                break;
                            case "PostedDate":
                                rply.PostedDate = Convert.ToDateTime(child.InnerText);
                                break;
                            case "Text":
                                rply.ReplyText = child.InnerText;
                                break;
                        }
                    }
                    lstrply.Add(rply);
                }
            }
            catch (Exception ex)
            {
                return lstrply;
            }
            return lstrply;
        }
    }
}