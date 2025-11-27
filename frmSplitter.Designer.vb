<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSplitter
    Inherits System.Windows.Forms.Form

    ''' <summary>
    ''' 清理所有正在使用的资源。
    ''' </summary>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSplitter))
        PanelTop = New Windows.Forms.Panel()
        btnAbout = New Windows.Forms.Button()
        btnGotoBottom = New Windows.Forms.Button()
        btnGotoTop = New Windows.Forms.Button()
        btnNextLine = New Windows.Forms.Button()
        lblInsertBlank = New Windows.Forms.Label()
        nudInsertBlankHeight = New Windows.Forms.NumericUpDown()
        btnPrevLine = New Windows.Forms.Button()
        chkGlobalWidth = New Windows.Forms.CheckBox()
        chkTrimWhite = New Windows.Forms.CheckBox()
        btnLoadProject = New Windows.Forms.Button()
        btnZoomOut = New Windows.Forms.Button()
        btnZoomIn = New Windows.Forms.Button()
        btnRedo = New Windows.Forms.Button()
        btnUndo = New Windows.Forms.Button()
        btnOpenFiles = New Windows.Forms.Button()
        btnOpenFolder = New Windows.Forms.Button()
        btnExport = New Windows.Forms.Button()
        pnlOptions = New Windows.Forms.Panel()
        lblFormat = New Windows.Forms.Label()
        cboFormat = New Windows.Forms.ComboBox()
        lblJpegQuality = New Windows.Forms.Label()
        nudJpegQuality = New Windows.Forms.NumericUpDown()
        lblPadding = New Windows.Forms.Label()
        nudPadLeft = New Windows.Forms.NumericUpDown()
        nudPadRight = New Windows.Forms.NumericUpDown()
        nudPadTop = New Windows.Forms.NumericUpDown()
        nudPadBottom = New Windows.Forms.NumericUpDown()
        lblPadLeft = New Windows.Forms.Label()
        lblPadRight = New Windows.Forms.Label()
        lblPadTop = New Windows.Forms.Label()
        lblPadBottom = New Windows.Forms.Label()
        pnlScroll = New Windows.Forms.Panel()
        picMain = New Windows.Forms.PictureBox()
        StatusStrip1 = New Windows.Forms.StatusStrip()
        lblStatus = New Windows.Forms.ToolStripStatusLabel()
        PanelTop.SuspendLayout()
        CType(nudInsertBlankHeight, ComponentModel.ISupportInitialize).BeginInit()
        pnlOptions.SuspendLayout()
        CType(nudJpegQuality, ComponentModel.ISupportInitialize).BeginInit()
        CType(nudPadLeft, ComponentModel.ISupportInitialize).BeginInit()
        CType(nudPadRight, ComponentModel.ISupportInitialize).BeginInit()
        CType(nudPadTop, ComponentModel.ISupportInitialize).BeginInit()
        CType(nudPadBottom, ComponentModel.ISupportInitialize).BeginInit()
        pnlScroll.SuspendLayout()
        CType(picMain, ComponentModel.ISupportInitialize).BeginInit()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' PanelTop
        ' 
        PanelTop.BackColor = Drawing.Color.FromArgb(CByte(255), CByte(236), CByte(209))
        PanelTop.Controls.Add(btnAbout)
        PanelTop.Controls.Add(btnGotoBottom)
        PanelTop.Controls.Add(btnGotoTop)
        PanelTop.Controls.Add(btnNextLine)
        PanelTop.Controls.Add(lblInsertBlank)
        PanelTop.Controls.Add(nudInsertBlankHeight)
        PanelTop.Controls.Add(btnPrevLine)
        PanelTop.Controls.Add(chkGlobalWidth)
        PanelTop.Controls.Add(chkTrimWhite)
        PanelTop.Controls.Add(btnLoadProject)
        PanelTop.Controls.Add(btnZoomOut)
        PanelTop.Controls.Add(btnZoomIn)
        PanelTop.Controls.Add(btnRedo)
        PanelTop.Controls.Add(btnUndo)
        PanelTop.Controls.Add(btnOpenFiles)
        PanelTop.Controls.Add(btnOpenFolder)
        PanelTop.Dock = Windows.Forms.DockStyle.Top
        PanelTop.Location = New System.Drawing.Point(0, 0)
        PanelTop.Margin = New System.Windows.Forms.Padding(0)
        PanelTop.Name = "PanelTop"
        PanelTop.Padding = New System.Windows.Forms.Padding(8, 6, 8, 6)
        PanelTop.Size = New System.Drawing.Size(961, 60)
        PanelTop.TabIndex = 0
        ' 
        ' btnAbout
        ' 
        btnAbout.Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Right
        btnAbout.Location = New System.Drawing.Point(861, 10)
        btnAbout.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnAbout.Name = "btnAbout"
        btnAbout.Size = New System.Drawing.Size(88, 24)
        btnAbout.TabIndex = 13
        btnAbout.Text = "关于(&A)"
        btnAbout.UseVisualStyleBackColor = True
        ' 
        ' btnGotoBottom
        ' 
        btnGotoBottom.Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Right
        btnGotoBottom.Location = New System.Drawing.Point(715, 37)
        btnGotoBottom.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnGotoBottom.Name = "btnGotoBottom"
        btnGotoBottom.Size = New System.Drawing.Size(86, 24)
        btnGotoBottom.TabIndex = 12
        btnGotoBottom.Text = "底部(End)"
        btnGotoBottom.UseVisualStyleBackColor = True
        ' 
        ' btnGotoTop
        ' 
        btnGotoTop.Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Right
        btnGotoTop.Location = New System.Drawing.Point(715, 11)
        btnGotoTop.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnGotoTop.Name = "btnGotoTop"
        btnGotoTop.Size = New System.Drawing.Size(86, 23)
        btnGotoTop.TabIndex = 11
        btnGotoTop.Text = "顶部(Home)"
        btnGotoTop.UseVisualStyleBackColor = True
        ' 
        ' btnNextLine
        ' 
        btnNextLine.Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Right
        btnNextLine.Location = New System.Drawing.Point(627, 37)
        btnNextLine.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnNextLine.Name = "btnNextLine"
        btnNextLine.Size = New System.Drawing.Size(80, 24)
        btnNextLine.TabIndex = 10
        btnNextLine.Text = "下一蓝线"
        btnNextLine.UseVisualStyleBackColor = True
        ' 
        ' lblInsertBlank
        ' 
        lblInsertBlank.AutoSize = True
        lblInsertBlank.Location = New System.Drawing.Point(346, 39)
        lblInsertBlank.Name = "lblInsertBlank"
        lblInsertBlank.Size = New System.Drawing.Size(122, 24)
        lblInsertBlank.TabIndex = 11
        lblInsertBlank.Text = "插入空白高度:"
        ' 
        ' nudInsertBlankHeight
        ' 
        nudInsertBlankHeight.Location = New System.Drawing.Point(474, 37)
        nudInsertBlankHeight.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        nudInsertBlankHeight.Name = "nudInsertBlankHeight"
        nudInsertBlankHeight.Size = New System.Drawing.Size(56, 30)
        nudInsertBlankHeight.TabIndex = 12
        ' 
        ' btnPrevLine
        ' 
        btnPrevLine.Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Right
        btnPrevLine.Location = New System.Drawing.Point(627, 10)
        btnPrevLine.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnPrevLine.Name = "btnPrevLine"
        btnPrevLine.Size = New System.Drawing.Size(80, 24)
        btnPrevLine.TabIndex = 9
        btnPrevLine.Text = "上一蓝线"
        btnPrevLine.UseVisualStyleBackColor = True
        ' 
        ' chkGlobalWidth
        ' 
        chkGlobalWidth.AutoSize = True
        chkGlobalWidth.Location = New System.Drawing.Point(8, 37)
        chkGlobalWidth.Name = "chkGlobalWidth"
        chkGlobalWidth.Size = New System.Drawing.Size(180, 28)
        chkGlobalWidth.TabIndex = 8
        chkGlobalWidth.Text = "导出图片统一宽度"
        chkGlobalWidth.UseVisualStyleBackColor = True
        ' 
        ' chkTrimWhite
        ' 
        chkTrimWhite.AutoSize = True
        chkTrimWhite.Location = New System.Drawing.Point(188, 37)
        chkTrimWhite.Name = "chkTrimWhite"
        chkTrimWhite.Size = New System.Drawing.Size(144, 28)
        chkTrimWhite.TabIndex = 0
        chkTrimWhite.Text = "去除纯白边缘"
        chkTrimWhite.UseVisualStyleBackColor = True
        ' 
        ' btnLoadProject
        ' 
        btnLoadProject.Location = New System.Drawing.Point(188, 8)
        btnLoadProject.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnLoadProject.Name = "btnLoadProject"
        btnLoadProject.Size = New System.Drawing.Size(88, 24)
        btnLoadProject.TabIndex = 7
        btnLoadProject.Text = "读取工程(&L)"
        btnLoadProject.UseVisualStyleBackColor = True
        ' 
        ' btnZoomOut
        ' 
        btnZoomOut.Location = New System.Drawing.Point(535, 8)
        btnZoomOut.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnZoomOut.Name = "btnZoomOut"
        btnZoomOut.Size = New System.Drawing.Size(52, 24)
        btnZoomOut.TabIndex = 5
        btnZoomOut.Text = "－"
        btnZoomOut.UseVisualStyleBackColor = True
        ' 
        ' btnZoomIn
        ' 
        btnZoomIn.Location = New System.Drawing.Point(473, 8)
        btnZoomIn.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnZoomIn.Name = "btnZoomIn"
        btnZoomIn.Size = New System.Drawing.Size(52, 24)
        btnZoomIn.TabIndex = 4
        btnZoomIn.Text = "＋"
        btnZoomIn.UseVisualStyleBackColor = True
        ' 
        ' btnRedo
        ' 
        btnRedo.Location = New System.Drawing.Point(409, 8)
        btnRedo.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnRedo.Name = "btnRedo"
        btnRedo.Size = New System.Drawing.Size(56, 24)
        btnRedo.TabIndex = 3
        btnRedo.Text = "↷ 重做"
        btnRedo.UseVisualStyleBackColor = True
        ' 
        ' btnUndo
        ' 
        btnUndo.Location = New System.Drawing.Point(345, 8)
        btnUndo.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnUndo.Name = "btnUndo"
        btnUndo.Size = New System.Drawing.Size(56, 24)
        btnUndo.TabIndex = 2
        btnUndo.Text = "↶ 撤销"
        btnUndo.UseVisualStyleBackColor = True
        ' 
        ' btnOpenFiles
        ' 
        btnOpenFiles.Location = New System.Drawing.Point(96, 8)
        btnOpenFiles.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnOpenFiles.Name = "btnOpenFiles"
        btnOpenFiles.Size = New System.Drawing.Size(84, 24)
        btnOpenFiles.TabIndex = 1
        btnOpenFiles.Text = "打开文件(&O)"
        btnOpenFiles.UseVisualStyleBackColor = True
        ' 
        ' btnOpenFolder
        ' 
        btnOpenFolder.Location = New System.Drawing.Point(8, 8)
        btnOpenFolder.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnOpenFolder.Name = "btnOpenFolder"
        btnOpenFolder.Size = New System.Drawing.Size(80, 24)
        btnOpenFolder.TabIndex = 0
        btnOpenFolder.Text = "打开文件夹"
        btnOpenFolder.UseVisualStyleBackColor = True
        ' 
        ' btnExport
        ' 
        btnExport.Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Right
        btnExport.Location = New System.Drawing.Point(860, 13)
        btnExport.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        btnExport.Name = "btnExport"
        btnExport.Size = New System.Drawing.Size(88, 24)
        btnExport.TabIndex = 6
        btnExport.Text = "导出分割图(&E)"
        btnExport.UseVisualStyleBackColor = True
        ' 
        ' pnlOptions
        ' 
        pnlOptions.BackColor = Drawing.Color.FromArgb(CByte(255), CByte(244), CByte(223))
        pnlOptions.Controls.Add(lblFormat)
        pnlOptions.Controls.Add(cboFormat)
        pnlOptions.Controls.Add(lblJpegQuality)
        pnlOptions.Controls.Add(nudJpegQuality)
        pnlOptions.Controls.Add(lblPadding)
        pnlOptions.Controls.Add(nudPadLeft)
        pnlOptions.Controls.Add(nudPadRight)
        pnlOptions.Controls.Add(nudPadTop)
        pnlOptions.Controls.Add(nudPadBottom)
        pnlOptions.Controls.Add(lblPadLeft)
        pnlOptions.Controls.Add(btnExport)
        pnlOptions.Controls.Add(lblPadRight)
        pnlOptions.Controls.Add(lblPadTop)
        pnlOptions.Controls.Add(lblPadBottom)
        pnlOptions.Dock = Windows.Forms.DockStyle.Top
        pnlOptions.Location = New System.Drawing.Point(0, 60)
        pnlOptions.Name = "pnlOptions"
        pnlOptions.Size = New System.Drawing.Size(961, 48)
        pnlOptions.TabIndex = 3
        ' 
        ' lblFormat
        ' 
        lblFormat.AutoSize = True
        lblFormat.Location = New System.Drawing.Point(476, 15)
        lblFormat.Name = "lblFormat"
        lblFormat.Size = New System.Drawing.Size(50, 24)
        lblFormat.TabIndex = 13
        lblFormat.Text = "格式:"
        ' 
        ' cboFormat
        ' 
        cboFormat.DropDownStyle = Windows.Forms.ComboBoxStyle.DropDownList
        cboFormat.FormattingEnabled = True
        cboFormat.Location = New System.Drawing.Point(517, 11)
        cboFormat.Name = "cboFormat"
        cboFormat.Size = New System.Drawing.Size(72, 32)
        cboFormat.TabIndex = 14
        ' 
        ' lblJpegQuality
        ' 
        lblJpegQuality.AutoSize = True
        lblJpegQuality.Location = New System.Drawing.Point(600, 15)
        lblJpegQuality.Name = "lblJpegQuality"
        lblJpegQuality.Size = New System.Drawing.Size(81, 24)
        lblJpegQuality.TabIndex = 15
        lblJpegQuality.Text = "JPG质量:"
        ' 
        ' nudJpegQuality
        ' 
        nudJpegQuality.Location = New System.Drawing.Point(672, 13)
        nudJpegQuality.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        nudJpegQuality.Name = "nudJpegQuality"
        nudJpegQuality.Size = New System.Drawing.Size(56, 30)
        nudJpegQuality.TabIndex = 16
        nudJpegQuality.Value = New Decimal(New Integer() {90, 0, 0, 0})
        ' 
        ' lblPadding
        ' 
        lblPadding.AutoSize = True
        lblPadding.Location = New System.Drawing.Point(8, 15)
        lblPadding.Name = "lblPadding"
        lblPadding.Size = New System.Drawing.Size(86, 24)
        lblPadding.TabIndex = 1
        lblPadding.Text = "四边空白:"
        ' 
        ' nudPadLeft
        ' 
        nudPadLeft.Location = New System.Drawing.Point(100, 13)
        nudPadLeft.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        nudPadLeft.Name = "nudPadLeft"
        nudPadLeft.Size = New System.Drawing.Size(48, 30)
        nudPadLeft.TabIndex = 3
        ' 
        ' nudPadRight
        ' 
        nudPadRight.Location = New System.Drawing.Point(196, 13)
        nudPadRight.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        nudPadRight.Name = "nudPadRight"
        nudPadRight.Size = New System.Drawing.Size(48, 30)
        nudPadRight.TabIndex = 5
        ' 
        ' nudPadTop
        ' 
        nudPadTop.Location = New System.Drawing.Point(292, 13)
        nudPadTop.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        nudPadTop.Name = "nudPadTop"
        nudPadTop.Size = New System.Drawing.Size(48, 30)
        nudPadTop.TabIndex = 7
        ' 
        ' nudPadBottom
        ' 
        nudPadBottom.Location = New System.Drawing.Point(388, 13)
        nudPadBottom.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        nudPadBottom.Name = "nudPadBottom"
        nudPadBottom.Size = New System.Drawing.Size(48, 30)
        nudPadBottom.TabIndex = 9
        ' 
        ' lblPadLeft
        ' 
        lblPadLeft.AutoSize = True
        lblPadLeft.Location = New System.Drawing.Point(72, 15)
        lblPadLeft.Name = "lblPadLeft"
        lblPadLeft.Size = New System.Drawing.Size(32, 24)
        lblPadLeft.TabIndex = 2
        lblPadLeft.Text = "左:"
        ' 
        ' lblPadRight
        ' 
        lblPadRight.AutoSize = True
        lblPadRight.Location = New System.Drawing.Point(168, 15)
        lblPadRight.Name = "lblPadRight"
        lblPadRight.Size = New System.Drawing.Size(32, 24)
        lblPadRight.TabIndex = 4
        lblPadRight.Text = "右:"
        ' 
        ' lblPadTop
        ' 
        lblPadTop.AutoSize = True
        lblPadTop.Location = New System.Drawing.Point(264, 15)
        lblPadTop.Name = "lblPadTop"
        lblPadTop.Size = New System.Drawing.Size(32, 24)
        lblPadTop.TabIndex = 6
        lblPadTop.Text = "上:"
        ' 
        ' lblPadBottom
        ' 
        lblPadBottom.AutoSize = True
        lblPadBottom.Location = New System.Drawing.Point(360, 15)
        lblPadBottom.Name = "lblPadBottom"
        lblPadBottom.Size = New System.Drawing.Size(32, 24)
        lblPadBottom.TabIndex = 8
        lblPadBottom.Text = "下:"
        ' 
        ' pnlScroll
        ' 
        pnlScroll.AutoScroll = True
        pnlScroll.BackColor = Drawing.Color.White
        pnlScroll.Controls.Add(picMain)
        pnlScroll.Dock = Windows.Forms.DockStyle.Fill
        pnlScroll.Location = New System.Drawing.Point(0, 108)
        pnlScroll.Name = "pnlScroll"
        pnlScroll.Size = New System.Drawing.Size(961, 445)
        pnlScroll.TabIndex = 1
        ' 
        ' picMain
        ' 
        picMain.Location = New System.Drawing.Point(0, 0)
        picMain.Name = "picMain"
        picMain.Size = New System.Drawing.Size(100, 50)
        picMain.SizeMode = Windows.Forms.PictureBoxSizeMode.AutoSize
        picMain.TabIndex = 0
        picMain.TabStop = False
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.BackColor = Drawing.Color.FromArgb(CByte(255), CByte(236), CByte(209))
        StatusStrip1.ImageScalingSize = New System.Drawing.Size(24, 24)
        StatusStrip1.Items.AddRange(New Windows.Forms.ToolStripItem() {lblStatus})
        StatusStrip1.Location = New System.Drawing.Point(0, 553)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New System.Drawing.Size(961, 31)
        StatusStrip1.TabIndex = 2
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' lblStatus
        ' 
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New System.Drawing.Size(76, 24)
        lblStatus.Text = "就绪……"
        ' 
        ' frmSplitter
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(11.0F, 24.0F)
        AutoScaleMode = Windows.Forms.AutoScaleMode.Font
        BackColor = Drawing.Color.FromArgb(CByte(250), CByte(245), CByte(240))
        ClientSize = New System.Drawing.Size(961, 584)
        Controls.Add(pnlScroll)
        Controls.Add(pnlOptions)
        Controls.Add(StatusStrip1)
        Controls.Add(PanelTop)
        Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        KeyPreview = True
        Name = "frmSplitter"
        StartPosition = Windows.Forms.FormStartPosition.CenterScreen
        Text = "图片分割标记工具 - 左键: 蓝线  右键: 红线删除"
        PanelTop.ResumeLayout(False)
        PanelTop.PerformLayout()
        CType(nudInsertBlankHeight, ComponentModel.ISupportInitialize).EndInit()
        pnlOptions.ResumeLayout(False)
        pnlOptions.PerformLayout()
        CType(nudJpegQuality, ComponentModel.ISupportInitialize).EndInit()
        CType(nudPadLeft, ComponentModel.ISupportInitialize).EndInit()
        CType(nudPadRight, ComponentModel.ISupportInitialize).EndInit()
        CType(nudPadTop, ComponentModel.ISupportInitialize).EndInit()
        CType(nudPadBottom, ComponentModel.ISupportInitialize).EndInit()
        pnlScroll.ResumeLayout(False)
        pnlScroll.PerformLayout()
        CType(picMain, ComponentModel.ISupportInitialize).EndInit()
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents PanelTop As System.Windows.Forms.Panel
    Friend WithEvents btnAbout As System.Windows.Forms.Button
    Friend WithEvents btnGotoBottom As System.Windows.Forms.Button
    Friend WithEvents btnGotoTop As System.Windows.Forms.Button
    Friend WithEvents btnNextLine As System.Windows.Forms.Button
    Friend WithEvents btnPrevLine As System.Windows.Forms.Button
    Friend WithEvents chkGlobalWidth As System.Windows.Forms.CheckBox
    Friend WithEvents btnLoadProject As System.Windows.Forms.Button
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents btnZoomOut As System.Windows.Forms.Button
    Friend WithEvents btnZoomIn As System.Windows.Forms.Button
    Friend WithEvents btnRedo As System.Windows.Forms.Button
    Friend WithEvents btnUndo As System.Windows.Forms.Button
    Friend WithEvents btnOpenFiles As System.Windows.Forms.Button
    Friend WithEvents btnOpenFolder As System.Windows.Forms.Button
    Friend WithEvents pnlOptions As System.Windows.Forms.Panel
    Friend WithEvents chkTrimWhite As System.Windows.Forms.CheckBox
    Friend WithEvents lblPadding As System.Windows.Forms.Label
    Friend WithEvents nudPadLeft As System.Windows.Forms.NumericUpDown
    Friend WithEvents nudPadRight As System.Windows.Forms.NumericUpDown
    Friend WithEvents nudPadTop As System.Windows.Forms.NumericUpDown
    Friend WithEvents nudPadBottom As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPadLeft As System.Windows.Forms.Label
    Friend WithEvents lblPadRight As System.Windows.Forms.Label
    Friend WithEvents lblPadTop As System.Windows.Forms.Label
    Friend WithEvents lblPadBottom As System.Windows.Forms.Label
    Friend WithEvents lblFormat As System.Windows.Forms.Label
    Friend WithEvents cboFormat As System.Windows.Forms.ComboBox
    Friend WithEvents lblJpegQuality As System.Windows.Forms.Label
    Friend WithEvents nudJpegQuality As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblInsertBlank As System.Windows.Forms.Label
    Friend WithEvents nudInsertBlankHeight As System.Windows.Forms.NumericUpDown
    Friend WithEvents pnlScroll As System.Windows.Forms.Panel
    Friend WithEvents picMain As System.Windows.Forms.PictureBox
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents lblStatus As System.Windows.Forms.ToolStripStatusLabel

End Class
