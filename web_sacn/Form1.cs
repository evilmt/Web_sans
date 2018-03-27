using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SufeiUtil;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;




namespace WindowsFormsApp2
{
    
    public partial class Form1 : Form
    {
        public List<string> dick_dir = new List<string>();//字典内存缓存
        public static int dick_max =0;//字典总数量
        public static UInt64 dick_long2 = 0;//字典数量
        public static UInt64 dick_long = 0;//字典个数
        public static int dick_state = -1;//状态
        public static int threads = 0; //线程
        private System.Timers.Timer T1 = new System.Timers.Timer();// 定时器
        private System.Timers.Timer T2 = new System.Timers.Timer();
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("GET");
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf("不提示");
            listView1.GridLines = true;//表格是否显示网格线
            listView1.FullRowSelect = true;//是否选中整行
            listView1.Scrollable = true;//是否自动显示滚动条
            listView1.MultiSelect = true;//是否可以选择多行
            ListView.CheckForIllegalCrossThreadCalls = false;//使用多线程调用组件
            fliedirdick(Environment.CurrentDirectory+"/dick");// 遍历字典目录
            // --------------------------------------------------
            T1.Enabled = true;
            T1.AutoReset = true;
            T1.Interval = 1000;
            T1.Elapsed += new System.Timers.ElapsedEventHandler(t1_Tick);
            // --------------------------------------------------
            T2.Enabled = true;
            T2.AutoReset = true;
            T2.Interval = 1000;
            T2.Elapsed += new System.Timers.ElapsedEventHandler(t2_Tick);
            T2.Stop();
            // --------------------------------------------------
            
        }
        private void set_listView(string url, string state, string size) //添加到表格
        {
            Monitor.Enter(this);//线程锁
            ListViewItem item = new ListViewItem();
            item.SubItems[0].Text = listView1.Items.Count.ToString();
            item.SubItems.Add(url);
            item.SubItems.Add(state);
            item.SubItems.Add(size);
            listView1.Items.Add(item);
            Monitor.Exit(this);//解除

        }
        private void fliedirdick(string dir) //遍历字典目录
        {
            DirectoryInfo dick = new DirectoryInfo(dir);
            FileSystemInfo[] fsinfos = dick.GetFileSystemInfos();
            foreach (FileSystemInfo fsinfo in fsinfos)
            {
                if (fsinfo is DirectoryInfo)
                {
                    fliedirdick(fsinfo.FullName); // 子目录
                }
                else
                {
                    string fliedick = fsinfo.FullName;
                    fliedick = fliedick.Replace(Environment.CurrentDirectory+"\\dick\\","");
                    checkedListBox1.Items.Add(fliedick);
                }
            }


        }
        private void dickscan(object data) // HTTP发包
        {
            string dick = (string)data;
            Console.WriteLine(dick);
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                //URL = Form1.Get_url(),//URL     必需项  
                URL = textBox1.Text+ dick,
                Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别  
                                //Encoding = Encoding.Default,  
                //Method = "get",//URL     可选项 默认为Get  
                Timeout = Convert.ToInt32(numericUpDown2.Value.ToString())*1000,//连接超时时间     可选项默认为100000  
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000  
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写  
                //Cookie = "",//字符串Cookie     可选项  
                UserAgent = textBox3.Text,//浏览器UA
                //用户的浏览器类型，版本，操作系统     可选项有默认值  
                Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值  
                ContentType = "text/html",//返回类型    可选项有默认值  
                Referer = "https://www.baidu.com",//来源URL     可选项  
                //Allowautoredirect = true,//是否根据３０１跳转
                //CerPath = "d:\\123.cer",//
                Connectionlimit = 1024000,//最大连接数
                //Postdata = "",//Post数据
                PostDataType = PostDataType.FilePath,//默认为传入String类型，也可以设置PostDataType.Byte传入Byte类型数据  
                //ProxyIp = "192.168.1.105：8015",//代理服务器ID 端口可以直接加到后面以：分开就行了    可选项 不需要代理 时可以不设置这三个参数  
                //ProxyPwd = "123456",//代理服务器密码     可选项  
                //ProxyUserName = "administrator",//代理服务器账户名     可选项  
                ResultType = ResultType.Byte,//返回数据类型，是Byte还是String  
                //PostdataByte = System.Text.Encoding.Default.GetBytes("测试一下"),//如果PostDataType为Byte时要设置本属性的值  
               // CookieCollection = new System.Net.CookieCollection(),//可以直接传一个Cookie集合进来  
            };

