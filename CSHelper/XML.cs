using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;

namespace CSHelper
{
    public class XML
    {
        /// <summary>
        /// 返回XML文件中指定节点的值
        /// </summary>
        /// <param name="fileName">XML文件绝对路径</param>
        /// <param name="nodeName">节点名称</param>
        /// <returns>节点的值</returns>
        public string ReadXml(string fileName, string nodeName)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds.ReadXml(fileName);
            foreach (DataTable dt in ds.Tables)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    if (dc.ColumnName == nodeName)
                    {
                        result = dt.Rows[0][nodeName].ToString().Trim();
                        break;
                    }
                }
                if (result.Length > 0)
                    break;
            }
            return result;
        }

        /// <summary>
        /// 读取XML文件节点键值集合
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public Dictionary<string, string> ReadXml(string file)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();
            DataSet ds = new DataSet();
            ds.ReadXml(file);
            foreach (DataColumn col in ds.Tables[0].Columns)
            {
                dics.Add(col.ColumnName, ds.Tables[0].Rows[0][col].ToString());
            }
            return dics;
        }

        /// <summary>
        /// 设置XML文件中节点的值
        /// </summary>
        /// <param name="nodeNameAndValues">节点名和值的二维数组</param>
        public void SaveXml(string file, Dictionary<string, string> dics)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(file);
                foreach (string node in dics.Keys)
                {
                    XmlNode xn = xd.SelectSingleNode("//" + node);
                    xn.FirstChild.Value = dics[node];
                }
                xd.Save(file);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 设置XML文件中指定节点的值
        /// </summary>
        /// <param name="fileName">XML文件绝对路径</param>
        /// <param name="nodeName">节点名称</param>
        /// <param name="nodeText">设置的值</param>
        /// <returns>1:保存成功</returns>
        public string SaveXml(string fileName, string nodeName, string nodeText)
        {
            string result = "";
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(fileName);
                //xd.DocumentElement;
                XmlNode xnl = xd.SelectSingleNode("//" + nodeName);
                xnl.InnerText = nodeText;
                xd.Save(fileName);
                result = "1";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
