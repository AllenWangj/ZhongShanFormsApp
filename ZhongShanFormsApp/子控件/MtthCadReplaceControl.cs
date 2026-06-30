using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace ZhongShanFormsApp.子界面
{
    public partial class MtthCadReplaceControl : UserControl
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

        private ReaLTaiizor.Controls.ForeverButton btnGenerate;
        private ReaLTaiizor.Controls.ForeverButton btnDownloadFile;      // 下载结果压缩包
        private ReaLTaiizor.Controls.ForeverButton btnShowErrors;        // 生成报错清单

        private ProgressBar progressBar1;
        private Label lblStatus;

        private Label _labelPlanStar;
        private Label _labelPlan2Star;

        // -------------------- 状态变量 --------------------
        private int _errorCount = 0;
        private string _resultFileId = null;   // 结果压缩包ID
        private string _errorFileId = null;    // 错误清单ID

        public MtthCadReplaceControl()
        {
            InitializeComponent();
            //this.Font = new Font("微软雅黑", 9F, FontStyle.Regular);
            InitializeManualControls();
            SetInitialButtonStates();
        }
        private void InitializeManualControls()
        {
            this.Text = "模图处理";
            this.Size = new System.Drawing.Size(500, 300);
            //this.StartPosition = FormStartPosition.CenterScreen;  // 屏幕居中

            // ========== 第1行：目标节点图DXF ==========
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
                Text = "目标节点图DXF：",
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

            // ========== 第2行：标准模图库DXF ==========
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
                Text = "标准模图库DXF：",
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

            // ========== 第3行：生成按钮 ==========
            btnGenerate = new ReaLTaiizor.Controls.ForeverButton
            {
                Text = "模图处理",
                Location = new Point(60, 120),
                Size = new Size(100, 35),
                Rounded = true,
                Font = new Font(Font.FontFamily, 10),
                BaseColor = Color.DodgerBlue,
                TextColor = Color.White,
            };
            btnGenerate.Click += BtnGenerate_Click;

            btnDownloadFile = new ReaLTaiizor.Controls.ForeverButton
            {
                Text = "下载文件",
                Location = new Point(60 + 100 + 15, 120),
                Size = new Size(100, 35),
                Enabled = false,
                Rounded = true,
                Font = new Font(Font.FontFamily, 10),
                BaseColor = Color.Gray,
                TextColor = Color.White,
            };
            btnDownloadFile.Click += BtnDownloadFile_Click;

            btnShowErrors = new ReaLTaiizor.Controls.ForeverButton
            {
                Text = "生成报错清单",
                Location = new Point(60 + 100 + 15 + 100 + 15, 120),
                Size = new Size(100, 35),
                Enabled = false,
                Rounded = true,
                Font = new Font(Font.FontFamily, 10),
                BaseColor = Color.Gray,
                TextColor = Color.White,
            };
            btnShowErrors.Click += BtnShowErrors_Click;

            // ========== 进度条 ==========
            progressBar1 = new ProgressBar
            {
                Location = new Point(20, 80 + 35 + 10 + 35 + 10 + 20),
                Size = new Size(480, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // ========== 状态标签 ==========
            lblStatus = new Label
            {
                Text = "就绪",
                Location = new Point(20, progressBar1.Location.Y + progressBar1.Size.Height + 10),
                Size = new Size(540, 25)
            };

            this.Controls.AddRange(new Control[] { _labelPlan1,labelPlan, txtPlanPath, btnBrowsePlan,
                                                   _labelPlan2 ,labelElevation, txtElevationPath, btnBrowseElevation,
                                                   btnGenerate,  btnDownloadFile, btnShowErrors,
                                                   progressBar1, lblStatus });

        }

        private void SetInitialButtonStates()
        {
            btnGenerate.Enabled = true;
            btnDownloadFile.Enabled = false;
            btnShowErrors.Enabled = false;
        }

        private void SetAfterGenerationStates(bool hasErrors)
        {
            btnGenerate.Enabled = true;           // 允许重新生成
            btnGenerate.BaseColor = Color.DodgerBlue;
            btnGenerate.Refresh();

            btnDownloadFile.Enabled = true;       // 始终可下载压缩包
            btnDownloadFile.BaseColor = Color.DodgerBlue;
            btnDownloadFile.Refresh();

            btnShowErrors.Enabled = hasErrors;    // 有错误才可下载错误清单
            btnShowErrors.BaseColor = hasErrors ? Color.DodgerBlue : Color.Gray;
            btnShowErrors.Refresh();
        }

        private void SetBeforeGenerationStates()
        {
            btnDownloadFile.Enabled = false;       // 始终可下载压缩包
            btnDownloadFile.BaseColor = Color.Gray;
            btnDownloadFile.Refresh();

            btnShowErrors.Enabled = false;
            btnShowErrors.BaseColor = Color.Gray;
            btnShowErrors.Refresh();
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

        private void BtnBrowseElevation_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DXF文件|*.dxf|所有文件|*.*";
                ofd.Title = "选择立面DXF文件";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtElevationPath.Text = ofd.FileName;
                }
            }
        }

        // ========== 下载文件（结果压缩包） ==========
        private async void BtnDownloadFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_resultFileId))
            {
                MessageBox.Show("没有可下载的文件，请先执行生成操作。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnDownloadFile.Enabled = false;
                progressBar1.Visible = true;
                lblStatus.Text = "正在下载压缩包...";
                lblStatus.ForeColor = Color.Blue;

                string savedPath = await DownloadAndSaveFileAsync(_resultFileId, "结果压缩包", "ZIP压缩文件|*.zip", $"result_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                if (!string.IsNullOrEmpty(savedPath))
                {
                    lblStatus.Text = $"压缩包已保存：{savedPath}";
                    lblStatus.ForeColor = Color.Green;
                    MessageBox.Show("压缩包已保存。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "下载失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"下载失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDownloadFile.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        // ========== 生成报错清单 ==========
        private async void BtnShowErrors_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_errorFileId))
            {
                MessageBox.Show("没有错误清单，请先执行生成操作。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnShowErrors.Enabled = false;
                progressBar1.Visible = true;
                lblStatus.Text = "正在下载错误清单...";
                lblStatus.ForeColor = Color.Blue;

                byte[] errorBytes = await DownloadFileBytesAsync(_errorFileId);

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "保存错误清单";
                    sfd.Filter = "Excel文件|*.xlsx|所有文件|*.*";
                    sfd.FileName = $"错误清单_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string savePath = sfd.FileName;
                        File.WriteAllBytes(savePath, errorBytes);
                        lblStatus.Text = $"错误清单已保存：{savePath}";
                        lblStatus.ForeColor = Color.Green;
                        MessageBox.Show("错误清单已保存。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        lblStatus.Text = "用户取消保存";
                        lblStatus.ForeColor = Color.Gray;
                        MessageBox.Show("用户取消了保存", "确定", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "下载失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"下载失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnShowErrors.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            SetBeforeGenerationStates();
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

            btnGenerate.Enabled = false;
            btnDownloadFile.Enabled = false;
            btnShowErrors.Enabled = false;
            btnBrowseElevation.Enabled = false;
            btnBrowsePlan.Enabled = false;
            progressBar1.Visible = true;
            lblStatus.Text = "正在上传并处理，请稍候...";
            lblStatus.ForeColor = Color.Blue;

            try
            {
                var (resultId, errorId, rejectCount, manualReviewCount, autoReplaceCount) = await CallGenerateApiAndSaveAsync(txtPlanPath.Text, txtElevationPath.Text);

                // 保存 ID 供后续下载使用
                _resultFileId = resultId;
                _errorFileId = errorId;
                _errorCount = rejectCount;  // 拒绝数量作为错误数量

                int aaa = await GetErrorCountAsync(errorId);

                // 更新界面状态
                //bool hasErrors = rejectCount > 0 || manualReviewCount > 0;
                //lblStatus.Text = $"处理完成 | 自动替换:{autoReplaceCount} 待人工审核:{manualReviewCount} 拒绝:{rejectCount}";
                //lblStatus.ForeColor = rejectCount > 0 ? Color.Red : (manualReviewCount > 0 ? Color.Orange : Color.Green);

                bool hasErrors = aaa > 0;
                SetAfterGenerationStates(hasErrors);

                //// 弹窗显示详细信息
                //string message = $"处理完成！\n\n" +
                //                $"📊 统计结果：\n" +
                //                $"  ✅ 自动替换：{autoReplaceCount}\n" +
                //                $"  ⚠️ 待人工审核：{manualReviewCount}\n" +
                //                $"  ❌ 拒绝：{rejectCount}\n\n";
                //if (rejectCount > 0)
                //    message += $"其中 {rejectCount} 处被拒绝，请下载报错清单查看详情。";
                //else if (manualReviewCount > 0)
                //    message += $"有 {manualReviewCount} 处需要人工审核。";
                //else
                //    message += "所有面板均已成功替换！";
                string message = $"处理完成！\n\n" +
                                $"  错误数量：{aaa}\n\n";

                MessageBox.Show(message, "处理完成", MessageBoxButtons.OK,
                    rejectCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);


                lblStatus.Text = "生成成功！";
                lblStatus.ForeColor = Color.Green;
                //MessageBox.Show($"幕墙表皮已生成并保存到：{savedZipPath}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                progressBar1.Visible = false;
            }
        }

        private async Task<(string resultFileId, string errorFileId, int rejectCount, int manualReviewCount, int autoReplaceCount)> CallGenerateApiAndSaveAsync(string target_dxf, string template_dxf)
        {
            // 为这个长时间请求创建独立的 HttpClient，设置足够长的超时
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(90);   // 根据实际算法耗时调整

            using var form = new MultipartFormDataContent();

            // 流会在 using 块结束时自动释放，但请求期间必须保持打开
            using var planStream = File.OpenRead(target_dxf);
            using var elevStream = File.OpenRead(template_dxf);

            var planContent = new StreamContent(planStream);
            var elevContent = new StreamContent(elevStream);
            planContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            elevContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            form.Add(planContent, "target_dxf", Path.GetFileName(target_dxf));
            form.Add(elevContent, "template_dxf", Path.GetFileName(template_dxf));

            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            var response = await httpClient.PostAsync("http://127.0.0.1:8002/replace", form);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 检查状态
            string status = root.GetProperty("status").GetString() ?? "";
            if (status != "ok")
            {
                string message = root.GetProperty("message").GetString() ?? "未知错误";
                throw new Exception($"任务失败：{message}");
            }

            // 提取文件ID
            var files = root.GetProperty("files");
            string resultFileId = files.GetProperty("result_package").GetProperty("file_id").GetString() ?? "";
            string errorFileId = files.GetProperty("report_list").GetProperty("file_id").GetString() ?? "";

            // 提取统计信息
            var summary = root.GetProperty("summary");
            var counts = summary.GetProperty("decision_counts");
            int autoReplace = counts.GetProperty("auto_replace").GetInt32();
            int manualReview = counts.GetProperty("manual_review").GetInt32();
            int reject = counts.GetProperty("reject").GetInt32();

            if (string.IsNullOrEmpty(resultFileId) || string.IsNullOrEmpty(errorFileId))
                throw new Exception("返回的文件 ID 不完整");

            return (resultFileId, errorFileId, reject, manualReview, autoReplace);
        }

        // 获取错误数量（只下载错误清单到内存，不保存）
        private async Task<int> GetErrorCountAsync(string errorFileId)
        {
            byte[] bytes = await DownloadFileBytesAsync(errorFileId);
            return GetErrorCountFromExcel(bytes);
        }

        // 下载文件并保存（用于结果压缩包）
        private async Task<string> DownloadAndSaveFileAsync(string fileId, string title, string filter, string defaultFileName)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:8002/download/{fileId}");
            response.EnsureSuccessStatusCode();

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = $"保存{title}";
                sfd.Filter = filter;
                sfd.FileName = defaultFileName;
                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    lblStatus.Text = "用户取消保存";
                    lblStatus.ForeColor = Color.Gray;
                    MessageBox.Show("用户取消了保存", "确定", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
                string savePath = sfd.FileName;
                using (var fs = File.Create(savePath))
                {
                    await response.Content.CopyToAsync(fs);
                }
                return savePath;
            }
        }

        // 下载文件到字节数组
        private async Task<byte[]> DownloadFileBytesAsync(string fileId)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:8002/download/{fileId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        // 解析错误数量（Excel第一张表，假设第一行是标题，数据行从第二行开始）
        private int GetErrorCountFromExcel(byte[] excelBytes)
        {
            if (excelBytes == null || excelBytes.Length == 0)
                return 0;

            using var stream = new MemoryStream(excelBytes);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1); // 第一个工作表（索引从1开始）
            if (worksheet == null) return 0;

            // 获取最后使用的行
            var lastRow = worksheet.LastRowUsed();
            if (lastRow == null) return 0;

            int rowCount = lastRow.RowNumber();
            // 假设第一行为标题，错误数据从第2行开始
            return rowCount > 1 ? rowCount - 1 : 0;
        }

    }
}
