using ZhongShanFormsApp.子控件;
using ZhongShanFormsApp.子界面;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using System.Windows.Forms;

namespace ZhongShanFormsApp
{
    public partial class MainForm : Form
    {
        private Panel contentPanel;
        private Button btnCurtainWall;
        private Button btnNodeProcess;

        private CurtainWallControl curtainWallControl;
        private CadPreprocessControl cadPreprocessControl;
        private MtthCadReplaceControl mtthCadReplaceControl;
        private CadNodeLocalization cadNodeLocalization;
        private CurtainwallProfile userControl2;

        private FlowLayoutPanel flowMenu;

        // 定义主按钮的文字列表（去重后的顺序）
        private readonly List<string> mainButtonTexts = new List<string>
        {
            "📄 导入图纸", 
            "✂️ 节点图处理",
            "📋 模图处理",
            "📋 节点定位",
            "📦 生成表皮",
            "🧩 生成模型", 
            "📋 生成提料清单",
            "📦 具体材料信息",
        };
        // 配置每个主按钮对应的子窗体中的蓝色按钮文字列表
        // 键：主按钮文字，值：该子窗体中蓝色按钮的文字列表
        private readonly Dictionary<string, List<string>> blueButtonsConfig = new Dictionary<string, List<string>>();


        public MainForm()
        {
            InitializeComponent();

            // ========== 初始化子控件 ==========
            curtainWallControl = new CurtainWallControl();
            cadPreprocessControl = new CadPreprocessControl();
            curtainWallControl.Dock = DockStyle.None;
            curtainWallControl.Anchor = AnchorStyles.None;
            cadPreprocessControl.Dock = DockStyle.None;
            cadPreprocessControl.Anchor = AnchorStyles.None;
            // ========== 初始化panel ==========
            contentPanel = new Panel();
            contentPanel.Resize += (s, e) => CenterCurrentControl();

            this.BackColor = Color.FromArgb(30, 30, 35); // 深灰背景
            this.Text = "建筑辅助设计工具";
            this.StartPosition = FormStartPosition.CenterScreen;
            //this.WindowState = FormWindowState.Maximized; // 启动最大化
            // 设置一个合适的初始大小（例如 1000×700）
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 500);  // 限制最小尺寸，防止布局错乱

            InitializeConfig();
            CreateModernMenu();
        }

        private void InitializeConfig()
        {
            // 为“导入图纸”配置三个蓝色按钮
            blueButtonsConfig["📄 导入图纸"] = new List<string> { "立面图", "平面图", "大样图", "节点图" };

            // 为“节点图处理”配置两个蓝色按钮
            blueButtonsConfig["✂️ 节点图处理"] = new List<string> { "导入图纸", "轻量化处理", "输出" };

            // 为“生成模型”配置三个蓝色按钮
            blueButtonsConfig["🧩 生成模型"] = new List<string> { "文件导出" };

            // 为“生成提料清单”配置两个蓝色按钮
            blueButtonsConfig["📋 生成提料清单"] = new List<string> { };

            // 为“具体材料信息”配置三个蓝色按钮
            blueButtonsConfig["📦 具体材料信息"] = new List<string> { };
        }

