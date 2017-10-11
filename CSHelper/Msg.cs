using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace CSHelper
{
    public class Msg
    {
        /// <summary>
        /// 显示通知
        /// </summary>
        /// <param name="info">通知文本</param>
        /// <param name="dialog">是否显示对话框</param>
        public void ShowInfo(string info, bool dialog)
        {
            if (dialog)
                MessageBox.Show(info, "系统消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                new FrmMessage(info, Color.Green).Show();

        }

        /// <summary>
        /// 显示错误
        /// </summary>
        /// <param name="warning">错误文本</param>
        /// <param name="dialog">是否显示对话框</param>
        public void ShowWarning(string warning, bool dialog)
        {
            if (dialog)
                MessageBox.Show(warning, "系统消息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                new FrmMessage(warning, Color.Red).Show();
        }

        /// <summary>
        /// 显示确认提示框
        /// </summary>
        /// <param name="question">提问内容</param>
        /// <param name="custom">是否显示自定义对话框</param>
        /// <returns>Yes/No</returns>
        public bool ShowQuestion(string question, bool custom)
        {
            bool result = false;
            if (custom)
            {
                if (new FrmQuestion(question).ShowDialog() == DialogResult.Yes)
                {
                    result = true;
                }
            }
            else
            {
                if (MessageBox.Show(question, "系统消息", MessageBoxButtons.YesNo, MessageBoxIcon.Question,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
