Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Xml.Serialization

Partial Public Class frmSplitter

    ' 当前基础大图（已经做过删除操作的版本，没有画线）
    Private _baseImage As Bitmap = Nothing
    ' 原始拼接高度（刚 LoadImages 时的总高度，不随删除/插空改变）
    Private _originalHeight As Integer = 0
    ' 当前缩放比例
    Private _zoom As Single = 1.0F

    ' 所有原始文件路径（按数字排序）
    Private _imageFiles As New List(Of String)

    ' 工程文件夹（与图片文件夹记忆分离）
    Private _projectFolder As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Projects")

    ' 配置 INI 文件
    Private ReadOnly _iniPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vbnet_image_splitter.ini")

    ' 每张图片的信息
    Private Class ImageInfo
        Public Property FilePath As String
        Public Property Width As Integer
        Public Property Height As Integer
        Public Property TopY As Integer
        Public ReadOnly Property BottomY As Integer
            Get
                Return TopY + Height
            End Get
        End Property
    End Class

    Private _imageInfos As New List(Of ImageInfo)

    Private Enum LineType
        Blue
    End Enum

    Private Class SplitLine
        Public Property Y As Integer
        Public Property Type As LineType
    End Class

    ' 所有分割线（逻辑坐标）
    Private _lines As New List(Of SplitLine)

    ' 撤销 / 重做动作接口
    Private Interface IEditAction
        Sub DoAction()
        Sub UndoAction()
        ReadOnly Property FocusY As Integer
    End Interface

    Private _history As New List(Of IEditAction)
    Private _historyIndex As Integer = -1            ' 指向已经执行到的动作

    ' 工程文件路径
    Private _projectFilePath As String = Nothing

    ' 删除预览（红色斜线区域）
    Private _previewDeleteActive As Boolean = False
    Private _previewDeleteTop As Integer = 0
    Private _previewDeleteBottom As Integer = 0
    Private _rightButtonDown As Boolean = False
    Private _rightDragStartViewY As Integer = 0
    Private _rightDragStartViewX As Integer = 0
    Private _previewBlueDeleteActive As Boolean = False
    Private _previewBlueLines As New List(Of Integer)()

    ' 已删除区域计数（当前状态下有效的删除段数）
    Private _deletedRegionCount As Integer = 0

    ' 当前工程文件路径（从工程打开时记住，用于保存时回写原工程）
    Private _currentProjectPath As String = Nothing



    '==================== 基本工具与事件 ====================

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MinimumSize = New Size(Me.Width, 0)  ' 高度不限制，0 表示无限制
        Try
            If Not Directory.Exists(_projectFolder) Then
                Directory.CreateDirectory(_projectFolder)
            End If
        Catch
        End Try

        StyleButtons()
        InitOptionControls()
        LoadIniOptions()
        UpdateStatus("就绪……  左键添加蓝线，右键预览红线删除。")
    End Sub

    Private Sub frmSplitter_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' 判断是否存在正在编辑的内容：已经加载了图片/工程
        Dim hasEditing As Boolean =
        (_baseImage IsNot Nothing AndAlso _imageFiles IsNot Nothing AndAlso _imageFiles.Count > 0)

        ' 仅在用户主动关闭窗体时拦截（避免系统关机等情况卡死）
        If hasEditing AndAlso e.CloseReason = CloseReason.UserClosing Then
            Dim result As DialogResult = MessageBox.Show(
            "当前有文件正在编辑，确定要退出程序吗？",
            "确认退出",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

            If result = DialogResult.No Then
                ' 取消关闭
                e.Cancel = True
                Return
            End If
        End If

        ' 正常关闭时仍然保存 INI 选项
        SaveIniOptions()
    End Sub


    Private Sub StyleButtons()
        Dim buttonList As New List(Of Button) From {
            btnOpenFolder, btnOpenFiles, btnUndo, btnRedo,
            btnZoomIn, btnZoomOut, btnExport, btnLoadProject,
            btnPrevLine, btnNextLine, btnGotoTop, btnGotoBottom, btnAbout
        }
        For Each btn In buttonList
            btn.FlatStyle = FlatStyle.Flat
            btn.BackColor = Color.FromArgb(255, 224, 178)
            btn.ForeColor = Color.FromArgb(102, 51, 0)
            btn.FlatAppearance.BorderColor = Color.FromArgb(255, 183, 77)
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 213, 153)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 204, 128)
        Next
    End Sub

    Private Sub InitOptionControls()
        ' 导出格式
        cboFormat.Items.Clear()
        cboFormat.Items.Add("PNG")
        cboFormat.Items.Add("JPG")
        cboFormat.SelectedIndex = 0 ' 默认 PNG

        nudJpegQuality.Minimum = 10
        nudJpegQuality.Maximum = 100
        nudJpegQuality.Value = 90

        ' 插入空白高度
        nudInsertBlankHeight.Minimum = 0
        nudInsertBlankHeight.Maximum = 2000
        nudInsertBlankHeight.Value = 0

        UpdateJpegQualityEnabled()
    End Sub

    Private Sub UpdateJpegQualityEnabled()
        If cboFormat.SelectedItem IsNot Nothing AndAlso cboFormat.SelectedItem.ToString().ToUpper() = "JPG" Then
            nudJpegQuality.Enabled = True
        Else
            nudJpegQuality.Enabled = False
        End If
    End Sub

    Private Sub cboFormat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFormat.SelectedIndexChanged
        UpdateJpegQualityEnabled()
    End Sub

    Private Sub UpdateStatus(Optional message As String = Nothing)
        Dim baseMsg As String
        If String.IsNullOrEmpty(message) Then
            baseMsg = "就绪"
        Else
            baseMsg = message
        End If

        Dim imageCount As Integer = _imageInfos.Count
        Dim blockCount As Integer = 0

        If _baseImage IsNot Nothing Then
            Dim blueYs = _lines.
                Where(Function(l) l.Type = LineType.Blue).
                Select(Function(l) l.Y).
                ToList()

            If Not blueYs.Contains(0) Then
                blueYs.Add(0)
            End If
            If Not blueYs.Contains(_baseImage.Height) Then
                blueYs.Add(_baseImage.Height)
            End If

            blueYs = blueYs.Distinct().OrderBy(Function(y) y).ToList()
            For i = 0 To blueYs.Count - 2
                If blueYs(i + 1) > blueYs(i) Then
                    blockCount += 1
                End If
            Next
        End If

        Dim zoomPercent As Integer = CInt(Math.Round(_zoom * 100))

        If lblStatus IsNot Nothing Then
            lblStatus.Text = $"{baseMsg}  | 图片:{imageCount}  块数:{blockCount}  已删:{_deletedRegionCount}  缩放:{zoomPercent}%"
        End If
    End Sub

    Private Function ExtractNumberFromName(fileName As String) As Integer
        Dim nameOnly = Path.GetFileNameWithoutExtension(fileName)
        Dim m = Regex.Match(nameOnly, "\d+")
        If m.Success Then
            Dim v As Integer
            If Integer.TryParse(m.Value, v) Then
                Return v
            End If
        End If
        Return 0
    End Function

    ' 确保最顶部有一条蓝线（Y = 0）
    Private Sub EnsureTopBlueLine()
        If _baseImage Is Nothing Then Return
        If Not _lines.Exists(Function(l) l.Type = LineType.Blue AndAlso l.Y = 0) Then
            _lines.Add(New SplitLine() With {.Y = 0, .Type = LineType.Blue})
        End If
    End Sub

    '==================== INI 读写 ====================

    Private Sub LoadIniOptions()
        If Not File.Exists(_iniPath) Then
            Return
        End If

        Try
            Dim lines = File.ReadAllLines(_iniPath, Encoding.UTF8)
            For Each raw In lines
                Dim line = raw.Trim()
                If line.Length = 0 OrElse line.StartsWith("#") OrElse line.StartsWith("[") Then
                    Continue For
                End If
                Dim idx = line.IndexOf("="c)
                If idx <= 0 Then Continue For
                Dim key = line.Substring(0, idx).Trim()
                Dim value = line.Substring(idx + 1).Trim()

                Select Case key.ToLower()
                    Case "trimwhite"
                        Dim b As Boolean
                        If Boolean.TryParse(value, b) Then
                            chkTrimWhite.Checked = b
                        End If
                    Case "padleft"
                        Dim n As Integer
                        If Integer.TryParse(value, n) Then
                            If n >= nudPadLeft.Minimum AndAlso n <= nudPadLeft.Maximum Then
                                nudPadLeft.Value = n
                            End If
                        End If
                    Case "padright"
                        Dim n As Integer
                        If Integer.TryParse(value, n) Then
                            If n >= nudPadRight.Minimum AndAlso n <= nudPadRight.Maximum Then
                                nudPadRight.Value = n
                            End If
                        End If
                    Case "padtop"
                        Dim n As Integer
                        If Integer.TryParse(value, n) Then
                            If n >= nudPadTop.Minimum AndAlso n <= nudPadTop.Maximum Then
                                nudPadTop.Value = n
                            End If
                        End If
                    Case "padbottom"
                        Dim n As Integer
                        If Integer.TryParse(value, n) Then
                            If n >= nudPadBottom.Minimum AndAlso n <= nudPadBottom.Maximum Then
                                nudPadBottom.Value = n
                            End If
                        End If
                    Case "globalwidth"
                        Dim b As Boolean
                        If Boolean.TryParse(value, b) Then
                            chkGlobalWidth.Checked = b
                        End If
                    Case "exportformat"
                        Dim fmt = value.ToUpper()
                        If fmt = "PNG" OrElse fmt = "JPG" Then
                            cboFormat.SelectedItem = fmt
                        End If
                    Case "jpegquality"
                        Dim n As Integer
                        If Integer.TryParse(value, n) Then
                            If n >= nudJpegQuality.Minimum AndAlso n <= nudJpegQuality.Maximum Then
                                nudJpegQuality.Value = n
                            End If
                        End If
                    Case "insertblankheight"
                        Dim n2 As Integer
                        If Integer.TryParse(value, n2) Then
                            If n2 >= nudInsertBlankHeight.Minimum AndAlso n2 <= nudInsertBlankHeight.Maximum Then
                                nudInsertBlankHeight.Value = n2
                            End If
                        End If
                End Select
            Next
        Catch ex As Exception
            ' 忽略配置读取错误，使用默认
        End Try
    End Sub

    Private Sub SaveIniOptions()
        Try
            Dim sb As New StringBuilder()
            sb.AppendLine("[Export]")
            sb.AppendLine("TrimWhite=" & chkTrimWhite.Checked.ToString())
            sb.AppendLine("PadLeft=" & CInt(nudPadLeft.Value).ToString())
            sb.AppendLine("PadRight=" & CInt(nudPadRight.Value).ToString())
            sb.AppendLine("PadTop=" & CInt(nudPadTop.Value).ToString())
            sb.AppendLine("PadBottom=" & CInt(nudPadBottom.Value).ToString())
            sb.AppendLine("GlobalWidth=" & chkGlobalWidth.Checked.ToString())
            Dim fmt As String = "PNG"
            If cboFormat.SelectedItem IsNot Nothing Then
                fmt = cboFormat.SelectedItem.ToString().ToUpper()
            End If
            sb.AppendLine("ExportFormat=" & fmt)
            sb.AppendLine("JpegQuality=" & CInt(nudJpegQuality.Value).ToString())
            sb.AppendLine("InsertBlankHeight=" & CInt(nudInsertBlankHeight.Value).ToString())

            File.WriteAllText(_iniPath, sb.ToString(), Encoding.UTF8)
        Catch ex As Exception
            ' 忽略保存错误
        End Try
    End Sub

    '==================== 打开文件 / 文件夹 ====================

    Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
        Using dialogFolder As New FolderBrowserDialog()
            If dialogFolder.ShowDialog() = DialogResult.OK Then
                Dim exts = {".jpg", ".jpeg", ".png", ".bmp", ".gif"}
                Dim fileList = Directory.GetFiles(dialogFolder.SelectedPath).
                    Where(Function(fullName) exts.Contains(Path.GetExtension(fullName).ToLower())).
                    OrderBy(Function(fullName) ExtractNumberFromName(fullName)).
                    ToList()

                If fileList.Count = 0 Then
                    MessageBox.Show("该文件夹中没有常见格式的图片。")
                    Return
                End If

                StartNewProject(fileList)
            End If
        End Using
    End Sub

    Private Sub btnOpenFiles_Click(sender As Object, e As EventArgs) Handles btnOpenFiles.Click
        Using dialogOpen As New OpenFileDialog()
            dialogOpen.Filter = "图片|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            dialogOpen.Multiselect = True
            If dialogOpen.ShowDialog() = DialogResult.OK Then
                Dim fileList = dialogOpen.FileNames.
                    OrderBy(Function(fullName) ExtractNumberFromName(fullName)).
                    ToList()
                If fileList.Count = 0 Then Return
                StartNewProject(fileList)
            End If
        End Using
    End Sub

    Private Sub StartNewProject(fileList As List(Of String))
        ' 新工程：清空状态
        _imageFiles = fileList
        _imageInfos.Clear()
        _lines.Clear()
        _history.Clear()
        _historyIndex = -1
        _previewDeleteActive = False
        _projectFilePath = Nothing
        _deletedRegionCount = 0

        LoadImages(fileList)
        EnsureTopBlueLine()
        RenderView()

        ' 为该工程创建一个新的工程文件路径（时间戳），在工程文件夹下
        Try
            If Not Directory.Exists(_projectFolder) Then
                Directory.CreateDirectory(_projectFolder)
            End If
        Catch
        End Try
        _projectFilePath = Path.Combine(_projectFolder,
            "Project_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xml")

        SaveProject()  ' 初始状态写一次
        UpdateStatus("新工程已创建。")
    End Sub

    '==================== 生成大图 ====================

    Private Sub LoadImages(fileList As List(Of String))
        ' 载入，并生成一张超长大图（以最宽为基准，右侧填白）
        Dim bitmapList As New List(Of Bitmap)
        Dim maxWidth As Integer = 0
        Dim totalHeight As Integer = 0

        For Each fullName In fileList
            Dim bmp As New Bitmap(fullName)
            bitmapList.Add(bmp)
            maxWidth = Math.Max(maxWidth, bmp.Width)
            totalHeight += bmp.Height
        Next

        Dim big As New Bitmap(maxWidth, totalHeight)
        Using g As Graphics = Graphics.FromImage(big)
            g.Clear(Color.White)
            Dim curY As Integer = 0
            For i = 0 To bitmapList.Count - 1
                Dim oneBmp As Bitmap = bitmapList(i)
                g.DrawImage(oneBmp, 0, curY, oneBmp.Width, oneBmp.Height)
                Dim info As New ImageInfo() With {
                    .FilePath = fileList(i),
                    .Width = oneBmp.Width,
                    .Height = oneBmp.Height,
                    .TopY = curY
                }
                _imageInfos.Add(info)
                curY += oneBmp.Height
            Next
        End Using

        ' 记录原始拼接高度（后续删除区域/插入空白不会改这个值）
        _originalHeight = big.Height

        For Each bmp In bitmapList
            bmp.Dispose()
        Next

        If _baseImage IsNot Nothing Then
            _baseImage.Dispose()
        End If
        _baseImage = big
        _zoom = 1.0F

        RenderView()
        UpdateStatus("已加载图片数量：" & _imageInfos.Count)
    End Sub

    '==================== 渲染视图 ====================

    Private Sub RenderView(Optional focusY As Integer = -1)
        If _baseImage Is Nothing Then
            picMain.Image = Nothing
            UpdateStatus("未加载图片。")
            Return
        End If

        EnsureTopBlueLine()

        Dim scaledW As Integer = CInt(_baseImage.Width * _zoom)
        Dim scaledH As Integer = CInt(_baseImage.Height * _zoom)
        Dim viewBmp As New Bitmap(scaledW, scaledH)

        Using g As Graphics = Graphics.FromImage(viewBmp)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.Clear(Color.White)
            g.DrawImage(_baseImage, New Rectangle(0, 0, scaledW, scaledH))

            ' 画蓝色分割线
            For Each ln In _lines.OrderBy(Function(l) l.Y)
                Dim yDisp As Integer = CInt(ln.Y * _zoom)
                Using penBlue As New Pen(Color.Blue, 1)
                    g.DrawLine(penBlue, 0, yDisp, scaledW, yDisp)
                End Using
            Next

            ' 若正在预览删除选中的蓝线，则用红色粗线标记这些蓝线
            If _previewBlueDeleteActive AndAlso _previewBlueLines IsNot Nothing AndAlso _previewBlueLines.Count > 0 Then
                Using penHighlight As New Pen(Color.Red, 3)
                    For Each yLine As Integer In _previewBlueLines
                        Dim yDisp As Integer = CInt(yLine * _zoom)
                        g.DrawLine(penHighlight, 0, yDisp, scaledW, yDisp)
                    Next
                End Using
            End If

            ' 若正在预览删除区域，则画红色斜线标记
            If _previewDeleteActive Then
                Dim topDisp As Integer = CInt(_previewDeleteTop * _zoom)
                Dim bottomDisp As Integer = CInt(_previewDeleteBottom * _zoom)
                If bottomDisp < topDisp Then
                    Dim tmp = topDisp
                    topDisp = bottomDisp
                    bottomDisp = tmp
                End If
                Dim h As Integer = bottomDisp - topDisp
                If h > 0 Then
                    Using penRed As New Pen(Color.Red, 1)
                        Dim stepSize As Integer = 20
                        Dim startX As Integer = -h
                        While startX < scaledW + h
                            g.DrawLine(penRed, startX, bottomDisp, startX + h, topDisp)
                            startX += stepSize
                        End While
                    End Using
                End If
            End If
        End Using

        Dim oldImage = picMain.Image
        picMain.Image = viewBmp
        picMain.Size = viewBmp.Size
        If oldImage IsNot Nothing Then oldImage.Dispose()

        If focusY >= 0 Then
            Dim yDisp = CInt(focusY * _zoom)
            ScrollToY(yDisp)
        End If

        UpdateStatus()
    End Sub

    Private Sub ScrollToY(yDisp As Integer)
        Dim target = yDisp - pnlScroll.ClientSize.Height \ 2
        If target < 0 Then target = 0
        pnlScroll.AutoScrollPosition = New Point(0, target)
    End Sub

    Private Function ViewYToBaseY(viewY As Integer) As Integer
        Return CInt(viewY / _zoom)
    End Function

    Private Function GetCurrentCenterBaseY() As Integer
        Dim offsetY As Integer = -pnlScroll.AutoScrollPosition.Y
        Dim centerViewY As Integer = offsetY + pnlScroll.ClientSize.Height \ 2
        Return ViewYToBaseY(centerViewY)
    End Function

    '==================== 蓝线导航 ====================

    Private Sub btnPrevLine_Click(sender As Object, e As EventArgs) Handles btnPrevLine.Click
        JumpToBlueLine(-1)
    End Sub

    Private Sub btnNextLine_Click(sender As Object, e As EventArgs) Handles btnNextLine.Click
        JumpToBlueLine(1)
    End Sub

    Private Sub JumpToBlueLine(direction As Integer)
        If _baseImage Is Nothing Then Return
        Dim blueLines = _lines.Where(Function(l) l.Type = LineType.Blue).Select(Function(l) l.Y).ToList()
        If blueLines.Count = 0 Then Return

        If Not blueLines.Contains(0) Then blueLines.Add(0)
        If Not blueLines.Contains(_baseImage.Height) Then blueLines.Add(_baseImage.Height)
        blueLines = blueLines.Distinct().OrderBy(Function(y) y).ToList()

        Dim centerY = GetCurrentCenterBaseY()
        Dim targetY As Integer = centerY

        If direction < 0 Then
            ' 上一条蓝线
            Dim candidates = blueLines.Where(Function(y) y < centerY).OrderByDescending(Function(y) y)
            If candidates.Any() Then
                targetY = candidates.First()
            Else
                targetY = blueLines.First()
            End If
        ElseIf direction > 0 Then
            ' 下一条蓝线
            Dim candidates = blueLines.Where(Function(y) y > centerY).OrderBy(Function(y) y)
            If candidates.Any() Then
                targetY = candidates.First()
            Else
                targetY = blueLines.Last()
            End If
        End If

        ScrollToY(CInt(targetY * _zoom))
        UpdateStatus($"已跳转到蓝线 Y={targetY}")
    End Sub

    Private Sub btnGotoTop_Click(sender As Object, e As EventArgs) Handles btnGotoTop.Click
        If _baseImage Is Nothing Then Return
        ScrollToY(0)
        UpdateStatus("已跳转到顶部。")
    End Sub

    Private Sub btnGotoBottom_Click(sender As Object, e As EventArgs) Handles btnGotoBottom.Click
        If _baseImage Is Nothing Then Return
        ScrollToY(CInt(_baseImage.Height * _zoom))
        UpdateStatus("已跳转到底部。")
    End Sub

    '==================== 鼠标添加 / 删除 ====================

    Private Sub picMain_MouseDown(sender As Object, e As MouseEventArgs) Handles picMain.MouseDown
        If _baseImage Is Nothing Then Return

        If e.Button = MouseButtons.Left Then
            Dim baseY As Integer = ViewYToBaseY(e.Y)
            If baseY < 0 OrElse baseY > _baseImage.Height Then Return
            ' 添加蓝色分割线
            Dim actionAdd As New AddLineAction(Me, baseY, LineType.Blue)
            PushAndDoAction(actionAdd)

        ElseIf e.Button = MouseButtons.Right Then
            ' 记录右键按下位置，用于判断是点击还是拖拽选区
            _rightButtonDown = True
            _rightDragStartViewY = e.Y
            _rightDragStartViewX = e.X

        ElseIf e.Button = MouseButtons.Middle Then
            ' 中键：在当前Y位置插入一段空白，高度由数值框指定
            Dim baseY As Integer = ViewYToBaseY(e.Y)
            If baseY < 0 OrElse baseY > _baseImage.Height Then Return
            Dim h As Integer = CInt(nudInsertBlankHeight.Value)
            If h <= 0 Then
                MessageBox.Show("插入空白高度需要大于0。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim actionInsert As New InsertBlankAction(Me, baseY, h)
            PushAndDoAction(actionInsert)
        End If
    End Sub

    Private Sub picMain_MouseMove(sender As Object, e As MouseEventArgs) Handles picMain.MouseMove
        ' 当前版本不做拖拽实时预览，只在 MouseUp 判断是否为拖拽选区
    End Sub

    Private Sub picMain_MouseUp(sender As Object, e As MouseEventArgs) Handles picMain.MouseUp
        If _baseImage Is Nothing Then Return

        If e.Button = MouseButtons.Right AndAlso _rightButtonDown Then
            _rightButtonDown = False
            Dim dx As Integer = Math.Abs(e.X - _rightDragStartViewX)
            Dim dy As Integer = Math.Abs(e.Y - _rightDragStartViewY)
            Dim threshold As Integer = 4

            If dx <= threshold AndAlso dy <= threshold Then
                ' 视为右键点击：按旧逻辑删除一段区域
                Dim baseY As Integer = ViewYToBaseY(e.Y)
                HandleRightClickDeleteRegion(baseY)
            Else
                ' 视为右键拖拽选区：删除选区内的蓝线（先高亮预览）
                Dim baseY1 As Integer = ViewYToBaseY(_rightDragStartViewY)
                Dim baseY2 As Integer = ViewYToBaseY(e.Y)
                Dim yTop As Integer = Math.Min(baseY1, baseY2)
                Dim yBottom As Integer = Math.Max(baseY1, baseY2)
                If yBottom <= yTop Then Return

                ' 找到选区内的蓝线（排除顶部Y=0）
                Dim targetLines As List(Of Integer) = _lines.
                Where(Function(l) l.Type = LineType.Blue AndAlso l.Y > yTop AndAlso l.Y < yBottom AndAlso l.Y <> 0).
                Select(Function(l) l.Y).
                Distinct().
                OrderBy(Function(y) y).
                ToList()

                If targetLines.Count = 0 Then
                    ' 没有蓝线，无需任何提示
                    RenderView()
                    Return
                End If

                ' 预览：用红色粗线包裹这些蓝线
                _previewBlueLines = targetLines
                _previewBlueDeleteActive = True
                RenderView()

                Dim msg As String = $"是否删除 Y={yTop} 到 Y={yBottom} 之间的 {targetLines.Count} 条蓝色分割线？" &
                Environment.NewLine & "(不会修改底图，仅移除这些蓝线标记。)"
                Dim res = MessageBox.Show(msg, "删除蓝线", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                ' 关闭预览
                _previewBlueDeleteActive = False
                _previewBlueLines.Clear()

                If res = DialogResult.No Then
                    ' 取消删除，恢复普通蓝线显示
                    RenderView()
                    Return
                End If

                ' 真正删除这些蓝线（可撤销 / 重做）
                Dim actionDelLines As New DeleteLinesInRangeAction(Me, yTop, yBottom)
                PushAndDoAction(actionDelLines)
            End If
        End If
    End Sub

    Private Sub HandleRightClickDeleteRegion(baseY As Integer)
        If _baseImage Is Nothing Then Return
        If baseY < 0 OrElse baseY > _baseImage.Height Then Return

        ' 以离该点最近的上方蓝线为起点，预览删除区域，再确认
        Dim aboveBlue = _lines.
        Where(Function(l) l.Type = LineType.Blue AndAlso l.Y < baseY).
        OrderByDescending(Function(l) l.Y).
        FirstOrDefault()

        If aboveBlue Is Nothing Then
            MessageBox.Show("上方没有蓝色分割线，无法确定删除区域。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim yTop As Integer = aboveBlue.Y
        Dim yBottom As Integer = baseY
        If yBottom <= yTop Then
            MessageBox.Show("红线位置必须在蓝线下方。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 打开预览
        _previewDeleteTop = yTop
        _previewDeleteBottom = yBottom
        _previewDeleteActive = True
        RenderView()

        Dim msg = $"是否删除 Y={yTop} 到 Y={yBottom} 之间的区域？" &
              Environment.NewLine & "(是: 删除此区域; 否: 取消预览)"
        Dim res = MessageBox.Show(msg, "删除区域", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        ' 关闭预览（不管选什么）
        _previewDeleteActive = False

        If res = DialogResult.No Then
            ' 直接刷新，恢复原图
            RenderView()
            Return
        End If

        ' 真正执行删除动作（记录到历史，可撤销 / 重做）
        Dim actionDelete As New DeleteRegionAction(Me, yTop, yBottom)
        PushAndDoAction(actionDelete)
    End Sub

    '==================== 撤销 / 重做 ====================

    Private Sub PushAndDoAction(actionItem As IEditAction)
        ' 丢弃当前索引之后的历史（清空重做栈）
        If _historyIndex < _history.Count - 1 Then
            _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1)
        End If

        _history.Add(actionItem)

        ' 保留最近 7 步
        'If _history.Count > 7 Then
        '_history.RemoveAt(0)
        'If _historyIndex > -1 Then
        '_historyIndex -= 1
        'End If
        'End If

        _historyIndex = _history.Count - 1

        actionItem.DoAction()
        If TypeOf actionItem Is DeleteRegionAction Then
            _deletedRegionCount += 1
            If _deletedRegionCount < 0 Then _deletedRegionCount = 0
        End If
        RenderView(actionItem.FocusY)

        ' 每一步操作都自动保存工程
        SaveProject()
    End Sub

    Private Sub btnUndo_Click(sender As Object, e As EventArgs) Handles btnUndo.Click
        If _historyIndex < 0 OrElse _historyIndex >= _history.Count Then Return
        Dim actionItem = _history(_historyIndex)
        actionItem.UndoAction()
        If TypeOf actionItem Is DeleteRegionAction Then
            _deletedRegionCount -= 1
            If _deletedRegionCount < 0 Then _deletedRegionCount = 0
        End If
        Dim focus = actionItem.FocusY
        _historyIndex -= 1
        RenderView(focus)
        SaveProject()
    End Sub

    Private Sub btnRedo_Click(sender As Object, e As EventArgs) Handles btnRedo.Click
        If _historyIndex >= _history.Count - 1 Then Return
        _historyIndex += 1
        Dim actionItem = _history(_historyIndex)
        actionItem.DoAction()
        If TypeOf actionItem Is DeleteRegionAction Then
            _deletedRegionCount += 1
            If _deletedRegionCount < 0 Then _deletedRegionCount = 0
        End If
        RenderView(actionItem.FocusY)
        SaveProject()
    End Sub

    '==================== 缩放 ====================

    Private Sub btnZoomIn_Click(sender As Object, e As EventArgs) Handles btnZoomIn.Click
        If _baseImage Is Nothing Then Return
        _zoom *= 1.25F
        If _zoom > 5.0F Then _zoom = 5.0F
        RenderView()
    End Sub

    Private Sub btnZoomOut_Click(sender As Object, e As EventArgs) Handles btnZoomOut.Click
        If _baseImage Is Nothing Then Return
        _zoom /= 1.25F
        If _zoom < 0.1F Then _zoom = 0.1F
        RenderView()
    End Sub

    '==================== 导出分割区域 ====================

    Private Class BlockExportInfo
        Public Property Index As Integer
        Public Property YTop As Integer
        Public Property YBottom As Integer
        Public Property Width As Integer
        Public Property IsAllWhite As Boolean
    End Class

    Private Function IsAllWhiteRegion(src As Bitmap, rect As Rectangle) As Boolean
        Dim maxX As Integer = rect.Right - 1
        Dim maxY As Integer = rect.Bottom - 1
        For y As Integer = rect.Top To maxY
            For x As Integer = rect.Left To maxX
                Dim c As Color = src.GetPixel(x, y)
                If Not (c.R = 255 AndAlso c.G = 255 AndAlso c.B = 255) Then
                    Return False
                End If
            Next
        Next
        Return True
    End Function

    Private Function ProcessRegionForExport(regionBmp As Bitmap, isAllWhite As Boolean) As Bitmap
        If regionBmp Is Nothing Then
            Return Nothing
        End If

        ' 一定要先 Clone 一份，避免后面 Using 把原图 Dispose 掉
        Dim result As Bitmap = CType(regionBmp.Clone(
                                 New Rectangle(0, 0, regionBmp.Width, regionBmp.Height),
                                 regionBmp.PixelFormat), Bitmap)

        ' 1. Trim 四边纯白（如果开启，且不是全白块）
        If chkTrimWhite.Checked AndAlso Not isAllWhite Then
            Dim w As Integer = result.Width
            Dim h As Integer = result.Height
            Dim minX As Integer = w
            Dim maxX As Integer = -1
            Dim minY As Integer = h
            Dim maxY As Integer = -1

            For y As Integer = 0 To h - 1
                For x As Integer = 0 To w - 1
                    Dim c As Color = result.GetPixel(x, y)
                    If Not (c.R = 255 AndAlso c.G = 255 AndAlso c.B = 255) Then
                        If x < minX Then minX = x
                        If x > maxX Then maxX = x
                        If y < minY Then minY = y
                        If y > maxY Then maxY = y
                    End If
                Next
            Next

            If maxX >= minX AndAlso maxY >= minY Then
                Dim newW As Integer = maxX - minX + 1
                Dim newH As Integer = maxY - minY + 1
                Dim trimmed As New Bitmap(newW, newH)
                Using g As Graphics = Graphics.FromImage(trimmed)
                    g.Clear(Color.White)
                    g.DrawImage(result,
                            New Rectangle(0, 0, newW, newH),
                            New Rectangle(minX, minY, newW, newH),
                            GraphicsUnit.Pixel)
                End Using
                result.Dispose()
                result = trimmed
            End If
        End If

        ' 2. 四边增加空白
        Dim padLeft As Integer = CInt(nudPadLeft.Value)
        Dim padRight As Integer = CInt(nudPadRight.Value)
        Dim padTop As Integer = CInt(nudPadTop.Value)
        Dim padBottom As Integer = CInt(nudPadBottom.Value)

        If padLeft > 0 OrElse padRight > 0 OrElse padTop > 0 OrElse padBottom > 0 Then
            Dim newW As Integer = result.Width + padLeft + padRight
            Dim newH As Integer = result.Height + padTop + padBottom
            If newW <= 0 OrElse newH <= 0 Then
                result.Dispose()
                Return Nothing
            End If

            Dim padded As New Bitmap(newW, newH)
            Using g As Graphics = Graphics.FromImage(padded)
                g.Clear(Color.White)
                g.DrawImage(result,
                        New Rectangle(padLeft, padTop, result.Width, result.Height),
                        New Rectangle(0, 0, result.Width, result.Height),
                        GraphicsUnit.Pixel)
            End Using
            result.Dispose()
            result = padded
        End If

        Return result
    End Function


    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If _baseImage Is Nothing Then
            MessageBox.Show("未加载图片。")
            Return
        End If

        ' 收集蓝线位置
        Dim blueLines As List(Of Integer) = _lines.
        Where(Function(l) l.Type = LineType.Blue).
        Select(Function(l) l.Y).
        ToList()

        If Not blueLines.Contains(0) Then
            blueLines.Add(0)
        End If
        If Not blueLines.Contains(_baseImage.Height) Then
            blueLines.Add(_baseImage.Height)
        End If

        blueLines = blueLines.Distinct().OrderBy(Function(y) y).ToList()

        If blueLines.Count < 2 Then
            MessageBox.Show("没有足够的蓝色分割线来导出。")
            Return
        End If

        ' 选择导出目录
        Dim exportDir As String = Nothing
        Using dialogFolder As New FolderBrowserDialog()
            dialogFolder.Description = "选择导出分割图片的文件夹"
            If dialogFolder.ShowDialog() <> DialogResult.OK Then
                Return
            End If
            exportDir = dialogFolder.SelectedPath
        End Using

        ' 导出格式 / 扩展名
        Dim fmtStr As String = "PNG"
        If cboFormat.SelectedItem IsNot Nothing Then
            fmtStr = cboFormat.SelectedItem.ToString().ToUpperInvariant()
        End If
        Dim ext As String = If(fmtStr = "JPG", ".jpg", ".png")

        ' 计算全局最大宽度
        Dim globalMaxWidth As Integer
        If _imageInfos IsNot Nothing AndAlso _imageInfos.Count > 0 Then
            globalMaxWidth = _imageInfos.Max(Function(im) im.Width)
        Else
            globalMaxWidth = _baseImage.Width
        End If

        ' 组装分块列表，并检测纯白块
        Dim blocks As New List(Of BlockExportInfo)()
        Dim index As Integer = 1
        Dim whiteCount As Integer = 0

        For i As Integer = 0 To blueLines.Count - 2
            Dim yTop As Integer = blueLines(i)
            Dim yBottom As Integer = blueLines(i + 1)
            If yBottom <= yTop Then
                Continue For
            End If

            Dim regionMaxWidth As Integer = globalMaxWidth
            If Not chkGlobalWidth.Checked Then
                Dim rangeInfos = _imageInfos.
                Where(Function(im) im.BottomY > yTop AndAlso im.TopY < yBottom)
                If rangeInfos.Any() Then
                    regionMaxWidth = rangeInfos.Max(Function(im) im.Width)
                End If
            End If

            Dim regionHeight As Integer = yBottom - yTop
            If regionHeight <= 0 OrElse regionMaxWidth <= 0 Then
                Continue For
            End If

            Dim rectWidth As Integer = regionMaxWidth
            If rectWidth > _baseImage.Width Then
                rectWidth = _baseImage.Width
            End If

            Dim rect As New Rectangle(0, yTop, rectWidth, regionHeight)
            If rect.Bottom > _baseImage.Height Then
                rect.Height = _baseImage.Height - yTop
            End If
            If rect.Width <= 0 OrElse rect.Height <= 0 Then
                Continue For
            End If

            Dim isWhite As Boolean = IsAllWhiteRegion(_baseImage, rect)
            If isWhite Then
                whiteCount += 1
            End If

            Dim info As New BlockExportInfo() With {
            .Index = index,
            .YTop = yTop,
            .YBottom = yBottom,
            .Width = rectWidth,
            .IsAllWhite = isWhite
        }
            blocks.Add(info)
            index += 1
        Next

        If blocks.Count = 0 Then
            MessageBox.Show("没有可导出的分块。")
            Return
        End If

        ' 是否导出纯白块
        Dim exportWhiteBlocks As Boolean = True
        If whiteCount > 0 Then
            Dim msgWhite As String =
            "发现 " & whiteCount.ToString() & " 个完全纯白分块，是否一起导出？" & Environment.NewLine &
            "是：包括纯白分块" & Environment.NewLine &
            "否：跳过纯白分块"
            Dim resWhite = MessageBox.Show(msgWhite, "纯白分块提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If resWhite = DialogResult.No Then
                exportWhiteBlocks = False
            End If
        End If

        ' 生成目标文件名列表，用于覆盖检查
        Dim targetFiles As New List(Of String)()
        For Each block As BlockExportInfo In blocks
            If block.IsAllWhite AndAlso Not exportWhiteBlocks Then
                Continue For
            End If
            Dim fileName As String = System.IO.Path.Combine(
            exportDir,
            String.Format("part_{0:000}{1}", block.Index, ext))
            targetFiles.Add(fileName)
        Next

        If targetFiles.Count = 0 Then
            MessageBox.Show("全部分块都是纯白，且已选择跳过纯白分块，未导出任何文件。")
            UpdateStatus("未导出任何文件（全部为纯白且被跳过）。")
            Return
        End If

        ' 覆盖确认
        Dim anyExist As Boolean = targetFiles.Any(Function(path) System.IO.File.Exists(path))
        If anyExist Then
            Dim resOverwrite = MessageBox.Show(
            "目标文件夹中已存在部分 part_XXX 文件，是否覆盖？",
            "覆盖确认",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning)
            If resOverwrite = DialogResult.No Then
                UpdateStatus("已取消导出。")
                Return
            End If
        End If

        ' 真正导出
        For Each block As BlockExportInfo In blocks
            If block.IsAllWhite AndAlso Not exportWhiteBlocks Then
                Continue For
            End If

            Dim yTop As Integer = block.YTop
            Dim yBottom As Integer = block.YBottom
            If yTop < 0 Then yTop = 0
            If yBottom > _baseImage.Height Then yBottom = _baseImage.Height

            Dim regionHeight As Integer = yBottom - yTop
            Dim regionWidth As Integer = block.Width
            If regionHeight <= 0 OrElse regionWidth <= 0 Then
                Continue For
            End If
            If regionWidth > _baseImage.Width Then
                regionWidth = _baseImage.Width
            End If

            Dim bmpExport As Bitmap = Nothing

            Using bmpRegion As New Bitmap(regionWidth, regionHeight)
                Using g As Graphics = Graphics.FromImage(bmpRegion)
                    g.Clear(Color.White)
                    g.DrawImage(_baseImage,
                            New Rectangle(0, 0, regionWidth, regionHeight),
                            New Rectangle(0, yTop, regionWidth, regionHeight),
                            GraphicsUnit.Pixel)
                End Using

                bmpExport = ProcessRegionForExport(bmpRegion, block.IsAllWhite)
            End Using

            If bmpExport Is Nothing Then
                Continue For
            End If
            If bmpExport.Width <= 0 OrElse bmpExport.Height <= 0 Then
                bmpExport.Dispose()
                Continue For
            End If

            Dim fileName As String = System.IO.Path.Combine(
            exportDir,
            String.Format("part_{0:000}{1}", block.Index, ext))

            Try
                If fmtStr = "PNG" Then
                    bmpExport.Save(fileName, ImageFormat.Png)
                Else
                    ' JPG 带质量
                    Dim codecs = ImageCodecInfo.GetImageEncoders()
                    Dim jpegCodec = codecs.FirstOrDefault(Function(c) c.MimeType = "image/jpeg")
                    If jpegCodec IsNot Nothing Then
                        Dim quality As Long = CLng(nudJpegQuality.Value)
                        Dim encParams As New System.Drawing.Imaging.EncoderParameters(1)
                        encParams.Param(0) = New System.Drawing.Imaging.EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality,
                        quality)
                        bmpExport.Save(fileName, jpegCodec, encParams)
                    Else
                        bmpExport.Save(fileName, ImageFormat.Jpeg)
                    End If
                End If
            Catch ex As Exception
                UpdateStatus("导出分块失败: " & fileName & " - " & ex.Message)
            Finally
                bmpExport.Dispose()
            End Try
        Next

        MessageBox.Show("导出完成。")
        UpdateStatus("导出完成。")
    End Sub

    '==================== 工程保存 / 读取 ====================

    Private Sub SaveProject()
        If _baseImage Is Nothing OrElse _imageFiles Is Nothing OrElse _imageFiles.Count = 0 Then Return
        If String.IsNullOrEmpty(_projectFilePath) Then
            Try
                If Not Directory.Exists(_projectFolder) Then
                    Directory.CreateDirectory(_projectFolder)
                End If
            Catch
            End Try
            _projectFilePath = Path.Combine(_projectFolder,
                "Project_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xml")
        End If

        Try
            Dim proj As New ProjectData()
            proj.ImageFiles = _imageFiles.ToList()
            proj.Actions = New List(Of ProjectAction)()
            proj.TotalHeight = _baseImage.Height
            proj.OriginalHeight = _originalHeight

            ' 导出相关选项
            proj.TrimWhite = chkTrimWhite.Checked
            proj.PadLeft = CInt(nudPadLeft.Value)
            proj.PadRight = CInt(nudPadRight.Value)
            proj.PadTop = CInt(nudPadTop.Value)
            proj.PadBottom = CInt(nudPadBottom.Value)
            proj.GlobalWidth = chkGlobalWidth.Checked
            Dim fmt As String = If(cboFormat.SelectedItem IsNot Nothing, cboFormat.SelectedItem.ToString().ToUpper(), "PNG")
            proj.ExportFormat = fmt
            proj.JpegQuality = CInt(nudJpegQuality.Value)
            proj.InsertBlankHeight = CInt(nudInsertBlankHeight.Value)

            ' 把历史中 0.._historyIndex 的动作序列转成 ProjectAction
            For i = 0 To _historyIndex
                Dim actionItem = _history(i)
                If TypeOf actionItem Is AddLineAction Then
                    Dim actAdd = DirectCast(actionItem, AddLineAction)
                    Dim pa As New ProjectAction() With {
                        .ActionType = "AddLine",
                        .LineType = "Blue",
                        .Y = actAdd.YField
                    }
                    proj.Actions.Add(pa)
                ElseIf TypeOf actionItem Is DeleteRegionAction Then
                    Dim actDel = DirectCast(actionItem, DeleteRegionAction)
                    Dim pa As New ProjectAction() With {
                        .ActionType = "DeleteRegion",
                        .YTop = actDel.YTopField,
                        .YBottom = actDel.YBottomField
                    }
                    proj.Actions.Add(pa)
                ElseIf TypeOf actionItem Is InsertBlankAction Then
                    Dim actIns = DirectCast(actionItem, InsertBlankAction)
                    Dim pa As New ProjectAction() With {
                        .ActionType = "InsertBlank",
                        .InsertY = actIns.InsertYField,
                        .InsertHeight = actIns.HeightField
                    }
                    proj.Actions.Add(pa)
                End If
            Next

            Dim serializer As New XmlSerializer(GetType(ProjectData))
            Using fs As New FileStream(_projectFilePath, FileMode.Create, FileAccess.Write)
                serializer.Serialize(fs, proj)
            End Using
        Catch ex As Exception
            Console.WriteLine("保存工程失败: " & ex.Message)
        End Try
    End Sub

    Private Sub btnLoadProject_Click(sender As Object, e As EventArgs) Handles btnLoadProject.Click
        Using dialogOpen As New OpenFileDialog()
            dialogOpen.Filter = "工程文件|*.xml"
            ' 打开工程文件时自动定位到工程文件夹
            If Directory.Exists(_projectFolder) Then
                dialogOpen.InitialDirectory = _projectFolder
            End If
            If dialogOpen.ShowDialog() <> DialogResult.OK Then Return

            Try
                Dim proj As ProjectData
                Dim serializer As New XmlSerializer(GetType(ProjectData))
                Using fs As New FileStream(dialogOpen.FileName, FileMode.Open, FileAccess.Read)
                    proj = CType(serializer.Deserialize(fs), ProjectData)
                End Using

                ' 记住当前工程文件路径，后续 SaveProject 一律保存回这个文件
                _projectFilePath = dialogOpen.FileName

                ' 更新工程文件夹记忆，但不影响图片文件打开
                Try
                    _projectFolder = Path.GetDirectoryName(dialogOpen.FileName)
                Catch
                End Try

                ' 检查文件存在性
                For Each fullName In proj.ImageFiles
                    If Not File.Exists(fullName) Then
                        Throw New Exception("图片文件缺失：" & fullName)
                    End If
                Next

                ' 按工程中的文件重新加载
                _imageFiles = proj.ImageFiles.ToList()
                _imageInfos.Clear()
                _lines.Clear()
                _history.Clear()
                _historyIndex = -1
                _previewDeleteActive = False
                _deletedRegionCount = 0

                LoadImages(_imageFiles)

                ' 对比原始高度：如果不一致，说明源图片尺寸发生了变化
                If proj.OriginalHeight > 0 AndAlso _originalHeight <> proj.OriginalHeight Then
                    Throw New Exception("工程记录的原始高度与当前图片拼接高度不一致，可能图片尺寸已修改。")
                End If

                ' 载入工程里的选项
                chkTrimWhite.Checked = proj.TrimWhite
                nudPadLeft.Value = Math.Max(nudPadLeft.Minimum, Math.Min(nudPadLeft.Maximum, proj.PadLeft))
                nudPadRight.Value = Math.Max(nudPadRight.Minimum, Math.Min(nudPadRight.Maximum, proj.PadRight))
                nudPadTop.Value = Math.Max(nudPadTop.Minimum, Math.Min(nudPadTop.Maximum, proj.PadTop))
                nudPadBottom.Value = Math.Max(nudPadBottom.Minimum, Math.Min(nudPadBottom.Maximum, proj.PadBottom))
                chkGlobalWidth.Checked = proj.GlobalWidth
                If Not String.IsNullOrEmpty(proj.ExportFormat) Then
                    Dim fmt = proj.ExportFormat.ToUpper()
                    If fmt = "PNG" OrElse fmt = "JPG" Then
                        cboFormat.SelectedItem = fmt
                    End If
                End If
                If proj.JpegQuality >= nudJpegQuality.Minimum AndAlso proj.JpegQuality <= nudJpegQuality.Maximum Then
                    nudJpegQuality.Value = proj.JpegQuality
                End If
                If proj.InsertBlankHeight >= nudInsertBlankHeight.Minimum AndAlso proj.InsertBlankHeight <= nudInsertBlankHeight.Maximum Then
                    nudInsertBlankHeight.Value = proj.InsertBlankHeight
                End If
                UpdateJpegQualityEnabled()

                ' 确保顶部蓝线
                EnsureTopBlueLine()

                ' 重放动作
                For Each pa In proj.Actions
                    If pa.ActionType = "AddLine" Then
                        Dim actionAdd As New AddLineAction(Me, pa.Y, LineType.Blue)
                        PushAndDoAction(actionAdd)
                    ElseIf pa.ActionType = "DeleteRegion" Then
                        Dim actionDel As New DeleteRegionAction(Me, pa.YTop, pa.YBottom)
                        PushAndDoAction(actionDel)
                    ElseIf pa.ActionType = "InsertBlank" Then
                        Dim actionIns As New InsertBlankAction(Me, pa.InsertY, pa.InsertHeight)
                        PushAndDoAction(actionIns)
                    End If
                Next

                '_projectFilePath = dialogOpen.FileName
                MessageBox.Show("工程加载完成。")
                UpdateStatus("工程加载完成。")
            Catch ex As Exception
                MessageBox.Show("加载工程失败：" & ex.Message & vbCrLf & "将按新工程处理。")
                UpdateStatus("加载工程失败。")
            End Try
        End Using
    End Sub

    '==================== 动作类 ====================

    Private Class AddLineAction
        Implements IEditAction

        Private ReadOnly _form As frmSplitter
        Private ReadOnly _line As SplitLine

        Public Sub New(frm As frmSplitter, y As Integer, tp As LineType)
            _form = frm
            _line = New SplitLine() With {.Y = y, .Type = tp}
        End Sub

        Public ReadOnly Property FocusY As Integer Implements IEditAction.FocusY
            Get
                Return _line.Y
            End Get
        End Property

        Public ReadOnly Property YField As Integer
            Get
                Return _line.Y
            End Get
        End Property

        Public Sub DoAction() Implements IEditAction.DoAction
            _form._lines.Add(_line)
        End Sub

        Public Sub UndoAction() Implements IEditAction.UndoAction
            _form._lines.Remove(_line)
        End Sub
    End Class

    Private Class InsertBlankAction
        Implements IEditAction

        Private ReadOnly _form As frmSplitter
        Private ReadOnly _insertY As Integer
        Private ReadOnly _height As Integer

        Public Sub New(frm As frmSplitter, insertY As Integer, height As Integer)
            _form = frm
            _insertY = insertY
            _height = height
        End Sub

        Public ReadOnly Property FocusY As Integer Implements IEditAction.FocusY
            Get
                Return _insertY
            End Get
        End Property

        Public ReadOnly Property InsertYField As Integer
            Get
                Return _insertY
            End Get
        End Property

        Public ReadOnly Property HeightField As Integer
            Get
                Return _height
            End Get
        End Property


        Public Sub DoAction() Implements IEditAction.DoAction
            If _form._baseImage Is Nothing Then Return
            Dim oldBmp = _form._baseImage
            Dim width As Integer = oldBmp.Width
            Dim newHeight As Integer = oldBmp.Height + _height
            Dim newBmp As New Bitmap(width, newHeight)
            Using g As Graphics = Graphics.FromImage(newBmp)
                ' 上半部分
                If _insertY > 0 Then
                    g.DrawImage(oldBmp,
                            New Rectangle(0, 0, width, _insertY),
                            New Rectangle(0, 0, width, _insertY),
                            GraphicsUnit.Pixel)
                End If
                ' 插入空白
                g.FillRectangle(Brushes.White, 0, _insertY, width, _height)
                ' 下半部分
                Dim bottomHeight As Integer = oldBmp.Height - _insertY
                If bottomHeight > 0 Then
                    g.DrawImage(oldBmp,
                            New Rectangle(0, _insertY + _height, width, bottomHeight),
                            New Rectangle(0, _insertY, width, bottomHeight),
                            GraphicsUnit.Pixel)
                End If
            End Using
            _form._baseImage = newBmp
            oldBmp.Dispose()

            ' 更新分割线：插入位置及以下整体下移
            For Each lineItem In _form._lines
                If lineItem.Y >= _insertY Then
                    lineItem.Y += _height
                End If
            Next
            _form.EnsureTopBlueLine()
        End Sub

        Public Sub UndoAction() Implements IEditAction.UndoAction
            If _form._baseImage Is Nothing Then Return
            Dim curBmp = _form._baseImage
            Dim width As Integer = curBmp.Width
            Dim newHeight As Integer = curBmp.Height - _height
            If newHeight <= 0 Then Return
            Dim restored As New Bitmap(width, newHeight)
            Using g As Graphics = Graphics.FromImage(restored)
                ' 上半部分
                If _insertY > 0 Then
                    g.DrawImage(curBmp,
                            New Rectangle(0, 0, width, _insertY),
                            New Rectangle(0, 0, width, _insertY),
                            GraphicsUnit.Pixel)
                End If
                ' 下半部分：去掉插入空白区域
                Dim bottomHeight As Integer = newHeight - _insertY
                If bottomHeight > 0 Then
                    g.DrawImage(curBmp,
                            New Rectangle(0, _insertY, width, bottomHeight),
                            New Rectangle(0, _insertY + _height, width, bottomHeight),
                            GraphicsUnit.Pixel)
                End If
            End Using
            _form._baseImage = restored
            curBmp.Dispose()

            ' 更新分割线：插入位置及以下整体上移
            For Each lineItem In _form._lines
                If lineItem.Y >= _insertY Then
                    lineItem.Y -= _height
                End If
            Next
            _form.EnsureTopBlueLine()
        End Sub
    End Class

    Private Class DeleteLinesInRangeAction
        Implements IEditAction

        Private ReadOnly _form As frmSplitter
        Private ReadOnly _yTop As Integer
        Private ReadOnly _yBottom As Integer
        Private _removedLines As List(Of SplitLine)

        Public Sub New(frm As frmSplitter, yTop As Integer, yBottom As Integer)
            _form = frm
            If yBottom < yTop Then
                Dim t As Integer = yTop
                yTop = yBottom
                yBottom = t
            End If
            _yTop = yTop
            _yBottom = yBottom
        End Sub

        Public ReadOnly Property FocusY As Integer Implements IEditAction.FocusY
            Get
                Return _yTop
            End Get
        End Property

        Public Sub DoAction() Implements IEditAction.DoAction
            ' 只删除指定范围内的蓝线，保留顶部Y=0的蓝线
            _removedLines = _form._lines.
            Where(Function(l) l.Type = LineType.Blue AndAlso l.Y > _yTop AndAlso l.Y < _yBottom AndAlso l.Y <> 0).
            ToList()
            For Each ln In _removedLines
                _form._lines.Remove(ln)
            Next
            _form.EnsureTopBlueLine()
        End Sub

        Public Sub UndoAction() Implements IEditAction.UndoAction
            If _removedLines Is Nothing Then Return
            For Each ln In _removedLines
                _form._lines.Add(ln)
            Next
            _form.EnsureTopBlueLine()
        End Sub
    End Class



    Private Class DeleteRegionAction
        Implements IEditAction

        Private ReadOnly _form As frmSplitter
        Private ReadOnly _yTop As Integer
        Private ReadOnly _yBottom As Integer

        Private _deletedChunk As Bitmap
        Private _deletedLines As List(Of SplitLine)
        Private _heightDel As Integer

        Public Sub New(frm As frmSplitter, yTop As Integer, yBottom As Integer)
            _form = frm
            If yBottom < yTop Then
                Dim t = yTop
                yTop = yBottom
                yBottom = t
            End If
            _yTop = yTop
            _yBottom = yBottom
        End Sub

        Public ReadOnly Property FocusY As Integer Implements IEditAction.FocusY
            Get
                Return _yTop
            End Get
        End Property

        Public ReadOnly Property YTopField As Integer
            Get
                Return _yTop
            End Get
        End Property

        Public ReadOnly Property YBottomField As Integer
            Get
                Return _yBottom
            End Get
        End Property

        Public Sub DoAction() Implements IEditAction.DoAction
            DeleteRegion()
        End Sub

        Private Sub DeleteRegion()
            Dim oldBmp = _form._baseImage
            Dim width = oldBmp.Width
            _heightDel = _yBottom - _yTop
            If _heightDel <= 0 Then Return
            Dim newHeight = oldBmp.Height - _heightDel

            ' 保存删除块
            _deletedChunk = New Bitmap(width, _heightDel)
            Using gDel As Graphics = Graphics.FromImage(_deletedChunk)
                gDel.DrawImage(oldBmp,
                               New Rectangle(0, 0, width, _heightDel),
                               New Rectangle(0, _yTop, width, _heightDel),
                               GraphicsUnit.Pixel)
            End Using

            ' 新大图
            Dim newBmp As New Bitmap(width, newHeight)
            Using g As Graphics = Graphics.FromImage(newBmp)
                ' 上半部分
                g.DrawImage(oldBmp,
                            New Rectangle(0, 0, width, _yTop),
                            New Rectangle(0, 0, width, _yTop),
                            GraphicsUnit.Pixel)
                ' 下半部分上移
                Dim bottomHeight = oldBmp.Height - _yBottom
                g.DrawImage(oldBmp,
                            New Rectangle(0, _yTop, width, bottomHeight),
                            New Rectangle(0, _yBottom, width, bottomHeight),
                            GraphicsUnit.Pixel)
            End Using

            _form._baseImage = newBmp
            oldBmp.Dispose()

            ' 处理分割线：删除区域内的线，并上移下方线
            _deletedLines = _form._lines.
                Where(Function(l) l.Y > _yTop AndAlso l.Y < _yBottom).
                ToList()

            For Each lineItem In _deletedLines
                _form._lines.Remove(lineItem)
            Next

            For Each lineItem In _form._lines
                If lineItem.Y >= _yBottom Then
                    lineItem.Y -= _heightDel
                End If
            Next
        End Sub

        Public Sub UndoAction() Implements IEditAction.UndoAction
            If _deletedChunk Is Nothing Then Return

            ' 恢复大图：把删除块插回去
            Dim curBmp = _form._baseImage
            Dim width = curBmp.Width
            Dim restoreHeight = curBmp.Height + _heightDel
            Dim restored As New Bitmap(width, restoreHeight)

            Using g As Graphics = Graphics.FromImage(restored)
                ' 上半部分
                g.DrawImage(curBmp,
                            New Rectangle(0, 0, width, _yTop),
                            New Rectangle(0, 0, width, _yTop),
                            GraphicsUnit.Pixel)
                ' 中间删除块
                g.DrawImage(_deletedChunk,
                            New Rectangle(0, _yTop, width, _heightDel),
                            New Rectangle(0, 0, width, _heightDel),
                            GraphicsUnit.Pixel)
                ' 下半部分，下移
                Dim bottomHeight = curBmp.Height - _yTop
                g.DrawImage(curBmp,
                            New Rectangle(0, _yTop + _heightDel, width, bottomHeight),
                            New Rectangle(0, _yTop, width, bottomHeight),
                            GraphicsUnit.Pixel)
            End Using

            _form._baseImage = restored
            curBmp.Dispose()

            ' 线：下方线整体下移
            For Each lineItem In _form._lines
                If lineItem.Y >= _yTop Then
                    lineItem.Y += _heightDel
                End If
            Next

            ' 把先前删除的线加回来
            For Each lineItem In _deletedLines
                _form._lines.Add(lineItem)
            Next
        End Sub
    End Class

    '==================== 快捷键 / 帮助 / 关于 ====================

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.O Then
            ' Ctrl+O 打开文件
            btnOpenFiles.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso e.Shift AndAlso e.KeyCode = Keys.O Then
            ' Ctrl+Shift+O 打开文件夹
            btnOpenFolder.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.L Then
            ' Ctrl+L 读取工程
            btnLoadProject.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.E Then
            ' Ctrl+E 导出
            btnExport.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.Z Then
            ' Ctrl+Z 撤销
            btnUndo.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.Y Then
            ' Ctrl+Y 重做
            btnRedo.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso (e.KeyCode = Keys.Add OrElse e.KeyCode = Keys.Oemplus) Then
            ' Ctrl+ + 放大
            btnZoomIn.PerformClick()
            e.Handled = True
        ElseIf e.Control AndAlso (e.KeyCode = Keys.Subtract OrElse e.KeyCode = Keys.OemMinus) Then
            ' Ctrl+ - 缩小
            btnZoomOut.PerformClick()
            e.Handled = True
        ElseIf e.Alt AndAlso e.KeyCode = Keys.Up Then
            ' Alt+↑ 上一蓝线
            btnPrevLine.PerformClick()
            e.Handled = True
        ElseIf e.Alt AndAlso e.KeyCode = Keys.Down Then
            ' Alt+↓ 下一蓝线
            btnNextLine.PerformClick()
            e.Handled = True
        ElseIf e.KeyCode = Keys.Home Then
            ' Home 顶部
            btnGotoTop.PerformClick()
            e.Handled = True
        ElseIf e.KeyCode = Keys.End Then
            ' End 底部
            btnGotoBottom.PerformClick()
            e.Handled = True
        ElseIf e.KeyCode = Keys.F1 Then
            ' F1 关于 / 帮助
            ShowAbout()
            e.Handled = True
        End If
    End Sub

    Private Sub btnAbout_Click(sender As Object, e As EventArgs) Handles btnAbout.Click
        ShowAbout()
    End Sub
    ' 建议放在 Form1 类内部，靠前位置：构造统一的简明帮助文本
    Private Function BuildHelpText() As String
        Dim s As String = ""

        s &= "简明帮助:" & Environment.NewLine
        s &= "1. 打开图片与工程:" & Environment.NewLine
        s &= "   - 打开文件夹: 按钮""打开文件夹"" 或 Ctrl+Shift+O" & Environment.NewLine
        s &= "   - 打开文件: 按钮""打开文件(&O)"" 或 Ctrl+O" & Environment.NewLine
        s &= "   - 读取工程: 按钮""读取工程(&L)"" 载入上次保存的工程及选项" & Environment.NewLine
        s &= Environment.NewLine

        s &= "2. 分割线操作:" & Environment.NewLine
        s &= "   - 默认顶部存在一条蓝线(Y=0)，始终保留" & Environment.NewLine
        s &= "   - 左键单击: 在点击位置添加一条蓝色分割线" & Environment.NewLine
        s &= "   - 右键单击: 以上方最近蓝线为起点，当前位置为终点，预览红色删除区域并确认删除" & Environment.NewLine
        s &= "   - 右键拖拽: 选中一段纵向区域，高亮(红色粗线包裹)选中区域内所有蓝线，确认后删除蓝线" & Environment.NewLine
        s &= "   - 中键单击: 在当前位置插入""插入空白高度""指定像素的纯白空白，可撤销" & Environment.NewLine
        s &= Environment.NewLine

        s &= "3. 撤销 / 重做:" & Environment.NewLine
        s &= "   - 撤销: 按钮""↶ 撤销"" 或 Ctrl+Z" & Environment.NewLine
        s &= "   - 重做: 按钮""↷ 重做"" 或 Ctrl+Y" & Environment.NewLine
        s &= "   - 支持添加 / 删除分割线、删除区域、插入空白等操作的多步撤销重做" & Environment.NewLine
        s &= Environment.NewLine

        s &= "4. 缩放与导航:" & Environment.NewLine
        s &= "   - 放大: 按钮""＋"" 或 Ctrl+ +" & Environment.NewLine
        s &= "   - 缩小: 按钮""－"" 或 Ctrl+ -" & Environment.NewLine
        s &= "   - 上一蓝线: 按钮""上一蓝线"" 或 Alt+↑" & Environment.NewLine
        s &= "   - 下一蓝线: 按钮""下一蓝线"" 或 Alt+↓" & Environment.NewLine
        s &= "   - 跳到顶部: 按钮""顶部(Home)"" 或 Home" & Environment.NewLine
        s &= "   - 跳到底部: 按钮""底部(End)"" 或 End" & Environment.NewLine
        s &= Environment.NewLine

        s &= "5. 导出设置:" & Environment.NewLine
        s &= "   - 全局统一宽度: 勾选""导出图片统一宽度"" 时所有分块按本批次最大宽度导出，宽度不足处以白色填充" & Environment.NewLine
        s &= "   - 未勾选时: 每个分块按其所在区域中实际最大宽度导出" & Environment.NewLine
        s &= "   - 去除纯白边缘: 勾选""去除纯白边缘"" 时导出前自动 Trim 掉四边纯白边框" & Environment.NewLine
        s &= "   - 四边空白: ""左/右/上/下"" 四个数值框以像素为单位为导出分块增加额外白边" & Environment.NewLine
        s &= "   - 插入空白高度: 数值框""插入空白高度"" 配合中键在视图位置插入一段纯白空白" & Environment.NewLine
        s &= "   - 导出格式: 下拉框""格式"" 可选择 PNG 或 JPG" & Environment.NewLine
        s &= "   - JPG质量: 选择 JPG 时可通过""JPG质量"" 设置导出图像质量(10-100)" & Environment.NewLine
        s &= Environment.NewLine

        s &= "6. 纯白分块检测:" & Environment.NewLine
        s &= "   - 导出前自动统计每个分块是否为完全纯白" & Environment.NewLine
        s &= "   - 如发现纯白分块，会提示: 是否导出这些纯白分块" & Environment.NewLine
        s &= "   - 选择不导出时，这些纯白分块不会生成文件" & Environment.NewLine
        s &= Environment.NewLine

        s &= "7. 工程与配置:" & Environment.NewLine
        s &= "   - 工程文件: 自动保存蓝线位置、删除区域、插入空白、导出选项等" & Environment.NewLine
        s &= "   - 读取工程: 若图片文件及高度匹配，则会自动恢复分割线和已删除区域" & Environment.NewLine
        s &= "   - INI 配置: 程序同目录生成 .ini 保存全局选项，下次启动自动加载" & Environment.NewLine
        s &= Environment.NewLine

        s &= "8. 其他说明:" & Environment.NewLine
        s &= "   - 删除区域和插入空白会同步更新下方所有分割线的坐标，并支持撤销重做" & Environment.NewLine
        s &= "   - 缩放操作不会改变真实分割坐标，只影响显示比例" & Environment.NewLine
        s &= "   - 状态栏会显示当前缩放比例、有效分块数量、已删除区域计数等信息" & Environment.NewLine

        Return s
    End Function

    Private Sub ShowAbout()
        Dim aboutForm As New Form()
        aboutForm.Text = "关于 - 图片分割标记工具"
        aboutForm.StartPosition = FormStartPosition.CenterParent
        aboutForm.FormBorderStyle = FormBorderStyle.FixedDialog
        aboutForm.MaximizeBox = False
        aboutForm.MinimizeBox = False
        aboutForm.ClientSize = New Size(520, 420)

        Dim lblTitle As New Label()
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Microsoft YaHei UI", 11.0F, FontStyle.Bold)
        lblTitle.Location = New Point(16, 16)
        lblTitle.Text = "图片分割标记工具"

        Dim lblInfo As New Label()
        lblInfo.AutoSize = True
        lblInfo.Location = New Point(18, 48)
        lblInfo.Text = "作者: aCr/FTSTT   (使用 AI 协助编程开发)"

        Dim link As New LinkLabel()
        link.AutoSize = True
        link.Location = New Point(18, 72)
        link.Text = "官网: https://ftstt.github.io/vbnet_image_splitter/"
        AddHandler link.LinkClicked,
            Sub(sender2 As Object, e2 As LinkLabelLinkClickedEventArgs)
                Try
                    Dim psi As New ProcessStartInfo() With {
                        .FileName = "https://ftstt.github.io/vbnet_image_splitter/",
                        .UseShellExecute = True
                    }
                    Process.Start(psi)
                Catch ex As Exception
                    MessageBox.Show("无法打开官网：" & ex.Message)
                End Try
            End Sub

        Dim txtHelp As New TextBox()
        txtHelp.Multiline = True
        txtHelp.ReadOnly = True
        txtHelp.ScrollBars = ScrollBars.Vertical
        txtHelp.BorderStyle = BorderStyle.FixedSingle
        txtHelp.Location = New Point(20, 104)
        txtHelp.Size = New Size(480, 260)
        txtHelp.Font = New Font("Microsoft YaHei UI", 9.0F, FontStyle.Regular)
        txtHelp.Text = BuildHelpText()
        Dim btnOk As New Button()
        btnOk.Text = "关闭"
        btnOk.Size = New Size(80, 26)
        btnOk.Location = New Point(420, 374)
        AddHandler btnOk.Click,
            Sub(sender2 As Object, e2 As EventArgs)
                aboutForm.Close()
            End Sub

        aboutForm.Controls.Add(lblTitle)
        aboutForm.Controls.Add(lblInfo)
        aboutForm.Controls.Add(link)
        aboutForm.Controls.Add(txtHelp)
        aboutForm.Controls.Add(btnOk)

        aboutForm.ShowDialog(Me)
    End Sub

End Class
