using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using ZhongShanFormsApp.子控件;

namespace ZhongShanFormsApp
{
    public partial class MainForm : Form
    {
        private Panel contentPanel;
        private FlowLayoutPanel menuContainer;

        private Dictionary<string, UserControl> _controlCache = new Dictionary<string, UserControl>();
        private RoundedButton _currentSelectedBtn = null;
        private Dictionary<RoundedButton, List<RoundedButton>> _menuGroups = new Dictionary<RoundedButton, List<RoundedButton>>();

        public MainForm()
        {
            InitializeComponent();

            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            this.BackColor = Color.FromArgb(30, 30, 35);

            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 600);
            this.Text = "建筑辅助设计工具";
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateLayout();
            ShowWelcomeScreen();
        }

        private void CreateLayout()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ================= 左侧菜单面板 =================
            Panel leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 35),
                Margin = new Padding(0)
            };

            // 顶部标题栏
            Panel titlePanel = new Panel { Dock = DockStyle.Top, Height = 80 };
            Label titleLabel = new Label
            {
                Text = "功能导航",
                Font = new Font("微软雅黑", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            Panel line = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Color.FromArgb(80, 80, 90) };

            titlePanel.Controls.Add(titleLabel);
            titlePanel.Controls.Add(line);

            // 菜单容器 (稍微调整了Top Padding让顶部不那么挤)
            menuContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(25, 20, 25, 20),
                BackColor = Color.FromArgb(30, 30, 35)
            };

            var menuData = new Dictionary<string, List<string>>
            {
                { "📄 导入图纸", new List<string> { "立面图读取", "平面图读取", "大样图读取", "节点图读取" } },
                { "🔗 图纸关联", new List<string> { "大样关联立面" } },
                { "✂️ 节点图处理", new List<string> { "节点图轻量化" } },
                { "📦 生成表皮", new List<string> { "生成定位线", "生成表皮" } },
                { "📍 节点定位", new List<string> { "节点种子库", "构件节点匹配" } },
                { "🧩 生成模型", new List<string> { "模型导出" } }
            };

            foreach (var menu in menuData)
            {
                CreateMenuGroup(menu.Key, menu.Value);
            }

            leftPanel.Controls.Add(menuContainer);
            leftPanel.Controls.Add(titlePanel);

            // ================= 右侧内容区 =================
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(20),
                Margin = new Padding(0)
            };

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(contentPanel, 1, 0);
            this.Controls.Add(mainLayout);
        }

        private void ShowWelcomeScreen()
        {
            contentPanel.Controls.Clear();
            Label lblWelcome = new Label
            {
                Text = "欢迎使用建筑设计助手\n\n请从左侧菜单选择功能",
                Font = new Font("微软雅黑", 18F),
                ForeColor = Color.FromArgb(80, 80, 90),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            contentPanel.Controls.Add(lblWelcome);
        }

        private void CreateMenuGroup(string mainTitle, List<string> subTitles)
        {
            // 一级菜单 (加大了底部 Margin 为 15)
            RoundedButton btnMain = new RoundedButton
            {
                Text = mainTitle,
                Width = 210,
                Height = 45,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 15), // <--- 调整了这里的间距
                Tag = "Main",
                TextAlign = ContentAlignment.MiddleCenter,
                NormalColor = Color.FromArgb(52, 58, 64),
                HoverColor = Color.FromArgb(73, 80, 87)
            };

            // 二级菜单
            List<RoundedButton> subButtons = new List<RoundedButton>();
            foreach (var subTitle in subTitles)
            {
                RoundedButton btnSub = new RoundedButton
                {
                    Text = subTitle,
                    Width = 180,
                    Height = 36,
                    Font = new Font("微软雅黑", 9.5F, FontStyle.Regular),
                    Tag = subTitle,
                    Visible = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(15, 0, 0, 8), // <--- 子菜单也适当加大了间距
                    NormalColor = Color.FromArgb(38, 42, 48),
                    HoverColor = Color.FromArgb(55, 60, 66)
                };

                btnSub.Click += SubMenu_Click;
                subButtons.Add(btnSub);
            }

            _menuGroups.Add(btnMain, subButtons);

            // 手风琴逻辑
            btnMain.Click += (s, e) =>
            {
                bool isCurrentlyExpanded = subButtons.Any(b => b.Visible);

                foreach (var group in _menuGroups)
                {
                    foreach (var btn in group.Value) btn.Visible = false;
                    group.Key.IsSelected = false;
                    group.Key.Invalidate();
                }

                if (!isCurrentlyExpanded)
                {
                    foreach (var btn in subButtons) btn.Visible = true;

                    // ★ 修改这里：直接调用点击方法，不依赖 PerformClick 机制
                    if (subButtons.Count > 0)
                    {
                        SubMenu_Click(subButtons[0], EventArgs.Empty);
                    }
                }
            };

            menuContainer.Controls.Add(btnMain);
            foreach (var btnSub in subButtons)
            {
                menuContainer.Controls.Add(btnSub);
            }
        }

        private void SubMenu_Click(object sender, EventArgs e)
        {
            RoundedButton btn = sender as RoundedButton;
            string subTitle = btn.Tag.ToString();

            if (_currentSelectedBtn != null)
            {
                _currentSelectedBtn.IsSelected = false;
                _currentSelectedBtn.Invalidate();
            }
            btn.IsSelected = true;
            btn.Invalidate();
            _currentSelectedBtn = btn;

            UserControl targetControl = GetOrCreateControl(subTitle);
            if (targetControl != null)
            {
                contentPanel.Controls.Clear();
                targetControl.Dock = DockStyle.Fill;
                contentPanel.Controls.Add(targetControl);
            }
        }

        private UserControl GetOrCreateControl(string title)
        {
            if (_controlCache.ContainsKey(title)) return _controlCache[title];

            UserControl ctrl = title switch
            {
                "立面图读取" => new ElevationReadControl(),
                "平面图读取" => new PlanReadControl(),
                "大样图读取" => new DetailReadControl(),
                "节点图读取" => new NodeReadControl(),
                "大样关联立面" => new DetailFacadeAssocControl(), // ★ 修改这一行
                "节点图轻量化" => new NodeLightweightControl(),
                "生成定位线" => new GenerateLocatorControl(),
                "生成表皮" => new SkinGenerationControl(),
                "节点种子库" => new NodeLibraryControl(),
                "构件节点匹配" => new MemberNodeMatchControl(),
                _ => new PlaceholderControl(title)
            };

            _controlCache[title] = ctrl;
            return ctrl;
        }
    }

    // ================= 自定义圆角按钮类 =================
    public class RoundedButton : Button
    {
        public int BorderRadius { get; set; } = 6;
        public Color NormalColor { get; set; } = Color.FromArgb(52, 58, 64);
        public Color HoverColor { get; set; } = Color.FromArgb(73, 80, 87);
        public Color SelectedColor { get; set; } = Color.FromArgb(0, 120, 215);

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; this.Invalidate(); }
        }

        protected override bool ShowFocusCues => false;

        public RoundedButton()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor, true);

            this.SetStyle(ControlStyles.Selectable, false); // 保持不抢焦点，防止虚线框

            this.UseVisualStyleBackColor = false;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.Transparent;
            this.ForeColor = Color.White;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // 阻断系统默认的背景擦除
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // 强行刷父背景色
            if (this.Parent != null)
            {
                g.Clear(this.Parent.BackColor);
            }

            // 向内缩进 1 像素
            Rectangle rect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);

            Color bgColor = IsSelected ? SelectedColor : (ClientRectangle.Contains(this.PointToClient(Cursor.Position)) ? HoverColor : NormalColor);

            using (GraphicsPath path = GetRoundedRectPath(rect, BorderRadius))
            {
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(bgColor, 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            TextRenderer.DrawText(g, this.Text, this.Font, rect, this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        // 恢复底层鼠标事件，确保 Click 能够正常触发
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.Invalidate();
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r2 = radius * 2f;
            path.AddArc(rect.X, rect.Y, r2, r2, 180, 90);
            path.AddArc(rect.Right - r2, rect.Y, r2, r2, 270, 90);
            path.AddArc(rect.Right - r2, rect.Bottom - r2, r2, r2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r2, r2, r2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}