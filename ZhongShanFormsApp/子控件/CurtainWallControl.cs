using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace ZhongShanFormsApp.子控件
{
    public partial class CurtainWallControl : UserControl
    {
        public readonly HttpClient _httpClient = new HttpClient();

        // 控件字段声明
        private Label _labelPlan;
        private Label labelPlan;
        private TextBox txtPlanPath;
        private Button btnBrowsePlan;
        private Label labelElevation;
        private TextBox txtElevationPath;
        private Button btnBrowseElevation;
        private Button btnGenerate;
        private ProgressBar progressBar1;
        private Label lblStatus;

        public CurtainWallControl()
        {
            InitializeComponent();
            InitializeManualControls();
        }
        private void InitializeManualControls()
        {
            this.Text = "幕墙表皮生成";
            this.Size = new System.Drawing.Size(500, 300);
            //this.StartPosition = FormStartPosition.CenterScreen;  // 屏幕居中

            // ========== 第1行：工程zip文件 ==========
            _labelPlan = new Label
            {
                Text = "*",
                Location = new System.Drawing.Point(10, 28),
                Size = new System.Drawing.Size(13, 25),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight
            };
            labelPlan = new Label
            {
                Text = "工程zip文件：",
                Location = new System.Drawing.Point(20, 30),
                Size = new System.Drawing.Size(100, 25)
            };
            txtPlanPath = new TextBox
            {
                Location = new System.Drawing.Point(130, 30),
                Size = new System.Drawing.Size(220, 25),
                ReadOnly = true
            };
            btnBrowsePlan = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(360, 28),
                Size = new System.Drawing.Size(80, 28)
            };
            btnBrowsePlan.Click += BtnBrowsePlan_Click;

            // ========== 第2行：生成按钮 ==========
            btnGenerate = new Button
            {
                Text = "生成幕墙表皮",
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

            this.Controls.AddRange(new Control[] { _labelPlan, labelPlan, txtPlanPath, btnBrowsePlan,
                                                   btnGenerate, progressBar1, lblStatus });
        }

        private void BtnBrowsePlan_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "ZIP文件|*.zip|所有文件|*.*";
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
            //btnBrowseElevation.Enabled = false;
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
                //btnBrowseElevation.Enabled = true;
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
            //using var request_json = File.OpenRead(request_json);

            var planContent = new StreamContent(planStream);
            //var request_json = new StreamContent(request_json);
            planContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //request_json.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            form.Add(planContent, "project_zip", Path.GetFileName(planPath));
            //form.Add(elevContent, "request_json", Path.GetFileName(request_json));

            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            var response = await httpClient.PostAsync("http://127.0.0.1:8000/build/upload-zip", form);
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
