using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace DreamSoft
{
    /// <summary>
    /// WinMsg.xaml 的交互逻辑
    /// </summary>
    public partial class WinMsg : Window
    {
        static WinMsg win;
        public WinMsg()
        {
            InitializeComponent();
        }
        public static WinMsg GetMsg()
        {
            if (win == null)
                win = new WinMsg();
            return win;
        }
        public void ShowMsg(string msg, Color c)
        {
            TextBlock tb = new TextBlock();
            tb.Text = msg;
            tb.FontSize = 12;
            tb.FontWeight = FontWeights.Bold;
            tb.Foreground = new SolidColorBrush(c);
            tb.Margin = new Thickness(5);
            tb.TextWrapping = TextWrapping.Wrap;
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            sp.Children.Add(tb);
            sv.ScrollToBottom();

            win.Left = SystemParameters.WorkArea.Width - win.Width;
            win.Top = SystemParameters.WorkArea.Height - win.Height - 20;

            DoubleAnimation daOpacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1));
            daOpacity.BeginTime = TimeSpan.FromSeconds(3);

            //DoubleAnimation daHeight = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            //daHeight.BeginTime = TimeSpan.FromSeconds(4);

            daOpacity.Completed += new EventHandler(da_Completed);
            Storyboard.SetTarget(daOpacity, tb);

            tb.BeginAnimation(TextBlock.OpacityProperty, daOpacity);
            //tb.BeginAnimation(TextBlock.HeightProperty, daHeight);

            win.Show();
        }

        void da_Completed(object sender, EventArgs e)
        {
            AnimationTimeline timeline = (sender as AnimationClock).Timeline;
            /* !!! 通过附加属性把UI对象取回 !!! */
            object uiElement = Storyboard.GetTarget(timeline);

            TextBlock t = uiElement as TextBlock;
            StackPanel sp = t.Parent as StackPanel;
            sp.Children.Remove(t);
            

            if (sp.Children.Count <= 0)
            {
                win.Close();
                win = null;
                return;
            }

            win.Left = SystemParameters.WorkArea.Width - win.Width;
            win.Top = SystemParameters.WorkArea.Height - win.Height - 20;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            win = null;
        }
    }
}
