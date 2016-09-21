using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using System.Xml;
using Discuss.Models;
using System.Xml.Linq;

namespace Discuss.Helpers
{
    public static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }
    }
    public static class XElementExtensions
    {
        public static XmlElement ToXmlElement(this XElement el)
        {
            var doc = new XmlDocument();
            doc.Load(el.CreateReader());
            return doc.DocumentElement;
        }
    }
    public static class GenericHelper
    {


        public static List<SelectListItem> GetUniversityList()
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            try
            {
                XmlDocument doc = new XmlDocument();
                string filePath = HttpContext.Current.Server.MapPath("~/App_Data/UniversityList.xml");
                doc.Load(filePath);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node != null)
                    {
                        SelectListItem item = new SelectListItem();
                        item.Value = node.Attributes["Code"].InnerText;
                        item.Text = node.InnerText;
                        lst.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
            }

            return lst;
        }

        public static IEnumerable<System.Web.Mvc.SelectListItem> GetDepartmentList()
        {
            List<System.Web.Mvc.SelectListItem> lst = new List<System.Web.Mvc.SelectListItem>();
            try
            {
                XmlDocument doc = new XmlDocument();
                string filePath = HttpContext.Current.Server.MapPath("~/App_Data/DepartmentList.xml");
                doc.Load(filePath);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node != null)
                    {
                        System.Web.Mvc.SelectListItem item = new System.Web.Mvc.SelectListItem();
                        item.Value = node.Attributes["Code"].InnerText;
                        item.Text = node.InnerText;
                        lst.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
            }
            return lst;
        }

        public static IEnumerable<SelectListItem> GetCourseList()
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            try
            {
                XmlDocument doc = new XmlDocument();
                string filePath = HttpContext.Current.Server.MapPath("~/App_Data/CourseList.xml");
                doc.Load(filePath);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node != null)
                    {
                        System.Web.Mvc.SelectListItem item = new System.Web.Mvc.SelectListItem();
                        item.Value = node.Attributes["Code"].InnerText;
                        item.Text = node.Attributes["Name"].InnerText;
                        lst.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.StackTrace);
            }
            return lst;
        }
    } 
}