            if (comboBox1.Text == "GET")
            {
                item.Method = "get";
            }
            else
            {
                item.Method = "head";
            }

            progressBar1.Value += 1;



            //得到HTML代码  
            HttpResult result = http.GetHtml(item);
            //取出返回的Cookie  
            string cookie = result.Cookie;
            //返回的Html内容  
            string html = result.Html;
            Console.WriteLine(Convert.ToInt32(numericUpDown2.Value.ToString()));
            string input = textBox4.Text;
            string pattern = @"\d{3}";
            string parm="";
            int target = 0;
        


            if (comboBox2.Text == "提示")
            {
                foreach (Match match in Regex.Matches(input, pattern))
                {
                    
                    Console.WriteLine(match.Value);
                    
                    if (result.StatusCode.GetHashCode().ToString() == match.Value )
                    {
                        set_listView(result.ResponseUri, html.Length.ToString(), result.StatusCode.GetHashCode().ToString());
                        return;
                    }
                }
                
            }
            else
            {
                int test = 0,tes=0;
                foreach (Match match in Regex.Matches(input, pattern))
                {
                    test++;
                    Console.WriteLine(match.Value);
                  
                    if (result.StatusCode.GetHashCode().ToString() != match.Value)
                    {
                        tes++;
                    }
                }

                if (test == tes)
                {

                    set_listView(result.ResponseUri, html.Length.ToString(), result.StatusCode.GetHashCode().ToString());
                }
            }

            

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                
                //表示访问成功，具体的大家就参考HttpStatusCode类  
            }


        }
        private void button2_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            dick_max = 0;
            dick_long = 0;
            dick_long2 = 0;
            dick_state = 1;
            Thread.Sleep(3000);
            dick_dir.Clear(); // 清空数据+内存清理
            GC.Collect(); //强制内存释放

        }
        private void dirdick(object data) //读字典
        {
            Monitor.Enter(this);//线程锁
            string dick = Environment.CurrentDirectory + "\\dick\\"+(string)data;
            System.IO.StreamReader file = new System.IO.StreamReader(dick);
            string text = "";
            dick_dir.Add(text);
            dick_dir.Add(text);
            dick_dir.Add(text);
            while ((text = file.ReadLine()) != null)
            {
                dick_dir.Add(text);
                dick_max++;
            }

            file.Close();
            Monitor.Exit(this);//解除
            dick_long++;

        }
        private void button4_Click(object sender, EventArgs e)
        {
            dick_state = 2;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    //MessageBox.Show(checkedListBox1.GetItemText(checkedListBox1.Items[i]));
                    ThreadPool.QueueUserWorkItem(new WaitCallback(dirdick), checkedListBox1.Items[i].ToString());
                    }

                }
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    dick_long2++;
                }

            }
            threads = (int)numericUpDown1.Value;
            T2.Start(); 

        }
        private void t1_Tick(object sender, EventArgs e)
        {
            label10.Text = dick_max.ToString();
            label8.Text = dick_long.ToString();
            label11.Text = progressBar1.Value.ToString();
            if (dick_state == 0)
            {
                label6.Text = "扫描中";
            }
            if (dick_state == 1)
            {
                label6.Text = "已停止扫描";
            }
            if (dick_state == 2)
            {
                label6.Text = "读取字典中";
            }

        }
        private void t2_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(dick_long2);
            if (dick_long2 == dick_long)
            {
                ThreadPool.SetMaxThreads(threads, threads*3);
                progressBar1.Maximum = dick_max;
                dick_state = 0;
                for (int i = 0; i < dick_max % threads; i++)
                {
                    ThreadPool.QueueUserWorkItem(dickscan, dick_dir[dick_max - i]);
                    //Application.DoEvents();
                }
                for (int i = 0; i < dick_max- dick_max / (threads - 1);) //根据线程 数据分割 
                {
                    ThreadPool.QueueUserWorkItem(xxxxx, i);
                    i += dick_max / threads;
                    Console.WriteLine(i);
                    //Application.DoEvents();
                }
                
                T2.Stop();
            }
        }

        private void xxxxx(object data)
        {
            int x = (int)data;
            for (int i = x; i < x + dick_max / threads; i++)
            {
                if (dick_state != 1)
                {


                    if (dick_state != 0)
                    {
                        break;
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(dickscan, dick_dir[i]);
                        //dickscan(dick_dir[i]);
                        //Application.DoEvents();

                    }

                }


                
            }

            

            
            
            
        }

    }

}


