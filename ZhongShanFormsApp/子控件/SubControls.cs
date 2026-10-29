using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;
using ZhongShanFormsApp.配置;

namespace ZhongShanFormsApp.子控件
{
    // ==========================================
    // 8090 立面图读取 (单文件)
    // ==========================================
    public class ElevationReadControl : BaseTaskControl
    {
        private TextBox txtFile;
        protected override string TaskTitle => "立面图读取 (Task 8090)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Elevation}{ApiConfig.Path_Elevation}";
        protected override string DefaultSaveFileName => "Elevation_Result.zip";

        public ElevationReadControl() { txtFile = CreateFileRow("上传立面图 DXF：", 20, "DXF文件|*.dxf").txt; }
        protected override void AddParameters(MultipartFormDataContent form) => AddFileToForm(form, txtFile.Text, "file");

        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.ElevationTaskId = taskId;
    }

    // ==========================================
    // 8091 平面图读取 (单文件 + ID / 4文件 切换)
    // ==========================================
    public class PlanReadControl : BaseTaskControl
    {
        private TextBox txtFile;
        private RadioButton rbAutoId, rbManualJson;
        private Panel panelAuto, panelManual;
        private TextBox txtElevationId;
        private TextBox txtWest, txtEast, txtNorth, txtSouth;

        protected override string TaskTitle => "平面图读取 (Task 8091)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Plan}{ApiConfig.Path_Plan}";
        protected override string DefaultSaveFileName => "Plan_Result.zip";

