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

            // ================= 1. 顶部标题 (固定) =================
            lblTitle = new Label
            {
                Text = TaskTitle,
                Font = new Font("微软雅黑", 16F, FontStyle.Bold),
                Dock = DockStyle.Top, // 靠上
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ================= 2. 底部操作区 (垂直居中布局) =================
            TableLayoutPanel bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 150, // 预留足够高度容纳三行内容
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White
            };
            // 分配三行的高度
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); // 按钮行
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); // 进度条行
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // 状态文字行

            // 提交按钮
            btnSubmit = new RoundedButton
            {
                Text = "提交任务",
                Size = new Size(160, 40),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                NormalColor = Color.DodgerBlue,
                HoverColor = Color.FromArgb(30, 144, 255),
                ForeColor = Color.White,
                Anchor = AnchorStyles.None // 保证在格子内绝对居中
            };
            btnSubmit.Click += async (s, e) => await ExecuteTaskAsync();

            // 进度条
            progress = new ProgressBar
            {
                Size = new Size(800, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false,
                Anchor = AnchorStyles.None
            };

            // 状态文字 (★ 修改：靠左显示)
            lblStatus = new Label
            {
                Text = "就绪",
                AutoSize = true,
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Left, // 锚定在左侧
                TextAlign = ContentAlignment.MiddleLeft
            };


            bottomLayout.Controls.Add(btnSubmit, 0, 0);
            bottomLayout.Controls.Add(progress, 0, 1);
            bottomLayout.Controls.Add(lblStatus, 0, 2);


            // ================= 3. 中间内容区 (自适应) =================
            customContentPanel = new Panel
            {
                Dock = DockStyle.Fill, // 占满剩余空间
                AutoScroll = true,     // 内容过多时显示滚动条
                Padding = new Padding(0, 10, 0, 10)
            };

            // 添加顺序很重要：Fill -> Bottom -> Top
            this.Controls.Add(customContentPanel);
            this.Controls.Add(bottomLayout);
            this.Controls.Add(lblTitle);
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