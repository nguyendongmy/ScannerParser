Imports SQL3KeyValueDotNet_v2
Imports System.IO
Imports System.Data.SqlClient
Imports GWS.ProASPP.DTO

Module ScannerCommon
    Private _isFileStatusExist As Boolean = False
    Private _SQL3KeyValueDotNet_v2_Path As String
    Private WrittenFileLogInfoList As HashSet(Of String)
    Public Structure Tier1_Operator
        Public ClientName As String
        Public Operator_List As Hashtable
    End Structure
    Public Structure Scanner_ModuleID
        Public MergePattern1 As String
        Public MergePattern2 As String
    End Structure

    Private _Scanner_ModuleID_List As List(Of Scanner_ModuleID)

    Public Sub Set_Scanner_ModuleID_List(ByVal Scanner_ModuleID_List As List(Of Scanner_ModuleID))
        _Scanner_ModuleID_List = Scanner_ModuleID_List
    End Sub

    Public Function Get_Scanner_ModuleID_List() As List(Of Scanner_ModuleID)
        Return _Scanner_ModuleID_List
    End Function
    Public Tier1_Operator_List As Hashtable


    Public _tblSCNRParseList As List(Of tblSCNRParse)
    Public _tblSCNRParseListLTE As List(Of tblSCNRParse)


    Public Tier1Network As HashSet(Of String)

    Public Sub addTier1Network()
        'AT&T UMTS, AT&T LTE, Sprint CDMA, Sprint LTE, T-Mobile UMTS, T-Mobile LTE, Verizon CDMA, Verizon LTE
        Tier1Network.Add("ATT|UMTS")

    End Sub
    Private _isRunLogFile As Boolean
    Private _GAPP_TeamName As String = ""
    Private _ID_SERVER_TRANSACTION As String = ""
    Private _isLibMode As Boolean
    Private _GAPPDestPath As String
    Private _initializationPath As String
    Private _SQL3KeyValueDotNetDLLPath As String = ""
    Private _sqlConnectionString As String
    Private _sqlConnectionStringGAPP As String
    Private _DatabaseName As String = ""
    Private _isSkipEmptyBand As Boolean
    Private _GAPPSQLConnectionString As String
    Private _SourceServer As String
    Private _AddNoRecMode As Integer = 1
    Private _ParsingInfoWriter As StreamWriter
    Private _Status_Record_Counter As Integer

    Public Sub SetGAPPTeamName(ByVal TeamName As String)
        _GAPP_TeamName = TeamName
    End Sub

    Public Sub Set_ID_SERVER_TRANSACTION(ByVal ID_SERVER_TRANSACTION As String)
        _ID_SERVER_TRANSACTION = ID_SERVER_TRANSACTION
    End Sub

    Public Function Get_SQL3KeyValueDotNet_v2_Path() As String
        Return _SQL3KeyValueDotNet_v2_Path
    End Function

    Public Sub GetScannerConfigFile()

        ScannerCommon.Tier1_Operator_List = New Hashtable()
        _Scanner_ModuleID_List = New List(Of Scanner_ModuleID)()

        Try
            Using sr As New StreamReader("scanner_config.txt")
                Dim line As String
                Do While sr.Peek() >= 0
                    line = sr.ReadLine()
                    If line.StartsWith("#") Then
                        Continue Do
                    End If
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
                    ElseIf strs(0).Trim.ToLower = "Tier1_Operator".ToLower Then

                        'Tier1_Operator|Default|AT&T:LTE(700,850,1900,2100,2300),UMTS(700,850,1900,2100);T-Mobile:LTE(600,700,850,1900,2100), UMTS(700,850,1900,2100);Sprint:LTE(700,850,1900,2100,2500),CDMA(850,1900,2100);Verizon:LTE(700,850,1900,2100),CDMA(850,1900,2100)
                        'Tier1_Operator|AT&T BM| AT&T:LTE(700,850,1900,2100,2300),UMTS(700,850,1900,2100);T-Mobile:LTE(600,700,850,1900,2100), UMTS(700,850,1900,2100);Sprint:LTE(700,850,1900,2100,2500),CDMA(850,1900,2100);Verizon:LTE(700,850,1900,2100),CDMA(850,1900,2100)



                        Dim Tier1_Operator_Item As New Tier1_Operator()

                        Dim OpList As String() = strs(2).Split(";")

                        Dim Operator_List As New Hashtable()
                        Tier1_Operator_Item.ClientName = strs(1)
                        Dim tmpStr As String

                        For Each tmpStr In OpList
                            Dim tmp1 As String() = tmpStr.Split(":")
                            Operator_List.Add(tmp1(0).Trim, tmp1(1))
                        Next
                        Tier1_Operator_Item.Operator_List = Operator_List
                        Tier1_Operator_List.Add(Tier1_Operator_Item.ClientName.Trim, Tier1_Operator_Item)
                    ElseIf strs(0).Trim.ToLower = "Scanner_ModuleID_pairs".ToLower Then
                        Dim tmpModule As New Scanner_ModuleID()
                        tmpModule.MergePattern1 = strs(1) + "-S"
                        tmpModule.MergePattern2 = strs(2) + "-S"
                        _Scanner_ModuleID_List.Add(tmpModule)

                    End If
                Loop
            End Using
        Catch e As Exception
            Console.WriteLine("The file could not be read:")
            Console.WriteLine(e.Message)
        End Try
    End Sub


    Public Sub SetStatusRecordCounter(ByVal counter As Integer)
        _Status_Record_Counter = counter
    End Sub

    Public Function GetStatusRecordCounter() As Integer
        Return _Status_Record_Counter
    End Function


    Public Sub WriteParsingInfoWriter(ByVal str As String)
        _ParsingInfoWriter.WriteLine(str)
    End Sub


    Public Sub OpenParsingInfoWriter(ByVal filePath As String)
        If _ID_SERVER_TRANSACTION.ToString() = "" Then
            Dim tt As String = "5031"
        End If
        If _GAPP_TeamName.Trim <> "" Then
            Dim fileNameOnly = _GAPP_TeamName + "_TRANSACTION" + _ID_SERVER_TRANSACTION + "_FileInfoStatus.txt"
            If File.Exists(ScannerCommon.GetGAPPDestPath() + fileNameOnly) Then
                _ParsingInfoWriter = File.AppendText(ScannerCommon.GetGAPPDestPath() + fileNameOnly)
            Else
                _ParsingInfoWriter = File.AppendText(filePath + "\" + _GAPP_TeamName + "_TRANSACTION" + _ID_SERVER_TRANSACTION + "_FileInfoStatus.txt")
            End If

        Else
            _ParsingInfoWriter = File.AppendText(filePath + "\FileInfoStatus.txt")
        End If

    End Sub


    Public Sub CloseParsingInfoWriter()
        _ParsingInfoWriter.Close()
    End Sub

    Public Function GetAddNoRecMode()
        Return _AddNoRecMode
    End Function

    Public Sub WriteInforWriterFirst(ByVal filePath As String, ByVal totalFile As Integer, ByVal ClientName As String, ByVal MarketName As String, ByVal Campaign As String, ByVal EnableDecoding As String)

        Dim parsingInfoFileName As String = filePath + "\FileInfoStatus.txt"

        Dim fileNameOnly = _GAPP_TeamName + "_TRANSACTION" + _ID_SERVER_TRANSACTION + "_FileInfoStatus.txt"

        If _GAPP_TeamName.Trim <> "" Then            
            If File.Exists(ScannerCommon.GetGAPPDestPath() + fileNameOnly) Then
                _isFileStatusExist = True
                _ParsingInfoWriter = File.AppendText(ScannerCommon.GetGAPPDestPath() + fileNameOnly)
            Else
                parsingInfoFileName = filePath + "\" + _GAPP_TeamName + "_TRANSACTION" + _ID_SERVER_TRANSACTION + "_FileInfoStatus.txt"
                _ParsingInfoWriter = New StreamWriter(parsingInfoFileName)
            End If

        Else
            _ParsingInfoWriter = New StreamWriter(parsingInfoFileName)
        End If

        If Not _isFileStatusExist Then
            _ParsingInfoWriter.WriteLine("ID,MsgTime,Action,Msg")
        End If

        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,---------------------------------------------------------------------------------------------------")
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,PARSING PROCESS started")
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,ScannerParser DLL version " + ScannerCommon.GetDLLVersionOnly())
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,ScannerParser DLL build " + ScannerCommon.GetDLLVersionBuild())
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,System Name: " + System.Net.Dns.GetHostName)
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,Files Selected: " + totalFile.ToString())
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,Client Name: " + ClientName.Replace(",", ""))
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,Market Name: " + MarketName.Replace(",", ""))
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,Campaign: " + Campaign)
        _ParsingInfoWriter.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",1,Enable SIP Decoding: " + EnableDecoding)

        _ParsingInfoWriter.Close()

    End Sub

    Public Sub CloseInfoWriterFinal(ByVal filePath)

        Dim fileName As String = filePath + "\FileInfoStatus.txt"

        Dim fileNameOnly = _GAPP_TeamName + "_TRANSACTION" + _ID_SERVER_TRANSACTION + "_FileInfoStatus.txt"

        If _GAPP_TeamName.Trim <> "" Then
            If File.Exists(ScannerCommon.GetGAPPDestPath() + fileNameOnly) Then
                fileName = ScannerCommon.GetGAPPDestPath() + fileNameOnly
            Else
                fileName = filePath + "\" + _GAPP_TeamName + "_TRANSACTION" + _ID_SERVER_TRANSACTION + "_FileInfoStatus.txt"
            End If

        End If



        Dim count As Integer = -1
        Dim tmpList = System.IO.File.ReadAllLines(fileName).ToList()

        If fileName <> "" Then
            _ParsingInfoWriter = New StreamWriter(fileName)
            For Each tmpStr In tmpList
                count = count + 1
                If count > 0 Then
                    Dim strs As String() = tmpStr.Split(",")
                    Dim resultInt As Integer = 0
                    If Integer.TryParse(strs(0), resultInt) Then
                        tmpStr = tmpStr.Replace(strs(0) + ",", "")
                    End If
                    tmpStr = count.ToString() + "," + tmpStr
                End If
                _ParsingInfoWriter.WriteLine(tmpStr)
            Next
            _ParsingInfoWriter.Close()
        End If

        If _GAPP_TeamName.Trim <> "" Then
            If Not File.Exists(ScannerCommon.GetGAPPDestPath() + fileNameOnly) Then
                If File.Exists(fileName) Then
                    My.Computer.FileSystem.MoveFile(fileName, ScannerCommon.GetGAPPDestPath() + fileNameOnly, True)
                End If

            End If
        End If

    End Sub

    Public Sub SetSourceServer(ByVal SourceServer As String)
        _SourceServer = SourceServer
    End Sub

    Public Function GetSourceServer() As String
        Return _SourceServer
    End Function

    Public Sub SetisRunLogFile(ByVal isRunLogFile As Boolean)
        _isRunLogFile = isRunLogFile
    End Sub

    Public Function isRunLogFile() As Boolean
        Return _isRunLogFile
    End Function

    Public Sub SetLibMode(ByVal libMode As Boolean)
        _isLibMode = libMode
    End Sub

    Public Sub SetGAPPDestPath(ByVal DestPath As String)

        If Not DestPath.EndsWith("\") And DestPath.Trim.Length > 0 Then
            DestPath = DestPath + "\"
        End If
        _GAPPDestPath = DestPath
    End Sub
    Public Function GetGAPPDestPath() As String
        Return _GAPPDestPath
    End Function
    Public Sub SetGAPPSQLConnectionString(ByVal GAPPSQLConnectionString As String)
        _GAPPSQLConnectionString = GAPPSQLConnectionString
    End Sub

    Public Function GetGAPPSQLConnectionString()
        Return _GAPPSQLConnectionString
    End Function


    Public Function GetisSkipEmptyBand() As Boolean
        Return _isSkipEmptyBand
    End Function

    Public Sub SetDatabaseName(ByVal DatabaseName As String)
        _DatabaseName = DatabaseName
    End Sub

    Public Sub SetSQL3KeyValueDotNetDLLPath(ByVal SQL3KeyValueDotNetDLLPath As String)
        _SQL3KeyValueDotNetDLLPath = SQL3KeyValueDotNetDLLPath
    End Sub

    Public _SIBSQLCommand As SqlCommand
    Public _SIBSQLConnection As SqlConnection

    Private _DLLVersion As String
    Private _DLLVersionBuild As String

    Public Sub SetDLLVersionBuild(ByVal DLLVersionBuild As String)
        _DLLVersionBuild = DLLVersionBuild
    End Sub

    Public Sub SetDllVersion(ByVal dllVersion As String)
        _DLLVersion = dllVersion
    End Sub

    Public Sub SetConnectionString(ByVal connectionStr As String)
        _sqlConnectionString = connectionStr
    End Sub

    Public Sub SetConnectionStringGAPP(ByVal connectionStr As String)
        _sqlConnectionStringGAPP = connectionStr
    End Sub

    Public Function GetConnectionStringGAPP() As String
        Return _sqlConnectionStringGAPP
    End Function


    Public Function GetDLLVersion() As String
        'SCNRdll: v17.xx.xx  mm/dd/yyyy h:mm:ss AM/PM        
        Return "SCNRdll: " + _DLLVersion + " " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")
    End Function

    Public Function GetDLLVersionOnly() As String
        Return _DLLVersion
    End Function

    Public Function GetDLLVersionBuild() As String

        Return "SCNRdll: " + _DLLVersion + " " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")
    End Function



    Public Sub SetInitializationPath(ByVal initializationPath As String)
        _initializationPath = initializationPath
    End Sub

    Public FileLogInfoList As List(Of FileLogInfo)

    Public Class FileLogInfo
        Public isGetInputChannel As Boolean
        Public FileName As String
        Public GSM_RSSI_InputChannel As String
        Public GSM_BSIC_InputChannel As String
        Public CDMAInputChannel As String
        Public WCDMAInputChannel As String
        Public LTEInputChannel As String
        Public GSMChannelFound As List(Of String)
        Public WCDMAChannelFound As List(Of String)
        Public CDMAChannelFound As List(Of String)
        Public LTEChannelFound As List(Of String)
        Public GAPPLogFileName As String
        Public ParseTechList As List(Of String)
        Public path As String

    End Class


    Public Class LTESIB1
        Public MCC0 As String = ""
        Public MCC1 As String = ""
        Public MCC2 As String = ""
        Public MNC0 As String = ""
        Public MNC1 As String = ""
        Public MNC2 As String = ""
        Public TAC As String = ""
        Public CI As String = ""
        Public SIB1cellBarred As String = ""
        Public SIB1intraFreqReselection As String = ""
        Public SIB1qRxLevMin As String = ""
        Public SIB1pMax As String = ""
        Public SIB1tddSubFrameAssignment As String = ""
        Public SIB1tddSpecialSubFramePatterns As String = ""
        'SIB1cellBarred|SIB1intraFreqReselection|SIB1qRxLevMin(dbm)|SIB1pMax|SIB1tddSubFrameAssignment|SIB1tddSpecialSubFramePatterns
    End Class

    Public Class CDMASIB
        Public Message As String
        Public Base_ID As String
        Public Base_LAT As String
        Public Base_LONG As String
    End Class

    Public _BandLookUp As Hashtable = Nothing

    Public server3030Path As String
    Public Max_Size_SpectrumScan_Repository As Long
    Public server3040Path As String
    Public Max_Size_SpectrumSxms_Repository As Long
    Public _lstSpectrumScanFiles As List(Of String)


    Public Sub Scanner_WriteLogFile(ByVal myPath As String)

        If IsNothing(WrittenFileLogInfoList) Then
            WrittenFileLogInfoList = New HashSet(Of String)()
        End If
        'Write to LogFile
        Dim i, j As Integer
        Dim FileInfoLog As System.IO.TextWriter

        Dim parserHelper As New ParserHelper

        'If Dir(myPath & "FileInfoLog.txt") <> "" Then
        '    File.Move(myPath & "FileInfoLog.txt", myPath & "FileInfoLog.txt" & Date.Now.ToString.Replace("/", "").Replace(":", ""))
        'End If
        FileInfoLog = File.CreateText(myPath & "FileInfoLog.txt")
        For i = 0 To ScannerCommon.FileLogInfoList.Count - 1

            Dim isWriteBizLTE As Boolean = False
            Dim isLTEInput As Boolean = False
            Dim isWriteBizCDMA As Boolean = False
            Dim isCDMAInput As Boolean = False
            Dim isWriteBizUMTS As Boolean = False
            Dim isUMTSInput As Boolean = False

            If Not IsNothing(ScannerCommon.FileLogInfoList(i).FileName) Then


                Dim LogFile As String = FileLogInfoList(i).FileName.Substring(FileLogInfoList(i).FileName.LastIndexOf("\") + 1, FileLogInfoList(i).FileName.LastIndexOf(".") - FileLogInfoList(i).FileName.LastIndexOf("\") - 1)
                Dim FileNameOnly = FileLogInfoList(i).FileName.Substring(FileLogInfoList(i).FileName.LastIndexOf("\") + 1)
                If _isLibMode Then
                    myPath = ScannerCommon.FileLogInfoList(i).path
                End If


                'Dim GSMLogFile As String = myPath & LogFile & "_PARSE_GSM_FileInfoLog.txt"
                Dim CDMALogFile As String = myPath & LogFile & "_PARSE_CDMA_FileInfoLog.txt"
                Dim UMTSLogFile As String = myPath & LogFile & "_PARSE_UMTS_FileInfoLog.txt"
                Dim LTELogFile As String = myPath & LogFile & "_PARSE_LTE_FileInfoLog.txt"

                Dim CDMALogFileMF As String = myPath & FileNameOnly
                Dim UMTSLogFileMF As String = myPath & FileNameOnly
                Dim LTELogFileMF As String = myPath & FileNameOnly

                Dim GAPPFileInfoLogGSM As System.IO.TextWriter = Nothing
                Dim GAPPFileInfoLogCDMA As System.IO.TextWriter = Nothing
                Dim GAPPFileInfoLogUMTS As System.IO.TextWriter = Nothing
                Dim GAPPFileInfoLogLTE As System.IO.TextWriter = Nothing


                FileInfoLog.WriteLine("-------------------------------------------")
                FileInfoLog.WriteLine(FileNameOnly)

                If _isLibMode Then
                    'GAPPFileInfoLogGSM = File.CreateText(GSMLogFile)
                    If ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("CDMA") Then
                        If Not WrittenFileLogInfoList.Contains(CDMALogFile) Then
                            GAPPFileInfoLogCDMA = File.CreateText(CDMALogFile)
                            GAPPFileInfoLogCDMA.WriteLine("-------------------------------------------")
                            GAPPFileInfoLogCDMA.WriteLine(FileNameOnly)
                        End If

                    End If

                    If ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("UMTS") Then
                        If Not WrittenFileLogInfoList.Contains(UMTSLogFile) Then
                            GAPPFileInfoLogUMTS = File.CreateText(UMTSLogFile)
                            GAPPFileInfoLogUMTS.WriteLine("-------------------------------------------")
                            GAPPFileInfoLogUMTS.WriteLine(FileNameOnly)
                        End If
                    End If

                    If ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("LTE") Then
                        If Not WrittenFileLogInfoList.Contains(LTELogFile) Then
                            GAPPFileInfoLogLTE = File.CreateText(LTELogFile)
                            GAPPFileInfoLogLTE.WriteLine("-------------------------------------------")
                            GAPPFileInfoLogLTE.WriteLine(FileNameOnly)
                        End If

                    End If


                End If

                If ScannerCommon.FileLogInfoList(i).GSM_RSSI_InputChannel <> Nothing Then
                    If ScannerCommon.FileLogInfoList(i).GSM_RSSI_InputChannel.Trim <> "" Then
                        FileInfoLog.WriteLine(ScannerCommon.FileLogInfoList(i).GSM_RSSI_InputChannel.Trim)
                    End If
                End If
                If ScannerCommon.FileLogInfoList(i).GSM_BSIC_InputChannel <> Nothing Then
                    If ScannerCommon.FileLogInfoList(i).GSM_BSIC_InputChannel.Trim <> "" Then
                        FileInfoLog.WriteLine(ScannerCommon.FileLogInfoList(i).GSM_BSIC_InputChannel.Trim)

                    End If
                End If

                If ScannerCommon.FileLogInfoList(i).CDMAInputChannel <> Nothing Then
                    If ScannerCommon.FileLogInfoList(i).CDMAInputChannel.Trim <> "" Then
                        FileInfoLog.WriteLine(ScannerCommon.FileLogInfoList(i).CDMAInputChannel.Trim)
                        If _isLibMode And ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("CDMA") Then

                            If Not WrittenFileLogInfoList.Contains(CDMALogFile) Then
                                isCDMAInput = True
                                GAPPFileInfoLogCDMA.WriteLine(ScannerCommon.FileLogInfoList(i).CDMAInputChannel.Trim)
                            End If

                        End If

                    End If
                End If
                If ScannerCommon.FileLogInfoList(i).WCDMAInputChannel <> Nothing Then
                    If ScannerCommon.FileLogInfoList(i).WCDMAInputChannel.Trim <> "" Then
                        FileInfoLog.WriteLine(ScannerCommon.FileLogInfoList(i).WCDMAInputChannel.Trim)
                        If _isLibMode And ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("UMTS") Then
                            If Not WrittenFileLogInfoList.Contains(UMTSLogFile) Then
                                isUMTSInput = True
                                GAPPFileInfoLogUMTS.WriteLine(ScannerCommon.FileLogInfoList(i).WCDMAInputChannel.Trim)
                            End If

                        End If

                    End If
                End If

                If ScannerCommon.FileLogInfoList(i).LTEInputChannel <> Nothing Then
                    If ScannerCommon.FileLogInfoList(i).LTEInputChannel.Trim <> "" Then
                        FileInfoLog.WriteLine(ScannerCommon.FileLogInfoList(i).LTEInputChannel.Trim)
                        If _isLibMode And ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("LTE") Then
                            If Not WrittenFileLogInfoList.Contains(LTELogFile) Then
                                isLTEInput = True
                                GAPPFileInfoLogLTE.WriteLine(ScannerCommon.FileLogInfoList(i).LTEInputChannel.Trim.Trim)
                            End If

                        End If

                    End If
                End If

                For j = 0 To ScannerCommon.FileLogInfoList(i).GSMChannelFound.Count - 1
                    If ScannerCommon.FileLogInfoList(i).GSMChannelFound(j) <> Nothing Then
                        If ScannerCommon.FileLogInfoList(i).GSMChannelFound(j).IndexOf(":") > 0 Then
                            FileInfoLog.WriteLine(ScannerCommon.FileLogInfoList(i).GSMChannelFound(j).Trim.Trim)
                        End If

                        'If _isLibMode Then
                        '    If ScannerCommon.FileLogInfoList(i).GSMChannelFound(j).IndexOf(":") > 0 Then
                        '        parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(GSMLogFile & "FileInfoLog.txt", ".biz"), Path.GetFileName(ScannerCommon.FileLogInfoList(i).FileName), ScannerCommon.FileLogInfoList(i).GSMChannelFound(j).Substring(ScannerCommon.FileLogInfoList(i).GSMChannelFound(j).IndexOf(":") + 1), String.Empty, String.Empty, String.Empty)
                        '    End If
                        'End If

                    End If
                Next

                Dim tmpChannelFound As List(Of String) = ScannerCommon.FileLogInfoList(i).CDMAChannelFound.Distinct.ToList()

                For j = 0 To tmpChannelFound.Count - 1
                    If tmpChannelFound(j) <> Nothing And tmpChannelFound(j).IndexOf("(") >= 0 Then
                        If tmpChannelFound(j).IndexOf(":") > 0 Then
                            FileInfoLog.WriteLine(tmpChannelFound(j).Trim)
                        End If


                        If _isLibMode And ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("CDMA") Then
                            If Not WrittenFileLogInfoList.Contains(CDMALogFile) Then
                                GAPPFileInfoLogCDMA.WriteLine(tmpChannelFound(j).Trim)
                                If tmpChannelFound(j).IndexOf(":") > 0 Then
                                    WrittenFileLogInfoList.Add(CDMALogFile)
                                    isWriteBizCDMA = True
                                    parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(CDMALogFile, ".biz"), Path.GetFileName(CDMALogFileMF), String.Empty, String.Empty, tmpChannelFound(j).Substring(tmpChannelFound(j).IndexOf(":") + 1), String.Empty)
                                End If

                            End If

                        End If

                    End If

                    If _isLibMode And j = tmpChannelFound.Count - 1 Then
                        If Not WrittenFileLogInfoList.Contains(CDMALogFile) Then
                            If isCDMAInput And Not isWriteBizCDMA Then
                                parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(CDMALogFile, ".biz"), Path.GetFileName(CDMALogFileMF), String.Empty, String.Empty, tmpChannelFound(j).Substring(tmpChannelFound(j).IndexOf(":") + 1), String.Empty)
                            End If
                        End If

                    End If

                Next

                tmpChannelFound = ScannerCommon.FileLogInfoList(i).WCDMAChannelFound.Distinct.ToList()

                For j = 0 To tmpChannelFound.Count - 1
                    If tmpChannelFound(j) <> Nothing And tmpChannelFound(j).IndexOf("(") >= 0 Then
                        If tmpChannelFound(j).IndexOf(":") > 0 Then
                            FileInfoLog.WriteLine(tmpChannelFound(j).Trim)
                        End If


                        If _isLibMode And ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("UMTS") Then
                            If Not WrittenFileLogInfoList.Contains(UMTSLogFile) Then
                                GAPPFileInfoLogUMTS.WriteLine(tmpChannelFound(j).Trim)
                                If tmpChannelFound(j).IndexOf(":") > 0 Then
                                    WrittenFileLogInfoList.Add(UMTSLogFile)
                                    isWriteBizUMTS = True
                                    parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(UMTSLogFile, ".biz"), Path.GetFileName(UMTSLogFileMF), String.Empty, tmpChannelFound(j).Substring(tmpChannelFound(j).IndexOf(":") + 1), String.Empty, String.Empty)
                                End If

                            End If

                        End If



                    End If

                    If _isLibMode And j = tmpChannelFound.Count - 1 Then
                        If isUMTSInput And Not isWriteBizUMTS Then
                            If Not WrittenFileLogInfoList.Contains(UMTSLogFile) Then
                                WrittenFileLogInfoList.Add(UMTSLogFile)
                                parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(UMTSLogFile, ".biz"), Path.GetFileName(UMTSLogFileMF), String.Empty, tmpChannelFound(j).Substring(tmpChannelFound(j).IndexOf(":") + 1), String.Empty, String.Empty)
                            End If

                        End If
                    End If

                Next

                tmpChannelFound = ScannerCommon.FileLogInfoList(i).LTEChannelFound.Distinct.ToList()


                For j = 0 To tmpChannelFound.Count - 1
                    If tmpChannelFound(j) <> Nothing And tmpChannelFound(j).IndexOf("(") >= 0 Then
                        If tmpChannelFound(j).IndexOf(":") > 0 Then
                            FileInfoLog.WriteLine(tmpChannelFound(j).Trim)
                        End If


                        If _isLibMode And ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("LTE") Then
                            If Not WrittenFileLogInfoList.Contains(LTELogFile) Then
                                GAPPFileInfoLogLTE.WriteLine(tmpChannelFound(j).Trim)
                                If tmpChannelFound(j).IndexOf(":") > 0 Then
                                    WrittenFileLogInfoList.Add(LTELogFile)
                                    isWriteBizLTE = True
                                    'LTELogFile: c:\GTPTest\STTicket\Temp\2017-08-23-12-52-09-3030-0032-0001-3030-S_PARSE_LTE_FileInfoLog.txt
                                    parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(LTELogFile, ".biz"), Path.GetFileName(LTELogFileMF), String.Empty, String.Empty, String.Empty, tmpChannelFound(j).Substring(tmpChannelFound(j).IndexOf(":") + 1))
                                End If
                            End If

                        End If

                    End If

                    If _isLibMode And j = tmpChannelFound.Count - 1 Then
                        If isLTEInput And Not isWriteBizLTE Then
                            If Not WrittenFileLogInfoList.Contains(LTELogFile) Then
                                WrittenFileLogInfoList.Add(LTELogFile)
                                parserHelper.ParserTxtBinaryFile(Path.ChangeExtension(LTELogFile, ".biz"), Path.GetFileName(LTELogFileMF), String.Empty, String.Empty, String.Empty, tmpChannelFound(j).Substring(tmpChannelFound(j).IndexOf(":") + 1))
                            End If

                        End If
                    End If


                Next

                If _isLibMode Then

                    If ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("CDMA") Then
                        If Not IsNothing(GAPPFileInfoLogCDMA) Then
                            GAPPFileInfoLogCDMA.Close()
                            GAPPFileInfoLogCDMA.Dispose()
                        End If

                    End If

                    If ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("UMTS") Then
                        If Not IsNothing(GAPPFileInfoLogUMTS) Then
                            GAPPFileInfoLogUMTS.Close()
                            GAPPFileInfoLogUMTS.Dispose()
                        End If

                    End If

                    If ScannerCommon.FileLogInfoList(i).ParseTechList.Contains("LTE") Then
                        If Not IsNothing(GAPPFileInfoLogLTE) Then
                            GAPPFileInfoLogLTE.Close()
                            GAPPFileInfoLogLTE.Dispose()
                        End If

                    End If

                End If

            End If

        Next

        FileInfoLog.Close()

        'ScannerCommon.FileLogInfoList = New List(Of FileLogInfo)

        'If _isLibMode Then
        '    InsertTotblScannerParse()
        'End If


    End Sub

    Public Sub InsertTotblScannerParse(ByVal tblSCNRParseList As List(Of tblSCNRParse))

        Dim propscnrparse As tblSCNRParse
        For Each propscnrparse In tblSCNRParseList
            Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
            dbclass.InserttblSCNRParse(propscnrparse)
            UpdateFTPFile_timescnr(propscnrparse.id_server_file)
        Next

    End Sub


    Private Sub UpdateFTPFile_timescnr(id_server_file As Long)

        Using gappsqlconnection = New SqlConnection
            gappsqlconnection.ConnectionString = ScannerCommon.GetGAPPSQLConnectionString()
            gappsqlconnection.Open()
            Using cmdUpdateFTPFile = New SqlCommand
                cmdUpdateFTPFile.Connection = gappsqlconnection
                cmdUpdateFTPFile.CommandText = "Update [tblftpfile] set time_scnr = getdate() where id_server_file = @id_server_file"
                cmdUpdateFTPFile.Parameters.AddWithValue("@id_server_file", id_server_file)
                cmdUpdateFTPFile.ExecuteNonQuery()
            End Using
        End Using
    End Sub



    Public Function GetFileInputList(ByVal TechType As String, ByVal fileList As GAPPData()) As List(Of Scanner.ScannerFileInputObject)
        Dim fileInputObjList As New List(Of Scanner.ScannerFileInputObject)

        For Each tmpFileName In fileList
            If Not IsNothing(tmpFileName.FileName) Then
                Dim tmpTime As String = tmpFileName.FileName.Substring(tmpFileName.FileName.LastIndexOf("\") + 1)
                Dim date1 = DateTime.ParseExact(tmpTime.Substring(0, "2017-03-21-14-10-10".Length), "yyyy-MM-dd-HH-mm-ss", Nothing)

                Dim fileInputObj = New Scanner.ScannerFileInputObject()
                fileInputObj.FileName = tmpFileName.FileName
                fileInputObj.id_server_transaction = tmpFileName.id_server_transaction
                fileInputObj.id_server_file = tmpFileName.id_server_file
                fileInputObj.file_path = tmpFileName.file_path
                fileInputObj.status_flag = tmpFileName.status_flag
                fileInputObj.FileMergeName = ""
                fileInputObj.Campaign = tmpFileName.Campaign
                fileInputObj.MarketName = tmpFileName.MarketName
                fileInputObj.ClientName = tmpFileName.ClientName
                fileInputObj.Team = tmpFileName.Team

                'C:\GTPTest\STTicket\ST _ 1046\2017-04-07-09-36-00-3030-0018-0001-3030-S.mf
                If TechType = "LTE" Then
                    For Each fileMerge In fileList
                        If Not IsNothing(fileMerge.FileName) Then
                            If fileMerge.FileName <> tmpFileName.FileName Then
                                Dim tmpTime2 As String = fileMerge.FileName.Substring(fileMerge.FileName.LastIndexOf("\") + 1)

                                Dim date2 = DateTime.ParseExact(tmpTime2.Substring(0, "2017-03-21-14-10-10".Length), "yyyy-MM-dd-HH-mm-ss", Nothing)

                                If (Math.Abs((date1 - date2).TotalSeconds) < 150) And (Mid(tmpTime, 26, 4) = Mid(tmpTime2, 26, 4)) Then
                                    If ScannerCommon.Get_Scanner_ModuleID_List().Count > 0 Then
                                        Dim Scanner_ModuleID_List As List(Of ScannerCommon.Scanner_ModuleID) = ScannerCommon.Get_Scanner_ModuleID_List()
                                        For Each tmpModule In Scanner_ModuleID_List
                                            If (tmpFileName.FileName.ToUpper().IndexOf(tmpModule.MergePattern1) > 0) And fileMerge.FileName.ToUpper().IndexOf(tmpModule.MergePattern2) > 0 Then
                                                fileInputObj.FileMergeName = fileMerge.FileName
                                                fileInputObj.MergePattern1 = tmpModule.MergePattern1
                                                fileInputObj.MergePattern2 = tmpModule.MergePattern2

                                            End If
                                            If tmpFileName.FileName.ToUpper().IndexOf(tmpModule.MergePattern2) > 0 And (fileMerge.FileName.ToUpper().IndexOf(tmpModule.MergePattern1) > 0) Then
                                                fileInputObj.FileMergeName = fileMerge.FileName
                                                fileInputObj.isSkip = True
                                                fileInputObj.MergePattern1 = tmpModule.MergePattern1
                                                fileInputObj.MergePattern2 = tmpModule.MergePattern2
                                            End If
                                        Next

                                    Else
                                        If (tmpFileName.FileName.ToUpper().IndexOf("-3040-S") > 0 Or tmpFileName.FileName.ToUpper().IndexOf("-3041-S") > 0) And fileMerge.FileName.ToUpper().IndexOf("-3030-S") > 0 Then
                                            fileInputObj.FileMergeName = fileMerge.FileName
                                        End If
                                        If tmpFileName.FileName.ToUpper().IndexOf("-3030-S") > 0 And (fileMerge.FileName.ToUpper().IndexOf("-3040-S") > 0 Or fileMerge.FileName.ToUpper().IndexOf("-3041-S") > 0) Then
                                            fileInputObj.FileMergeName = fileMerge.FileName
                                            fileInputObj.isSkip = True
                                        End If
                                    End If
                                    Exit For
                                End If
                            End If
                        End If


                    Next
                End If
                fileInputObjList.Add(fileInputObj)
            End If

        Next

        Return fileInputObjList.OrderBy(Function(x) x.FileName).ToList()

    End Function

    Public Function GetBand() As Hashtable
        Dim _BandLookUpList As New Hashtable()
        _isSkipEmptyBand = True
        Try
            Using sr As New StreamReader("band_look_up.csv")
                Dim line As String
                line = sr.ReadLine()
                Do While sr.Peek() >= 0
                    line = sr.ReadLine()
                    If line.ToUpper().IndexOf("Else") >= 0 Then
                        _isSkipEmptyBand = False
                    End If
                    Dim strs As String() = line.Split(", ")

                    Dim tmpInt As Integer
                    If Integer.TryParse(strs(0), tmpInt) Then
                        If Not _BandLookUpList.ContainsKey(tmpInt) Then
                            _BandLookUpList.Add(tmpInt, strs(1) + "|" + strs(2) + "|" + strs(3) + "|" + strs(4))

                        End If

                    End If
                Loop

            End Using
        Catch ex As Exception

        End Try

        Return _BandLookUpList

    End Function


    Private Function CheckDatabase() As String


        Try
            If _DatabaseName = "" Then
                Return "Database name Is empty !"
            End If
            'IF NOT EXISTS (SELECT name  FROM master.sys.server_principals WHERE name = 'LENOVO-PC\thanh') BEGIN CREATE LOGIN [LENOVO-PC\thanh] FROM WINDOWS; EXEC master..sp_addsrvrolemember @loginame= 'LENOVO-PC\thanh', @rolename = 'sysadmin';  END 

            'Dim createLogIncmd As String = "IF Not EXISTS (SELECT name  FROM master.sys.server_principals WHERE name = '" + My.User.Name + "') BEGIN CREATE LOGIN [" + My.User.Name + "] FROM WINDOWS; EXEC master..sp_addsrvrolemember @loginame= '" + My.User.Name + "', @rolename = 'sysadmin';  END "
            'Dim createLogIncmd As String = "IF NOT EXISTS (SELECT name  FROM master.sys.server_principals WHERE name = '" + My.User.Name + "') BEGIN CREATE LOGIN [" + My.User.Name + "] FROM WINDOWS; ALTER ROLE sysadmin ADD MEMBER [" + My.User.Name + "];  END "

            Dim dr As SqlDataReader


            Dim command = New SqlCommand()
            Dim connection = New SqlConnection()
            connection.ConnectionString = _sqlConnectionString
            connection.Open()


            command = New SqlCommand()
            command.Connection = connection
            command.CommandText = "SELECT * from sys.databases WHERE name = '" + _DatabaseName + "'"
            command.Connection = connection

            dr = command.ExecuteReader()
            Dim isExist As Boolean = True
            If Not dr.HasRows Then
                isExist = False
            End If
            dr.Close()
            command.Dispose()
            If Not isExist Then
                'Create new database
                Dim cmd As New SqlCommand()
                cmd.Connection = connection
                cmd.CommandText = "CREATE DATABASE " + _DatabaseName
                cmd.ExecuteNonQuery()
                cmd.Dispose()

                cmd = New SqlCommand()
                cmd.Connection = connection
                cmd.CommandText = "ALTER AUTHORIZATION ON DATABASE::" + _DatabaseName + " TO sa"
                cmd.ExecuteNonQuery()
                cmd.Dispose()


            End If

            _sqlConnectionString = _sqlConnectionString.Replace("master", _DatabaseName)

            connection.Close()

            Return ""
        Catch ex As Exception

            Return "SQL Account " + My.User.Name + " does not exist." + Environment.NewLine _
                      & "To parse scanner data, you will need to create this SQL credential And assign the sysadmin role, as follows: " + Environment.NewLine _
                      & "1. Open SQL Management Studio with ‘sa’ credentials (NOT Windows Authentication credentials) " + Environment.NewLine _
                      & "2. Go to Security -> Logins and right-click ‘New Login…’" + Environment.NewLine _
                      & "3. Enter Login name = " & My.User.Name & Environment.NewLine _
                      & "4. Select ‘Server Roles’ in left pane, and check the box for ‘sysadmin’" + Environment.NewLine _
                      & "5. Click OK"

        End Try

        Return ""

    End Function


    Public Function DatabaseInitialization(ByVal FileName As String) As Boolean

        Dim msg As String = CheckDatabase()

        If msg <> "" Then
            If Not _isLibMode Then
                MessageBox.Show(msg)
            End If

            Return False
        End If
        Try
            Dim strSql As String

            _SIBSQLConnection = New SqlConnection()
            _SIBSQLCommand = New SqlCommand()


            _SIBSQLConnection.ConnectionString = _sqlConnectionString
            _SIBSQLCommand.Connection = _SIBSQLConnection
            _SIBSQLConnection.Open()

            '-----------------------------------------------------------------------------------------------------------------------
            '-- Set the TRUSTWORTHY property of the DB
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "DECLARE @dbName varchar(200) " &
                     "select @dbName = DB_NAME() " &
                     "DECLARE @q varchar(200) " &
                     "SET @q = 'Alter Database ' + @dbName + ' SET TRUSTWORTHY ON' " &
                     "EXEC(@q)"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()

            '-----------------------------------------------------------------------------------------------------------------------
            '-- If SQKeyValueUnload procedure exists, then execute SQKeyValueUnload, modified by Alan/Jigar
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQKeyValueUnload]') and xtype in (N'FS', N'PC')) exec [dbo].[SQKeyValueUnload] "
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()

            '-----------------------------------------------------------------------------------------------------------------------
            '-- Delete previously created procedures
            '-----------------------------------------------------------------------------------------------------------------------

            strSql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQGSMKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQGSMKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQUMTSKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQUMTSKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQLTERRCKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQLTERRCKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQLTENASKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQLTENASKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQCDMAKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQCDMAKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQEVDOKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQEVDOKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQCDMAScannerKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQCDMAScannerKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQEVDOScannerKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQEVDOScannerKeyValue] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQKeyValueInit]') and xtype in (N'FS', N'PC')) drop procedure [dbo].[SQKeyValueInit] " &
                    "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQKeyValueUnload]') and xtype in (N'FS', N'PC')) drop procedure [dbo].[SQKeyValueUnload] " &
                    "if exists (select * from sys.assemblies where name = 'SQL3KeyValueDotNet') drop Assembly SQL3KeyValueDotNet"

            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '-----------------------------------------------------------------------------------------------------------------------
            '-- Set path to the installation directory of the Layer3 Key Value DLL
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "DECLARE @dllPath varchar(200) " &
                     "SET @dllPath = '<<MYCURRENTFOLDER>>' " &
                     "Select @dllPath as Path into #tmpPathTable"
            strSql = strSql.Replace("<<MYCURRENTFOLDER>>", _SQL3KeyValueDotNetDLLPath)
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '-----------------------------------------------------------------------------------------------------------------------
            '-- Bind DLL to Database
            '-- Update path with the correct location of the dll
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "DECLARE @dllPath varchar(200) " &
                     "Select @dllPath = Path from #tmpPathTable " &
                     "CREATE ASSEMBLY [SQL3KeyValueDotNet] " &
                     "FROM @dllPath + '\SQL3KeyValueDotNet_v2.dll' " &
                     "WITH PERMISSION_SET = UNSAFE"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '-----------------------------------------------------------------------------------------------------------------------
            '-- Create functions
            '-----------------------------------------------------------------------------------------------------------------------
            '-----------------------------------------------------------------------------------------------------------------------
            '-----------------------------------------------------------------------------------------------------------------------
            '-- Initialization
            '-- Parameters: @Path:        Path to where the SQL3KeyValueDotNet_v2.dll (@Path does not contain the file name!)
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "CREATE PROCEDURE SQKeyValueInit @Path nvarchar(max) AS " &
                     "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQKeyValueInit"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '-----------------------------------------------------------------------------------------------------------------------
            '-- Unloading dll
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "CREATE PROCEDURE SQKeyValueUnload AS " &
                     "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQKeyValueUnload"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '-----------------------------------------------------------------------------------------------------------------------
            '-- LTE RRC KeyValue
            '-- Parameters: @Message:     RRC message (field Msg of table LTERRCMessages)
            '--             @ChnType:     Channel type (field ChnType of table LTERRCMessages)
            '--             @KeyValue:    string searching for in the decoded message
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "CREATE FUNCTION  dbo.SQLTERRCKeyValue(@Message nvarchar(max), @ChanType int, @KeyValue nvarchar(max)) " &
                     "RETURNS nvarchar(max) AS " &
                     "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQLTERRCGetKeyValue"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '            -----------------------------------------------------------------------------------------------------------------------
            '-- CDMA TSMx Scanner KeyValue
            '-- Parameters: @Type:        CDMA L3 sib type (field SIBType of table MsgWCDMAScannerSIB)
            '--             @Message:     CDMA L3 message (field Message of table MsgWCDMAScannerSIB)
            '--             @KeyValue:    string searching for in the decoded message
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "CREATE FUNCTION  dbo.SQCDMAScannerKeyValue(@sibType nvarchar(max), @Message nvarchar(max), @KeyValue nvarchar(max)) " &
                    "RETURNS nvarchar(max) AS " &
                     "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQCDMAScannerGetKeyValue"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '            -----------------------------------------------------------------------------------------------------------------------
            '-----------------------------------------------------------------------------------------------------------------------
            '-- EVDO TSMx Scanner KeyValue
            '-- Parameters: @Type:        EVDO L3 sib type (field SIBType of table MsgWCDMAScannerSIB)
            '--             @Message:     EVDO L3 message (field Message of table MsgWCDMAScannerSIB)
            '--             @KeyValue:    string searching for in the decoded message
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "CREATE FUNCTION  dbo.SQEVDOScannerKeyValue(@sibType nvarchar(max), @Message nvarchar(max), @KeyValue nvarchar(max)) " &
                    "RETURNS nvarchar(max) AS " &
                  "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQEVDOScannerGetKeyValue "
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()


            '-----------------------------------------------------------------------------------------------------------------------
            '-- Calling the SQKeyValueInit
            '-- This is necessary so the L3 DLL is loaded and can be used by the above functions.
            '-- !!Important!!
            '-- This function must be called everytime the SQL Server is restarted for every DB where the assembly is loaded!
            '-- Example
            '-- SQKeyValueInit 'C:\Program Files\SwissQual\Diversity\L3KeyValue'
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "DECLARE @dllPath varchar(200) " &
                     "Select @dllPath = Path from #tmpPathTable " &
                     "EXEC SQKeyValueInit @dllPath"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()



            '-----------------------------------------------------------------------------------------------------------------------
            '-- Drop temp table
            '-----------------------------------------------------------------------------------------------------------------------
            strSql = "DROP TABLE #tmpPathTable"
            _SIBSQLCommand.CommandText = strSql
            _SIBSQLCommand.ExecuteNonQuery()

            Return True
        Catch ex As Exception
            MessageBox.Show("SQL Account " + My.User.Name + " does not exist." + Environment.NewLine _
                      & "To parse scanner data, you will need to create this SQL credential And assign the sysadmin role, as follows: " + Environment.NewLine _
                      & "1. Open SQL Management Studio with ‘sa’ credentials (NOT Windows Authentication credentials) " + Environment.NewLine _
                      & "2. Go to Security -> Logins and right-click ‘New Login…’" + Environment.NewLine _
                      & "3. Enter Login name = " & My.User.Name & Environment.NewLine _
                      & "4. Select ‘Server Roles’ in left pane, and check the box for ‘sysadmin’" + Environment.NewLine _
                      & "5. Click OK")

            Return False
        End Try


    End Function


    'Public Function DatabaseInitialization(ByVal FileName As String) As Boolean

    '    Dim msg As String = CheckDatabase()

    '    If msg <> "" Then
    '        If Not _isLibMode Then
    '            MessageBox.Show(msg)
    '        End If

    '        Return False
    '    End If
    '    Try
    '        Dim strSql As String

    '        _SIBSQLConnection = New SqlConnection()
    '        _SIBSQLCommand = New SqlCommand()


    '        _SIBSQLConnection.ConnectionString = _sqlConnectionString
    '        _SIBSQLCommand.Connection = _SIBSQLConnection
    '        _SIBSQLConnection.Open()

    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Set the TRUSTWORTHY property of the DB
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "DECLARE @dbName varchar(200) " &
    '                 "select @dbName = DB_NAME() " &
    '                 "DECLARE @q varchar(200) " &
    '                 "SET @q = 'Alter Database ' + @dbName + ' SET TRUSTWORTHY ON' " &
    '                 "EXEC(@q)"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()

    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- If SQKeyValueUnload procedure exists, then execute SQKeyValueUnload, modified by Alan/Jigar
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQKeyValueUnload]') and xtype in (N'FS', N'PC')) exec [dbo].[SQKeyValueUnload] "
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()

    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Delete previously created procedures
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQLTERRCKeyValue]') and xtype in (N'FS', N'FN')) drop function [dbo].[SQLTERRCKeyValue] " &
    '                 "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQKeyValueInit]') and xtype in (N'FS', N'PC')) drop procedure [dbo].[SQKeyValueInit] " &
    '                 "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SQKeyValueUnload]') and xtype in (N'FS', N'PC')) drop procedure [dbo].[SQKeyValueUnload] " &
    '                 "if exists (select * from sys.assemblies where name = 'SQL3KeyValueDotNet') drop Assembly SQL3KeyValueDotNet"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()


    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Set path to the installation directory of the Layer3 Key Value DLL
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "DECLARE @dllPath varchar(200) " &
    '                 "SET @dllPath = '<<MYCURRENTFOLDER>>' " &
    '                 "Select @dllPath as Path into #tmpPathTable"
    '        strSql = strSql.Replace("<<MYCURRENTFOLDER>>", _SQL3KeyValueDotNetDLLPath)
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()


    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Bind DLL to Database
    '        '-- Update path with the correct location of the dll
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "DECLARE @dllPath varchar(200) " &
    '                 "Select @dllPath = Path from #tmpPathTable " &
    '                 "CREATE ASSEMBLY [SQL3KeyValueDotNet] " &
    '                 "FROM @dllPath + '\SQL3KeyValueDotNet_v2.dll' " &
    '                 "WITH PERMISSION_SET = UNSAFE"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()


    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Create functions
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Initialization
    '        '-- Parameters: @Path:        Path to where the SQL3KeyValueDotNet_v2.dll (@Path does not contain the file name!)
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "CREATE PROCEDURE SQKeyValueInit @Path nvarchar(max) AS " &
    '                 "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQKeyValueInit"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()


    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Unloading dll
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "CREATE PROCEDURE SQKeyValueUnload AS " &
    '                 "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQKeyValueUnload"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()


    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- LTE RRC KeyValue
    '        '-- Parameters: @Message:     RRC message (field Msg of table LTERRCMessages)
    '        '--             @ChnType:     Channel type (field ChnType of table LTERRCMessages)
    '        '--             @KeyValue:    string searching for in the decoded message
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "CREATE FUNCTION  dbo.SQLTERRCKeyValue(@Message nvarchar(max), @ChanType int, @KeyValue nvarchar(max)) " &
    '                 "RETURNS nvarchar(max) AS " &
    '                 "EXTERNAL NAME SQL3KeyValueDotNet.[SQL3KeyValueDotNet_v2.SQL3KeyValueDotNet_v2].SQLTERRCGetKeyValue"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()

    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Calling the SQKeyValueInit
    '        '-- This is necessary so the L3 DLL is loaded and can be used by the above functions.
    '        '-- !!Important!!
    '        '-- This function must be called everytime the SQL Server is restarted for every DB where the assembly is loaded!
    '        '-- Example
    '        '-- SQKeyValueInit 'C:\Program Files\SwissQual\Diversity\L3KeyValue'
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "DECLARE @dllPath varchar(200) " &
    '                 "Select @dllPath = Path from #tmpPathTable " &
    '                 "EXEC SQKeyValueInit @dllPath"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()



    '        '-----------------------------------------------------------------------------------------------------------------------
    '        '-- Drop temp table
    '        '-----------------------------------------------------------------------------------------------------------------------
    '        strSql = "DROP TABLE #tmpPathTable"
    '        _SIBSQLCommand.CommandText = strSql
    '        _SIBSQLCommand.ExecuteNonQuery()

    '        Return True
    '    Catch ex As Exception
    '        MessageBox.Show("SQL Account " + My.User.Name + " does not exist." + Environment.NewLine _
    '                  & "To parse scanner data, you will need to create this SQL credential And assign the sysadmin role, as follows: " + Environment.NewLine _
    '                  & "1. Open SQL Management Studio with ‘sa’ credentials (NOT Windows Authentication credentials) " + Environment.NewLine _
    '                  & "2. Go to Security -> Logins and right-click ‘New Login…’" + Environment.NewLine _
    '                  & "3. Enter Login name = " & My.User.Name & Environment.NewLine _
    '                  & "4. Select ‘Server Roles’ in left pane, and check the box for ‘sysadmin’" + Environment.NewLine _
    '                  & "5. Click OK")

    '        Return False
    '    End Try


    'End Function

    Public Function GetCDMASIB(ByVal input As String) As CDMASIB

        Dim CDMASIBOuput As New CDMASIB()


        Dim commandText As String =
        "DECLARE @CDMAL3val varchar(250) " &
         " SET @CDMAL3val = '" + input + "'" &
        " select 'CDMA L3 Paging' as Message, " &
        " dbo.SQCDMAScannerKeyValue(1007, @CDMAL3val, 'Paging Channel Message;PDU Format;PDU with no Extended-Encryption Fields included;Broadcast Channel Message PDU;SDU;SPM;BASE_ID')[Base_ID]," &
       " dbo.SQCDMAScannerKeyValue(1007, @CDMAL3val, 'Paging Channel Message;PDU Format;PDU with no Extended-Encryption Fields included;Broadcast Channel Message PDU;SDU;SPM;BASE_LAT')[Base_LAT]," &
       " dbo.SQCDMAScannerKeyValue(1007, @CDMAL3val, 'Paging Channel Message;PDU Format;PDU with no Extended-Encryption Fields included;Broadcast Channel Message PDU;SDU;SPM;BASE_LONG')[Base_LONG]"
        _SIBSQLCommand.CommandText = commandText

        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            CDMASIBOuput.Message = Convert.ToString(dr("Message"))
            CDMASIBOuput.Base_ID = Convert.ToString(dr("Base_ID"))
            CDMASIBOuput.Base_LAT = Convert.ToString(dr("Base_LAT")).Split(" ")(0)
            CDMASIBOuput.Base_LONG = Convert.ToString(dr("Base_LONG")).Split(" ")(0)
        End While
        dr.Close()

        Return CDMASIBOuput

    End Function

    Public Function GetSIB1(ByVal input As String) As LTESIB1
        Dim SIB1Output As New LTESIB1()

        Dim commandText As String =
        "DECLARE @SIB1val varchar(250) " &
         " SET @SIB1val = '" + input + "'" &
        "   select 'LTE SIB1' as Message, " &
        "(dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;plmn_IdentityList;[0] plmn_IdentityList;element;plmn_Identity;mcc;MCC_MNC_Digit [0]'))[MCC0]," &
       "(dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;plmn_IdentityList;[0] plmn_IdentityList;element;plmn_Identity;mcc;MCC_MNC_Digit [1]'))[MCC1]," &
       "(dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;plmn_IdentityList;[0] plmn_IdentityList;element;plmn_Identity;mcc;MCC_MNC_Digit [2]'))[MCC2]," &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;plmn_IdentityList;[0] plmn_IdentityList;element;plmn_Identity;mnc;MCC_MNC_Digit [0]'))[MNC0]," &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;plmn_IdentityList;[0] plmn_IdentityList;element;plmn_Identity;mnc;MCC_MNC_Digit [1]'))[MNC1]," &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;plmn_IdentityList;[0] plmn_IdentityList;element;plmn_Identity;mnc;MCC_MNC_Digit [2]'))[MNC2]," &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;trackingAreaCode'))[TAC_hex], " &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;cellIdentity;CI'))[CI], " &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;cellBarred'))[SIB1cellBarred]," &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellAccessRelatedInfo;intraFreqReselection'))[SIB1intraFreqReselection]," &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;cellSelectionInfo;q_RxLevMin')*2)[SIB1qRxLevMin(dbm)], " &
        " (dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;p_Max'))[SIB1pMax], " &
        "(dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;tdd_Config;subframeAssignment'))[SIB1tddSubFrameAssignment], " &                 'added by My 05/31/2018 #1277
        "(dbo.SQLTERRCKeyValue(@SIB1val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformationBlockType1;tdd_Config;specialSubframePatterns'))[SIB1tddSpecialSubFramePatterns] "          'added by My 05/31/2018 #1277
        _SIBSQLCommand.CommandText = commandText

        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            SIB1Output.MCC0 = Convert.ToString(dr("MCC0"))
            SIB1Output.MCC1 = Convert.ToString(dr("MCC1"))
            SIB1Output.MCC2 = Convert.ToString(dr("MCC2"))
            SIB1Output.MNC0 = Convert.ToString(dr("MNC0"))
            SIB1Output.MNC1 = Convert.ToString(dr("MNC1"))
            SIB1Output.MNC2 = Convert.ToString(dr("MNC2"))

            SIB1Output.TAC = Convert.ToString(dr("TAC_hex"))
            SIB1Output.CI = Convert.ToString(dr("CI"))
            SIB1Output.SIB1cellBarred = Convert.ToString(dr("SIB1cellBarred"))
            SIB1Output.SIB1intraFreqReselection = Convert.ToString(dr("SIB1intraFreqReselection"))
            SIB1Output.SIB1qRxLevMin = Convert.ToString(dr("SIB1qRxLevMin(dbm)"))
            SIB1Output.SIB1pMax = Convert.ToString(dr("SIB1pMax"))
            SIB1Output.SIB1tddSubFrameAssignment = Convert.ToString(dr("SIB1tddSubFrameAssignment"))
            SIB1Output.SIB1tddSpecialSubFramePatterns = Convert.ToString(dr("SIB1tddSpecialSubFramePatterns"))


        End While
        dr.Close()

        Return SIB1Output

    End Function

    Public Function GetSIB2(ByVal input As String) As String
        Dim SIB2 As String = ""
        Dim tmpString As String = "SIB2RAPreambles|SIB2PwrRampStep|SIB2InitRecdTrgPwr|SIB2PreambleTransMax|SIB2RARespWinSize|SIB2MACContResTimer|SIB2maxHARQMsg3Tx|SIB2PDSCHRefSigPwr|SIB2PDSCHPb|SIB2SRSbwConfig|SIB2SRSsubframeConfig|SIB2ACKNACKsimultTx|SIB2P0NominalPUSCH|SIB2Alpha|SIB2P0NomPUCCH|SIB2t300|SIB2t301|SIB2t310|SIB2n310|SIB2t311|SIB2n311"



        Dim commandText As String = "DECLARE @SIB2val varchar(250)" &
        "SET @SIB2val = '" & input & "'" &
        " select 'LTE SIB2' as Message, " &
        " dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;preambleInfo;numberOfRA_Preambles')SIB2RAPreambles," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;powerRampingParameters;powerRampingStep')SIB2PwrRampStep," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;powerRampingParameters;preambleInitialReceivedTargetPower')SIB2InitRecdTrgPwr," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;ra_SupervisionInfo;preambleTransMax')SIB2PreambleTransMax," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;ra_SupervisionInfo;ra_ResponseWindowSize')SIB2RARespWinSize," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;ra_SupervisionInfo;mac_ContentionResolutionTimer')SIB2MACContResTimer," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;rach_ConfigCommon;maxHARQ_Msg3Tx')SIB2maxHARQMsg3Tx," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;pdsch_ConfigCommon;referenceSignalPower')SIB2PDSCHRefSigPwr," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;pdsch_ConfigCommon;p_b')SIB2PDSCHPb," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;soundingRS_UL_ConfigCommon;setup;srs_BandwidthConfig')SIB2SRSbwConfig," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;soundingRS_UL_ConfigCommon;setup;srs_SubframeConfig')SIB2SRSsubframeConfig," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;soundingRS_UL_ConfigCommon;setup;ackNackSRS_SimultaneousTransmission')SIB2ACKNACKsimultTx," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;uplinkPowerControlCommon;p0_NominalPUSCH')SIB2P0NominalPUSCH," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;uplinkPowerControlCommon;alpha')SIB2Alpha," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;radioResourceConfigCommon;uplinkPowerControlCommon;p0_NominalPUCCH')SIB2P0NomPUCCH," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;ue_TimersAndConstants;t300')SIB2t300," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;ue_TimersAndConstants;t301')SIB2t301," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;ue_TimersAndConstants;t310')SIB2t310," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;ue_TimersAndConstants;n310')SIB2n310," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;ue_TimersAndConstants;t311')SIB2t311," &
        "dbo.SQLTERRCKeyValue(@SIB2val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib2;ue_TimersAndConstants;n311')SIB2n311"

        _SIBSQLCommand.CommandText = commandText
        Dim fields As String() = tmpString.Split("|")
        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            For Each field In fields
                SIB2 = SIB2 & Convert.ToString(dr(field)) & "|"
            Next
        End While

        dr.Close()

        Return SIB2.Substring(0, SIB2.Length - 1).Replace(",", ";")


    End Function

    Public Function GetSIB3(ByVal input As String) As String
        Dim SIB3 As String = ""
        Dim tmpString As String = "SIB3qHyst|SIB3sNonIntraSearch(db)|SIB3ThreshServLow(db)|SIB3cellReselPriority|SIB3qRxLevMin(dbm)|SIB3pMax(dbm)|SIB3sIntraSearch(db)"
        Dim commandText As String = "DECLARE @SIB3val varchar(250)" &
        "SET @SIB3val = '" & input & "'" &
        "select 'LTE SIB3' as Message, " &
        "dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;cellReselectionInfoCommon;q_Hyst')SIB3qHyst," &
        "dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;cellReselectionServingFreqInfo;s_NonIntraSearch')[SIB3sNonIntraSearch(db)]," &
        "(dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;cellReselectionServingFreqInfo;threshServingLow')*2)[SIB3ThreshServLow(db)]," &
        "dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;cellReselectionServingFreqInfo;cellReselectionPriority')SIB3cellReselPriority," &
        "(dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;intraFreqCellReselectionInfo;q_RxLevMin')*2)[SIB3qRxLevMin(dbm)]," &
        "dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;intraFreqCellReselectionInfo;p_Max')[SIB3pMax(dbm)]," &
        "dbo.SQLTERRCKeyValue(@SIB3val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib3;intraFreqCellReselectionInfo;s_IntraSearch')[SIB3sIntraSearch(db)]"

        _SIBSQLCommand.CommandText = commandText
        Dim fields As String() = tmpString.Split("|")
        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            For Each field In fields
                SIB3 = SIB3 & Convert.ToString(dr(field)) & "|"
            Next
        End While

        dr.Close()

        Return SIB3.Substring(0, SIB3.Length - 1).Replace(",", ";")
    End Function


    Public Function GetSIB4(ByVal input As String) As String
        Dim SIB4 As String = ""
        Dim tmpString As String = "SIB4IntraFreqCellList|SIB4PCIs|SIB4OffsetPCIs|SIB4BlackCellList|SIB4BlackCells"

        Dim commandText As String = "DECLARE @SIB4val varchar(250)" &
            "SET @SIB4val = '" & input & "'" &
           "select 'LTE SIB4' as Message," &
          "dbo.SQLTERRCKeyValue(@SIB4val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[x] sib_TypeAndInfo;element;sib4;intraFreqNeighCellList')[SIB4IntraFreqCellList]," &
          "dbo.SQLTERRCKeyValue(@SIB4val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib4;intraFreqNeighCellList;[x] intraFreqNeighCellList;element;physCellId')[SIB4PCIs]," &
          "dbo.SQLTERRCKeyValue(@SIB4val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib4;intraFreqNeighCellList;[x] intraFreqNeighCellList;element;q_OffsetCell')[SIB4OffsetPCIs]," &
          "dbo.SQLTERRCKeyValue(@SIB4val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[x] sib_TypeAndInfo;element;sib4;intraFreqBlackCellList')[SIB4BlackCellList]," &
          "dbo.SQLTERRCKeyValue(@SIB4val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib4;intraFreqBlackCellList;[x] intraFreqBlackCellList;element;start')[SIB4BlackCells]"

        _SIBSQLCommand.CommandText = commandText


        Dim fields As String() = tmpString.Split("|")
        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            For Each field In fields
                SIB4 = SIB4 & Convert.ToString(dr(field)) & "|"
            Next
        End While

        dr.Close()

        Return SIB4.Substring(0, SIB4.Length - 1).Replace(",", ";")
    End Function

    Public Function GetLTEBandByChannel(ByVal channel As Integer) As String
        Dim band As String
        Select Case channel
            Case 5000 To 5179, 5730 To 5849, 9660 To 9769 : band = "700"
            Case 5180 To 5379 : band = "700"
            Case 8690 To 8790, 9110 To 9210 : band = "850"
            Case 2400 To 2510, 2610 To 2624 : band = "850"
            Case 2511 To 2609, 2625 To 2649 : band = "850"
                'Case 600 To 749, 8065 To 8165 : band = "1900A"
                'Case 750 To 799, 8215 : band = "1900D"
                'Case 800 To 949, 8265 To 8365 : band = "1900B"
                'Case 950 To 999, 8415 : band = "1900E"
                'Case 1000 To 1049, 8465 : band = "1900F"
                'Case 1050 To 1199, 8515 To 8615 : band = "1900C"
                'Case 8665 : band = "1900G"
            Case 600 To 749, 8040 To 8189 : band = "1900"
            Case 750 To 799, 8190 To 8239 : band = "1900"
            Case 800 To 949, 8240 To 8389 : band = "1900"
            Case 950 To 999, 8390 To 8439 : band = "1900"
            Case 1000 To 1049, 8440 To 8489 : band = "1900"
            Case 1050 To 1199, 8490 To 8639 : band = "1900"
            Case 8640 To 8689 : band = "1900"                  'change 8690 to 8689    v16.2.7.sid
            Case 1 To 599 : band = "2100"
            Case 1950 To 2049 : band = "2100"
            Case 2050 To 2149 : band = "2100"
            Case 2150 To 2199 : band = "2100"
            Case 2200 To 2249 : band = "2100"
            Case 2250 To 2299 : band = "2100"
            Case 2300 To 2399 : band = "2100"
                'Case 7500 To 7599, 7600 To 7699 : band = "2000S"
            Case 7500 To 7599 : band = "2100"
            Case 7600 To 7699 : band = "2100"
            Case 38800 To 38850 : band = "2300"
            Case 38851 To 39099 : band = "2300"
            Case 39100 To 39150 : band = "2300"
            Case 9770 To 9819 : band = "2300"
            Case 9820 To 9869 : band = "2300"
                'Case 39650 To 41589, 39650 To 41589, 37750 To 38249 : band = "2500"
            Case 39650 To 41589, 37750 To 38249 : band = "2500"
            Case 1200 To 1949 : band = "1800"
            Case 2750 To 3449 : band = "2600"
            Case 3450 To 3799 : band = "900"
                '---------------------v16.2.9.sid-------------------------------------------
            Case 4150 To 4249 : band = "2100"             'US AWS band, Band 10
            Case 4250 To 4349 : band = "2100"             'US AWS band, Band 10
            Case 4350 To 4399 : band = "2100"             'US AWS band, Band 10
            Case 4400 To 4449 : band = "2100"             'US AWS band, Band 10
            Case 4450 To 4499 : band = "2100"             'US AWS band, Band 10
            Case 4500 To 4599 : band = "2100"             'US AWS band, Band 10
            Case 4600 To 4649 : band = "2100"             'US AWS-3 band, Band 10
            Case 4650 To 4699 : band = "2100"             'US AWS-3 band, Band 10
            Case 4700 To 4749 : band = "2100"             'US AWS-3 band, Band 10

                '--------------------------------------------------------------------------------
            Case 6150 To 6449 : band = "800"
            Case 6600 To 7399 : band = "3500"
            Case 36200 To 36349 : band = "2000"
                ''---------------------v16.2.9.sid-------------------------------------------
            Case 45720 To 46019 : band = "700"               'USA 700L spectrum, Band 44
            Case 65536 To 66435 : band = "2100"               'Europe, Africa 2100, Band 65
            Case 66436 To 66535 : band = "2100"             'US AWS band, Band 66
            Case 66536 To 66635 : band = "2100"             'US AWS band, Band 66
            Case 66636 To 66685 : band = "2100"             'US AWS band, Band 66
            Case 66686 To 66735 : band = "2100"             'US AWS band, Band 66
            Case 66736 To 66785 : band = "2100"             'US AWS band, Band 66
            Case 66786 To 66885 : band = "2100"             'US AWS band, Band 66
            Case 66886 To 66935 : band = "2100"             'US AWS-3 band, Band 66
            Case 66936 To 66985 : band = "2100"             'US AWS-3 band, Band 66
            Case 66986 To 67035 : band = "2100"             'US AWS-3 band, Band 66
            Case 67036 To 67135 : band = "2100"             'US AWS-3 band, Band 66
            Case 67136 To 67335 : band = "2100"              'USA AWS-4 2100 Band 66
            Case 67336 To 67835 : band = "700"                'Europe, Africa 700, Band 67, 68
            Case 67836 To 68335 : band = "2500"              'non-US 2500 band, Band 69
            Case 68336 To 68585 : band = "2000"              'US DISH AWS-H, AWS-4 band, Band 70
            Case 68586 To 68935 : band = "600"              'US 600 MHz, Band 71
            Case 46790 To 54539 : band = "5000"             'US LAA 5000 MHz, Band 46
                '--------------------------------------------------------------------------------
            Case Else
                band = ""
        End Select
        Return band
    End Function



    Public Function GetSIB5(ByVal input As String) As String
        Dim SIB5 As String = ""
        Dim tmpString As String = "SIB5qRxLevMin(dbm)|SIB5pMax(dbm)|SIB5tReslectEUTRA(s)|SIB5threshXhi(db)|SIB5threshXlo(db)"

        Dim commandText As String = "DECLARE @SIB5val varchar(250)" &
            "SET @SIB5val = '" & input & "'" &
            "select 'LTE SIB5' as Message, " &
           "(dbo.SQLTERRCKeyValue(@SIB5val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib5;interFreqCarrierFreqList;[0] interFreqCarrierFreqList;element;q_RxLevMin')*2)[SIB5qRxLevMin(dbm)]," &
           "dbo.SQLTERRCKeyValue(@SIB5val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib5;interFreqCarrierFreqList;[0] interFreqCarrierFreqList;element;p_Max')[SIB5pMax(dbm)]," &
           "dbo.SQLTERRCKeyValue(@SIB5val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib5;interFreqCarrierFreqList;[0] interFreqCarrierFreqList;element;t_ReselectionEUTRA')[SIB5tReslectEUTRA(s)]," &
           "dbo.SQLTERRCKeyValue(@SIB5val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib5;interFreqCarrierFreqList;[0] interFreqCarrierFreqList;element;threshX_High')[SIB5threshXhi(db)]," &
           "dbo.SQLTERRCKeyValue(@SIB5val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib5;interFreqCarrierFreqList;[0] interFreqCarrierFreqList;element;threshX_Low')[SIB5threshXlo(db)]"



        _SIBSQLCommand.CommandText = commandText
        Dim fields As String() = tmpString.Split("|")
        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            For Each field In fields
                SIB5 = SIB5 & Convert.ToString(dr(field)) & "|"
            Next
        End While

        dr.Close()

        Return SIB5.Substring(0, SIB5.Length - 1).Replace(",", ";")
    End Function


    Public Function GetSIB6(ByVal input As String) As String
        '00110510FD9CE04A03305639C09404
        Dim SIB6 As String = ""
        Dim tmpString As String = "SIB6ThreshXHi(db)|SIB6ThreshXLo(db)|SIB6qRxLevMin(dbm)|SIB6pMaxUTRA|SIB6qQualMin(db)"

        Dim commandText As String = "DECLARE @SIB6val varchar(250)" &
            "SET @SIB6val = '" & input & "'" &
            "select 'LTE SIB6' as Message, " &
        "dbo.SQLTERRCKeyValue(@SIB6val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib6;carrierFreqListUTRA_FDD;[0] carrierFreqListUTRA_FDD;element;threshX_High')[SIB6ThreshXHi(db)]," &
        "dbo.SQLTERRCKeyValue(@SIB6val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib6;carrierFreqListUTRA_FDD;[0] carrierFreqListUTRA_FDD;element;threshX_Low')[SIB6ThreshXLo(db)]," &
        "(dbo.SQLTERRCKeyValue(@SIB6val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib6;carrierFreqListUTRA_FDD;[0] carrierFreqListUTRA_FDD;element;q_RxLevMin')*2)[SIB6qRxLevMin(dbm)]," &
        "dbo.SQLTERRCKeyValue(@SIB6val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib6;carrierFreqListUTRA_FDD;[0] carrierFreqListUTRA_FDD;element;p_MaxUTRA')SIB6pMaxUTRA," &
        "dbo.SQLTERRCKeyValue(@SIB6val,2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib6;carrierFreqListUTRA_FDD;[0] carrierFreqListUTRA_FDD;element;q_QualMin')[SIB6qQualMin(db)]"




        _SIBSQLCommand.CommandText = commandText
        Dim fields As String() = tmpString.Split("|")
        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            For Each field In fields
                SIB6 = SIB6 & Convert.ToString(dr(field)) & "|"
            Next
        End While

        dr.Close()

        Return SIB6.Substring(0, SIB6.Length - 1).Replace(",", ";")
    End Function

    Public Function GetSIB8(ByVal input As String) As String
        '001BF1AF53B9705A881057E8002012FDDB6852D880612B724C794D6601BC645CAE32CF207201C1F5D05FF14EA5310A4940820000010033EED52916C493095A29A3CF1DB9998C01E540C001A0A7A007364EEA7EFD0FD616007FAC2E77A1A99B3DE00000000000000000
        Dim SIB8 As String = ""
        Dim tmpString As String = "SIB8cdmaEUTRASynch|SIB8synchSysTime|SIB8searchWinSize"

        Dim commandText As String = "DECLARE @SIB8val varchar(250)" &
            "SET @SIB8val = '" & input & "'" &
            "select 'LTE SIB8' as Message, " &
            "dbo.SQLTERRCKeyValue(@SIB8val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib8;systemTimeInfo;cdma_EUTRA_Synchronisation')SIB8cdmaEUTRASynch," &
            "dbo.SQLTERRCKeyValue(@SIB8val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib8;systemTimeInfo;cdma_SystemTime;synchronousSystemTime')SIB8synchSysTime," &
            "dbo.SQLTERRCKeyValue(@SIB8val, 2,'BCCH_DL_SCH_Message;message;c1;systemInformation;criticalExtensions;systemInformation_r8;sib_TypeAndInfo;[0] sib_TypeAndInfo;element;sib8;searchWindowSize')SIB8searchWinSize"


        _SIBSQLCommand.CommandText = commandText
        Dim fields As String() = tmpString.Split("|")
        Dim dr As SqlDataReader = _SIBSQLCommand.ExecuteReader()

        While dr.Read()
            For Each field In fields
                SIB8 = SIB8 & Convert.ToString(dr(field)) & "|"
            Next
        End While

        dr.Close()


        Return SIB8.Substring(0, SIB8.Length - 1).Replace(",", ";")

    End Function

    Public Function ChkSpectrumFile(filename As String) As Boolean
        If filename.ToUpper.Contains("SPECTRUMSCAN") Then
            Return True
        Else
            Return False
        End If
    End Function

    'SIB4IntraFreqCellList|SIB4PCIs|SIB4OffsetPCIs|SIB4BlackCellList|SIB4BlackCells
    'SIB6ThreshXHi(db)|SIB6ThreshXLo(db)|SIB6qRxLevMin(dbm)|SIB6pMaxUTRA|SIB6qQualMin(db)
    'SIB8cdmaEUTRASynch|SIB8synchSysTime|SIB8searchWinSize


End Module
