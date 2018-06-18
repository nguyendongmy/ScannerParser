Module GAPP1

    Public Structure GAPPData
        Public id_server_transaction As Integer
        Public id_server_file As Integer
        Public file_path As String
        Public Campaign As String
        Public ClientName As String
        Public MarketName As String
        Public FileName As String
        Public status_flag As String
        Public Team As String
    End Structure

    Private _SQL3KeyValueDotNet_v2_Path

    Public Sub GAPP(ByVal TechType As String, ByVal fileList As GAPPData())

        Dim _SendScnrSpec As New SendScnrSpectrum
        ScannerCommon._lstSpectrumScanFiles = New List(Of String)


        GetScannerConfigFile()

        ScannerCommon._tblSCNRParseList = New List(Of tblSCNRParse)
        ScannerCommon.FileLogInfoList = New List(Of FileLogInfo)
        Dim myPath As String = ""
        ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Temp\")
        ScannerCommon.SetLibMode(True)
        ScannerCommon.SetGAPPSQLConnectionString("Data Source=(local);Initial Catalog=GAPP;Integrated Security=True;User ID=sa;Password=gwspass;")

        For i = 0 To fileList.Length - 1
            Dim filelog As New FileLogInfo()

            filelog.GSMChannelFound = New List(Of String)
            filelog.WCDMAChannelFound = New List(Of String)
            filelog.CDMAChannelFound = New List(Of String)
            filelog.LTEChannelFound = New List(Of String)
            filelog.isGetInputChannel = False
            filelog.FileName = fileList(i).FileName
            filelog.ParseTechList = New List(Of String)
            ScannerCommon.FileLogInfoList.Add(filelog)

        Next

        Try
            Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = ScannerCommon.GetFileInputList("LTE", fileList)
            Dim file_index As Integer = 1

            For Each fileInputObj In fileInputObjList
                If TechType = "LTE" Or TechType.ToUpper = "ALL" Then
                    ScannerCommon.SetSQL3KeyValueDotNetDLLPath(AppDomain.CurrentDomain.BaseDirectory + _SQL3KeyValueDotNet_v2_Path)
                    If Not ScannerCommon.DatabaseInitialization("") Then
                        Return
                    End If


                    file_index = file_index + 1
                    If Not IsNothing(fileInputObj.FileName) Then
                        Dim scannerObj As New Scanner()
                        scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                        scannerObj.SetConfigFile("scanner_config.txt")

                        scannerObj.SetIsLibMode(True)
                        scannerObj.SetScannedTech("")
                        scannerObj.Parse(file_index, True, "LTE", fileInputObj, fileInputObj.MarketName.Trim.Replace(",", ""), fileInputObj.Campaign.Trim, fileInputObj.ClientName.Trim.Replace(",", ""))
                    End If
                End If


                If TechType = "UMTS" Or TechType.ToUpper = "ALL" Then
                    Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("UMTS", fileList)
                    Dim file_index As Integer = 1
                    For Each fileInputObj In fileInputObjList
                        If Not IsNothing(fileInputObj.FileName) Then
                            file_index = file_index + 1
                            Dim scannerObj As New Scanner()
                            scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                            scannerObj.SetConfigFile("scanner_config.txt")

                            scannerObj.SetIsLibMode(True)
                            scannerObj.SetScannedTech("")
                            scannerObj.Parse(file_index, True, "UMTS", fileInputObj, fileInputObj.MarketName.Trim.Replace(",", ""), fileInputObj.Campaign.Trim, fileInputObj.ClientName.Trim.Replace(",", ""))
                        End If

                    Next
                End If


            Next

            If Not IsNothing(_SIBSQLCommand) Then
                _SIBSQLCommand.Dispose()
                _SIBSQLConnection.Close()

            End If




            If TechType = "CDMA" Or TechType.ToUpper = "ALL" Then
                Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("CDMA", fileList)
                Dim file_index As Integer = 1
                For Each fileInputObj In fileInputObjList
                    file_index = file_index + 1
                    If Not IsNothing(fileInputObj.FileName) Then

                        Dim scannerObj As New Scanner()
                        scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                        scannerObj.SetConfigFile("scanner_config.txt")

                        scannerObj.SetIsLibMode(True)
                        scannerObj.SetScannedTech("")
                        scannerObj.Parse(file_index, True, "CDMA", fileInputObj, fileInputObj.MarketName.Trim.Replace(",", ""), fileInputObj.Campaign.Trim, fileInputObj.ClientName.Trim.Replace(",", ""))
                    End If

                Next
            End If

            If TechType = "SPECTRUMSCAN" Or TechType.ToUpper = "ALL" Then
                Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = GetFileInputList("SPECTRUMSCAN", fileList)
                Dim file_index As Integer = 1
                For Each fileInputObj In fileInputObjList
                    file_index = file_index + 1
                    If Not IsNothing(fileInputObj.FileName) Then

                        Dim scannerObj As New Scanner()
                        scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                        scannerObj.SetConfigFile("scanner_config.txt")
                        scannerObj.SetIsLibMode(True)
                        scannerObj.SetScannedTech("")
                        scannerObj.Parse(file_index, True, "SPECTRUMSCAN", fileInputObj, fileInputObj.MarketName.Trim.Replace(",", ""), fileInputObj.Campaign.Trim, fileInputObj.ClientName.Trim.Replace(",", ""))
                    End If

                Next
            End If

        Catch ex As Exception
            Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
            dbclass.TrapErrorInDB(ex.ToString, "", "")
        End Try

        Scanner_WriteLogFile(ScannerCommon.GetGAPPDestPath())

        _SendScnrSpec.Move3030Files(ScannerCommon._lstSpectrumScanFiles)
        _SendScnrSpec.Move3040Files(ScannerCommon._lstSpectrumScanFiles)


    End Sub

    Private Sub GetScannerConfigFile()
        Try
            Using sr As New System.IO.StreamReader("scanner_config.txt")
                Dim line As String
                Do While sr.Peek() >= 0
                    line = sr.ReadLine()
                    Dim strs As String() = line.Split("|")
                    If strs(0).Trim.ToLower() = "version" Then
                        ScannerCommon.SetDllVersion(strs(1).Trim)
                    ElseIf strs(0).Trim.ToLower = "sql_connection" Then
                        ScannerCommon.SetConnectionString(strs(1))
                    ElseIf strs(0).Trim.ToLower = "sql_connection_gapp" Then
                        ScannerCommon.SetConnectionStringGAPP(strs(1))
                    ElseIf strs(0).Trim.ToLower = "database_name" Then
                        ScannerCommon.SetDatabaseName(strs(1))
                    ElseIf strs(0).Trim.ToLower = "SQL3KeyValueDotNet_v2.dll_path".ToLower Then
                        _SQL3KeyValueDotNet_v2_Path = strs(1).Trim
                    End If
                Loop
            End Using
        Catch e As Exception
            Console.WriteLine("The file could not be read:")
            Console.WriteLine(e.Message)
        End Try
    End Sub

End Module
