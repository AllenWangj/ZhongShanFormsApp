using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZhongShanFormsApp.子控件
{
    public partial class CadNodeLocalization : UserControl
    {

        public readonly HttpClient _httpClient = new HttpClient();

        // 控件字段声明
        private Label _labelPlan1;
        private Label labelPlan;
        private TextBox txtPlanPath;
        private Button btnBrowsePlan;

        private Label _labelPlan2;
        private Label labelElevation;
        private TextBox txtElevationPath;
        private Button btnBrowseElevation;

        private Label _labelPlan3;
        private Label labelTarget;
        private TextBox txtTargetPath;
        private Button btnBrowseTarget;

        private Button btnGenerate;
        private ProgressBar progressBar1;
        private Label lblStatus;

        public CadNodeLocalization()
        {
            InitializeComponent();
            InitializeManualControls();
        }
        private void InitializeManualControls()
        {
            this.Text = "节点定位";
            this.Size = new System.Drawing.Size(500, 300);

            // ========== 第1行：工程zip文件 ==========
            _labelPlan1 = new Label
            {
                Text = "*",
                Location = new System.Drawing.Point(10, 28),
                Size = new System.Drawing.Size(13, 25),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight
            };
            labelPlan = new Label
            {
                Text = "节点图库DXF：",
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

            // ========== 第2行：立面图DXF ==========
            _labelPlan2 = new Label
            {
                Text = "*",
                Location = new System.Drawing.Point(10, 68),
                Size = new System.Drawing.Size(13, 25),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight
            };
            labelElevation = new Label
            {
                Text = "立面图DXF：",
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(100, 25)
            };
            txtElevationPath = new TextBox
            {
                Location = new System.Drawing.Point(130, 70),
                Size = new System.Drawing.Size(220, 25),
                ReadOnly = true
            };
            btnBrowseElevation = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(360, 68),
                Size = new System.Drawing.Size(80, 28)
            };
            btnBrowseElevation.Click += BtnBrowseElevation_Click;

            // ========== 第3行：大样图DXF ==========
            _labelPlan3 = new Label
            {
                Text = "*",
                Location = new System.Drawing.Point(10, 108),
                Size = new System.Drawing.Size(13, 25),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleRight
            };
            labelTarget = new Label
            {
                Text = "大样图DXF：",
                Location = new System.Drawing.Point(20, 110),
                Size = new System.Drawing.Size(100, 25)
            };
            txtTargetPath = new TextBox
            {
                Location = new System.Drawing.Point(130, 110),
                Size = new System.Drawing.Size(220, 25),
                ReadOnly = true
            };
            btnBrowseTarget = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(360, 108),
                Size = new System.Drawing.Size(80, 28)
            };
            btnBrowseTarget.Click += BtnBrowseTarget_Click;

            // ========== 第4行：生成按钮 ==========
            btnGenerate = new Button
            {
                Text = "节点定位",
                Location = new System.Drawing.Point(130, 160),
                Size = new System.Drawing.Size(240, 35)
            };
            btnGenerate.Click += BtnGenerate_Click;

            // ========== 进度条 ==========
            progressBar1 = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 210),
                Size = new System.Drawing.Size(440, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            lblStatus = new Label
            {
                Text = "就绪",
                Location = new System.Drawing.Point(20, 240),
                Size = new System.Drawing.Size(440, 25)
            };

            this.Controls.AddRange(new Control[] { _labelPlan1,labelPlan, txtPlanPath, btnBrowsePlan,
                                                   _labelPlan2,labelElevation, txtElevationPath, btnBrowseElevation,
                                                   _labelPlan3,labelTarget, txtTargetPath, btnBrowseTarget,
                                                   btnGenerate, progressBar1, lblStatus });
        }

        private void BtnBrowsePlan_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DXF文件|*.dxf|所有文件|*.*";
                ofd.Title = "选择节点图库DXF文件";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPlanPath.Text = ofd.FileName;
                }
            }
        }

        private void BtnBrowseElevation_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DXF文件|*.dxf|所有文件|*.*";
                ofd.Title = "选择立面图DXF文件";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtElevationPath.Text = ofd.FileName;
                }
            }
        }

        private void BtnBrowseTarget_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DXF文件|*.dxf|所有文件|*.*";
                ofd.Title = "选择大样图DXF文件";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtTargetPath.Text = ofd.FileName;
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
            if (string.IsNullOrEmpty(txtElevationPath.Text) || !File.Exists(txtElevationPath.Text))
            {
                MessageBox.Show("请先选择有效的立面DXF文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtTargetPath.Text) || !File.Exists(txtTargetPath.Text))
            {
                MessageBox.Show("请先选择有效的大样图DXF文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnGenerate.Enabled = false;
            btnBrowseElevation.Enabled = false;
            btnBrowsePlan.Enabled = false;
            btnBrowseTarget.Enabled = false;
            progressBar1.Visible = true;
            lblStatus.Text = "正在上传并处理，请稍候...";

            try
            {
                string savedZipPath = await CallGenerateApiAndSaveAsync(txtPlanPath.Text, txtElevationPath.Text, txtTargetPath.Text);
                lblStatus.Text = "生成成功！";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show($"结果已生成并保存到：{savedZipPath}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                btnBrowseElevation.Enabled = true;
                btnBrowsePlan.Enabled = true;
                btnBrowseTarget.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private async Task<string> CallGenerateApiAndSaveAsync(string detailPath, string elevationPath, string targetPath)
        {
            // 为这个长时间请求创建独立的 HttpClient，设置足够长的超时
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(90);   // 根据实际算法耗时调整

            using var form = new MultipartFormDataContent();

            //using var planStream = File.OpenRead(detailPath);
            //using var elevStream = File.OpenRead(elevationPath);
            //using var targetStream = File.OpenRead(targetPath);

            //var planContent = new StreamContent(planStream);
            //var elevContent = new StreamContent(elevStream);
            //var targetContent = new StreamContent(targetStream);

            //planContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //elevContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //targetContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            //form.Add(planContent, "detail_dxf", Path.GetFileName(detailPath));
            //form.Add(elevContent, "elevation_dxf", Path.GetFileName(elevationPath));
            //form.Add(targetContent, "target_dxf", Path.GetFileName(targetPath));

            //// ========== 第一步：提交任务，获取 job_id ==========
            //httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            //var response = await httpClient.PostAsync("http://127.0.0.1:8003/api/v1/pipeline/full", form);


            // 使用流内容，不手动设置 Content-Type
            using var detailStream = new FileStream(detailPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using var elevStream = new FileStream(elevationPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using var targetStream = new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

            var detailContent = new StreamContent(detailStream);
            var elevContent = new StreamContent(elevStream);
            var targetContent = new StreamContent(targetStream);

            // 可选：显式设置 Content-Type
            detailContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            elevContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            targetContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            form.Add(detailContent, "detail_dxf", Path.GetFileName(detailPath));
            form.Add(elevContent, "elevation_dxf", Path.GetFileName(elevationPath));
            form.Add(targetContent, "target_dxf", Path.GetFileName(targetPath));

            // 调试日志
            System.Diagnostics.Debug.WriteLine($"===== 请求开始 =====");
            System.Diagnostics.Debug.WriteLine($"detail_dxf: {detailPath}");
            System.Diagnostics.Debug.WriteLine($"elevation_dxf: {elevationPath}");
            System.Diagnostics.Debug.WriteLine($"target_dxf: {targetPath}");
            System.Diagnostics.Debug.WriteLine($"Boundary: {form.Headers.ContentType}");

            var response = await httpClient.PostAsync("http://127.0.0.1:8003/api/v1/pipeline/full", form);

            string responseBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"响应状态码: {(int)response.StatusCode} {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"响应内容: {responseBody}");
            System.Diagnostics.Debug.WriteLine($"===== 请求结束 =====");

            response.EnsureSuccessStatusCode();

            // 读取返回的JSON
            string responseJson = await response.Content.ReadAsStringAsync();

            // 解析JSON获取job_id
            string jobId = ExtractJobIdFromJson(responseJson);

            if (string.IsNullOrEmpty(jobId))
            {
                throw new Exception("未能从返回结果中获取到 job_id");
            }

            // ========== 第二步：使用 job_id 下载文件 ==========
            string downloadUrl = $"http://127.0.0.1:8003/api/v1/jobs/{jobId}/download";
            var downloadResponse = await httpClient.GetAsync(downloadUrl);
            downloadResponse.EnsureSuccessStatusCode();

            // 保存下载的文件
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "ZIP压缩文件|*.zip|所有文件|*.*";
                sfd.FileName = $"result_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                sfd.Title = "保存处理结果";

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    throw new OperationCanceledException("用户取消了保存操作");
                }

                using (var fileStream = File.Create(sfd.FileName))
                {
                    await downloadResponse.Content.CopyToAsync(fileStream);
                }
                return sfd.FileName;
            }
        }


        /// <summary>
        /// 从返回的JSON中提取 job_id
        /// </summary>
        private string ExtractJobIdFromJson(string json)
        {
            try
            {
                // 使用 System.Text.Json 解析
                using var document = System.Text.Json.JsonDocument.Parse(json);
                if (document.RootElement.TryGetProperty("job_id", out var jobIdElement))
                {
                    return jobIdElement.GetString();
                }

                // 如果没有 job_id，尝试其他可能的字段名
                if (document.RootElement.TryGetProperty("id", out var idElement))
                {
                    return idElement.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"解析JSON失败: {ex.Message}\nJSON内容: {json}");
            }
        }



    }
}
