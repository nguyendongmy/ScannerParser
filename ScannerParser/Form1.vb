Imports System.Collections.Generic
Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Threading
Imports System.Collections.Concurrent
Imports System.Data.SqlClient
Imports ScannerParser.GTPService

Public Class Form1
    Private fileList() As String
    Private _SQL3KeyValueDotNet_v2_Path As String
    Private clients As _Client()
    Private Sub Button_SelectFolder_Click(sender As System.Object, e As System.EventArgs) Handles Button_SelectFolder.Click

        Dim selectedUploadFiles() As String
        Dim myFiles1 As New OpenFileDialog
        'Try

        'myFiles1.DefaultExt = "csv"
        myFiles1.Multiselect = True
        myFiles1.Title = "Select File "
        myFiles1.Filter = "Select Files to upload  (*.mf)|*.mf;|(*.mf)|*.mf"
        myFiles1.RestoreDirectory = True 'mod 7/6/07

        If (myFiles1.ShowDialog() = DialogResult.OK) Then
            Dim FileInfo As System.IO.FileInfo

            selectedUploadFiles = myFiles1.FileNames
            Array.Sort(selectedUploadFiles)

            fileList = selectedUploadFiles

            If selectedUploadFiles.Length > 0 Then
                btnremovefiles.Enabled = True
                btnremovefiles.Visible = True
            End If

            Dim count_network_file As Integer = 0

            For i = 0 To selectedUploadFiles.Length - 1
                If Not (selectedUploadFiles(i).StartsWith("\\")) Then
                    Dim lvi As New ListViewItem
                    ' First Column can be the listview item's Text  
                    lvi.Text = selectedUploadFiles(i)
                    ' Second Column is the first sub item  
                    ' Add the ListViewItem to the ListView  
                    ListView_FileList.Items.Add(lvi)
                    FileInfo = New System.IO.FileInfo(selectedUploadFiles(i))
                    lvi.SubItems.Add(FileInfo.Length)
                Else
                    count_network_file = count_network_file + 1
                End If

            Next

            If count_network_file > 0 Then
                MessageBox.Show("Scanner files must reside on the local system.  Do not select non-local files.")
            End If

        End If
    End Sub



    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If ComboBox_ClientName.Text.Trim = "" Then
            MessageBox.Show("Please select Client Name")
            Return
        End If
        If ComboBox_Market.Text.Trim = "" Then
            MessageBox.Show("Please select market")
            Return
        End If
        If ComboBox_Campaign.Text.Trim = "" Then
            MessageBox.Show("Please select campain")
            Return
        End If

        If IsNothing(fileList) Then
            MessageBox.Show("Please select input files")
            Return
        End If
        Dim market_name As String = ComboBox_Market.Text.Trim
        Dim client_name As String = ComboBox_ClientName.Text.Trim
        Dim campain As String = ComboBox_Campaign.Text.Trim


        ScannerCommon._tblSCNRParseList = New List(Of tblSCNRParse)
        ScannerCommon._tblSCNRParseListLTE = New List(Of tblSCNRParse)


        ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Tmp1\")
        ScannerCommon.SetLibMode(False)
        Dim isLibMode As Boolean = False
        Dim timeStart As DateTime = DateTime.Now

        If CheckBox_ALL.Checked = False And CheckBox_LTE.Checked = False And CheckBox_UMTS.Checked = False And CheckBox_CDMA.Checked = False And CheckBox_SPECTRUMSCAN.Checked = False Then
            MessageBox.Show("Please select technology")
            Return
        End If

        ScannerCommon.FileLogInfoList = New List(Of FileLogInfo)
        Dim myPath As String = ""
        Dim fileListGAPP As GAPPData()
        ReDim fileListGAPP(fileList.Length - 1)
        Dim i As Integer


        For i = 0 To fileList.Length - 1
            Dim filelog As New FileLogInfo()

            filelog.FileName = fileList(i)
            myPath = fileList(i).Substring(0, fileList(i).LastIndexOf("\")) + "\"
            filelog.GSMChannelFound = New List(Of String)
            filelog.WCDMAChannelFound = New List(Of String)
            filelog.CDMAChannelFound = New List(Of String)
            filelog.LTEChannelFound = New List(Of String)
            filelog.isGetInputChannel = False
            filelog.ParseTechList = New List(Of String)


            ScannerCommon.FileLogInfoList.Add(filelog)
            fileListGAPP(i).FileName = fileList(i)

        Next

        If CheckBox_LTE.Checked Or CheckBox_ALL.Checked Then

            If CheckBox1.Checked Then
                ScannerCommon.SetSQL3KeyValueDotNetDLLPath(AppDomain.CurrentDomain.BaseDirectory + _SQL3KeyValueDotNet_v2_Path)
                If Not ScannerCommon.DatabaseInitialization("") Then
                    Return
                End If
            End If

            Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("LTE", fileListGAPP)


            For Each fileInputObj In fileInputObjList

                Dim scannerObj As New Scanner()
                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                scannerObj.SetConfigFile("scanner_config.txt")

                scannerObj.SetIsLibMode(isLibMode)
                scannerObj.SetScannedTech("")
                scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "LTE", fileInputObj, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
            Next
        End If

        If CheckBox_UMTS.Checked Or CheckBox_ALL.Checked Then
            Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = ScannerCommon.GetFileInputList("UMTS", fileListGAPP)
            Dim file_index As Integer = 1
            For Each fileInputObj In fileInputObjList
                file_index = file_index + 1
                Dim scannerObj As New Scanner()
                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                scannerObj.SetConfigFile("scanner_config.txt")

                scannerObj.SetIsLibMode(isLibMode)
                scannerObj.SetScannedTech("")
                scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "UMTS", fileInputObj, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
            Next
        End If

        If CheckBox_CDMA.Checked Or CheckBox_ALL.Checked Then

            Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("CDMA", fileListGAPP)
            Dim file_index As Integer = 1
            For Each fileInputObj In fileInputObjList
                file_index = file_index + 1
                Dim scannerObj As New Scanner()
                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                scannerObj.SetConfigFile("scanner_config.txt")

                scannerObj.SetIsLibMode(isLibMode)
                scannerObj.SetScannedTech("")
                scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "CDMA", fileInputObj, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
            Next
        End If

        If CheckBox_SPECTRUMSCAN.Checked Or CheckBox_ALL.Checked Then
            Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("SPECTRUMSCAN", fileListGAPP)
            Dim file_index As Integer = 1
            For Each fileInputObj In fileInputObjList
                file_index = file_index + 1
                Dim scannerObj As New Scanner()
                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                scannerObj.SetConfigFile("scanner_config.txt")

                scannerObj.SetIsLibMode(isLibMode)
                scannerObj.SetScannedTech("")
                scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "SPECTRUMSCAN", fileInputObj, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
            Next
        End If

        Scanner_WriteLogFile(myPath)

        If CheckBox1.Checked And (CheckBox_LTE.Checked Or CheckBox_ALL.Checked) And Not IsNothing(_SIBSQLCommand) Then
            _SIBSQLCommand.Dispose()
            _SIBSQLConnection.Close()
        End If

        MessageBox.Show("Total Time in seconds: " + (DateTime.Now - timeStart).TotalSeconds.ToString())
        MessageBox.Show("Completed")

    End Sub

    Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Button_TestGAPP.Visible = True
        Button1.Visible = False

        CheckBox_LTE.Checked = True
        CheckBox_CDMA.Checked = True
        CheckBox_UMTS.Checked = True
        ComboBox_Market.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        ComboBox_Market.AutoCompleteSource = AutoCompleteSource.ListItems
        GetConfigFile()
        GetClientList()
    End Sub

    Private Sub GetClientList()

        Dim proxy As New GTPServiceClient()
        proxy.Open()



        clients = proxy.GetClient()
        Dim c As New _Client
        For Each c In clients
            ComboBox_ClientName.Items.Add(c.ClientName)
        Next

        Dim CampaignList As _Campaign() = proxy.GetCampaign()
        If CampaignList.Count > 0 Then
            For i = 0 To CampaignList.Count - 1
                ComboBox_Campaign.Items.Add(CampaignList(i).campaign_name)
            Next

        End If

        proxy.Close()

    End Sub

    Private Sub GetMarket(ByVal market_status)

        Dim proxy1 As GTPServiceClient
        proxy1 = New GTPServiceClient()

        Dim Markets As _MarketName()

        Markets = proxy1.GetMarket(market_status)

        ComboBox_Market.DataSource = Markets
        ComboBox_Market.DisplayMember = "name_market"
        ComboBox_Market.ValueMember = "id_market"

        proxy1.Close()
    End Sub

    Private Sub GetConfigFile()
        Try
            Using sr As New StreamReader("scanner_config.txt")
                Dim line As String
                Do While sr.Peek() >= 0
                    line = sr.ReadLine()
                    Dim strs As String() = line.Split("|")
                    If strs(0).Trim.ToLower = "sql_connection_gapp" Then
                        ScannerCommon.SetConnectionStringGAPP(strs(1))
                    End If
                Loop
            End Using
        Catch e As Exception
            Console.WriteLine("The file could not be read:")
            Console.WriteLine(e.Message)
        End Try
    End Sub


    Private Sub Button2_Click_1(sender As System.Object, e As System.EventArgs) Handles Button_TestGAPP.Click
        Dim fileList() As GAPPData
        ReDim Preserve fileList(0)
        ScannerCommon._tblSCNRParseList = New List(Of tblSCNRParse)
        ScannerCommon._tblSCNRParseListLTE = New List(Of tblSCNRParse)

        ScannerCommon.SetGAPPSQLConnectionString("Data Source=(local);Initial Catalog=GAPP;Integrated Security=True;User ID=sa;Password=gwspass;")
        Dim inputFiles2 As String() = Directory.GetFiles("c:\GTPTest\STTicket\FileInfoStatus for 2 Transactions\5031-Worcester MA\")
        Dim inputFiles1 As String() = Directory.GetFiles("c:\GTPTest\STTicket\FileInfoStatus for 2 Transactions\4913-Portland OR\")
        Dim inputFiles As New List(Of String)


        inputFiles.AddRange(inputFiles1)
        inputFiles.AddRange(inputFiles2)
        'Dim inputFiles As String() = Directory.GetFiles("c:\GTPTest\STTicket\ST_1201\_Raw\")
        Dim i As Integer

        For i = 0 To inputFiles.Count - 1

            Dim FileStr = inputFiles(i)

            If FileStr.ToLower.IndexOf(".mf".ToLower()) >= 0 Then
                Dim fileGAPP As New GAPPData
                fileGAPP.id_server_file = 1
                fileGAPP.FileName = FileStr
                fileGAPP.Campaign = "16D2"
                If FileStr.IndexOf("5031-") >= 0 Then
                    fileGAPP.id_server_transaction = 5031
                    fileGAPP.ClientName = "AT&T BM"
                    fileGAPP.MarketName = "Washington, DC"
                    fileGAPP.Team = "TEAM904"
                Else
                    fileGAPP.id_server_transaction = 4913
                    fileGAPP.ClientName = "BELL BM"
                    fileGAPP.MarketName = "Fairfax, VA"
                    fileGAPP.Team = "TEAM904"
                End If
                fileList(fileList.Length - 1) = fileGAPP

                If i < inputFiles.Count - 1 Then
                    ReDim Preserve fileList(fileList.Length)
                End If



            End If

        Next

        GAPP.GAPP("TEAM30", fileList)

        MessageBox.Show("DONE")



    End Sub

    Private Sub ComboBox_ClientName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox_ClientName.SelectedIndexChanged

        Dim strClient As String = ComboBox_ClientName.Text
        Dim client_market_value As Integer = -1
        Dim i As Integer

        For i = 0 To clients.Length - 1
            If clients(i).ClientName = ComboBox_ClientName.Text Then
                client_market_value = clients(i).client_market_value
            End If
        Next

        If client_market_value <> -1 Then
            GetMarket(client_market_value)
        End If

    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If Not CheckBox1.Checked Then
            MessageBox.Show("Disabling SIP decoding will prevent identification of any LTE networks.  All LTE operators will be output as No Service")
        End If
    End Sub

    Private Sub CheckBox_ALL_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_ALL.CheckedChanged
        If CheckBox_ALL.Checked Then
            CheckBox_LTE.Checked = True
            CheckBox_UMTS.Checked = True
            CheckBox_CDMA.Checked = True
            CheckBox_SPECTRUMSCAN.Checked = True
        Else
            CheckBox_LTE.Checked = False
            CheckBox_UMTS.Checked = False
            CheckBox_CDMA.Checked = False
            CheckBox_SPECTRUMSCAN.Checked = False
        End If
    End Sub

    Private Sub Button2_Click_2(sender As Object, e As EventArgs) Handles Button2.Click

        ScannerCommon.GetScannerConfigFile()

        _SQL3KeyValueDotNet_v2_Path = ScannerCommon.Get_SQL3KeyValueDotNet_v2_Path()

        If ComboBox_ClientName.Text.Trim = "" Then
            MessageBox.Show("Please select Client Name")
            Return
        End If
        If ComboBox_Market.Text.Trim = "" Then
            MessageBox.Show("Please select market")
            Return
        End If
        If ComboBox_Campaign.Text.Trim = "" Then
            MessageBox.Show("Please select campain")
            Return
        End If

        If IsNothing(fileList) Then
            MessageBox.Show("Please select input files")
            Return
        End If
        Dim market_name As String = ComboBox_Market.Text.Trim
        Dim client_name As String = ComboBox_ClientName.Text.Trim
        Dim campain As String = ComboBox_Campaign.Text.Trim


        ScannerCommon._tblSCNRParseList = New List(Of tblSCNRParse)
        ScannerCommon._tblSCNRParseListLTE = New List(Of tblSCNRParse)


        ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Tmp1\")
        ScannerCommon.SetLibMode(False)
        Dim isLibMode As Boolean = False
        ScannerCommon.SetGAPPSQLConnectionString("Data Source=(local);Initial Catalog=GAPP;Integrated Security=True;User ID=sa;Password=gwspass;")

        Dim timeStart As DateTime = DateTime.Now

        If CheckBox_ALL.Checked = False And CheckBox_LTE.Checked = False And CheckBox_UMTS.Checked = False And CheckBox_CDMA.Checked = False And CheckBox_SPECTRUMSCAN.Checked = False Then
            MessageBox.Show("Please select technology")
            Return
        End If

        ScannerCommon.FileLogInfoList = New List(Of FileLogInfo)
        Dim myPath As String = ""
        Dim fileListGAPP As GAPPData()
        ReDim fileListGAPP(fileList.Length - 1)
        Dim i As Integer

        Dim count As Integer = 0
        Dim scannerDataList1 As New List(Of Scanner.ScannerFileInputObject)

        Dim max_core As Integer = Environment.ProcessorCount

        'Dim appSettings = System.Configuration.ConfigurationSettings.AppSettings
        'Try
        '    max_core = appSettings("scannerMaxCore")

        '    If max_core < 0 Then
        '        max_core = Environment.ProcessorCount
        '    ElseIf max_core = 0 Then
        '        MessageBox.Show("scannerMaxCore is set to 0. Program will stop !")
        '        Return
        '    End If
        'Catch ex As Exception
        '    max_core = Environment.ProcessorCount
        'End Try
        Dim fileStatusPath As String = fileList(0).Substring(0, fileList(0).LastIndexOf("\"))

        ScannerCommon.WriteInforWriterFirst(fileStatusPath, fileList.Length, ComboBox_ClientName.Text, ComboBox_Market.Text, ComboBox_Campaign.Text, CheckBox1.Checked.ToString())
        ScannerCommon.SetStatusRecordCounter(2)

        For i = 0 To fileList.Length - 1
            Dim filelog As New FileLogInfo()

            filelog.FileName = fileList(i)
            myPath = fileList(i).Substring(0, fileList(i).LastIndexOf("\")) + "\"
            filelog.GSMChannelFound = New List(Of String)
            filelog.WCDMAChannelFound = New List(Of String)
            filelog.CDMAChannelFound = New List(Of String)
            filelog.LTEChannelFound = New List(Of String)
            filelog.isGetInputChannel = False
            filelog.ParseTechList = New List(Of String)
            ScannerCommon.FileLogInfoList.Add(filelog)
            fileListGAPP(i).FileName = fileList(i)

        Next


        count = 0

        If CheckBox1.Checked Then
            ScannerCommon.SetSQL3KeyValueDotNetDLLPath(AppDomain.CurrentDomain.BaseDirectory + _SQL3KeyValueDotNet_v2_Path)



            If Not ScannerCommon.DatabaseInitialization("") Then
                Return
            End If
        End If
        Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject)
        Dim fileInputObjList1 As List(Of Scanner.ScannerFileInputObject)

        fileInputObjList = GetFileInputList("LTE", fileListGAPP)
        fileInputObjList1 = GetFileInputList("UMTS", fileListGAPP)


        'If CheckBox_LTE.Checked Or CheckBox_ALL.Checked Then
        '    fileInputObjList = GetFileInputList("LTE", fileListGAPP)
        'ElseIf CheckBox_UMTS.Checked Or CheckBox_ALL.Checked Then
        '    fileInputObjList = GetFileInputList("UMTS", fileListGAPP)

        'End If
        'Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("LTE", fileListGAPP)

        'count = 0
        'scannerDataList1 = New List(Of Scanner.ScannerFileInputObject)
        'While count < fileInputObjList.Count
        '    scannerDataList1.Add(fileInputObjList(count))
        '    count = count + 1
        '    If count = max_core Or count = fileList.Count Then
        '        System.Threading.Tasks.Parallel.ForEach(scannerDataList1, Sub(data)

        '                                                                      Dim fileInputObj As Scanner.ScannerFileInputObject = data
        '                                                                      Dim scannerObj As New Scanner()
        '                                                                      scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
        '                                                                      scannerObj.SetConfigFile("scanner_config.txt")

        '                                                                      scannerObj.SetIsLibMode(isLibMode)
        '                                                                      scannerObj.SetScannedTech("")
        '                                                                      scannerObj.Parse(CheckBox1.Checked, "LTE", fileInputObj, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
        '                                                                  End Sub)
        '        scannerDataList1.Clear()

        '    End If

        'End While

        For Each fileInputObj In fileInputObjList

            Dim scannerObj As New Scanner()
            scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
            scannerObj.SetConfigFile("scanner_config.txt")

            scannerObj.SetIsLibMode(isLibMode)
            scannerObj.SetScannedTech("")
            If CheckBox_LTE.Checked Or CheckBox_ALL.Checked Then
                scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "LTE", fileInputObj, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
            End If

            For Each fileInputObj1 In fileInputObjList1
                If fileInputObj.FileName = fileInputObj1.FileName Then
                    If CheckBox_UMTS.Checked Or CheckBox_ALL.Checked Then
                        scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "UMTS", fileInputObj1, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
                    End If
                    If CheckBox_CDMA.Checked Or CheckBox_ALL.Checked Then
                        scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "CDMA", fileInputObj1, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
                    End If
                    If CheckBox_SPECTRUMSCAN.Checked Or CheckBox_ALL.Checked Then
                        scannerObj.Parse(fileInputObjList, CheckBox1.Checked, "SPECTRUMSCAN", fileInputObj1, market_name.Trim.Replace(",", ""), campain.Trim, client_name.Trim.Replace(",", ""))
                    End If
                End If
            Next

       

        Next


        Scanner_WriteLogFile(myPath)
        ScannerCommon.CloseInfoWriterFinal(fileStatusPath)

        If CheckBox1.Checked And (CheckBox_LTE.Checked Or CheckBox_ALL.Checked) And Not IsNothing(_SIBSQLCommand) Then
            _SIBSQLCommand.Dispose()
            _SIBSQLConnection.Close()
        End If


        MessageBox.Show("Total Time in seconds: " + (DateTime.Now - timeStart).TotalSeconds.ToString())
        MessageBox.Show("Completed")
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs)
        If Not Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB") Then
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB")
            Dim files As String() = Directory.GetFiles("\\Gapp-ftp\gapp_teams\TeamREMIS\CurrentScannerRelease\")

            For Each fileStr In files
                Dim FileName As String = fileStr.Substring(fileStr.LastIndexOf("\") + 1)
                File.Copy(fileStr, AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB\" + FileName)

            Next
        Else

            Dim files As String() = Directory.GetFiles("\\Gapp-ftp\gapp_teams\TeamREMIS\CurrentScannerRelease\")

            For Each fileStr In files
                Dim FileName As String = fileStr.Substring(fileStr.LastIndexOf("\") + 1)
                Dim server_scannerFile As New FileInfo(fileStr)
                If File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB\" + FileName) Then
                    Dim local_ScannerFile As New FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB\" + FileName)
                    If server_scannerFile.Length <> local_ScannerFile.Length Then
                        File.Copy(fileStr, AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB\" + FileName, True)
                    End If
                Else
                    File.Copy(fileStr, AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB\" + FileName)
                End If

            Next
        End If

        Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\ScannerParserSIB\ScannerParser.exe")

    End Sub

    Private Sub btnremovefiles_Click(sender As Object, e As EventArgs) Handles btnremovefiles.Click
        Dim item As ListViewItem
        item = New ListViewItem

        For Each item In ListView_FileList.SelectedItems

            ListView_FileList.Items.Remove(item)

        Next
        If ListView_FileList.Items.Count = 0 Then
            btnremovefiles.Visible = False
        End If
    End Sub
End Class
