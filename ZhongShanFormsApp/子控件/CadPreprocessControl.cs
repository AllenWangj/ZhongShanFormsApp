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
    public partial class CadPreprocessControl : UserControl
    {
        public readonly HttpClient _httpClient = new HttpClient();

        // 控件字段声明
        private Label _labelPlan;
        private Label labelPlan;
        private TextBox txtPlanPath;
        private Button btnBrowsePlan;
        private Button btnGenerate;
        private ProgressBar progressBar1;
        private Label lblStatus;

        public CadPreprocessControl()
        {
            InitializeComponent();
            InitializeManualControls();
        }

        private void InitializeManualControls()
        {
            this.Text = "节点图轻量化处理";
            this.Size = new System.Drawing.Size(520, 300);

            // ========== 第1行：节点图DXF文件 ==========
            _labelPlan = new Label
            {
                Text = "*",
                Location = new System.Drawing.Point(10, 42),
                Size = new System.Drawing.Size(13, 25),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight
            };
            labelPlan = new Label
            {
                Text = "节点图DXF文件：",
                Location = new System.Drawing.Point(15, 40),
                Size = new System.Drawing.Size(110, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtPlanPath = new TextBox
            {
                Location = new System.Drawing.Point(135, 40),
                Size = new System.Drawing.Size(280, 25),
                ReadOnly = true
            };
            btnBrowsePlan = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(425, 39),
                Size = new System.Drawing.Size(70, 27)
            };
            btnBrowsePlan.Click += BtnBrowsePlan_Click;

            // ========== 第2行：生成按钮 ==========
            btnGenerate = new Button
            {
                Text = "节点图处理",
                Location = new System.Drawing.Point(130, 120),
                Size = new System.Drawing.Size(240, 35)
            };
            btnGenerate.Click += BtnGenerate_Click;

            // ========== 进度条 ==========
            progressBar1 = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 170),
                Size = new System.Drawing.Size(440, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            lblStatus = new Label
            {
                Text = "就绪",
                Location = new System.Drawing.Point(20, 200),
                Size = new System.Drawing.Size(440, 25)
            };

            this.Controls.AddRange(new Control[] { _labelPlan,labelPlan, txtPlanPath, btnBrowsePlan,
                                                   btnGenerate, progressBar1, lblStatus });
        }

        private void BtnBrowsePlan_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DXF文件|*.dxf|所有文件|*.*";
                ofd.Title = "选择平面DXF文件";
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
                MessageBox.Show("请先选择有效的平面DXF文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnGenerate.Enabled = false;
            btnBrowsePlan.Enabled = false;
            progressBar1.Visible = true;
            lblStatus.Text = "正在上传并处理，请稍候...";

            try
            {
                string savedZipPath = await CallGenerateApiAndSaveAsync(txtPlanPath.Text);
                lblStatus.Text = "生成成功！";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show($"幕墙表皮已生成并保存到：{savedZipPath}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "生成失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"调用接口失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerate.Enabled = true;
                btnBrowsePlan.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private async Task<string> CallGenerateApiAndSaveAsync(string planPath)
        {
            // 为这个长时间请求创建独立的 HttpClient，设置足够长的超时
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(90);   // 根据实际算法耗时调整

            using var form = new MultipartFormDataContent();

            using var planStream = File.OpenRead(planPath);

            var planContent = new StreamContent(planStream);
            planContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            form.Add(planContent, "file", Path.GetFileName(planPath));

            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            var response = await httpClient.PostAsync("http://127.0.0.1:8001/api/v1/preprocess", form);
            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"状态码: {response.StatusCode}, 详情: {errorDetail}");
            }
            response.EnsureSuccessStatusCode();

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "ZIP压缩文件|*.zip|所有文件|*.*";
                sfd.FileName = "curtain_wall_result.zip";
                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    throw new OperationCanceledException("用户取消了保存操作");
                }

                using (var fileStream = File.Create(sfd.FileName))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
                return sfd.FileName;
            }
        }


    }
}
