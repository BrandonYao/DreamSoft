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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DreamSoft
{
    /// <summary>
    /// UCManual.xaml 的交互逻辑
    /// </summary>
    public partial class UCManual : UserControl
    {
        public UCManual()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(200);
                gridLayer.RowDefinitions.Add(rd);
                for (int j = 0; j < 5; j++)
                {
                    if (i == 0)
                        gridLayer.ColumnDefinitions.Add(new ColumnDefinition());
                    Button b = new Button();
                    b.FontSize = 18; b.FontWeight = FontWeights.Bold;
                    b.Content = "转到" + (i * 10 + j * 2 + 1).ToString().PadLeft(2, '0');
                    b.Tag = i * 10 + j * 2 + 1;
                    b.Margin = new Thickness(30);
                    b.Click += new RoutedEventHandler(b_Click);
                    gridLayer.Children.Add(b);
                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                }
            }
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            int lay = int.Parse(bt.Tag.ToString());
            PLC_SP.TurnTo(lay);
        }
    }
}
