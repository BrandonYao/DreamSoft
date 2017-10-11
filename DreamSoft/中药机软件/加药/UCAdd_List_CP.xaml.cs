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
using System.Data;
using System.Globalization;

namespace DreamSoft
{
    /// <summary>
    /// UCAdd_List.xaml 的交互逻辑
    /// </summary>
    public partial class UCAdd_List_CP : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();

        public UCAdd_List_CP()
        {
            InitializeComponent();
        }

        public class Drug
        {
            public int Num { get; set; }
            public int Short { get; set; }
            public string ShortPercent { get; set; }
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
        }
        //显示缺药信息
        private void ShowDrug()
        {
            //可容量
            string sql = "select sum(isnull(drugnummax,0)) as sum,sum(isnull(drugnum,0)) as num from drug_pos where maccode='{0}' group by maccode";
            sql = string.Format(sql, Config.Soft.MacCode);
            DataTable dtNum;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtNum);
            if (dtNum != null && dtNum.Rows.Count > 0)
            {
                tbSum.Text = dtNum.Rows[0]["sum"].ToString();
                tbShort.Text = (int.Parse(dtNum.Rows[0]["sum"].ToString()) - int.Parse(dtNum.Rows[0]["num"].ToString())).ToString();
            }
            sql = @"select sum(drugnum) as num,sum(drugnummax-drugnum) as short,cast(sum(drugnummax-drugnum)*1.0/sum(drugnummax)*100 as decimal(18,2)) as shortPercent,
dp.drugonlycode,di.drugname,di.drugspec,drugfactory from drug_pos dp 
left join drug_info di on dp.drugonlycode=di.drugonlycode 
where maccode='{0}' group by dp.drugonlycode,di.drugname,di.drugspec,drugfactory 
having sum(drugnummax-drugnum)>0";
            switch (cbSort.SelectedIndex)
            {
                case 0:
                    sql += "order by num";
                    break;
                case 1:
                    sql += "order by short desc";
                    break;
                case 2:
                    sql += "order by shortPercent desc";
                    break;
                default:
                    sql += "order by num";
                    break;
            }
            sql = string.Format(sql, Config.Soft.MacCode);
            DataTable dtDrug;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);

            Drug[] ds = null;
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                ds = new Drug[dtDrug.Rows.Count];
                for (int r = 0; r < dtDrug.Rows.Count; r++)
                {
                    ds[r] = new Drug
                    {
                        Num = int.Parse(dtDrug.Rows[r]["num"].ToString()),
                        Short = int.Parse(dtDrug.Rows[r]["short"].ToString()),
                        ShortPercent = dtDrug.Rows[r]["ShortPercent"].ToString(),
                        DrugOnlyCode = dtDrug.Rows[r]["drugonlycode"].ToString(),
                        DrugName = dtDrug.Rows[r]["drugname"].ToString(),
                        DrugSpec = dtDrug.Rows[r]["drugspec"].ToString(),
                        DrugFactory = dtDrug.Rows[r]["drugfactory"].ToString()
                    };
                }
            }
            lvList.ItemsSource = ds;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowDrug();
        }
        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            ShowDrug();
        } 
        //打印
        private void btPrint_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (lvList.Items.Count > 0)
            {
                Drug[] ds = lvList.ItemsSource as Drug[];

                PrintDialog pd = new PrintDialog();

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    double space_X = 20;
                    double space_Y = 10;

                    double size_G = 16;
                    double size_L = 14;

                    double wide_Col = 40;
                    //标题
                    FormattedText title = new FormattedText("加药清单-" + DateTime.Now.ToString(),
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_G, Brushes.Black);
                    dc.DrawText(title, new Point((pd.PrintableAreaWidth - title.Width) / 2, space_Y));//居中
                    space_Y += title.Height;
                    //横线
                    dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X, space_Y), new Point(pd.PrintableAreaWidth - space_X, space_Y));
                    //列标题
                    FormattedText col_1 = new FormattedText("缺药",
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_L, Brushes.Black);
                    dc.DrawText(col_1, new Point(space_X + (wide_Col-col_1.Width)/2 , space_Y)); 
                    FormattedText col_2 = new FormattedText("名称/规格/厂家",
                         CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_L, Brushes.Black);
                    dc.DrawText(col_2, new Point(space_X + wide_Col + 10, space_Y));
                    //3条竖线
                    dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X, space_Y), new Point(space_X, space_Y + col_2.Height));
                    dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X + wide_Col, space_Y), new Point(space_X + wide_Col, space_Y + col_2.Height));
                    dc.DrawLine(new Pen(Brushes.Black, 1), new Point(pd.PrintableAreaWidth - space_X, space_Y), new Point(pd.PrintableAreaWidth - space_X, space_Y + col_2.Height));

                    space_Y += col_2.Height;
                    //横线
                    dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X, space_Y), new Point(pd.PrintableAreaWidth - space_X, space_Y));
                    //循环
                    foreach(Drug d in ds)
                    {
                        //名称
                        FormattedText name = new FormattedText(d.DrugName,
                         CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_L, Brushes.Black);
                        dc.DrawText(name, new Point(space_X + wide_Col + 10, space_Y));
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X + wide_Col, space_Y), new Point(pd.PrintableAreaWidth - space_X, space_Y));
                        //规格
                        FormattedText spec = new FormattedText(d.DrugSpec,
                          CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_L, Brushes.Black);
                        dc.DrawText(spec, new Point(space_X + wide_Col + 10, space_Y + name.Height));
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X + wide_Col, space_Y + name.Height), new Point(pd.PrintableAreaWidth - space_X, space_Y + name.Height));
                        //厂家
                        FormattedText fac = new FormattedText(d.DrugFactory,
                         CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_L, Brushes.Black);
                        dc.DrawText(fac, new Point(space_X + wide_Col + 10, space_Y + name.Height * 2));
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X + wide_Col, space_Y + name.Height * 2), new Point(pd.PrintableAreaWidth - space_X, space_Y + name.Height * 2));
                        //缺药
                        FormattedText num = new FormattedText(d.Short.ToString(),
                         CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), 30, Brushes.Black);
                        dc.DrawText(num, new Point(space_X + (wide_Col - num.Width) / 2, space_Y + (name.Height * 3 - num.Height) / 2));
                        //三条竖线
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X, space_Y), new Point(space_X, space_Y + name.Height * 3));
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X + wide_Col, space_Y), new Point(space_X + wide_Col, space_Y + name.Height * 3));
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(pd.PrintableAreaWidth - space_X, space_Y), new Point(pd.PrintableAreaWidth - space_X, space_Y + name.Height * 3));
                        //横线
                        dc.DrawLine(new Pen(Brushes.Black, 1), new Point(space_X, space_Y + name.Height * 3), new Point(pd.PrintableAreaWidth - space_X, space_Y + name.Height * 3));
                    
                        space_Y += name.Height * 3;
                    }
                    //空白
                    FormattedText n = new FormattedText(".",
                     CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), size_L, Brushes.Black);
                    dc.DrawText(n, new Point(space_X, space_Y + n.Height));

                    dc.DrawText(n, new Point(space_X, space_Y + n.Height * 2));
                        
                }
                pd.PrintVisual(dv, "");
            } 
            Cursor = null;
        }
    }
}
