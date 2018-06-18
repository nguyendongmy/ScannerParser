Imports System.IO
Module GAPP

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


        ScannerCommon.GetScannerConfigFile()

        _SQL3KeyValueDotNet_v2_Path = ScannerCommon.Get_SQL3KeyValueDotNet_v2_Path()
        ScannerCommon.FileLogInfoList = New List(Of FileLogInfo)
        Dim myPath As String = ""

        ScannerCommon.SetLibMode(True)
        If IsNothing(DatabaseConnectionString) Then
            DatabaseConnectionString = ScannerCommon.GetConnectionStringGAPP()
        End If
        ScannerCommon.SetGAPPSQLConnectionString(DatabaseConnectionString)
        Dim TeamList As New HashSet(Of String)
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
        Dim SourceServer As String = "c:\GTPTest\STTicket\Temp\"
        Dim SCNR As String = "_CopiedFromFTP"
        'ScannerCommon.SetGAPPDestPath(SourceServer & fileList(0).Team & "\" & SCNR & "\")
        ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Temp\")


        Dim fileStatusPath As String

        Try
            ScannerCommon.SetSQL3KeyValueDotNetDLLPath(AppDomain.CurrentDomain.BaseDirectory + _SQL3KeyValueDotNet_v2_Path)
            ScannerCommon.DatabaseInitialization("")

            Dim fileInputObjList As List(Of Scanner.ScannerFileInputObject) = ScannerCommon.GetFileInputList("LTE", fileList)
            Dim fileInputObjList1 As List(Of Scanner.ScannerFileInputObject)

            fileInputObjList1 = ScannerCommon.GetFileInputList("UMTS", fileList)

            ScannerCommon._tblSCNRParseList = New List(Of tblSCNRParse)
            ScannerCommon._tblSCNRParseListLTE = New List(Of tblSCNRParse)

            Dim CompletedFile As New HashSet(Of String)


            For Each fileInputObj In fileInputObjList

                ScannerCommon.SetGAPPTeamName(fileInputObj.Team)
                ScannerCommon.Set_ID_SERVER_TRANSACTION(fileInputObj.id_server_transaction)
                fileStatusPath = fileInputObj.FileName.Substring(0, fileInputObj.FileName.LastIndexOf("\"))

                If Not IsNothing(fileInputObj.Team) Then
                    If Not TeamList.Contains(fileInputObj.Team + "_" + fileInputObj.id_server_transaction.ToString()) Then
                        TeamList.Add(fileInputObj.Team + "_" + fileInputObj.id_server_transaction.ToString())
                        Dim totalFile As Integer = 0
                        For j = 0 To fileList.Length - 1
                            If fileList(j).Team = fileInputObj.Team And fileInputObj.id_server_transaction = fileList(j).id_server_transaction Then
                                totalFile = totalFile + 1
                            End If
                        Next

                        ScannerCommon.WriteInforWriterFirst(fileStatusPath, totalFile, fileInputObj.ClientName, fileInputObj.MarketName, fileInputObj.Campaign, True)
                        ScannerCommon.SetStatusRecordCounter(2)
                    End If

                End If


                If TechType = "LTE" Or TechType.ToUpper = "ALL" Then
                    If Not IsNothing(fileInputObj.FileName) Then

                        CompletedFile.Add(fileInputObj.FileName)

                        'ScannerCommon.SetGAPPDestPath(SourceServer & fileInputObj.Team & "\" & SCNR & "\")
                        ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Temp\")
                        Dim scannerObj As New Scanner()
                        scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                        scannerObj.SetConfigFile("scanner_config.txt")

                        scannerObj.SetIsLibMode(True)
                        scannerObj.SetScannedTech("")
                        scannerObj.Parse(fileInputObjList, True, "LTE", fileInputObj, fileInputObj.MarketName.Trim.Replace(",", ""), fileInputObj.Campaign.Trim, fileInputObj.ClientName.Trim.Replace(",", ""))

                        Scanner_WriteLogFile(ScannerCommon.GetGAPPDestPath())

                    End If

                End If

                For Each fileInputObj1 In fileInputObjList1
                    If fileInputObj.FileName = fileInputObj1.FileName Then



                        If TechType = "UMTS" Or TechType.ToUpper = "ALL" Then
                            If Not IsNothing(fileInputObj1.FileName) Then

                                'ScannerCommon.SetGAPPDestPath(SourceServer & fileInputObj1.Team & "\" & SCNR & "\")
                                ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Temp\")
                                Dim scannerObj As New Scanner()
                                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                                scannerObj.SetConfigFile("scanner_config.txt")

                                scannerObj.SetIsLibMode(True)
                                scannerObj.SetScannedTech("")
                                scannerObj.Parse(fileInputObjList, True, "UMTS", fileInputObj1, fileInputObj1.MarketName.Trim.Replace(",", ""), fileInputObj1.Campaign.Trim, fileInputObj1.ClientName.Trim.Replace(",", ""))
                                Scanner_WriteLogFile(ScannerCommon.GetGAPPDestPath())

                            End If


                        End If

                        If TechType = "CDMA" Or TechType.ToUpper = "ALL" Then

                            If Not IsNothing(fileInputObj1.FileName) Then



                                'ScannerCommon.SetGAPPDestPath(SourceServer & fileInputObj1.Team & "\" & SCNR & "\")
                                ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Temp\")
                                Dim scannerObj As New Scanner()
                                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                                scannerObj.SetConfigFile("scanner_config.txt")

                                scannerObj.SetIsLibMode(True)
                                scannerObj.SetScannedTech("")
                                scannerObj.Parse(fileInputObjList, True, "CDMA", fileInputObj1, fileInputObj1.MarketName.Trim.Replace(",", ""), fileInputObj1.Campaign.Trim, fileInputObj1.ClientName.Trim.Replace(",", ""))
                                Scanner_WriteLogFile(ScannerCommon.GetGAPPDestPath())

                            End If

                        End If

                        If TechType = "SPECTRUMSCAN" Or TechType.ToUpper = "ALL" Then
                            If Not IsNothing(fileInputObj1.FileName) Then

                                'ScannerCommon.SetGAPPDestPath(SourceServer & fileInputObj1.Team & "\" & SCNR & "\")
                                ScannerCommon.SetGAPPDestPath("c:\GTPTest\STTicket\Temp\")

                                Dim scannerObj As New Scanner()
                                scannerObj.SetSIDFile("GWS_PCS_Cellular_SID_lookup.txt")
                                scannerObj.SetConfigFile("scanner_config.txt")
                                scannerObj.SetIsLibMode(True)
                                scannerObj.SetScannedTech("")
                                scannerObj.Parse(fileInputObjList, True, "SPECTRUMSCAN", fileInputObj1, fileInputObj1.MarketName.Trim.Replace(",", ""), fileInputObj1.Campaign.Trim, fileInputObj1.ClientName.Trim.Replace(",", ""))
                                Scanner_WriteLogFile(ScannerCommon.GetGAPPDestPath())

                            End If

                            Dim isNonMergedFileOr3040 As Boolean = ((fileInputObj.isSkip = False) And (fileInputObj.FileMergeName.Trim = "") Or CompletedFile.Contains(fileInputObj.FileMergeName))
                            Dim isMergedFile3030 As Boolean = fileInputObj.isSkip And (fileInputObj.FileMergeName.Trim <> "" And CompletedFile.Contains(fileInputObj.FileMergeName))



                            If isNonMergedFileOr3040 Or isMergedFile3030 Then

                                ScannerCommon.InsertTotblScannerParse(ScannerCommon._tblSCNRParseList)
                                ScannerCommon._tblSCNRParseList = New List(Of tblSCNRParse)

                                ScannerCommon.InsertTotblScannerParse(ScannerCommon._tblSCNRParseListLTE)
                                ScannerCommon._tblSCNRParseListLTE = New List(Of tblSCNRParse)
                            End If


                        End If



                    End If
                Next



            Next
            If Not IsNothing(_SIBSQLCommand) Then
                _SIBSQLCommand.Dispose()
                _SIBSQLConnection.Close()

            End If


        Catch ex As Exception
            Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
            dbclass.TrapErrorInDB(ex.ToString, "", "")
        End Try

        TeamList = New HashSet(Of String)

        For i = 0 To fileList.Length - 1


            If Not IsNothing(fileList(i).Team) Then
                If Not TeamList.Contains(fileList(i).Team + "_" + fileList(i).id_server_transaction.ToString()) Then
                    TeamList.Add(fileList(i).Team + "_" + fileList(i).id_server_transaction.ToString())
                    ScannerCommon.SetGAPPTeamName(fileList(i).Team)
                    ScannerCommon.Set_ID_SERVER_TRANSACTION(fileList(i).id_server_transaction)

                    fileStatusPath = Path.GetDirectoryName(fileList(i).FileName)
                    ScannerCommon.CloseInfoWriterFinal(fileStatusPath)
                End If

            End If

        Next
        Try
            _SendScnrSpec.Move3030Files(ScannerCommon._lstSpectrumScanFiles)
        Catch ex As Exception
            Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
            dbclass.TrapErrorInDB(ex.ToString, "", "")
        End Try

        Try
            _SendScnrSpec.Move3040Files(ScannerCommon._lstSpectrumScanFiles)
        Catch ex As Exception
            Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
            dbclass.TrapErrorInDB(ex.ToString, "", "")
        End Try

    End Sub


End Module
