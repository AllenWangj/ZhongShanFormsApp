using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZhongShanFormsApp
{
    public partial class ChildForm : Form
    {
        private string mainTitle;
        private List<string> buttonTexts;

        public ChildForm(string title, List<string> texts)
        {
            mainTitle = title;
            buttonTexts = texts;
            InitializeComponent();

            this.Text = mainTitle;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(52, 58, 64);  // 深灰色背景，与标题栏一致
            this.Size = new Size(550, 450);
            this.MinimumSize = new Size(400, 300);
            this.Paint += ChildForm_Paint;

            CreateLayout();
        }

        private void ChildForm_Paint(object sender, PaintEventArgs e)
        {
            var path = GetRoundedRectangle(this.ClientRectangle, 20);
            this.Region = new Region(path);
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void CreateLayout()
        {
            TableLayoutPanel mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(52, 58, 64); // 与标题栏同色，避免白边
            mainPanel.RowCount = 2;
            mainPanel.ColumnCount = 1;
            mainPanel.RowStyles.Clear();
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Panel titleBar = CreateTitleBar();
            mainPanel.Controls.Add(titleBar, 0, 0);

            Panel buttonContainer = CreateButtonContainer();
            mainPanel.Controls.Add(buttonContainer, 0, 1);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateTitleBar()
        {
            Panel titleBar = new Panel();
            titleBar.Dock = DockStyle.Fill;
            titleBar.BackColor = Color.FromArgb(52, 58, 64);
            titleBar.Margin = new Padding(0);
            titleBar.Padding = new Padding(0);
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;

            // 标题文字
            Label titleLabel = new Label();
            titleLabel.Text = mainTitle;
            titleLabel.ForeColor = Color.White;
            titleLabel.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            titleLabel.Location = new Point(12, 12);
            titleLabel.AutoSize = true;
            titleBar.Controls.Add(titleLabel);

            // 关闭按钮：完全贴合右上角，高度与标题栏一致
            Button closeBtn = new Button();
            closeBtn.Text = "✕";
            closeBtn.FlatStyle = FlatStyle.Flat;
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.BackColor = Color.Transparent;
            closeBtn.ForeColor = Color.White;
            closeBtn.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            closeBtn.Dock = DockStyle.Right;          // 靠右，自动贴合右上角
            closeBtn.Width = 45;                      // 宽度固定
            closeBtn.Height = 45;                     // 高度与标题栏一致
            closeBtn.Margin = new Padding(0);         // 无外边距
            closeBtn.Padding = new Padding(0);        // 无内边距
            closeBtn.Cursor = Cursors.Hand;
            closeBtn.Click += (s, e) => this.Close();
            // 悬停效果：背景色变为红色，且覆盖整个按钮区域
            closeBtn.MouseEnter += (s, e) => closeBtn.BackColor = Color.Crimson;
            closeBtn.MouseLeave += (s, e) => closeBtn.BackColor = Color.Transparent;

            titleBar.Controls.Add(closeBtn);
            return titleBar;
        }


        private Panel CreateButtonContainer()
        {
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Color.White;
            container.Padding = new Padding(20);

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoScroll = true;
            flow.FlowDirection = FlowDirection.TopDown;
            flow.WrapContents = false;
            flow.Padding = new Padding(10, 20, 10, 20);
            flow.BackColor = Color.White;

            foreach (string text in buttonTexts)
            {
                Button btn = new Button();
                btn.Text = text;
                btn.Size = new Size(200, 48);
                btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.FromArgb(52, 58, 64);
                btn.ForeColor = Color.White;
                btn.Cursor = Cursors.Hand;
                btn.Margin = new Padding(10, 8, 10, 8);
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.Tag = text;

                btn.Paint += (sender, e) =>
                {
                    Button b = sender as Button;
                    GraphicsPath path = new GraphicsPath();
                    int radius = 10;
                    path.AddArc(0, 0, radius, radius, 180, 90);
                    path.AddArc(b.Width - radius, 0, radius, radius, 270, 90);
                    path.AddArc(b.Width - radius, b.Height - radius, radius, radius, 0, 90);
                    path.AddArc(0, b.Height - radius, radius, radius, 90, 90);
                    b.Region = new Region(path);
                };

                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(73, 80, 87);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(52, 58, 64);
                btn.Click += DarkButton_Click;

                flow.Controls.Add(btn);
            }

            container.Controls.Add(flow);
            return container;
        }

        private Point mouseOffset;
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mouseOffset = new Point(-e.X, -e.Y);
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point newPos = this.Location;
                newPos.Offset(mouseOffset.X + e.X, mouseOffset.Y + e.Y);
                this.Location = newPos;
            }
        }

        private async void DarkButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string btnText = btn.Text;

            if (btnText.Contains("导入") || btnText.Contains("选择文件"))
            {
                string url = "http://businessdev.paikece.com/business/api/Health/CheckInfo";
                using (var client = new System.Net.Http.HttpClient())
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        string result = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(result, "接口返回", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"请求失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (btnText == "返回" || btnText == "关闭")
            {
                this.Close();
            }
            else
            {
                //MessageBox.Show($"你点击了：{btnText}\n功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show($"功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



    }
}