        public PlanReadControl()
        {
            txtFile = CreateFileRow("上传平面图 DXF (必填)：", 20, "DXF文件|*.dxf").txt;

            // 模式选择
            rbAutoId = new RadioButton { Text = "自动获取立面数据 (推荐)", Location = new Point(160, 60), Checked = true, AutoSize = true };
            rbManualJson = new RadioButton { Text = "手动上传立面 JSON", Location = new Point(400, 60), AutoSize = true };
            customContentPanel.Controls.AddRange(new Control[] { rbAutoId, rbManualJson });

            // 模式A：ID 面板
            panelAuto = new Panel { Location = new Point(0, 90), Size = new Size(650, 60) };
            Label lblId = new Label { Text = "立面 Task ID：", Location = new Point(0, 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtElevationId = new TextBox { Location = new Point(160, 0), Size = new Size(350, 25), ReadOnly = true, BackColor = Color.WhiteSmoke };
            panelAuto.Controls.AddRange(new Control[] { lblId, txtElevationId });

            // 模式B：手动 JSON 面板
            panelManual = new Panel { Location = new Point(0, 90), Size = new Size(650, 200), Visible = false };
            Label lblW = new Label { Text = "West JSON：", Location = new Point(0, 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtWest = new TextBox { Location = new Point(160, 0), Size = new Size(350, 25), ReadOnly = true };
            Button btnW = new Button { Text = "浏览...", Location = new Point(520, -1), Size = new Size(80, 28) };
            btnW.Click += (s, e) => { using var o = new OpenFileDialog { Filter = "JSON|*.json" }; if (o.ShowDialog() == DialogResult.OK) txtWest.Text = o.FileName; };

            Label lblE = new Label { Text = "East JSON：", Location = new Point(0, 44), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtEast = new TextBox { Location = new Point(160, 40), Size = new Size(350, 25), ReadOnly = true };
            Button btnE = new Button { Text = "浏览...", Location = new Point(520, 39), Size = new Size(80, 28) };
            btnE.Click += (s, e) => { using var o = new OpenFileDialog { Filter = "JSON|*.json" }; if (o.ShowDialog() == DialogResult.OK) txtEast.Text = o.FileName; };

            Label lblN = new Label { Text = "North JSON：", Location = new Point(0, 84), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtNorth = new TextBox { Location = new Point(160, 80), Size = new Size(350, 25), ReadOnly = true };
            Button btnN = new Button { Text = "浏览...", Location = new Point(520, 79), Size = new Size(80, 28) };
            btnN.Click += (s, e) => { using var o = new OpenFileDialog { Filter = "JSON|*.json" }; if (o.ShowDialog() == DialogResult.OK) txtNorth.Text = o.FileName; };

            Label lblS = new Label { Text = "South JSON：", Location = new Point(0, 124), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtSouth = new TextBox { Location = new Point(160, 120), Size = new Size(350, 25), ReadOnly = true };
            Button btnS = new Button { Text = "浏览...", Location = new Point(520, 119), Size = new Size(80, 28) };
            btnS.Click += (s, e) => { using var o = new OpenFileDialog { Filter = "JSON|*.json" }; if (o.ShowDialog() == DialogResult.OK) txtSouth.Text = o.FileName; };

            panelManual.Controls.AddRange(new Control[] { lblW, txtWest, btnW, lblE, txtEast, btnE, lblN, txtNorth, btnN, lblS, txtSouth, btnS });

            customContentPanel.Controls.Add(panelAuto);
            customContentPanel.Controls.Add(panelManual);

            // 切换事件
            rbAutoId.CheckedChanged += (s, e) => {
                panelAuto.Visible = rbAutoId.Checked;
                panelManual.Visible = !rbAutoId.Checked;
                txtElevationId.Text = GlobalStateManager.ElevationTaskId;
                if (string.IsNullOrEmpty(txtElevationId.Text)) txtElevationId.Text = "缺失！请先执行立面图读取";
            };
        }

        protected override void AddParameters(MultipartFormDataContent form)
        {
            AddFileToForm(form, txtFile.Text, "file");
            if (rbAutoId.Checked)
            {
                if (string.IsNullOrEmpty(GlobalStateManager.ElevationTaskId)) throw new Exception("缺失立面图 Task ID，请先执行立面图读取。");
                form.Add(new StringContent(GlobalStateManager.ElevationTaskId), "elevation_task_id");
            }
            else
            {
                AddFileToForm(form, txtWest.Text, "facade_west");
                AddFileToForm(form, txtEast.Text, "facade_east");
                AddFileToForm(form, txtNorth.Text, "facade_north");
                AddFileToForm(form, txtSouth.Text, "facade_south");
            }
        }

        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.PlanTaskId = taskId;
    }

    // ==========================================
    // 8092 大样图读取 (单文件)
    // ==========================================
    public class DetailReadControl : BaseTaskControl
    {
        private TextBox txtFile;
        protected override string TaskTitle => "大样图读取 (Task 8092)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Detail}{ApiConfig.Path_Detail}";
        protected override string DefaultSaveFileName => "Detail_Result.zip";

        public DetailReadControl() { txtFile = CreateFileRow("上传大样图 DXF：", 20, "DXF文件|*.dxf").txt; }
        protected override void AddParameters(MultipartFormDataContent form) => AddFileToForm(form, txtFile.Text, "file");

        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.DetailTaskId = taskId;
    }

    // ==========================================
    // 8093 节点图读取 (双文件)
    // ==========================================
    public class NodeReadControl : BaseTaskControl
    {
        private TextBox txtFile, txtTable;
        protected override string TaskTitle => "节点图读取 (Task 8093)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Node}{ApiConfig.Path_Node}";
        protected override string DefaultSaveFileName => "Node_Result.zip";

        public NodeReadControl()
        {
            txtFile = CreateFileRow("上传节点图 DXF：", 20, "DXF文件|*.dxf").txt;
            txtTable = CreateFileRow("上传构件编号表 XLSX：", 60, "Excel文件|*.xlsx").txt;
        }
        protected override void AddParameters(MultipartFormDataContent form)
        {
            AddFileToForm(form, txtFile.Text, "file");
            AddFileToForm(form, txtTable.Text, "part_table");
        }

        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.NodeTaskId = taskId;
    }

    // ==========================================
    // 8094 大样关联立面 (ID模式 / 文件模式) - 简化版
    // ==========================================
    public class DetailFacadeAssocControl : BaseTaskControl
    {
        private RadioButton rbIdMode, rbFileMode;
        private Panel panelId, panelFile;
        private TextBox txtDetailZip, txtFacadeJson;

        protected override string TaskTitle => "大样关联立面 (Task 8094)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Association}{ApiConfig.Path_Association}";
        protected override string DefaultSaveFileName => "Detail_Facade_Assoc.zip";

        public DetailFacadeAssocControl()
        {
            // 模式切换
            rbIdMode = new RadioButton { Text = "模式A：使用上游任务 ID", Location = new Point(160, 10), Checked = true, AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
            rbFileMode = new RadioButton { Text = "模式B：手动上传文件", Location = new Point(400, 10), AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
            customContentPanel.Controls.AddRange(new Control[] { rbIdMode, rbFileMode });

            // ---- Panel: ID模式 ----
            panelId = new Panel { Location = new Point(0, 40), Size = new Size(650, 120) };
            AddIdRow(panelId, "大样图 Task ID：", 0);
            AddIdRow(panelId, "立面图 Task ID：", 40);

            // ---- Panel: 文件模式 ----
            panelFile = new Panel { Location = new Point(0, 40), Size = new Size(650, 120), Visible = false };
            txtDetailZip = AddFileRow(panelFile, "大样结果 ZIP：", 0, "ZIP|*.zip");
            txtFacadeJson = AddFileRow(panelFile, "立面定位 JSON：", 40, "JSON|*.json");

            customContentPanel.Controls.Add(panelId);
            customContentPanel.Controls.Add(panelFile);

            // 切换逻辑
            rbIdMode.CheckedChanged += (s, e) => {
                panelId.Visible = rbIdMode.Checked;
                panelFile.Visible = !rbIdMode.Checked;

                if (rbIdMode.Checked)
                {
                    if (panelId.Controls[1] is TextBox txtDetail) txtDetail.Text = GlobalStateManager.DetailTaskId;
                    if (panelId.Controls[3] is TextBox txtElev) txtElev.Text = GlobalStateManager.ElevationTaskId;
                }
            };
        }

        protected override void AddParameters(MultipartFormDataContent form)
        {
            if (rbIdMode.Checked)
            {
                if (string.IsNullOrEmpty(GlobalStateManager.DetailTaskId) || string.IsNullOrEmpty(GlobalStateManager.ElevationTaskId))
                    throw new Exception("缺失大样图或立面图 Task ID，请先执行前置任务！");

                form.Add(new StringContent(GlobalStateManager.DetailTaskId), "detail_task_id");
                form.Add(new StringContent(GlobalStateManager.ElevationTaskId), "elevation_task_id");
            }
            else
            {
                AddFileToForm(form, txtDetailZip.Text, "detail_zip");
                AddFileToForm(form, txtFacadeJson.Text, "facade_json");
            }

            // 注：容差和映射参数已移除，由后端使用默认值处理
        }

        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.AssociationTaskId = taskId;
    }

    // ==========================================
    // 8095 节点图轻量化 (双文件)
    // ==========================================
    public class NodeLightweightControl : BaseTaskControl
    {
        private TextBox txtFile, txtTable;
        protected override string TaskTitle => "节点图轻量化 (Task 8095)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Lightweight}{ApiConfig.Path_Lightweight}";
        protected override string DefaultSaveFileName => "Node_Lightweight.zip";

        public NodeLightweightControl()
        {
            txtFile = CreateFileRow("上传节点图 DXF：", 20, "DXF文件|*.dxf").txt;
            txtTable = CreateFileRow("上传编号表 XLSX：", 60, "Excel文件|*.xlsx").txt;
        }
        protected override void AddParameters(MultipartFormDataContent form)
        {
            AddFileToForm(form, txtFile.Text, "file");
            AddFileToForm(form, txtTable.Text, "number_table");
        }
    }

    // ==========================================
    // 8096 生成定位线 (单文件)
    // ==========================================
    public class GenerateLocatorControl : BaseTaskControl
    {
        private TextBox txtFile;
        protected override string TaskTitle => "生成定位线 (Task 8096)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Locator}{ApiConfig.Path_Locator}";
        protected override string DefaultSaveFileName => "Locator_Result.zip";

        public GenerateLocatorControl() { txtFile = CreateFileRow("上传平面 DXF：", 20, "DXF文件|*.dxf").txt; }
        protected override void AddParameters(MultipartFormDataContent form) => AddFileToForm(form, txtFile.Text, "file");

        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.LocatorTaskId = taskId;
    }

    // ==========================================
    // 8097 表皮模型生成 (8文件 + ID/DXF 切换)
    // ==========================================
    public class SkinGenerationControl : BaseTaskControl
    {
        private TextBox txtWestDxf, txtEastDxf, txtSouthDxf, txtNorthDxf;
        private TextBox txtWestCfg, txtEastCfg, txtSouthCfg, txtNorthCfg;
        private RadioButton rbId, rbDxf;
        private Panel panelId, panelDxf;
        private TextBox txtLocatorId, txtPlanDxf;

        protected override string TaskTitle => "表皮模型生成 (Task 8097)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_Skin}{ApiConfig.Path_Skin}";
        protected override string DefaultSaveFileName => "Skin_Model.zip";

        public SkinGenerationControl()
        {
            // 4个立面DXF
            txtWestDxf = CreateFileRow("West DXF：", 10, "DXF|*.dxf").txt;
            txtEastDxf = CreateFileRow("East DXF：", 45, "DXF|*.dxf").txt;
            txtSouthDxf = CreateFileRow("South DXF：", 80, "DXF|*.dxf").txt;
            txtNorthDxf = CreateFileRow("North DXF：", 115, "DXF|*.dxf").txt;

            // 4个配置JSON
            txtWestCfg = CreateFileRow("West Config JSON：", 160, "JSON|*.json").txt;
            txtEastCfg = CreateFileRow("East Config JSON：", 195, "JSON|*.json").txt;
            txtSouthCfg = CreateFileRow("South Config JSON：", 230, "JSON|*.json").txt;
            txtNorthCfg = CreateFileRow("North Config JSON：", 265, "JSON|*.json").txt;

            // 定位线模式
            rbId = new RadioButton { Text = "使用上游定位线 ID (推荐)", Location = new Point(160, 310), Checked = true, AutoSize = true };
            rbDxf = new RadioButton { Text = "手动上传定位线 DXF", Location = new Point(400, 310), AutoSize = true };
            customContentPanel.Controls.AddRange(new Control[] { rbId, rbDxf });

            panelId = new Panel { Location = new Point(0, 340), Size = new Size(650, 30) };
            Label lblId = new Label { Text = "定位线 Task ID：", Location = new Point(0, 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtLocatorId = new TextBox { Location = new Point(160, 0), Size = new Size(350, 25), ReadOnly = true, BackColor = Color.WhiteSmoke };
            panelId.Controls.AddRange(new Control[] { lblId, txtLocatorId });

            panelDxf = new Panel { Location = new Point(0, 340), Size = new Size(650, 30), Visible = false };
            Label lblDxf = new Label { Text = "上传定位线 DXF：", Location = new Point(0, 4), Size = new Size(150, 25), TextAlign = ContentAlignment.MiddleRight };
            txtPlanDxf = new TextBox { Location = new Point(160, 0), Size = new Size(350, 25), ReadOnly = true };
            Button btnDxf = new Button { Text = "浏览...", Location = new Point(520, -1), Size = new Size(80, 28) };
            btnDxf.Click += (s, e) => { using var o = new OpenFileDialog { Filter = "DXF|*.dxf" }; if (o.ShowDialog() == DialogResult.OK) txtPlanDxf.Text = o.FileName; };
            panelDxf.Controls.AddRange(new Control[] { lblDxf, txtPlanDxf, btnDxf });

            customContentPanel.Controls.Add(panelId);
            customContentPanel.Controls.Add(panelDxf);

            rbId.CheckedChanged += (s, e) => {
                panelId.Visible = rbId.Checked;
                panelDxf.Visible = !rbId.Checked;
                txtLocatorId.Text = GlobalStateManager.LocatorTaskId;
                if (string.IsNullOrEmpty(txtLocatorId.Text)) txtLocatorId.Text = "缺失！请先执行生成定位线";
            };
        }

        protected override void AddParameters(MultipartFormDataContent form)
        {
            AddFileToForm(form, txtWestDxf.Text, "west_dxf");
            AddFileToForm(form, txtEastDxf.Text, "east_dxf");
            AddFileToForm(form, txtSouthDxf.Text, "south_dxf");
            AddFileToForm(form, txtNorthDxf.Text, "north_dxf");

            AddFileToForm(form, txtWestCfg.Text, "west_config");
            AddFileToForm(form, txtEastCfg.Text, "east_config");
            AddFileToForm(form, txtSouthCfg.Text, "south_config");
            AddFileToForm(form, txtNorthCfg.Text, "north_config");

            if (rbId.Checked)
            {
                if (string.IsNullOrEmpty(GlobalStateManager.LocatorTaskId)) throw new Exception("缺失定位线 Task ID！");
                form.Add(new StringContent(GlobalStateManager.LocatorTaskId), "locator_task_id");
            }
            else
            {
                AddFileToForm(form, txtPlanDxf.Text, "plan_dxf");
            }
        }
    }

    // ==========================================
    // 8098 节点种子库 (ID模式 / 文件模式) - 彻底修复 422 错误
    // ==========================================
    public class NodeLibraryControl : BaseTaskControl
    {
        private RadioButton rbIdMode, rbFileMode;
        private Panel panelId, panelFile;
        private TextBox txtAssocZip, txtDetailZip, txtFacadeGeo, txtFacadeMat, txtNodeRep, txtPlanRep;

        protected override string TaskTitle => "节点种子库 (Task 8098)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_NodeLibrary}{ApiConfig.Path_NodeLibrary}";
        protected override string DefaultSaveFileName => "Node_Library.zip";

        public NodeLibraryControl()
        {
            rbIdMode = new RadioButton { Text = "模式A：使用上游任务 ID", Location = new Point(160, 10), Checked = true, AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
            rbFileMode = new RadioButton { Text = "模式B：手动上传结果文件", Location = new Point(380, 10), AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
            customContentPanel.Controls.AddRange(new Control[] { rbIdMode, rbFileMode });

            panelId = new Panel { Location = new Point(0, 40), Size = new Size(650, 250) };
            AddIdRow(panelId, "关联立面 Task ID：", 0);
            AddIdRow(panelId, "大样图 Task ID：", 40);
            AddIdRow(panelId, "立面图 Task ID：", 80);
            AddIdRow(panelId, "节点图 Task ID：", 120);
            AddIdRow(panelId, "平面图 Task ID：", 160);

            panelFile = new Panel { Location = new Point(0, 40), Size = new Size(650, 300), Visible = false };
            txtAssocZip = AddFileRow(panelFile, "关联立面 ZIP：", 0, "ZIP|*.zip");
            txtDetailZip = AddFileRow(panelFile, "大样图 ZIP：", 40, "ZIP|*.zip");
            txtFacadeGeo = AddFileRow(panelFile, "立面定位 JSON：", 80, "JSON|*.json");
            txtFacadeMat = AddFileRow(panelFile, "立面材料 TXT：", 120, "TXT|*.txt");
            txtNodeRep = AddFileRow(panelFile, "节点信息 TXT：", 160, "TXT|*.txt");
            txtPlanRep = AddFileRow(panelFile, "平面通风口 TXT：", 200, "TXT|*.txt");

            customContentPanel.Controls.Add(panelId);
            customContentPanel.Controls.Add(panelFile);

            rbIdMode.CheckedChanged += (s, e) => {
                panelId.Visible = rbIdMode.Checked;
                panelFile.Visible = !rbIdMode.Checked;

                if (rbIdMode.Checked)
                {
                    if (panelId.Controls[1] is TextBox t1) t1.Text = GlobalStateManager.AssociationTaskId;
                    if (panelId.Controls[3] is TextBox t2) t2.Text = GlobalStateManager.DetailTaskId;
                    if (panelId.Controls[5] is TextBox t3) t3.Text = GlobalStateManager.ElevationTaskId;
                    if (panelId.Controls[7] is TextBox t4) t4.Text = GlobalStateManager.NodeTaskId;
                    if (panelId.Controls[9] is TextBox t5) t5.Text = GlobalStateManager.PlanTaskId;
                }
            };
        }

        protected override void AddParameters(MultipartFormDataContent form)
        {
            if (rbIdMode.Checked)
            {
                // ★ 核心修复：四选一（如果是关联ID，就绝不传大样ID；如果是大样ID，就不传关联ID）
                if (!string.IsNullOrEmpty(GlobalStateManager.AssociationTaskId))
                {
                    form.Add(new StringContent(GlobalStateManager.AssociationTaskId), "association_task_id");
                }
                else if (!string.IsNullOrEmpty(GlobalStateManager.DetailTaskId))
                {
                    form.Add(new StringContent(GlobalStateManager.DetailTaskId), "detail_task_id");
                }
                else
                {
                    throw new Exception("缺失大样关联或大样图 Task ID！");
                }

                // 其他三个必填 ID
                form.Add(new StringContent(GlobalStateManager.ElevationTaskId ?? ""), "elevation_task_id");
                form.Add(new StringContent(GlobalStateManager.NodeTaskId ?? ""), "node_task_id");
                form.Add(new StringContent(GlobalStateManager.PlanTaskId ?? ""), "plan_task_id");

                // 模式A下，不传任何文件
            }
            else
            {
                // ★ 核心修复：四选一（文件版）
                if (!string.IsNullOrEmpty(txtAssocZip.Text))
                {
                    AddFileToForm(form, txtAssocZip.Text, "associations_zip");
                }
                else if (!string.IsNullOrEmpty(txtDetailZip.Text))
                {
                    AddFileToForm(form, txtDetailZip.Text, "details_zip");
                }
                else
                {
                    throw new Exception("模式B下，必须上传【关联立面 ZIP】或【大样图 ZIP】其中一个！");
                }

                // 其余四个必填文件
                AddFileToForm(form, txtFacadeGeo.Text, "facade_geometry");
                AddFileToForm(form, txtFacadeMat.Text, "facade_materials");
                AddFileToForm(form, txtNodeRep.Text, "node_report");
                AddFileToForm(form, txtPlanRep.Text, "plan_report");

                // 模式B下，不传任何 Task ID 字段
            }
        }
        protected override void SaveTaskIdToGlobal(string taskId) => GlobalStateManager.NodeLibraryTaskId = taskId;
    }

    // ==========================================
    // 8099 构件节点匹配 (ID模式 / 文件模式) - 彻底修复 422 错误
    // ==========================================
    public class MemberNodeMatchControl : BaseTaskControl
    {
        private RadioButton rbIdMode, rbFileMode;
        private Panel panelId, panelFile;
        private TextBox txtFacadeGeo, txtFacadeMat, txtPlanRep, txtSeedsJson;

        protected override string TaskTitle => "构件节点匹配 (Task 8099)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_MemberNode}{ApiConfig.Path_MemberNode}";
        protected override string DefaultSaveFileName => "Member_Node_Match.zip";

        public MemberNodeMatchControl()
        {
            rbIdMode = new RadioButton { Text = "模式A：使用上游任务 ID", Location = new Point(160, 10), Checked = true, AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
            rbFileMode = new RadioButton { Text = "模式B：手动上传结果文件", Location = new Point(380, 10), AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
            customContentPanel.Controls.AddRange(new Control[] { rbIdMode, rbFileMode });

            panelId = new Panel { Location = new Point(0, 40), Size = new Size(650, 150) };
            AddIdRow(panelId, "立面图 Task ID：", 0);
            AddIdRow(panelId, "平面图 Task ID：", 40);
            AddIdRow(panelId, "节点库 Task ID：", 80);

            panelFile = new Panel { Location = new Point(0, 40), Size = new Size(650, 200), Visible = false };
            txtFacadeGeo = AddFileRow(panelFile, "立面定位 JSON：", 0, "JSON|*.json");
            txtFacadeMat = AddFileRow(panelFile, "立面材料 TXT：", 40, "TXT|*.txt");
            txtPlanRep = AddFileRow(panelFile, "平面报告 TXT：", 80, "TXT|*.txt");
            txtSeedsJson = AddFileRow(panelFile, "种子库 JSON：", 120, "JSON|*.json");

            customContentPanel.Controls.Add(panelId);
            customContentPanel.Controls.Add(panelFile);

            rbIdMode.CheckedChanged += (s, e) => {
                panelId.Visible = rbIdMode.Checked;
                panelFile.Visible = !rbIdMode.Checked;

                if (rbIdMode.Checked)
                {
                    if (panelId.Controls[1] is TextBox t1) t1.Text = GlobalStateManager.ElevationTaskId;
                    if (panelId.Controls[3] is TextBox t2) t2.Text = GlobalStateManager.PlanTaskId;
                    if (panelId.Controls[5] is TextBox t3) t3.Text = GlobalStateManager.NodeLibraryTaskId;
                }
            };
        }

        protected override void AddParameters(MultipartFormDataContent form)
        {
            if (rbIdMode.Checked)
            {
                // 只传 ID
                form.Add(new StringContent(GlobalStateManager.ElevationTaskId ?? ""), "elevation_task_id");
                form.Add(new StringContent(GlobalStateManager.PlanTaskId ?? ""), "plan_task_id");
                form.Add(new StringContent(GlobalStateManager.NodeLibraryTaskId ?? ""), "node_library_task_id");
            }
            else
            {
                // 只传文件
                AddFileToForm(form, txtFacadeGeo.Text, "facade_geometry");
                AddFileToForm(form, txtFacadeMat.Text, "facade_materials");
                AddFileToForm(form, txtPlanRep.Text, "plan_report");
                AddFileToForm(form, txtSeedsJson.Text, "seeds_json");
            }
        }
    }

    // ==========================================
    // 8100 模型拉伸 (9个必填 + 2个可选文件)
    // ==========================================
    public class ModelStretchControl : BaseTaskControl
    {
        // 必填项
        private TextBox txtElevationMembersJson, txtNodeDxf, txtSkinModel3dm;
        private TextBox txtSkinWestZip, txtSkinEastZip, txtSkinNorthZip, txtSkinSouthZip;
        private TextBox txtNumberTableXlsx, txtMemberNodeMatchTxt;

        // 可选项
        private TextBox txtElevationDxf, txtProjectConfigJson;

        protected override string TaskTitle => "模型拉伸 (Task 8100)";
        protected override string SubmitApiUrl => $"http://{ApiConfig.BaseIp}:{ApiConfig.Port_ModelStretch}{ApiConfig.Path_ModelStretch}";
        protected override string DefaultSaveFileName => "Model_Stretch_Result.zip";

        public ModelStretchControl()
        {
            // 必填项分组（标签加上“*”提示）
            txtElevationMembersJson = AddFileRow(customContentPanel, "* 立面构件 JSON：", 10, "JSON|*.json");
            txtNodeDxf = AddFileRow(customContentPanel, "* 节点图 DXF：", 45, "DXF|*.dxf");
            txtSkinModel3dm = AddFileRow(customContentPanel, "* 表皮合并 3DM：", 80, "3DM|*.3dm");

            txtSkinWestZip = AddFileRow(customContentPanel, "* WEST 表皮 ZIP：", 115, "ZIP|*.zip");
            txtSkinEastZip = AddFileRow(customContentPanel, "* EAST 表皮 ZIP：", 150, "ZIP|*.zip");
            txtSkinNorthZip = AddFileRow(customContentPanel, "* NORTH 表皮 ZIP：", 185, "ZIP|*.zip");
            txtSkinSouthZip = AddFileRow(customContentPanel, "* SOUTH 表皮 ZIP：", 220, "ZIP|*.zip");

            txtNumberTableXlsx = AddFileRow(customContentPanel, "* 构件编号表 XLSX：", 255, "Excel文件|*.xlsx");
            txtMemberNodeMatchTxt = AddFileRow(customContentPanel, "* 节点匹配 TXT：", 290, "TXT|*.txt");

            //// 可选项分组
            //Label lblOptional = new Label { Text = "以下为可选参数：", Location = new Point(0, 335), AutoSize = true, ForeColor = Color.Gray, Font = new Font("微软雅黑", 9F, FontStyle.Italic) };
            //customContentPanel.Controls.Add(lblOptional);

            //txtElevationDxf = AddFileRow(customContentPanel, "立面 DXF (仅归档)：", 365, "DXF|*.dxf");
            //txtProjectConfigJson = AddFileRow(customContentPanel, "项目规则 JSON：", 400, "JSON|*.json");
        }

        protected override void AddParameters(MultipartFormDataContent form)
        {
            // ===== 1. 强制校验必填项 =====
            if (string.IsNullOrEmpty(txtElevationMembersJson.Text)) throw new Exception("请上传【立面构件 JSON】");
            if (string.IsNullOrEmpty(txtNodeDxf.Text)) throw new Exception("请上传【节点图 DXF】");
            if (string.IsNullOrEmpty(txtSkinModel3dm.Text)) throw new Exception("请上传【表皮合并 3DM】");
            if (string.IsNullOrEmpty(txtSkinWestZip.Text)) throw new Exception("请上传【WEST 表皮 ZIP】");
            if (string.IsNullOrEmpty(txtSkinEastZip.Text)) throw new Exception("请上传【EAST 表皮 ZIP】");
            if (string.IsNullOrEmpty(txtSkinNorthZip.Text)) throw new Exception("请上传【NORTH 表皮 ZIP】");
            if (string.IsNullOrEmpty(txtSkinSouthZip.Text)) throw new Exception("请上传【SOUTH 表皮 ZIP】");
            if (string.IsNullOrEmpty(txtNumberTableXlsx.Text)) throw new Exception("请上传【构件编号表 XLSX】");
            if (string.IsNullOrEmpty(txtMemberNodeMatchTxt.Text)) throw new Exception("请上传【节点匹配 TXT】");

            // ===== 2. 添加必填文件 =====
            AddFileToForm(form, txtElevationMembersJson.Text, "elevation_members_json");
            AddFileToForm(form, txtNodeDxf.Text, "node_dxf");
            AddFileToForm(form, txtSkinModel3dm.Text, "skin_model_3dm");
            AddFileToForm(form, txtSkinWestZip.Text, "skin_west_zip");
            AddFileToForm(form, txtSkinEastZip.Text, "skin_east_zip");
            AddFileToForm(form, txtSkinNorthZip.Text, "skin_north_zip");
            AddFileToForm(form, txtSkinSouthZip.Text, "skin_south_zip");
            AddFileToForm(form, txtNumberTableXlsx.Text, "number_table_xlsx");
            AddFileToForm(form, txtMemberNodeMatchTxt.Text, "member_node_match_txt");

            //// ===== 3. 添加可选文件 (有值才传，不传空字符串) =====
            //if (!string.IsNullOrEmpty(txtElevationDxf.Text))
            //    AddFileToForm(form, txtElevationDxf.Text, "elevation_dxf");

            //if (!string.IsNullOrEmpty(txtProjectConfigJson.Text))
            //    AddFileToForm(form, txtProjectConfigJson.Text, "project_config_json");
        }
    }


    // 占位控件 (未开发功能)
    public class PlaceholderControl : UserControl
    {
        public PlaceholderControl(string title)
        {
            this.BackColor = Color.White;
            Label lbl = new Label { Text = $"{title}\n(功能开发中)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("微软雅黑", 20F) };
            this.Controls.Add(lbl);
        }
    }
}