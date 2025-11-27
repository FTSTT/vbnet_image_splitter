Imports System
Imports System.Collections.Generic

<Serializable()>
Public Class ProjectData
    Public Property ImageFiles As List(Of String)
    Public Property Actions As List(Of ProjectAction)
    Public Property TotalHeight As Integer          ' 当前编辑后的高度
    Public Property OriginalHeight As Integer       ' 原始拼接高度

    ' 导出 / 视图选项
    Public Property TrimWhite As Boolean
    Public Property PadLeft As Integer
    Public Property PadRight As Integer
    Public Property PadTop As Integer
    Public Property PadBottom As Integer
    Public Property GlobalWidth As Boolean
    Public Property ExportFormat As String
    Public Property JpegQuality As Integer
    Public Property InsertBlankHeight As Integer
End Class

<Serializable()>
Public Class ProjectAction
    Public Property ActionType As String    ' "AddLine" / "DeleteRegion"
    Public Property LineType As String      ' "Blue"
    Public Property Y As Integer
    Public Property YTop As Integer
    Public Property YBottom As Integer
    Public Property InsertY As Integer        ' InsertBlank 的插入位置
    Public Property InsertHeight As Integer   ' InsertBlank 的高度
End Class