        private void CreateModernMenu()
        {
            // 主布局：左右两栏
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.BackColor = Color.FromArgb(30, 30, 35);

            // ========== 左侧菜单区域 ==========
            Panel leftPanel = new Panel();
            leftPanel.BackColor = Color.FromArgb(40, 44, 52);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Padding = new Padding(0);  // 不要额外内边距，由内部控件控制

            // 1. 顶部标题栏（独立 Panel，Dock=Top）
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 80;              // 足够高度
            titlePanel.Padding = new Padding(0, 15, 0, 10);

            Label titleLabel = new Label();
            titleLabel.Text = "功能导航";
            titleLabel.Font = new Font("微软雅黑", 16F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Fill;
            titlePanel.Controls.Add(titleLabel);

            // 可选：在标题下方加一条装饰线
            Panel line = new Panel();
            line.Dock = DockStyle.Bottom;
            line.Height = 2;
            line.BackColor = Color.FromArgb(100, 100, 110);
            line.Margin = new Padding(10, 0, 10, 0);
            titlePanel.Controls.Add(line);

            // 2. 按钮容器（Dock=Fill，自动填充剩余空间）
            flowMenu = new FlowLayoutPanel();
            flowMenu.Dock = DockStyle.Fill;
            flowMenu.FlowDirection = FlowDirection.TopDown;
            flowMenu.WrapContents = false;
            flowMenu.AutoScroll = true;
            flowMenu.Padding = new Padding(25, 15, 25, 15);   // 左右各 25 像素，上下保持不变

            // 动态添加黑色圆角按钮
            foreach (string text in mainButtonTexts)
            {
                RoundedButton btn = new RoundedButton();
                btn.Text = text;
                btn.Width = 210;
                btn.Height = 48;
                btn.NormalColor = Color.FromArgb(52, 58, 64);
                btn.HoverColor = Color.FromArgb(73, 80, 87);
                btn.BackColor = btn.NormalColor;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                btn.TextAlign = ContentAlignment.MiddleCenter;   // 改为居中
                btn.Padding = new Padding(0);                    // 清除左边距
                //btn.TextAlign = ContentAlignment.MiddleLeft;
                //btn.Padding = new Padding(15, 0, 0, 0);
                btn.Margin = new Padding(0, 8, 0, 8);   // 上下间距 8 像素
                btn.Tag = text;
                btn.Click += MainButton_Click;
                flowMenu.Controls.Add(btn);
            }

            // 将标题栏和按钮容器添加到 leftPanel
            leftPanel.Controls.Add(flowMenu);      // 先加 flowMenu（Dock=Fill）
            leftPanel.Controls.Add(titlePanel);    // 再加 titlePanel（Dock=Top），确保它在上方
                                                   // 注意：后加的控件会显示在上层，但 Dock=Top 会自动置顶，顺序不影响

            // ========== 右侧内容区域 ==========
            contentPanel.BackColor = Color.FromArgb(245, 247, 250);
            contentPanel.Dock = DockStyle.Fill;

            Label welcome = new Label();
            welcome.Text = "欢迎使用建筑设计助手\n\n请从左侧菜单选择功能";
            welcome.Font = new Font("微软雅黑", 16F);
            welcome.ForeColor = Color.FromArgb(80, 80, 90);
            welcome.TextAlign = ContentAlignment.MiddleCenter;
            welcome.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(welcome);

            // 将左右面板加入主布局
            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(contentPanel, 1, 0);

            this.Controls.Add(mainLayout);
        }

        // 右侧panel通用的居中方法
        private void CenterCurrentControl()
        {
            if (contentPanel.Controls.Count == 0) return;
            Control ctrl = contentPanel.Controls[0];  // 当前显示的控件
                                                      // 计算居中位置（考虑控件本身大小）
            int x = Math.Max(0, (contentPanel.Width - ctrl.Width) / 2);
            int y = Math.Max(0, (contentPanel.Height - ctrl.Height) / 2);
            // 确保位置不为负数
            ctrl.Location = new Point(Math.Max(0, x), Math.Max(0, y));
        }

        // 主按钮点击事件处理
        private async void MainButton_Click(object sender, EventArgs e)
        {
            if (sender is not RoundedButton btn)
                return;
            RoundedButton clickedBtn = sender as RoundedButton;
            if (clickedBtn == null) return;
            // 清除所有菜单按钮的选中状态
            foreach (RoundedButton btn1 in flowMenu.Controls)  // flowMenu 是你的按钮容器变量名
            {
                btn1.IsSelected = false;
            }
            // 设置当前按钮为选中状态
            clickedBtn.IsSelected = true;

            string mainText = btn.Tag?.ToString() ?? string.Empty;

            switch (mainText)
            {
                case "✂️ 节点图处理":
                    //var form = new CadPreprocessForm();
                    //form.ShowDialog(); // 或 form.Show() 非模态
                    if (cadPreprocessControl == null)
                    {
                        cadPreprocessControl = new CadPreprocessControl();
                        // 确保控件大小固定且不自动拉伸
                        cadPreprocessControl.Dock = DockStyle.None;
                        cadPreprocessControl.Anchor = AnchorStyles.None;
                    }
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(cadPreprocessControl);
                    CenterCurrentControl();  // 居中显示
                    break;
                case "📋 模图处理":
                    if (mtthCadReplaceControl == null)
                    {
                        mtthCadReplaceControl = new MtthCadReplaceControl();
                        // 确保控件大小固定且不自动拉伸
                        mtthCadReplaceControl.Dock = DockStyle.None;
                        mtthCadReplaceControl.Anchor = AnchorStyles.None;
                    }
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(mtthCadReplaceControl);
                    CenterCurrentControl();  // 居中显示
                    break;
                case "📋 节点定位":
                    if (cadNodeLocalization == null)
                    {
                        cadNodeLocalization = new CadNodeLocalization();
                        // 确保控件大小固定且不自动拉伸
                        cadNodeLocalization.Dock = DockStyle.None;
                        cadNodeLocalization.Anchor = AnchorStyles.None;
                    }
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(cadNodeLocalization);
                    CenterCurrentControl();  // 居中显示
                    break;
                case "📦 生成表皮":
                    //var form1 = new CurtainWallForm();
                    //form1.ShowDialog(); // 或 form1.Show() 非模态
                    if (curtainWallControl == null)
                    {
                        curtainWallControl = new CurtainWallControl();
                        // 确保控件大小固定且不自动拉伸
                        curtainWallControl.Dock = DockStyle.None;
                        curtainWallControl.Anchor = AnchorStyles.None;
                    }
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(curtainWallControl);
                    CenterCurrentControl();  // 居中显示
                    break;
                case "🧩 生成模型":
                    //var form1 = new CurtainWallForm();
                    //form1.ShowDialog(); // 或 form1.Show() 非模态
                    if (userControl2 == null)
                    {
                        userControl2 = new CurtainwallProfile();
                        // 确保控件大小固定且不自动拉伸
                        userControl2.Dock = DockStyle.None;
                        userControl2.Anchor = AnchorStyles.None;
                    }
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(userControl2);
                    CenterCurrentControl();  // 居中显示
                    break;
                default:
                    //List<string> blueList;
                    //if (!blueButtonsConfig.TryGetValue(mainText, out blueList))
                    //    blueList = new List<string> { "默认功能", "返回" };
                    //ChildForm child = new ChildForm(mainText, blueList);
                    //child.ShowDialog();
                    contentPanel.Controls.Clear();
                    Label welcome = new Label();
                    welcome.Text = "功能正在开发中！";
                    welcome.Font = new Font("微软雅黑", 16F);
                    welcome.ForeColor = Color.FromArgb(80, 80, 90);
                    welcome.TextAlign = ContentAlignment.MiddleCenter;
                    welcome.Dock = DockStyle.Fill;
                    contentPanel.Controls.Add(welcome);
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {

            // 请求地址
            string url = "http://businessdev.paikece.com/business/api/Health/CheckInfo";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发送 GET 请求
                    HttpResponseMessage response = await client.GetAsync(url);

                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // 读取返回内容（字符串形式）
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // 弹窗显示结果
                    MessageBox.Show("请求成功", "接口返回结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (HttpRequestException ex)
                {
                    // 网络或 HTTP 错误
                    MessageBox.Show($"请求失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // 其他异常
                    MessageBox.Show($"发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        //private async void button2_Click(object sender, EventArgs e)
        //{

        //    // 请求地址
        //    string url = "http://127.0.0.1:8000/generate";

        //    using (HttpClient client = new HttpClient())
        //    {
        //        try
        //        {
        //            // 发送 GET 请求
        //            HttpResponseMessage response = await client.GetAsync(url);


        //            using var form = new MultipartFormDataContent();

        //            // 添加文件内容，参数名必须与 API 定义的字段名一致：plan_dxf, elevation_dxf
        //            using var planStream = File.OpenRead(planDxfPath);
        //            using var elevationStream = File.OpenRead(elevationDxfPath);

        //            var planContent = new StreamContent(planStream);
        //            var elevationContent = new StreamContent(elevationStream);

        //            planContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        //            elevationContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        //            form.Add(planContent, "plan_dxf", Path.GetFileName(planDxfPath));
        //            form.Add(elevationContent, "elevation_dxf", Path.GetFileName(elevationDxfPath));

        //            // 发送 POST 请求
        //            response = await client.PostAsync("/generate", form);

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                var error = await response.Content.ReadAsStringAsync();
        //                throw new HttpRequestException($"API 调用失败 ({response.StatusCode}): {error}");
        //            }

        //            // 将响应内容保存为 ZIP 文件
        //            using var fileStream = File.Create(outputZipPath);
        //            await response.Content.CopyToAsync(fileStream);

        //            // 弹窗显示结果
        //            MessageBox.Show("请求成功", "接口返回结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (HttpRequestException ex)
        //        {
        //            // 网络或 HTTP 错误
        //            MessageBox.Show($"请求失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        catch (Exception ex)
        //        {
        //            // 其他异常
        //            MessageBox.Show($"发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}



    }
}
