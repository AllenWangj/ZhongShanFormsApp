using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZhongShanFormsApp.子控件
{
    public partial class CurtainwallProfile : UserControl
    {
        public readonly HttpClient _httpClient = new HttpClient();

        // 控件字段声明
        private Label _labelPlan;
        private Label labelPlan;
        private TextBox txtPlanPath;
        private Button btnBrowsePlan;

        private Label labelFacade;
        private TextBox txtFacade;

        private Label labelFloors;
        private TextBox txtFloors;

        private Label labelFallback;
        private TextBox txtFallback;

        private Label labelMatchTol;
        private TextBox txtMatchTol;

        private Label labelNodeRegex;
        private TextBox txtNodeRegex;

        private Label labelMarkerLayerRegex;
        private TextBox txtMarkerLayerRegex;

        private Label labelMarkerMaxDist;
        private NumericUpDown txtMarkerMaxDist;

        private Label labelPanelsFilename;
        private TextBox txtPanelsFilename;

        private Label labelForceGfInternalVertical;
        RadioButton radioForceTrue;
        RadioButton radioForceFalse;

        private Button btnGenerate;
        private ProgressBar progressBar1;
        private Label lblStatus;

        public CurtainwallProfile()
        {
            InitializeComponent();
            InitializeManualControls();
        }
        private void InitializeManualControls()
        {
            this.Text = "模型生成";
            this.Size = new System.Drawing.Size(500, 300);
            //this.StartPosition = FormStartPosition.CenterScreen;  // 屏幕居中

            this.Size = new Size(580, 480);

            int labelWidth = 150;
            int txtWidth = 250;
            int startX = 30;
            int startY = 20;
            int rowHeight = 35;

            // ========== 第1行：project_zip ==========
            _labelPlan = new Label
            {
                Text = "*",
                Location = new Point(startX - 15, startY - 5),
                Size = new Size(18, 28),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight
            };
            labelPlan = new Label
            {
                Text = "项目ZIP文件：",
                Location = new Point(startX, startY),
                Size = new Size(labelWidth, 28)
            };
            txtPlanPath = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY),
                Size = new Size(220, 28),
                ReadOnly = true
            };
            btnBrowsePlan = new Button
            {
                Text = "浏览...",
                Location = new Point(startX + labelWidth + 5 + 225, startY),
                Size = new Size(70, 28)
            };
            btnBrowsePlan.Click += BtnBrowsePlan_Click;

            // ========== 第2行：facade ==========
            labelFacade = new Label
            {
                Text = "立面方向：",
                Location = new Point(startX, startY + rowHeight * 1),
                Size = new Size(labelWidth, 28)
            };
            txtFacade = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 1),
                Size = new Size(txtWidth, 28),
                Text = "WEST"  // 默认值
            };

            // ========== 第3行：floors ==========
            labelFloors = new Label
            {
                Text = "楼层列表：",
                Location = new Point(startX, startY + rowHeight * 2),
                Size = new Size(labelWidth, 28)
            };
            txtFloors = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 2),
                Size = new Size(txtWidth, 28),
                Text = "GF,L1,L2,L3"  // 默认值
            };

            // ========== 第4行：fallback_label ==========
            labelFallback = new Label
            {
                Text = "fallback_label：",
                Location = new Point(startX, startY + rowHeight * 3),
                Size = new Size(labelWidth, 28)
            };
            txtFallback = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 3),
                Size = new Size(txtWidth, 28),
                Text = "5022/01"  // 默认值
            };

            // ========== 第5行：match_tol ==========
            labelMatchTol = new Label
            {
                Text = "匹配容忍度：",
                Location = new Point(startX, startY + rowHeight * 4),
                Size = new Size(labelWidth, 28)
            };
            txtMatchTol = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 4),
                Size = new Size(txtWidth, 28),
                Text = "80"  // 默认值
            };

            // ========== 第6行：node_regex ==========
            labelNodeRegex = new Label
            {
                Text = "节点正则：",
                Location = new Point(startX, startY + rowHeight * 5),
                Size = new Size(labelWidth, 28)
            };
            txtNodeRegex = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 5),
                Size = new Size(txtWidth, 28),
                Text = "[0-9]{4}/[0-9]{2}"  // 默认值
            };

            // ========== 第7行：marker_layer_regex ==========
            labelMarkerLayerRegex = new Label
            {
                Text = "标记层正则：",
                Location = new Point(startX, startY + rowHeight * 6),
                Size = new Size(labelWidth, 28)
            };
            txtMarkerLayerRegex = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 6),
                Size = new Size(txtWidth, 28),
                Text = "ST_ELEV_NODE_MARKER"  // 默认值
            };

            // ========== 第8行：label_marker_max_dist ==========
            labelMarkerMaxDist = new Label
            {
                Text = "标记最大距离：",
                Location = new Point(startX, startY + rowHeight * 7),
                Size = new Size(labelWidth, 28)
            };
            txtMarkerMaxDist = new NumericUpDown
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 7),
                Size = new Size(txtWidth, 28),
                Minimum = 0,
                Maximum = 1000,
                DecimalPlaces = 2,
                Value = 800
            };

            // ========== 第9行：panels_filename ==========
            labelPanelsFilename = new Label
            {
                Text = "面板文件名：",
                Location = new Point(startX, startY + rowHeight * 8),
                Size = new Size(labelWidth, 28)
            };
            txtPanelsFilename = new TextBox
            {
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 8),
                Size = new Size(txtWidth, 28),
            };

            // ========== 第10行：force_gf_internal_vertical_5022 ==========
            labelForceGfInternalVertical = new Label
            {
                Text = "强制GF内部垂直：",
                Location = new Point(startX, startY + rowHeight * 9),
                Size = new Size(labelWidth, 28)
            };
            radioForceTrue = new RadioButton
            {
                Text = "是",
                Location = new Point(startX + labelWidth + 5, startY + rowHeight * 9),
                Size = new Size(50, 28),
                Checked = true
            };
            radioForceFalse = new RadioButton
            {
                Text = "否",
                Location = new Point(startX + labelWidth + 5 + 70, startY + rowHeight * 9),
                Size = new Size(50, 28),
                Checked = false
            };

            // ========== 第11行：生成按钮 ==========
            btnGenerate = new Button
            {
                Text = "生成模型",
                Location = new Point(startX + labelWidth + 5 + 45, startY + rowHeight * 10),
                Size = new Size(150, 35)
            };
            btnGenerate.Click += BtnGenerate_Click;

            // ========== 进度条 ==========
            progressBar1 = new ProgressBar
            {
                Location = new Point(startX, startY + rowHeight * 11 + 10),
                Size = new Size(520, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // ========== 状态标签 ==========
            lblStatus = new Label
            {
                Text = "就绪",
                Location = new Point(startX, startY + rowHeight * 12 + 10),
                Size = new Size(520, 25)
            };

            this.Controls.AddRange(new Control[] { _labelPlan, labelPlan, txtPlanPath, btnBrowsePlan,
                                           labelFacade, txtFacade,
                                           labelFloors, txtFloors,
                                           labelFallback, txtFallback,
                                           labelMatchTol, txtMatchTol,
                                           labelNodeRegex, txtNodeRegex,
                                           labelMarkerLayerRegex, txtMarkerLayerRegex,
                                           labelMarkerMaxDist, txtMarkerMaxDist,
                                           labelPanelsFilename,txtPanelsFilename,
                                           labelForceGfInternalVertical, radioForceTrue ,radioForceFalse,
                                           btnGenerate, progressBar1, lblStatus });
        }

        private void BtnBrowsePlan_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "ZIP文件|*.zip|所有文件|*.*";
                ofd.Title = "选择project_zip文件";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPlanPath.Text = ofd.FileName;
                }
            }
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPlanPath.Text) || !File.Exists(txtPlanPath.Text))
            {
                MessageBox.Show("请先选择有效的project_zip文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 禁用所有输入控件
            SetControlsEnabled(false);
            progressBar1.Visible = true;
            lblStatus.Text = "正在上传并处理，请稍候...";
            lblStatus.ForeColor = Color.Blue;

            try
            {
                string forceValue = radioForceTrue.Checked ? "true" : "false";
                string savedZipPath = await CallGenerateApiAndSaveAsync(txtPlanPath.Text, txtFacade.Text, txtFloors.Text, txtFallback.Text, txtMatchTol.Text, txtNodeRegex.Text, txtMarkerLayerRegex.Text, txtMarkerMaxDist.Text, forceValue);
                lblStatus.Text = "生成成功！";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show($"输出文件已生成并保存到：{savedZipPath}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "生成失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"调用接口失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复所有输入控件
                SetControlsEnabled(true);
                progressBar1.Visible = false;
            }
        }

        private async Task<string> CallGenerateApiAndSaveAsync(string planPath, string txtFacade, string txtFloors, string txtFallback, string txtMatchTol, string txtNodeRegex, string txtMarkerLayerRegex, string txtMarkerMaxDist, string forceValue)
        {
            // 为这个长时间请求创建独立的 HttpClient，设置足够长的超时
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(90);   // 根据实际算法耗时调整

            using var form = new MultipartFormDataContent();

            using var planStream = File.OpenRead(planPath);

            var planContent = new StreamContent(planStream);
            planContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            form.Add(planContent, "project_zip", Path.GetFileName(planPath));
            form.Add(new StringContent(txtFacade), "facade");
            form.Add(new StringContent(txtFloors), "floors");
            form.Add(new StringContent(txtFallback), "fallback_label");
            form.Add(new StringContent(txtMatchTol), "match_tol");
            form.Add(new StringContent(txtNodeRegex), "node_regex");
            form.Add(new StringContent(txtMarkerLayerRegex), "marker_layer_regex");
            form.Add(new StringContent(txtMarkerMaxDist), "label_marker_max_dist");
            form.Add(new StringContent(forceValue), "force_gf_internal_vertical_5022");

            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            var response = await httpClient.PostAsync("http://127.0.0.1:8004/api/v1/jobs", form);

            // 读取返回的JSON字符串
            string responseJson = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            // 保存JSON到txt文件
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "文本文件|*.txt|所有文件|*.*";
                sfd.FileName = $"result_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                sfd.Title = "保存接口返回的JSON数据";

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    throw new OperationCanceledException("用户取消了保存操作");
                }

                // 将JSON写入txt文件
                await File.WriteAllTextAsync(sfd.FileName, responseJson);
                return sfd.FileName;
            }
        }

        /// <summary>
        /// 启用或禁用所有输入控件
        /// </summary>
        private void SetControlsEnabled(bool enabled)
        {
            // 文本框
            txtPlanPath.Enabled = enabled;
            txtFacade.Enabled = enabled;
            txtFloors.Enabled = enabled;
            txtFallback.Enabled = enabled;
            txtMatchTol.Enabled = enabled;
            txtNodeRegex.Enabled = enabled;
            txtMarkerLayerRegex.Enabled = enabled;
            txtMarkerMaxDist.Enabled = enabled;
            txtPanelsFilename.Enabled = enabled;

            // 按钮
            btnBrowsePlan.Enabled = enabled;
            btnGenerate.Enabled = enabled;

            // 单选按钮
            radioForceTrue.Enabled = enabled;
            radioForceFalse.Enabled = enabled;
        }
    }
}
