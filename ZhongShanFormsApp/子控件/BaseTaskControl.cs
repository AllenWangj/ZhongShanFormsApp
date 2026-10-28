using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZhongShanFormsApp.子控件
{
    public abstract partial class BaseTaskControl : UserControl
    {
        protected Label lblTitle;
        protected Panel customContentPanel;
        protected Button btnSubmit;
        protected ProgressBar progress;
        protected Label lblStatus;

        // 子类需重写的属性
        protected abstract string TaskTitle { get; }
        protected abstract string SubmitApiUrl { get; }
        protected abstract string DefaultSaveFileName { get; }

        // 子类实现：往 form 里添加自己的文件和参数
        protected abstract void AddParameters(MultipartFormDataContent form);

        public BaseTaskControl()
        {
            this.AutoScroll = true; // 必须：应对长表单
            InitializeBaseUI();
        }

        private void InitializeBaseUI()
        {
            this.BackColor = Color.White;
            this.Padding = new Padding(30);

            lblTitle = new Label
            {
                Text = TaskTitle,
                Font = new Font("微软雅黑", 16F, FontStyle.Bold),
                Location = new Point(30, 30),
                AutoSize = true
            };

            // 自定义内容区（子类把文本框放这里）
            customContentPanel = new Panel
            {
                Location = new Point(30, 80),
                Size = new Size(650, 400),
                AutoScroll = true
            };

            // 提交按钮
            btnSubmit = new RoundedButton
            {
                Text = "提交任务",
                Location = new Point(30, 540),
                Size = new Size(150, 40),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                NormalColor = Color.DodgerBlue,
                HoverColor = Color.FromArgb(30, 144, 255), // 悬停时稍亮一点
                ForeColor = Color.White
            };
            btnSubmit.Click += async (s, e) => await ExecuteTaskAsync();

            progress = new ProgressBar
            {
                Location = new Point(30, 560),
                Size = new Size(650, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            lblStatus = new Label
            {
                Text = "就绪",
                Location = new Point(30, 590),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            this.Controls.AddRange(new Control[] { lblTitle, customContentPanel, btnSubmit, progress, lblStatus });
        }

        // ================= 核心提交流程 =================
        private async Task ExecuteTaskAsync()
        {
            btnSubmit.Enabled = false;
            progress.Visible = true;
            lblStatus.Text = "正在提交任务，请稍候...";
            lblStatus.ForeColor = Color.Blue;

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(60) };
                using var form = new MultipartFormDataContent();

                // 让子类把参数填进去
                AddParameters(form);

                var response = await client.PostAsync(SubmitApiUrl, form);
                string responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP {(int)response.StatusCode}: {responseJson}");

                string taskId = ParseTaskId(responseJson);
                if (string.IsNullOrEmpty(taskId)) throw new Exception("未能获取 task_id\n" + responseJson);

                lblStatus.Text = $"任务已提交 (ID: {taskId})，处理中...";

                // ★ 新增：让子类把 ID 存入全局
                SaveTaskIdToGlobal(taskId);

                // 轮询状态，成功后下载
                await PollAndDownloadAsync(client, taskId, SubmitApiUrl);

                lblStatus.Text = "处理完成并下载成功！";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "任务失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"执行失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSubmit.Enabled = true;
                progress.Visible = false;
            }
        }

        private string ParseTaskId(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("task_id", out var prop)) return prop.GetString();
            }
            catch { }
            return null;
        }

        private async Task PollAndDownloadAsync(HttpClient client, string taskId, string submitUrl)
        {
            string statusUrl = $"{submitUrl}/{taskId}";

            // 轮询，最大等待时间 60 分钟
            for (int i = 0; i < 1200; i++)
            {
                await Task.Delay(3000);
                var res = await client.GetAsync(statusUrl);
                string json = await res.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // 安全获取 status 字段
                    string status = "";
                    if (root.TryGetProperty("status", out var statusProp))
                    {
                        status = statusProp.GetString();
                    }

                    // 1. 判断成功
                    if (status == "completed" || status == "success")
                    {
                        await DownloadFileAsync(client, $"{statusUrl}/download");
                        return;
                    }

                    // 2. 判断失败
                    if (status == "failed" || status == "error")
                    {
                        string errMsg = "处理失败，请查看后端日志。";
                        // 尝试获取具体错误信息
                        if (root.TryGetProperty("error", out var errProp) && errProp.ValueKind != JsonValueKind.Null)
                        {
                            errMsg = errProp.ToString();
                        }
                        throw new Exception(errMsg);
                    }

                    // 如果是 queued, processing 等状态，什么都不做，继续下一次循环
                }
                catch (JsonException)
                {
                    // 如果返回的不是合法 JSON，可能是网关报错，继续重试
                }
            }
            throw new Exception("任务轮询超时，请检查服务器状态。");
        }

        private async Task DownloadFileAsync(HttpClient client, string downloadUrl)
        {
            var res = await client.GetAsync(downloadUrl);
            if (!res.IsSuccessStatusCode) throw new Exception("下载结果文件失败");

            using var sfd = new SaveFileDialog { FileName = DefaultSaveFileName, Filter = "ZIP压缩包|*.zip|所有文件|*.*" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using var fs = File.Create(sfd.FileName);
                await res.Content.CopyToAsync(fs);
                MessageBox.Show($"结果已保存至：\n{sfd.FileName}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ================= 辅助UI工具方法 =================
        protected (TextBox txt, Button btn) CreateFileRow(string label, int y, string filter)
        {
            Label lbl = new Label { Text = label, Location = new Point(0, y + 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            TextBox txt = new TextBox { Location = new Point(160, y), Size = new Size(350, 25), ReadOnly = true, BackColor = Color.WhiteSmoke };
            Button btn = new Button { Text = "浏览...", Location = new Point(520, y - 1), Size = new Size(80, 28) };

            btn.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = filter };
                if (ofd.ShowDialog() == DialogResult.OK) txt.Text = ofd.FileName;
            };
            customContentPanel.Controls.AddRange(new Control[] { lbl, txt, btn });
            return (txt, btn);
        }

        // 1. 添加一行：标签 + 文本框 (用于填写 ID)
        protected TextBox AddIdRow(Panel parent, string labelText, int y)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(0, y + 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            TextBox txt = new TextBox { Location = new Point(160, y), Size = new Size(350, 25), ReadOnly = true, BackColor = Color.WhiteSmoke };
            parent.Controls.AddRange(new Control[] { lbl, txt });
            return txt;
        }

        // 2. 添加一行：标签 + 文本框 + 浏览按钮 (用于上传文件)
        protected TextBox AddFileRow(Panel parent, string labelText, int y, string filter)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(0, y + 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            TextBox txt = new TextBox { Location = new Point(160, y), Size = new Size(350, 25), ReadOnly = true, BackColor = Color.WhiteSmoke };
            Button btn = new Button { Text = "浏览...", Location = new Point(520, y - 1), Size = new Size(80, 28) };

            btn.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = filter };
                if (ofd.ShowDialog() == DialogResult.OK) txt.Text = ofd.FileName;
            };
            parent.Controls.AddRange(new Control[] { lbl, txt, btn });
            return txt;
        }

        protected void AddFileToForm(MultipartFormDataContent form, string filePath, string formKey)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new Exception($"文件未选择或不存在: {formKey}");

             var stream = File.OpenRead(filePath);
            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, formKey, Path.GetFileName(filePath));
        }


        /// <summary>
        /// 任务提交成功后，由子类决定将这个 ID 存入全局的哪个变量
        /// </summary>
        protected virtual void SaveTaskIdToGlobal(string taskId) { }
    }
}