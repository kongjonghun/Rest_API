namespace EX03
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbParcel = new System.Windows.Forms.RadioButton();
            this.rbBuilding = new System.Windows.Forms.RadioButton();
            this.rbGu = new System.Windows.Forms.RadioButton();
            this.rbRoad = new System.Windows.Forms.RadioButton();
            this.rbBicycle = new System.Windows.Forms.RadioButton();
            this.axMap1 = new AxMapWinGIS.AxMap();
            this.legendPanel = new System.Windows.Forms.Panel();
            this.dgvLegend = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axMap1)).BeginInit();
            this.legendPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLegend)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.legendPanel);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.axMap1);
            this.splitContainer1.Size = new System.Drawing.Size(1092, 512);
            this.splitContainer1.SplitterDistance = 252;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbParcel);
            this.groupBox1.Controls.Add(this.rbBuilding);
            this.groupBox1.Controls.Add(this.rbGu);
            this.groupBox1.Controls.Add(this.rbRoad);
            this.groupBox1.Controls.Add(this.rbBicycle);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(215, 199);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "레이어 목록";
            // 
            // rbParcel
            // 
            this.rbParcel.AutoSize = true;
            this.rbParcel.Location = new System.Drawing.Point(6, 154);
            this.rbParcel.Name = "rbParcel";
            this.rbParcel.Size = new System.Drawing.Size(93, 19);
            this.rbParcel.TabIndex = 4;
            this.rbParcel.TabStop = true;
            this.rbParcel.Text = "필지 가격";
            this.rbParcel.UseVisualStyleBackColor = true;
            this.rbParcel.CheckedChanged += new System.EventHandler(this.LayerCheck);
            // 
            // rbBuilding
            // 
            this.rbBuilding.AutoSize = true;
            this.rbBuilding.Location = new System.Drawing.Point(6, 124);
            this.rbBuilding.Name = "rbBuilding";
            this.rbBuilding.Size = new System.Drawing.Size(93, 19);
            this.rbBuilding.TabIndex = 3;
            this.rbBuilding.TabStop = true;
            this.rbBuilding.Text = "건물 용도";
            this.rbBuilding.UseVisualStyleBackColor = true;
            this.rbBuilding.CheckedChanged += new System.EventHandler(this.LayerCheck);
            // 
            // rbGu
            // 
            this.rbGu.AutoSize = true;
            this.rbGu.Location = new System.Drawing.Point(6, 94);
            this.rbGu.Name = "rbGu";
            this.rbGu.Size = new System.Drawing.Size(73, 19);
            this.rbGu.TabIndex = 2;
            this.rbGu.TabStop = true;
            this.rbGu.Text = "자치구";
            this.rbGu.UseVisualStyleBackColor = true;
            this.rbGu.CheckedChanged += new System.EventHandler(this.LayerCheck);
            // 
            // rbRoad
            // 
            this.rbRoad.AutoSize = true;
            this.rbRoad.Location = new System.Drawing.Point(6, 64);
            this.rbRoad.Name = "rbRoad";
            this.rbRoad.Size = new System.Drawing.Size(93, 19);
            this.rbRoad.TabIndex = 1;
            this.rbRoad.TabStop = true;
            this.rbRoad.Text = "도로 링크";
            this.rbRoad.UseVisualStyleBackColor = true;
            this.rbRoad.CheckedChanged += new System.EventHandler(this.LayerCheck);
            // 
            // rbBicycle
            // 
            this.rbBicycle.AutoSize = true;
            this.rbBicycle.Location = new System.Drawing.Point(6, 34);
            this.rbBicycle.Name = "rbBicycle";
            this.rbBicycle.Size = new System.Drawing.Size(123, 19);
            this.rbBicycle.TabIndex = 0;
            this.rbBicycle.TabStop = true;
            this.rbBicycle.Text = "따릉이 대여소";
            this.rbBicycle.UseVisualStyleBackColor = true;
            this.rbBicycle.CheckedChanged += new System.EventHandler(this.LayerCheck);
            // 
            // axMap1
            // 
            this.axMap1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axMap1.Enabled = true;
            this.axMap1.Location = new System.Drawing.Point(0, 0);
            this.axMap1.Name = "axMap1";
            this.axMap1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMap1.OcxState")));
            this.axMap1.Size = new System.Drawing.Size(836, 512);
            this.axMap1.TabIndex = 0;
            this.axMap1.ProjectionMismatch += new AxMapWinGIS._DMapEvents_ProjectionMismatchEventHandler(this.axMap1_ProjectionMismatch);
            // 
            // legendPanel
            // 
            this.legendPanel.Controls.Add(this.dgvLegend);
            this.legendPanel.Location = new System.Drawing.Point(12, 241);
            this.legendPanel.Name = "legendPanel";
            this.legendPanel.Size = new System.Drawing.Size(215, 247);
            this.legendPanel.TabIndex = 1;
            // 
            // dgvLegend
            // 
            this.dgvLegend.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvLegend.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLegend.Location = new System.Drawing.Point(0, 0);
            this.dgvLegend.Name = "dgvLegend";
            this.dgvLegend.ReadOnly = true;
            this.dgvLegend.RowHeadersWidth = 51;
            this.dgvLegend.RowTemplate.Height = 27;
            this.dgvLegend.Size = new System.Drawing.Size(215, 247);
            this.dgvLegend.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 512);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axMap1)).EndInit();
            this.legendPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLegend)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbParcel;
        private System.Windows.Forms.RadioButton rbBuilding;
        private System.Windows.Forms.RadioButton rbGu;
        private System.Windows.Forms.RadioButton rbRoad;
        private System.Windows.Forms.RadioButton rbBicycle;
        private AxMapWinGIS.AxMap axMap1;
        private System.Windows.Forms.Panel legendPanel;
        private System.Windows.Forms.DataGridView dgvLegend;
    }
}

