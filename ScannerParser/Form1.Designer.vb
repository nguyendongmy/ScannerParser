<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.Button_SelectFolder = New System.Windows.Forms.Button()
        Me.ListView_FileList = New System.Windows.Forms.ListView()
        Me.FileName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Size = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.CheckBox_LTE = New System.Windows.Forms.CheckBox()
        Me.CheckBox_UMTS = New System.Windows.Forms.CheckBox()
        Me.CheckBox_CDMA = New System.Windows.Forms.CheckBox()
        Me.CheckBox_SPECTRUMSCAN = New System.Windows.Forms.CheckBox()
        Me.CheckBox_ALL = New System.Windows.Forms.CheckBox()
        Me.Button_TestGAPP = New System.Windows.Forms.Button()
        Me.ComboBox_ClientName = New System.Windows.Forms.ComboBox()
        Me.ComboBox_Market = New System.Windows.Forms.ComboBox()
        Me.ComboBox_Campaign = New System.Windows.Forms.ComboBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lblparsetech = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.btnremovefiles = New System.Windows.Forms.Button()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Button_SelectFolder
        '
        Me.Button_SelectFolder.Location = New System.Drawing.Point(14, 52)
        Me.Button_SelectFolder.Name = "Button_SelectFolder"
        Me.Button_SelectFolder.Size = New System.Drawing.Size(125, 32)
        Me.Button_SelectFolder.TabIndex = 3
        Me.Button_SelectFolder.Text = "Select Scanner Files"
        Me.Button_SelectFolder.UseVisualStyleBackColor = True
        '
        'ListView_FileList
        '
        Me.ListView_FileList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.FileName, Me.Size})
        Me.ListView_FileList.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListView_FileList.FullRowSelect = True
        Me.ListView_FileList.Location = New System.Drawing.Point(15, 135)
        Me.ListView_FileList.Name = "ListView_FileList"
        Me.ListView_FileList.Size = New System.Drawing.Size(537, 428)
        Me.ListView_FileList.TabIndex = 5
        Me.ListView_FileList.UseCompatibleStateImageBehavior = False
        Me.ListView_FileList.View = System.Windows.Forms.View.Details
        '
        'FileName
        '
        Me.FileName.Text = "FileName"
        Me.FileName.Width = 465
        '
        'Size
        '
        Me.Size.Text = "Size"
        Me.Size.Width = 74
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(399, 586)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 6
        Me.Button1.Text = "Start"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(186, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(71, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "Market Name"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(429, 10)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(54, 13)
        Me.Label2.TabIndex = 9
        Me.Label2.Text = "Campaign"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(10, 12)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(64, 13)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "Client Name"
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Checked = True
        Me.CheckBox1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox1.Location = New System.Drawing.Point(8, 8)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(128, 17)
        Me.CheckBox1.TabIndex = 16
        Me.CheckBox1.Text = "Enable SIP Decoding"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'CheckBox_LTE
        '
        Me.CheckBox_LTE.AutoSize = True
        Me.CheckBox_LTE.Location = New System.Drawing.Point(121, 6)
        Me.CheckBox_LTE.Name = "CheckBox_LTE"
        Me.CheckBox_LTE.Size = New System.Drawing.Size(46, 17)
        Me.CheckBox_LTE.TabIndex = 17
        Me.CheckBox_LTE.Text = "LTE"
        Me.CheckBox_LTE.UseVisualStyleBackColor = True
        '
        'CheckBox_UMTS
        '
        Me.CheckBox_UMTS.AutoSize = True
        Me.CheckBox_UMTS.Location = New System.Drawing.Point(173, 6)
        Me.CheckBox_UMTS.Name = "CheckBox_UMTS"
        Me.CheckBox_UMTS.Size = New System.Drawing.Size(57, 17)
        Me.CheckBox_UMTS.TabIndex = 18
        Me.CheckBox_UMTS.Text = "UMTS"
        Me.CheckBox_UMTS.UseVisualStyleBackColor = True
        '
        'CheckBox_CDMA
        '
        Me.CheckBox_CDMA.AutoSize = True
        Me.CheckBox_CDMA.Location = New System.Drawing.Point(234, 6)
        Me.CheckBox_CDMA.Name = "CheckBox_CDMA"
        Me.CheckBox_CDMA.Size = New System.Drawing.Size(57, 17)
        Me.CheckBox_CDMA.TabIndex = 19
        Me.CheckBox_CDMA.Text = "CDMA"
        Me.CheckBox_CDMA.UseVisualStyleBackColor = True
        '
        'CheckBox_SPECTRUMSCAN
        '
        Me.CheckBox_SPECTRUMSCAN.AutoSize = True
        Me.CheckBox_SPECTRUMSCAN.Location = New System.Drawing.Point(294, 6)
        Me.CheckBox_SPECTRUMSCAN.Name = "CheckBox_SPECTRUMSCAN"
        Me.CheckBox_SPECTRUMSCAN.Size = New System.Drawing.Size(115, 17)
        Me.CheckBox_SPECTRUMSCAN.TabIndex = 20
        Me.CheckBox_SPECTRUMSCAN.Text = "SPECTRUMSCAN"
        Me.CheckBox_SPECTRUMSCAN.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        Me.CheckBox_SPECTRUMSCAN.UseVisualStyleBackColor = True
        '
        'CheckBox_ALL
        '
        Me.CheckBox_ALL.AutoSize = True
        Me.CheckBox_ALL.Location = New System.Drawing.Point(71, 6)
        Me.CheckBox_ALL.Name = "CheckBox_ALL"
        Me.CheckBox_ALL.Size = New System.Drawing.Size(45, 17)
        Me.CheckBox_ALL.TabIndex = 21
        Me.CheckBox_ALL.Text = "ALL"
        Me.CheckBox_ALL.UseVisualStyleBackColor = True
        '
        'Button_TestGAPP
        '
        Me.Button_TestGAPP.Location = New System.Drawing.Point(480, 586)
        Me.Button_TestGAPP.Name = "Button_TestGAPP"
        Me.Button_TestGAPP.Size = New System.Drawing.Size(75, 23)
        Me.Button_TestGAPP.TabIndex = 22
        Me.Button_TestGAPP.Text = "Test GAPP"
        Me.Button_TestGAPP.UseVisualStyleBackColor = True
        '
        'ComboBox_ClientName
        '
        Me.ComboBox_ClientName.FormattingEnabled = True
        Me.ComboBox_ClientName.Location = New System.Drawing.Point(75, 7)
        Me.ComboBox_ClientName.Name = "ComboBox_ClientName"
        Me.ComboBox_ClientName.Size = New System.Drawing.Size(100, 21)
        Me.ComboBox_ClientName.TabIndex = 23
        '
        'ComboBox_Market
        '
        Me.ComboBox_Market.FormattingEnabled = True
        Me.ComboBox_Market.Location = New System.Drawing.Point(261, 7)
        Me.ComboBox_Market.Name = "ComboBox_Market"
        Me.ComboBox_Market.Size = New System.Drawing.Size(160, 21)
        Me.ComboBox_Market.TabIndex = 24
        '
        'ComboBox_Campaign
        '
        Me.ComboBox_Campaign.FormattingEnabled = True
        Me.ComboBox_Campaign.Location = New System.Drawing.Point(488, 7)
        Me.ComboBox_Campaign.Name = "ComboBox_Campaign"
        Me.ComboBox_Campaign.Size = New System.Drawing.Size(65, 21)
        Me.ComboBox_Campaign.TabIndex = 25
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(212, 586)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(106, 23)
        Me.Button2.TabIndex = 26
        Me.Button2.Text = "Start Process"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.SystemColors.Control
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.lblparsetech)
        Me.Panel1.Controls.Add(Me.CheckBox_ALL)
        Me.Panel1.Controls.Add(Me.CheckBox_LTE)
        Me.Panel1.Controls.Add(Me.CheckBox_UMTS)
        Me.Panel1.Controls.Add(Me.CheckBox_CDMA)
        Me.Panel1.Controls.Add(Me.CheckBox_SPECTRUMSCAN)
        Me.Panel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Panel1.Location = New System.Drawing.Point(142, 52)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(410, 33)
        Me.Panel1.TabIndex = 77
        '
        'lblparsetech
        '
        Me.lblparsetech.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblparsetech.Location = New System.Drawing.Point(3, 7)
        Me.lblparsetech.Name = "lblparsetech"
        Me.lblparsetech.Size = New System.Drawing.Size(65, 16)
        Me.lblparsetech.TabIndex = 80
        Me.lblparsetech.Text = "Parse Tech:"
        '
        'Panel2
        '
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(Me.CheckBox1)
        Me.Panel2.Location = New System.Drawing.Point(410, 93)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(142, 33)
        Me.Panel2.TabIndex = 82
        '
        'btnremovefiles
        '
        Me.btnremovefiles.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnremovefiles.Location = New System.Drawing.Point(13, 95)
        Me.btnremovefiles.Name = "btnremovefiles"
        Me.btnremovefiles.Size = New System.Drawing.Size(125, 29)
        Me.btnremovefiles.TabIndex = 86
        Me.btnremovefiles.Text = "Remove Scanner Files"
        Me.btnremovefiles.UseVisualStyleBackColor = True
        Me.btnremovefiles.Visible = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(562, 637)
        Me.Controls.Add(Me.btnremovefiles)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.ComboBox_Campaign)
        Me.Controls.Add(Me.ComboBox_Market)
        Me.Controls.Add(Me.ComboBox_ClientName)
        Me.Controls.Add(Me.Button_TestGAPP)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ListView_FileList)
        Me.Controls.Add(Me.Button_SelectFolder)
        Me.Name = "Form1"
        Me.Text = "Scanner Parser (SIB)"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button_SelectFolder As System.Windows.Forms.Button
    Friend WithEvents ListView_FileList As System.Windows.Forms.ListView
    Friend WithEvents FileName As System.Windows.Forms.ColumnHeader
    Friend WithEvents Size As System.Windows.Forms.ColumnHeader
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_LTE As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_UMTS As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_CDMA As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_SPECTRUMSCAN As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_ALL As System.Windows.Forms.CheckBox
    Friend WithEvents Button_TestGAPP As System.Windows.Forms.Button
    Friend WithEvents ComboBox_ClientName As ComboBox
    Friend WithEvents ComboBox_Market As ComboBox
    Friend WithEvents ComboBox_Campaign As ComboBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lblparsetech As Label
    Friend WithEvents Panel2 As Panel
    Friend WithEvents btnremovefiles As Button
End Class
