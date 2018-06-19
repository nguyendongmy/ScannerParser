Option Explicit On
Imports System.IO
Imports System.Math
Imports System.Text.RegularExpressions
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Data.SQLite
Imports System.Globalization
Imports Ionic.Zip

Public Class Scanner

    Private DoNotExistList As Hashtable
    Dim _count_CDMA_Record As Integer
    Dim _count_UMTS_Record As Integer

    Private _ScanRate3030 As Double
    Private _ScanRate3040 As Double
    Private _ScanPeriod3030 As Double
    Private _ScanPeriod3040 As Double


    Private _FileSize3030 As Integer
    Private _FileSize3040 As Integer

    Private _FileDuration3030 As Integer
    Private _FileDuration3040 As Integer
    Private _FileDurationMerge As Integer


    Private isGPSInterpolated As Boolean
    Private ListGPSLossLatLon As List(Of GPSLossLatLon)
    Private ListGPSLossLatLon3030 As List(Of GPSLossLatLon)
    Private _writeFirstMaxRecordList As HashSet(Of String)
    Private _lastEndRoundTimeStamp As DateTime
    Private _isNo3030Message As Boolean
    Private _isNo3040Message As Boolean


    Dim isSetlastEndRoundTimeStamp As Boolean = False
    Private _lastChannel As String
    Private _id_server_transaction As Integer
    Private _id_server_file As Integer
    Private _scanner_file_path As String
    Private _GAPP_SQL_Connection As String

    Private _ScanIDSet As HashSet(Of String)
    Private count_total_message As Integer
    Private _FirstScanIDData As FirstScanIDData
    Private _canWrite As Boolean = True
    Private _TempStringList As List(Of String)
    Private _Threshold_RSSI As Integer
    Private _Threshold_Operator As Integer

    Private _CDMA_ScanID_Maximum_Duration As Double
    Private _UMTS_ScanID_Maximum_Duration As Double
    Private _LTE_ScanID_Maximum_Duration As Double
    Private _LTE_ScanID_Maximum_Duration1 As Double

    Private _SpectrumScanLimits As Hashtable
    Private _SpectrumScanFreqThreshold As SpectrumScanFreqThreholdOutput  'added by My task #1261
    Private _currentNosData As NoSDataObj
    Private _currentRSSIValue As Double
    Private currentCDMAChannel As String = ""
    Private currentWCDMAChannel As String = ""
    Private _myBand As String = ""
    Private _channel As String = ""
    Private CDMASIDHashSet As New HashSet(Of String)
    Private WCDMASIDHashSet As New HashSet(Of String)
    Private LTESIDHashSet As New HashSet(Of String)
    Private _BandWidthList As New Hashtable()
    Private xmlChannelString As String = ""
    Private xmlChannelStringMerge As String = ""

    Private _EnableSipDecoding As Boolean

    Private _DataRowList As New List(Of DataRow)
    Private _DataRowList1 As New List(Of DataRow)

    Private _SQL3KeyValueDotNet_v2_Path As String

    Private _SIDFile As String
    Private _configFile As String
    Private _isLibMode As Boolean
    Private FileLogInfoList As List(Of FileLogInfo)
    Private FileInfoLog As System.IO.TextWriter


    Public Sub SetIdServerTransaction(ByVal id_server_transaction As Integer)
        _id_server_transaction = id_server_transaction
    End Sub

    Public Sub SetIdServerFile(ByVal id_server_file As Integer)
        _id_server_file = _id_server_file
    End Sub

    Public Sub SetScannerFilePath(ByVal file_path As String)
        _scanner_file_path = file_path
    End Sub

    Public Sub SetGAPPSQLConnection(ByVal GAPP_SQL_Connection As String)
        _GAPP_SQL_Connection = GAPP_SQL_Connection
    End Sub

    Public Sub SetConfigFile(ByVal configFile As String)
        _configFile = configFile
    End Sub


    Public Sub SetSIDFile(ByVal SIDFile As String)
        _SIDFile = SIDFile
    End Sub

    Public Sub SetIsLibMode(ByVal isLibMode As Boolean)
        _isLibMode = isLibMode
    End Sub

    Private Structure RECaddNoSCondition
        Public lastScanId As Integer
        Public operatorEmptyCount As Integer
        Public rssiNev999Count As Integer

    End Structure

    Private Structure FirstScanIDData
        Public time_stamp As String
        Public gps_time_stamp As String
        Public lat As String
        Public lon As String

    End Structure




    Private Structure GPSLossLatLon

        Public StartID As Integer
        Public EndID As Integer
        Public StartLat As Double
        Public EndLat As Double
        Public StartLon As Double
        Public EndLon As Double
        Public StartTime As Long
        Public EndTime As Long
        Public startTime1 As DateTime
        Public endTime1 As DateTime
        Public FileName As String


    End Structure


    Private Structure DataRow
        Public ID As Integer
        Public MessageData As Byte()
        Public TimeStampCreated As String
        Public TimeStampCreatedOriginal As String
        Public TimeStampCreatedValue As Long
        Public MessageType As String
        Public FileName As String
        Public FileFullName As String

    End Structure

    Public Structure ScannerFileInputObject
        Public FileName As String
        Public FileMergeName As String
        Public isSkip As Boolean
        Public id_server_transaction As Integer
        Public id_server_file As Integer
        Public file_path As String
        Public Campaign As String
        Public ClientName As String
        Public MarketName As String
        Public status_flag As String
        Public Team As String
        Public MergePattern1 As String
        Public MergePattern2 As String


    End Structure

    Private Structure NoSDataObj
        Public CarrierFrequencyMHz As String
        Public EARFCN As String
        Public CarrierBW As String
        Public Band As String
        Public Channel As String
        Public MCC As String
        Public MNC As String
        Public OperatorName As String
        Public FileName As String
        Public LogTrace As String
        Public prev_Time_Stamp As String
        Public prev_GPS_Time As String
        Public prev_Tech As String
        Public prev_ScanId As String
        Public prev_lat As Double
        Public prev_lon As Double

    End Structure


    Private Structure OutputFileWriter
        Public writer As StreamWriter
        Public isFirstRecrod As Boolean
        Public countNoOperator As Integer
        Public OperatorName As String
        Public currentNosData As NoSDataObj
        Public countRSSINev999 As Integer
        Public isDoNotExist As Boolean
        Public FileFullName As String
        Public ScanId As Long
        Public OperatorSetRow As SortedDictionary(Of Double, String)
        Public OperatorBandSetRow As SortedDictionary(Of Double, String)
        Public MapParseToBand As Hashtable

    End Structure


    Private Structure AddNoRecChannel
        Public channel As Integer
        Public countRSSINev999 As Integer
        Public ScanId As Long
        Dim lastRSSI As Long
        Dim nextRSSI As Long
        Dim isCheckRSSI As Boolean
        Dim LastScanId As Long
        Dim LastScanIdRSSI As Long
        Dim firstScanIdRSSI As Long
        Dim foundFirstRSSI As Boolean
        Dim ScanIDList As HashSet(Of Integer)
        Dim lastIndexFound As Integer
    End Structure


    Private Structure Channel
        Public band As String
        Public startChanel As String
        Public bandInt As Integer
    End Structure

    Private Structure SIDNIDStore
        Public SID As String
        Public NID As String
        Public time As DateTime
        Public band As String
    End Structure

    Private Structure InputChannelStore
        Public StartTime As DateTime
        Public EndTime As DateTime
        Public isSetStartTime As Boolean
        Public isFound As Boolean
        Public StartTimeTick As Long
        Public EndTimeTick As Long
        Public channel As String
        Public band As String
        Public isStartLate As Boolean
        Public isEndEarly As Boolean
        Public inputFileName As String
    End Structure

    Private Structure LTESIBInfo
        Dim SIB1 As String
        Dim SIB2 As String
        Dim SIB3 As String
        Dim SIB4 As String
        Dim SIB5 As String
        Dim SIB6 As String
        Dim SIB8 As String
        Dim SIB_PhysicalCellId As String
        Dim SIB_PhysicalCellId_Position As String
    End Structure


    Private Structure ChannelData
        Public value As Decimal
        Public value2 As Decimal
        Public channel As String
        Public SID As String
        Public NID As String
        Public outputStrData As String
        Public OperatorName As String
        Public Band As String
        Public CarrierBW As String
        Public CarrierSignalLevel As String
        Public FileName As String

    End Structure

    Private Structure ScannerData
        Public channelRepeatedCount As Double
        Public channelRepeatedCount3030 As Double
        Public channelRepeatedCount3040 As Double
        Public consecutiveChannelRepeatedCount As Integer
        Public inputChannelList As HashSet(Of String)
        Public inputChannelList3030 As HashSet(Of String)
        Public inputChannelList3040 As HashSet(Of String)
        Public channelData As List(Of ChannelData)

    End Structure

    Private Structure ChannelFound
        Public GSMChannelFound As String
        Public CDMAChannelFound As String
        Public WCDMAChannelFound As String
        Public LTEChanelFound As String
    End Structure

    Private _rowCount As Integer
    Private _rowCount1 As Integer
    Private _rowCount2 As Integer

    Private _AddNoRecChannelList As Hashtable

    Private _lastLTETimeStamp3040 As DateTime
    Private _lastLTETimeStamp3030 As DateTime
    Private _RECaddNoSCondition As New RECaddNoSCondition

    Private currentOperatorEmptyCount As Integer

    Private _scannedTech As String

    Private _MessageObject As Hashtable
    Private _LTEChannelFound As String
    Private _LTEChannelFoundHash As Hashtable
    Private _CDMAChannelFound As String
    Private _WCDMAChannelFound As String


    Private _CDMAChannelTmp As HashSet(Of String)

    Private _LTESIBInfo As LTESIBInfo
    Private isCDMAFirstRecord As Boolean
    Private isUMTSFirstRecord As Boolean
    Private isLTEFirstRecord As Boolean
    Private _ChannelFoundSet As Hashtable
    Private _OperatorSet As Hashtable
    Private _OperatorBandSet As Hashtable
    Private _SIDLookupList As Hashtable
    Private _isSkippedCDMA As Boolean
    Private _isSkippedWCDMA As Boolean

    Private _ScanID As Integer
    Private _LastScanID As Integer
    Private _CDMAvalue As Decimal
    Private _GSMvalue As Decimal
    Private _WCDMAvalue As Decimal
    Private _LTEvalue As Decimal
    Private _OutputWriter As StreamWriter


    Private _InforWriterCounter As Integer

    Private _OutputWriterList As StreamReader
    Private _MarketName As String
    Private _Campaign As String
    Private _ClientName As String
    Private _FileName As String
    Private _CollectedDate As String
    Private _CollectedDateFormat As String
    Private _isOutputGSM As Boolean
    Private _isOutputCDMA As Boolean
    Private _isOutputWCDMA As Boolean
    Private _isOutputUMTS As Boolean
    Private _isOutputLTE As Boolean
    Private _GSMOutputList As List(Of String)
    Private _CDMAOutputList As List(Of String)
    Private _UMTSOutputList As List(Of String)
    Private _LTEOutputList As List(Of String)

    'Scanner Data
    Private currentCDMAChannelData As ChannelData
    Private currentWCDMAChannelDaTa As ChannelData
    Private currentGSMChannelData As ChannelData
    Private currentLTEChannelData As ChannelData

    Private _GSMScannerData As ScannerData
    Private _CDMAScannerData As ScannerData
    Private _WCDMAScannerData As ScannerData
    Private _LTEScannerData As ScannerData


    Private isFirstRecord As Boolean
    Private isChannelFull As Boolean = False
    Private countOutisdePolygon As Integer = 0
    Dim isBlankPolygon As Boolean
    Dim currentTimeStamp As String


    Private ID As Integer
    Private myStr1 As String = ""
    Private LastTimeStamp As String
    Private isLTEWider As Boolean
    Private rptChannelCount As Integer
    Private rptCDMAChannelCount As Integer
    Private rptLTEChannelCount As Integer
    Private isWriteMAXRecord_WCDMA As Boolean

    Private isLTEWriteAll As Boolean
    Private currentChannel As Integer
    Private currentCarrierRSSI As Double
    Private currentRefN As Integer

    Private isSkipLTESynBlock As Boolean = False

    Private currentChannelList As String
    Private LTEChannelList As Hashtable
    Private ChannelWithAllRSSINev999 As Hashtable
    Private LTEChannelList1 As Hashtable
    Private LTEChannelList3030 As HashSet(Of String)
    Private LTEChannelList3040 As HashSet(Of String)
    Private WCDMAChannelList As Hashtable
    Private CDMAChannelList As Hashtable
    Private GSMChannelList As Hashtable

    Private LTEChannelListWithTime As Hashtable
    Private WCDMAChannelListWithTime As Hashtable
    Private CDMAChannelListWithTime As Hashtable
    Private GSMChannelListWithTime As Hashtable


    Private LTEMsgCode As Integer
    Private tmpMsgData() As Byte
    Private tmpID As Integer



    Private Structure GSMBand
        Dim band As String
        Dim isScanned As Boolean
    End Structure

    Private Structure ChannelCount
        Dim countChannel As Integer
        Dim TechName As String
        Dim CarrierName As String
        Dim currentCount As Integer
        Dim currentChannelList As String
        Dim AllChanelList As String
        Dim repeatedChannelCount As Integer
    End Structure


    Private Structure GSMScannerBCCHInfo
        Dim RFBand As String
        Dim Message As String
        Dim NumChannels As Integer
    End Structure

    Private Structure WCDMAScannerPilotInfo
        Dim RFBand As String
        Dim Channel As String
        Dim Band As String
        Dim PNThreshold As String
        Dim IO As String
        Dim DwellingTime As String
        Dim NumRecords As Integer
        Dim MCC As String
        Dim MNC As String
        Dim OperatorName As String
        Dim LogTrace As String
    End Structure

    Private Structure CDMAScannerPilotInfo
        Dim RFBand As String
        Dim Channel As String
        Dim Band As String
        Dim NoRxLev As String
        Dim NumRecords As Integer
        Dim SID As String
        Dim NID As String
        Dim OperatorName As String
        Dim LogTrace As String
        Dim ID As Integer
    End Structure


    Private Structure QVPFileInfo

        Dim CDMAInputChannel As String
        Dim WCDMAInputChannel As String
        Dim LTEInputChannel As String
        Dim GSMInputChannel As String

        Dim FileName As String
        Dim MDFSVersion As String
        Dim LocalDateTime As Date
        Dim SWVerzion As String
        Dim DSPVersion As String
        Dim NetworkDevice As String
        Dim NetworkType As String
        Dim TestLocation As String
        Dim EquipmentLocation As String
        Dim MNCMCC As String
        Dim CDMAChannels As String
        Dim WCDMAChannels As String
        Dim GSMChannels As String
        Dim LTEChannels As String
        Dim colgsmMaster As String
        Dim colgsm As String
        Dim colwcdmaMaster As String
        Dim colwcdma As String
        Dim colcdmaMaster As String
        Dim colcdma As String
        Dim collte As String
        Dim collteMaster As String
    End Structure
    Private Structure LTEstuffObj
        Dim Channel As Integer
        Dim CarrierFrequencyKHz As Double
        Dim CarrierRSSI As Single
        Dim Scan() As LTEstuff                      'mod Shared -> Dim, v12.1.12
        Dim activeFlg As Boolean                    'add v12.1.12 to scan multiple LTE chan
        Dim lastFoundTime As Date                   'add v12.1.12 to scan multiple LTE chan
    End Structure
    Private Structure LTEstuff
        Dim Latitude As Double
        Dim Longitude As Double
        Dim BandWidthKhz As Integer
        Dim CollectionTime As Date
        Dim Channel As Integer
        Dim CarrierRSSI As Single
        Dim SortingValue As Integer
        Dim RefScan() As LTEScan
        Dim SyncScan() As LTEScan

    End Structure

    Private Structure LTEScan
        Dim PhysicalCellId As Integer
        Dim DelaySpread As Integer
        Dim NumTxAntennaPortsWideBand As Integer  'added by My 05/31/2018 #1278
        Dim UniqueCellId As Integer
        Dim Antennas As Integer
        Dim TimeOffset As Long
        Dim RP() As Single
        Dim RQ() As Single
        Dim CINR() As Single
        Dim PSCH_RP() As Single
        Dim PSCH_RQ() As Single
        Dim RSSI As Single
        Dim CyclicPrefix As String
    End Structure


    Private Structure WCDMAstuffObj
        Dim Channel As Integer
        Dim Band As Integer
        Dim Delay As Integer
        Dim Io As Single
        Dim Scan() As WCDMAstuff
        Dim activeFlg As Boolean
        Dim lastFoundTime As Date
    End Structure
    Private Structure WCDMAstuff
        Dim Delay As Integer
        Dim CPICHSC As Integer
        Dim SpreadChips As Integer
        Dim CellID As Long
        Dim PSCEcNo As Single
        Dim SSCEcNo As Single
        Dim CPICHEc As Single
        Dim CPICHEcNo As Single
        Dim BCCHEcNo As Single
        Dim LAC As Single
        Dim SigInt As Single
    End Structure
    Private Structure CDMAstuffObj
        Dim Channel As Integer
        Dim Band As Integer
        Dim Delay As Integer
        Dim Io As Single
        Dim Scan() As CDMAstuff
    End Structure
    Private Structure CDMAstuff
        Dim Channel As Integer
        Dim Delay As Integer
        Dim CPICHSC As Integer
        Dim SpreadChips As Integer
        Dim SSCEcNo As Single
        Dim CPICHEc As Single
        Dim CPICHEcNo As Single
        Dim Base_ID As String
        Dim Base_LAT As String
        Dim Base_LONG As String
    End Structure
    Private Structure GSMstuff
        Dim Lat As Double
        Dim Lon As Double
        Dim FileName As String
        Dim DateTime As Date
        Dim MCC As String
        Dim MNC As String
        Dim CellId As String
        Dim RSSI As Single
        Dim Channel As Integer
        Dim Band As Integer
        Dim activeFlg As Boolean
    End Structure


    Private TimeZoneValue As Integer
    Private myComparer As Comparer
    Private lat As Double


    Private GSM_RFband As Integer
    Private WCDMAPilotInfo As WCDMAScannerPilotInfo
    Private CDMAPilotInfo As CDMAScannerPilotInfo

    Private Count_WCDMARecord As Integer
    Private WCDMA_NumRecords As Integer
    Private Count_CDMARecord As Integer
    Private CDMA_NumRecords As Integer

    Private GSM_BCCHInfo As GSMScannerBCCHInfo
    Private GSMBandList As List(Of GSMBand)



    Private ch_WCDMA, N_WCDMA, DescSortKeys_WCDMA() As Integer                          'N = Num of Readings
    Private ch_CDMA, N_CDMA, DescSortKeys_CDMA() As Integer                          'N = Num of Readings
    Private scan_WCDMA(), scanSort_WCDMA() As WCDMAstuff
    Private scan_CDMA(), scanSort_CDMA() As CDMAstuff
    Private NoRxLev_WCDMA As Single
    Private DescSort_WCDMA() As Single
    Private NoRxLev_CDMA As Single
    Private DescSort_CDMA() As Single

    Private DescSortKeys_GSM(), Count_GSMRecord As Integer                          'N = Num of Readings
    Private mygsm_GSM, gsmInit_GSM As GSMstuff
    Private NoRxLev_GSM As Single
    Private DescSort_GSM() As Single
    Private N_GSM As Integer

    Private GSM_NumRecords As Integer
    Private Count_GSM As Integer
    Private lon As Double
    Private dttm As String
    Private dttmOriginal As String
    Private dttmFormat As String
    Private coldt As Date
    Private ascii As New System.Text.ASCIIEncoding
    Private MaxFoundCDMA As Boolean
    Private ScnrChLstGSM(0) As GSMstuff
    Private ufc1band As String
    Private ufc1mode As String
    Private ufc1num As Integer
    Private ufc1StNode As Integer
    Private ufc1EndNode As Integer


    'following provides list of channels scanned

    Private ScnrChLstCDMAobj(0) As CDMAstuffObj
    Private CurrentScnrChLstCDMAobj(0) As CDMAstuffObj

    Private ScnrChLstWCDMAobj(0) As WCDMAstuffObj


    Private strMSp12am As String
    Private MSp12am As Long 'miliseconds past MidNight
    Private msGSM As Long
    Private msIS As Long
    Private msCDMA As Long
    Private TechChOpArray(,) As Object
    Private MasterPolyArray(,) As Object
    Private cs As Long 'checksum
    'Private filename As String
    Private EcIoUX03(0) As String 'catches top3 EcIo
    Private cdmaRptCount As Integer
    Private wcdmaRptCount As Integer
    Private gsmRptCount As Integer
    Private dttmDt As Date
    Private myFileInfo As QVPFileInfo
    Private forcedmaxCDMA, forcedmaxCDMA1, forcedmaxWCDMA, forcedmaxGSM, forcedmaxLTE As Boolean
    Dim gpsctr As Long
    Private lastCDMATimeStamp As Date
    Private lastGSMTimeStamp As DateTime
    Private lastUMTSTimeStamp As DateTime
    Private lastLTETimeStamp As DateTime
    Private ScnrChLstLTEobj(0) As LTEstuffObj
    Private ScnrChLstLTE(0) As LTEstuff

    Function getchcount(ByVal str As String) As Integer
        getchcount = 0
        Dim i As Integer
        For i = 0 To str.Length - 1
            If str.Chars(i).Equals("(".Chars(0)) Then getchcount += 1
        Next i
    End Function

    Public Sub SetScannedTech(ByVal scannedTech As String)
        _scannedTech = scannedTech
    End Sub

    Private isGetConfigFile As Boolean = False

    'added by My 06/04/2018 task #1261
    Private Class SpectrumScanFreqThreholdOutput
        Public Class FreqThreshold
            Private Minimum As Int64
            Private Maximum As Int64
            Public Sub New(input As String)
                If input.Contains(",") = False Then
                    input = input + ","
                End If
                Dim values = input.Replace("(", "").Replace(")", "").Split(",").ToList()
                Minimum = Convert.ToInt64(values.First())
                Maximum = Convert.ToInt64(values.Last())
            End Sub
            Public Function WithinRange(value As Int32)
                If String.IsNullOrEmpty(Minimum) Or String.IsNullOrEmpty(Maximum) Or Minimum < 0 Or Maximum < 0 Then
                    Return True
                End If
                Return value >= Minimum And value <= Maximum
            End Function
        End Class

        Private keyword As String
        Private defaultKeyword As String
        Private bandKeyword As String
        Private keywordLength As Int16

        Public thresholds As Dictionary(Of String, List(Of FreqThreshold))

        Public Sub New()
            keyword = "SpectrumScanFreqThresholdOutput".ToLower
            defaultKeyword = "Default"
            bandKeyword = "_Band"
            thresholds = New Dictionary(Of String, List(Of FreqThreshold))
        End Sub

        Public Sub Add(input As String)
            If Not input.Trim.ToLower.StartsWith(keyword) Then
                Exit Sub
            End If
            Dim inputs As List(Of String) = input.Split("|").ToList
            Dim band As String = GetBand(inputs.First)
            If Not thresholds.ContainsKey(band) Then
                thresholds.Add(band, New List(Of FreqThreshold))
            End If
            AddThreshold(thresholds(band), inputs.Last)
        End Sub

        Public Function WithinRange(band As String, value As Int32)
            If Not thresholds.ContainsKey(band) Or thresholds(band).Count = 0 Then
                Return True
            End If
            Return thresholds(band).Any(Function(f) f.WithinRange(value))
        End Function

        Private Sub AddThreshold(list As List(Of FreqThreshold), inputs As String)
            If inputs.Trim.EndsWith(";") = False Then
                inputs = inputs + ";"
            End If
            Dim values = inputs.Split(";")
            For Each value In values
                If String.IsNullOrEmpty(value) Then
                    Continue For
                End If
                list.Add(New FreqThreshold(value))
            Next
        End Sub
        Private Function GetBand(input As String) As String
            If input.ToLower().Contains(defaultKeyword.ToLower()) Then
                Return defaultKeyword
            End If
            Return input.Substring(keyword.Length + bandKeyword.Length)
        End Function
    End Class



    Private Sub GetConfigFile()

        _SpectrumScanLimits = New Hashtable()
        _SpectrumScanFreqThreshold = New SpectrumScanFreqThreholdOutput
        _CDMA_ScanID_Maximum_Duration = 2
        _UMTS_ScanID_Maximum_Duration = 2
        _LTE_ScanID_Maximum_Duration = 2
        _LTE_ScanID_Maximum_Duration1 = 5

        Try
            Using sr As New StreamReader(_configFile)
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
                    ElseIf strs(0).Trim.ToLower = "RECaddNoS_Threshold_RSSI".ToLower Then
                        _Threshold_RSSI = Convert.ToInt32(strs(1).Trim)
                    ElseIf strs(0).Trim.ToLower = "RECaddNoS_Threshold_Operator".ToLower Then
                        _Threshold_Operator = Convert.ToInt32(strs(1).Trim)
                    ElseIf strs(0).Trim.ToLower = "SpectrumScanMinimumRssiThresholdOutput_Default".ToLower Then
                        _SpectrumScanLimits.Add("Default", Convert.ToInt32(strs(1)))

                    ElseIf strs(0).Trim.ToLower.StartsWith("SpectrumScanFreqThresholdOutput_".ToLower) Then
                        _SpectrumScanFreqThreshold.Add(line)

                    ElseIf strs(0).Trim.ToLower = "CDMA_ScanID_Maximum_Duration".ToLower Then
                        _CDMA_ScanID_Maximum_Duration = Convert.ToDouble(strs(1))
                    ElseIf strs(0).Trim.ToLower = "UMTS_ScanID_Maximum_Duration".ToLower Then
                        _UMTS_ScanID_Maximum_Duration = Convert.ToDouble(strs(1))
                    ElseIf strs(0).Trim.ToLower = "LTE_ScanID_Maximum_Duration".ToLower Then
                        _LTE_ScanID_Maximum_Duration = Convert.ToDouble(strs(1))
                    ElseIf strs(0).Trim.ToLower = "LTE_ScanID_Maximum_Duration1".ToLower Then
                        _LTE_ScanID_Maximum_Duration1 = Convert.ToDouble(strs(1))
                    ElseIf strs(0).Trim.ToLower.StartsWith("SpectrumScanMinimumRssiThresholdOutput_Band".ToLower) Then
                        Dim len As Integer = "SpectrumScanMinimumRssiThresholdOutput_Band".Length
                        Dim band As String = strs(0).Substring(len)
                        _SpectrumScanLimits.Add(band, Convert.ToInt32(strs(1)))
                    End If
                Loop
            End Using
        Catch e As Exception
            Console.WriteLine("The file could not be read:")
            Console.WriteLine(e.Message)
        End Try
    End Sub



    Private Sub GetInputChannelLTE(ByVal fileInputObj As ScannerFileInputObject)
        Dim enc As New System.Text.ASCIIEncoding
        Dim drMerge As SQLiteDataReader
        Dim cmdMerge As New SQLiteCommand()
        Dim conMerge As New SQLiteConnection()
        Dim msgData As Byte()
        Dim msgType As Integer
        Dim strMsgData As String()

        If fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip Then

            Dim fileList As String()
            Dim filenameStr As String
            ReDim fileList(1)
            fileList(0) = fileInputObj.FileName.Replace("\", "\\")
            fileList(1) = fileInputObj.FileMergeName.Replace("\", "\\")


            For Each filenameStr In fileList

                Dim count As Integer = 0

                conMerge = New SQLiteConnection()
                cmdMerge = New SQLiteCommand()

                conMerge.ConnectionString = "Data Source=" & filenameStr & ";Version=3;New=False;Compress=True;"
                conMerge.Open()
                If _scannedTech.Trim.IndexOf("CDMA") Or _scannedTech.Trim.IndexOf("UMTS") < 0 Then
                    cmdMerge.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message WHERE MessageType = 10522 or MessageType = 1001 Or MessageType = 12288 Or MessageType = 10103 Or MessageType = 10104"
                Else
                    cmdMerge.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message WHERE MessageType = 10522 or MessageType = 1001 Or MessageType = 12288"
                End If

                cmdMerge.Connection = conMerge

                drMerge = cmdMerge.ExecuteReader()
                Dim isFirstTimeStamp As Boolean = False
                While drMerge.Read()
                    If Not (IsDBNull(drMerge("MessageData")) Or IsDBNull(drMerge("TimeStampCreated"))) Then

                        msgData = drMerge("MessageData")

                        dttm = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated")))
                        dttmDt = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated")))

                        If Not isFirstTimeStamp Then
                            TimeZoneValue = GetTimeOffset(filenameStr, dttm)
                            isFirstTimeStamp = True
                        End If

                        If TimeZoneValue <> 0 Then
                            dttm = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated"))).AddMinutes(TimeZoneValue)
                            dttmDt = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated"))).AddMinutes(TimeZoneValue)

                        End If

                        msgType = Convert.ToInt32(drMerge("MessageType"))

                        If drMerge("MessageType") = "12288" Then
                            xmlChannelString = enc.GetString(msgData)
                        End If


                    End If
                End While

                GetChannelInfo(xmlChannelString, FileInfoLog, filenameStr)

                cmdMerge.Dispose()
                conMerge.Close()
            Next

        End If


    End Sub


    Private Sub GetFileData(ByVal fileInputObj As ScannerFileInputObject)
        Dim enc As New System.Text.ASCIIEncoding
        Dim drMerge As SQLiteDataReader
        Dim cmdMerge As New SQLiteCommand()
        Dim conMerge As New SQLiteConnection()
        Dim msgData As Byte()
        Dim msgType As Integer
        Dim strMsgData As String()

        If fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip Then

            Dim fileList As String()
            Dim filenameStr As String
            ReDim fileList(1)
            fileList(0) = fileInputObj.FileName.Replace("\", "\\")
            fileList(1) = fileInputObj.FileMergeName.Replace("\", "\\")

            Dim totalCount3030 As Integer = 0
            Dim totalCount3040 As Integer = 0
            Dim firstTimeStamp As DateTime
            Dim lastTimeStamp As DateTime


            For Each filenameStr In fileList

                Dim count As Integer = 0

                conMerge = New SQLiteConnection()
                cmdMerge = New SQLiteCommand()

                conMerge.ConnectionString = "Data Source=" & filenameStr & ";Version=3;New=False;Compress=True;"
                conMerge.Open()
                If _scannedTech.Trim.IndexOf("CDMA") Or _scannedTech.Trim.IndexOf("UMTS") < 0 Then
                    cmdMerge.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message WHERE MessageType = 10522 or MessageType = 1001 Or MessageType = 12288 Or MessageType = 10103 Or MessageType = 10104"
                Else
                    cmdMerge.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message WHERE MessageType = 10522 or MessageType = 1001 Or MessageType = 12288"
                End If

                cmdMerge.Connection = conMerge

                drMerge = cmdMerge.ExecuteReader()
                Dim isFirstTimeStamp As Boolean = False
                While drMerge.Read()
                    If Not (IsDBNull(drMerge("MessageData")) Or IsDBNull(drMerge("TimeStampCreated"))) Then

                        msgData = drMerge("MessageData")

                        Dim row As New DataRow()

                        dttm = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated")))
                        dttmDt = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated")))
                        row.TimeStampCreatedOriginal = dttm
                        If Not isFirstTimeStamp Then
                            firstTimeStamp = dttm
                            TimeZoneValue = GetTimeOffset(filenameStr, dttm)
                            isFirstTimeStamp = True
                        End If
                        If dttm > lastTimeStamp Then
                            lastTimeStamp = dttm
                        End If


                        If TimeZoneValue <> 0 Then
                            dttm = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated"))).AddMinutes(TimeZoneValue)
                            dttmDt = DateTime.FromOADate(CDbl(drMerge("TimeStampCreated"))).AddMinutes(TimeZoneValue)

                        End If

                        msgType = Convert.ToInt32(drMerge("MessageType"))

                        If drMerge("MessageType") = "12288" Then
                            xmlChannelString = enc.GetString(msgData)
                        End If
                        If drMerge("MessageType") = "10522" Then
                            If filenameStr.ToUpper().IndexOf(fileInputObj.MergePattern2) > 0 Then
                                totalCount3030 = totalCount3030 + 1
                            ElseIf filenameStr.ToUpper().IndexOf(fileInputObj.MergePattern1) > 0 Then
                                totalCount3040 = totalCount3040 + 1
                            End If
                        End If
                        ID = Convert.ToInt32(drMerge("ID"))
                        count = count + 1

                        row.ID = ID
                        row.MessageType = msgType
                        row.TimeStampCreated = dttm
                        row.TimeStampCreatedValue = Convert.ToDateTime(row.TimeStampCreated).Ticks + count
                        row.MessageData = msgData
                        Dim FileNameOnly As String = filenameStr.Substring(filenameStr.LastIndexOf("\") + 1)

                        row.FileName = FileNameOnly.Substring(0, FileNameOnly.IndexOf("."))
                        row.FileFullName = filenameStr

                        If msgType = "10103" Or msgType = "10104" Then
                            _DataRowList1.Add(row)
                        Else
                            _DataRowList.Add(row)
                        End If

                    End If
                End While

                'GetChannelInfo(xmlChannelString, FileInfoLog, filenameStr)

                cmdMerge.Dispose()
                conMerge.Close()

                If (lastTimeStamp - firstTimeStamp).TotalMilliseconds > 0 Then
                    If filenameStr.ToUpper().IndexOf(fileInputObj.MergePattern2) > 0 Then
                        _ScanRate3030 = totalCount3030 / (lastTimeStamp - firstTimeStamp).TotalSeconds
                    ElseIf filenameStr.ToUpper().IndexOf(fileInputObj.MergePattern1) > 0 Then
                        _ScanRate3040 = totalCount3040 / (lastTimeStamp - firstTimeStamp).TotalSeconds
                    End If
                End If


            Next
            'ScanRate3030 = (Total Record Count of 3030 file) / (3030 File End Time – 3030 File Start Time) / (Number of LTE input channels on 3030)
            'ScanRate3040 = (Total Record Count of 3040 file) / (3040 File End Time – 3040 File Start Time) / (Number of LTE input channels on 3040)



            _DataRowList.Sort(Function(x, y) x.TimeStampCreatedValue.CompareTo(y.TimeStampCreatedValue))


        End If


    End Sub

    Private Sub GetFirstLatLon(ByVal fileInputObj As ScannerFileInputObject)
        'Get the first message 1001 for lat and lon
        Dim cnn As SQLiteConnection
        Dim cmd As SQLiteCommand
        Dim dr As SQLiteDataReader
        cmd = New SQLiteCommand
        Dim msgData As Byte()
        Dim strMsgData() As String
        Dim enc As New System.Text.ASCIIEncoding

        cnn = New SQLiteConnection
        cnn.ConnectionString = "Data Source=" & fileInputObj.FileName.Replace("\", "\\") & ";Version=3;New=False;Compress=True;"

        cmd = New SQLiteCommand
        cmd.Connection = cnn

        cmd.Connection = cnn
        cmd.CommandText = "select ID, MessageData, TimeStampCreated,MessageType  from message where MessageType =1001 limit 1 "
        If cmd.Connection.State <> ConnectionState.Open Then cmd.Connection.Open()
        dr = cmd.ExecuteReader()
        While dr.Read
            Dim isGetTime As Boolean = False
            Dim isGetLatLon As Boolean = False
            If Not (IsDBNull(dr("MessageData")) Or IsDBNull(dr("TimeStampCreated"))) Then
                dttm = DateTime.FromOADate(CDbl(dr("TimeStampCreated")))
                If TimeZoneValue <> 0 Then
                    dttm = DateTime.FromOADate(CDbl(dr("TimeStampCreated"))).AddMinutes(TimeZoneValue)
                End If
                isGetTime = True
            End If

            If Convert.ToInt32(dr("MessageType")) = 1001 Then
                msgData = dr("MessageData")
                strMsgData = enc.GetString(msgData).Split(",")
                lat = strMsgData(0)
                lon = strMsgData(1)
                isGetTime = True
            End If

            If isGetLatLon And isGetTime Then
                Exit While
            End If

        End While

        cmd.Dispose()
        dr.Close()
        dr.Dispose()
    End Sub

    Private Function GetTimeOffset(ByVal FileName As String, ByVal FirstDateTime As DateTime) As Integer

        Dim tmpTime As String = FileName.Substring(FileName.LastIndexOf("\") + 1)
        Dim date1 = DateTime.ParseExact(tmpTime.Substring(0, "2017-03-21-14-10-10".Length), "yyyy-MM-dd-HH-mm-ss", Nothing)
        Dim DiffMin As Integer = Round(DateDiff(DateInterval.Minute, FirstDateTime, date1, 0))
        Dim TimeOffset = Int(DiffMin / 30 + 0.5) * 30

        Return TimeOffset
    End Function


    Public Sub Parse(ByVal fileInputObjList As List(Of Scanner.ScannerFileInputObject), ByVal EnableSIPDecoding As Boolean, ByVal TechType As String, ByVal fileInputObj As ScannerFileInputObject, ByVal MarketName As String, ByVal Campaign As String, ByVal ClientName As String)

        _count_CDMA_Record = 0
        _count_UMTS_Record = 0

        _SQL3KeyValueDotNet_v2_Path = ScannerCommon.Get_SQL3KeyValueDotNet_v2_Path()
        If fileInputObj.isSkip Then
            Return
        End If

        _FileSize3030 = 0

        _ScanPeriod3030 = 0
        _ScanPeriod3040 = 0


        _FileSize3040 = 0

        _AddNoRecChannelList = New Hashtable()

        _rowCount = 0
        _rowCount1 = 0

        _BandWidthList = New Hashtable()
        ChannelWithAllRSSINev999 = New Hashtable()
        _writeFirstMaxRecordList = New HashSet(Of String)
        _MessageObject = New Hashtable()

        _ScanIDSet = New HashSet(Of String)

        _FirstScanIDData = New FirstScanIDData()
        _TempStringList = New List(Of String)
        _RECaddNoSCondition = New RECaddNoSCondition()
        _RECaddNoSCondition.lastScanId = 1
        _RECaddNoSCondition.operatorEmptyCount = 0
        _RECaddNoSCondition.rssiNev999Count = 0

        _DataRowList = New List(Of DataRow)
        _DataRowList1 = New List(Of DataRow)

        If Not isGetConfigFile Then
            isGetConfigFile = True
            GetConfigFile()
        End If
        If Not fileInputObj.isSkip Then
            ListGPSLossLatLon = GetLatLon(fileInputObj, fileInputObj.FileName)
            If TechType = "LTE" And fileInputObj.FileMergeName.Trim <> "" Then
                ListGPSLossLatLon3030 = GetLatLon(fileInputObj, fileInputObj.FileMergeName)
            End If

        End If

        GetFirstLatLon(fileInputObj)

        currentOperatorEmptyCount = 0

        _LTEChannelFound = ""
        _CDMAChannelFound = ""
        _WCDMAChannelFound = ""

        _EnableSipDecoding = EnableSIPDecoding

        _CDMAChannelTmp = New HashSet(Of String)

        If IsNothing(ScannerCommon._BandLookUp) Then
            ScannerCommon._BandLookUp = ScannerCommon.GetBand()
        ElseIf ScannerCommon._BandLookUp.Count = 0 Then
            ScannerCommon._BandLookUp = ScannerCommon.GetBand()
        End If

        isUMTSFirstRecord = True
        isLTEFirstRecord = True
        isCDMAFirstRecord = True

        MarketName = MarketName.Replace(",", "")
        Campaign = Campaign.Replace(",", "")
        ClientName = ClientName.Replace(",", "")

        _OperatorSet = New Hashtable()
        _OperatorBandSet = New Hashtable()
        _ChannelFoundSet = New Hashtable()

        GetSidLooup()


        _isSkippedCDMA = False
        _isSkippedWCDMA = False

        _CDMAScannerData = New ScannerData()
        _WCDMAScannerData = New ScannerData()
        _LTEScannerData = New ScannerData()

        _CDMAScannerData.channelData = New List(Of ChannelData)
        _CDMAScannerData.inputChannelList = New HashSet(Of String)

        _WCDMAScannerData.channelData = New List(Of ChannelData)
        _WCDMAScannerData.inputChannelList = New HashSet(Of String)

        _LTEScannerData.channelData = New List(Of ChannelData)
        _LTEScannerData.inputChannelList = New HashSet(Of String)
        _LTEScannerData.inputChannelList3030 = New HashSet(Of String)
        _LTEScannerData.inputChannelList3040 = New HashSet(Of String)

        currentCDMAChannelData = New ChannelData()
        currentWCDMAChannelDaTa = New ChannelData()
        currentLTEChannelData = New ChannelData()

        _MarketName = MarketName
        _Campaign = Campaign
        _ClientName = ClientName
        Dim tmpCollectedDates As String() = fileInputObj.FileName.Substring(fileInputObj.FileName.LastIndexOf("\") + 1).Trim().Substring(0, 10).Split("-")
        _CollectedDate = tmpCollectedDates(1) + "/" + tmpCollectedDates(2) + "/" + tmpCollectedDates(0)
        'modified by My 06/15/2018 task 1261
        _CollectedDateFormat = Convert.ToDateTime(_CollectedDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)

        Dim StartTime As DateTime = DateTime.Now



        Dim ctr As Long

        'Dim myProgress As New ProgressBar
        Dim errCtr As Long
        Dim TechFileCtr As Integer
        Dim myPath As String


        Dim MyTablesCtr As Integer = 0


        Dim msgData As Byte()
        Dim cnn As SQLiteConnection
        Dim cmd As SQLiteCommand
        Dim dr As SQLiteDataReader

        Dim strMsgData() As String
        Dim enc As New System.Text.ASCIIEncoding
        Dim tmpInt As Integer
        Dim count_file As Integer = -1

        '2016-11-28-13-00-42-3040-0006-0006-3040-S.mf

        cnn = New SQLiteConnection
        cnn.ConnectionString = "Data Source=" & fileInputObj.FileName.Replace("\", "\\") & ";Version=3;New=False;Compress=True;"

        cmd = New SQLiteCommand
        cmd.Connection = cnn

        cmd.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message "

        If TechType.ToUpper() = "LTE" Then
            cmd.CommandText = "select count(*)  from message where MessageType = 10521 or MessageType = 10522 limit 1"
        ElseIf TechType.ToUpper() = "UMTS" Then
            cmd.CommandText = "select count(*)  from message where MessageType =10103 or MessageType =10104 "
        ElseIf TechType.ToUpper() = "CDMA" Then
            cmd.CommandText = "select count(*)  from message where MessageType =10103 or MessageType =10104 limit 1 "

        ElseIf TechType.ToUpper() = "GSM" Then
            cmd.CommandText = "select count(*)  from message where MessageType =10010 or MessageType =10011 limit 1"

        ElseIf TechType.ToUpper() = "SPECTRUMSCAN" Then
            cmd.CommandText = "select count(*)  from message where MessageType =10520 limit 1"

        End If


        If cmd.Connection.State <> ConnectionState.Open Then cmd.Connection.Open()


        Dim count1 As Integer = 0
        count1 = Convert.ToInt32(cmd.ExecuteScalar())
        If count1 <= 0 And fileInputObj.FileMergeName.Trim() = "" Then

            If Not _isLibMode Then
                'MessageBox.Show("There is no " + TechType + " data in input raw files")
            End If
            Return

        End If

        cmd.Dispose()
        cnn.Close()


        Dim kk As Integer = -1
        Dim kk1 As Integer = -1


        kk = -1


        myPath = Mid(fileInputObj.FileName, 1, InStrRev(fileInputObj.FileName, "\"))
        TechFileCtr = -1


        Dim newFile As Boolean = True
        _LastScanID = 0
        _ScanID = 1

        Dim filePath As String = fileInputObj.FileName.Substring(0, fileInputObj.FileName.LastIndexOf("\"))



        _FileName = fileInputObj.FileName.Substring(fileInputObj.FileName.LastIndexOf("\") + 1)
        _FileName = _FileName.Substring(0, _FileName.LastIndexOf("."))
        Dim outputFileName As String = filePath + "\" + _FileName + "_PARSE_" + TechType + ".txt"


        If TechType = "LTE" Then
            If fileInputObj.FileMergeName <> "" And Not fileInputObj.isSkip Then
                Dim fileInputObjMerge As ScannerFileInputObject

                For Each fileInputObjTmp In fileInputObjList

                    If fileInputObj.FileMergeName = fileInputObjTmp.FileName Then
                        fileInputObjMerge = fileInputObjTmp
                        Exit For
                    End If


                Next

                Dim tmpFileMergeName As String = fileInputObj.FileMergeName.Substring(fileInputObj.FileMergeName.LastIndexOf("\") + 1)
                tmpFileMergeName = tmpFileMergeName.Substring(0, tmpFileMergeName.LastIndexOf("."))
                Dim outputFileMergeName As String = filePath + "\" + tmpFileMergeName + "_PARSE_" + TechType + ".txt"
                Dim _OutputMergeWriter = New StreamWriter(outputFileMergeName)
                '_OutputMergeWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,DelaySpread_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,DelaySpread_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,DelaySpread_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,DelaySpread_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,DelaySpread_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,DelaySpread_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                _OutputMergeWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,NumTxAntennaPortsWideBand_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,NumTxAntennaPortsWideBand_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,NumTxAntennaPortsWideBand_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,NumTxAntennaPortsWideBand_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,NumTxAntennaPortsWideBand_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,NumTxAntennaPortsWideBand_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")

                Dim LogTrace As String = ScannerCommon.GetDLLVersion() + "| This file merged with: " + fileInputObj.FileMergeName
                'modified by My 05/31/2018 #1277
                _OutputMergeWriter.WriteLine("HEADER," & dttm & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",LTE,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + LogTrace + ",-999,-999,-999,-999,-999,-999,SIB1cellBarred|SIB1intraFreqReselection|SIB1qRxLevMin(dbm)|SIB1pMax|SIB1tddSubFrameAssignment|SIB1tddSpecialSubFramePatterns,SIB2RAPreambles|SIB2PwrRampStep|SIB2InitRecdTrgPwr|SIB2PreambleTransMax|SIB2RARespWinSize|SIB2MACContResTimer|SIB2maxHARQMsg3Tx|SIB2PDSCHRefSigPwr|SIB2PDSCHPb|SIB2SRSbwConfig|SIB2SRSsubframeConfig|SIB2ACKNACKsimultTx|SIB2P0NominalPUSCH|SIB2Alpha|SIB2P0NomPUCCH|SIB2t300|SIB2t301|SIB2t310|SIB2n310|SIB2t311|SIB2n311,SIB3qHyst|SIB3sNonIntraSearch(db)|SIB3ThreshServLow(db)|SIB3cellReselPriority|SIB3qRxLevMin(dbm)|SIB3pMax(dbm)|SIB3sIntraSearch(db),SIB4IntraFreqCellList|SIB4PCIs|SIB4OffsetPCIs|SIB4BlackCellList|SIB4BlackCells,SIB5qRxLevMin(dbm)|SIB5pMax(dbm)|SIB5tReslectEUTRA(s)|SIB5threshXhi(db)|SIB5threshXlo(db),SIB6ThreshXHi(db)|SIB6ThreshXLo(db)|SIB6qRxLevMin(dbm)|SIB6pMaxUTRA|SIB6qQualMin(db),SIB8cdmaEUTRASynch|SIB8synchSysTime|SIB8searchWinSize")
                '_OutputMergeWriter.WriteLine("HEADER," & dttm & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",LTE,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + LogTrace + ",-999,-999,-999,-999,-999,-999,SIB1cellBarred|SIB1intraFreqReselection|SIB1qRxLevMin(dbm)|SIB1pMax,SIB2RAPreambles|SIB2PwrRampStep|SIB2InitRecdTrgPwr|SIB2PreambleTransMax|SIB2RARespWinSize|SIB2MACContResTimer|SIB2maxHARQMsg3Tx|SIB2PDSCHRefSigPwr|SIB2PDSCHPb|SIB2SRSbwConfig|SIB2SRSsubframeConfig|SIB2ACKNACKsimultTx|SIB2P0NominalPUSCH|SIB2Alpha|SIB2P0NomPUCCH|SIB2t300|SIB2t301|SIB2t310|SIB2n310|SIB2t311|SIB2n311,SIB3qHyst|SIB3sNonIntraSearch(db)|SIB3ThreshServLow(db)|SIB3cellReselPriority|SIB3qRxLevMin(dbm)|SIB3pMax(dbm)|SIB3allowedMeasBandwidth|SIB3sIntraSearch(db),SIB4IntraFreqCellList|SIB4PCIs|SIB4OffsetPCIs|SIB4BlackCellList|SIB4BlackCells,SIB5qRxLevMin(dbm)|SIB5pMax(dbm)|SIB5tReslectEUTRA(s)|SIB5threshXhi(db)|SIB5threshXlo(db),SIB6ThreshXHi(db)|SIB6ThreshXLo(db)|SIB6qRxLevMin(dbm)|SIB6pMaxUTRA|SIB6qQualMin(db),SIB8cdmaEUTRASynch|SIB8synchSysTime|SIB8searchWinSize")

                If _isLibMode Then
                    Try
                        Dim fs As FileStream = _OutputMergeWriter.BaseStream
                        Dim FileFullName As String = fs.Name
                        'Insert to tblSCNRParse

                        Dim propscnrparse As New tblSCNRParse
                        propscnrparse.band = ""
                        propscnrparse.id_server_trans = fileInputObjMerge.id_server_transaction
                        propscnrparse.id_server_file = fileInputObjMerge.id_server_file
                        propscnrparse.operator_name = "PARSE"
                        propscnrparse.status_flag = "1"
                        propscnrparse.isMerged = 1
                        propscnrparse.tech = TechType
                        If FileFullName.Length > 0 Then
                            propscnrparse.scnr_file_name = FileFullName.Substring(FileFullName.LastIndexOf("\") + 1)
                        End If
                        propscnrparse.file_path = ScannerCommon.GetGAPPDestPath()

                        ScannerCommon._tblSCNRParseListLTE.Add(propscnrparse)
                        _OutputMergeWriter.Close()
                        My.Computer.FileSystem.MoveFile(FileFullName, ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name, True)
                        If ScannerCommon.ChkSpectrumFile(FileFullName) Then
                            ZipMyFile(ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name)
                            File.Delete(ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name)
                        End If
                    Catch ex As Exception
                        _OutputMergeWriter.Close()
                    End Try
                Else
                    _OutputMergeWriter.Close()
                End If

            End If
        End If




        If TechType <> "SPECTRUMSCAN" Then
            _OutputWriter = New StreamWriter(outputFileName)
        End If

        If TechType = "CDMA" Then
            _OutputWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,Spread_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,Spread_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,Spread_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,Spread_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,Spread_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,Spread_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
            '_OutputWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,ULInterference_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,ULInterference_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,ULInterference_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,ULInterference_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,ULInterference_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,ULInterference_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
        ElseIf TechType = "UMTS" Then
            _OutputWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
            '_OutputWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,ULInterference_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,ULInterference_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,ULInterference_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,ULInterference_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,ULInterference_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,ULInterference_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
        ElseIf TechType = "LTE" Then
            'modified by My 05/31/2018 #1278
            _OutputWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,NumTxAntennaPortsWideBand_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,NumTxAntennaPortsWideBand_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,NumTxAntennaPortsWideBand_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,NumTxAntennaPortsWideBand_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,NumTxAntennaPortsWideBand_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,NumTxAntennaPortsWideBand_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
            '_OutputWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,DelaySpread_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,DelaySpread_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,DelaySpread_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,DelaySpread_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,DelaySpread_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,DelaySpread_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")


        ElseIf TechType = "SPECTRUMSCAN" Then
            'Time_Stamp	Lat	Lon	MsgID	FreqID	Technology	Band	FreqkHz	RSSI_dBm	RSSI_watts	FileName	Collected_Date	MarketName	Campaign	ClientName	LogTrace
            '_OutputWriter.WriteLine("Time_Stamp,Lat,Lon,MsgID,FreqID,Technology,Band,FreqkHz,RSSI_dBm,RSSI_watts,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
        End If

        myFileInfo.FileName = fileInputObj.FileName
        myFileInfo.CDMAChannels = ""
        myFileInfo.WCDMAChannels = ""
        myFileInfo.GSMChannels = ""
        myFileInfo.LTEChannels = ""




        CDMAChannelList = New Hashtable()
        LTEChannelList = New Hashtable()
        WCDMAChannelList = New Hashtable()

        CDMAChannelListWithTime = New Hashtable()
        LTEChannelListWithTime = New Hashtable()
        WCDMAChannelListWithTime = New Hashtable()

        If TechType <> "SPECTRUMSCAN" Then
            GetChannelList(TechType, fileInputObj)
            GetDoNotExistList(fileInputObj.FileName, TechType)
        End If

        ScannerCommon.OpenParsingInfoWriter(filePath)

        Dim status_counter As Integer = ScannerCommon.GetStatusRecordCounter()

        If (TechType = "LTE" And (fileInputObj.FileMergeName.Trim = "" And Not fileInputObj.isSkip)) Or TechType = "UMTS" Or TechType = "CDMA" Or TechType = "SPECTRUMSCAN" Then
            Dim tmpFileName = outputFileName
            Dim myFile As New FileInfo(fileInputObj.FileName)
            Dim sizeInBytes As Long = myFile.Length
            Dim channelCount As String = LTEChannelList.Count.ToString()
            If TechType = "UMTS" Then
                channelCount = WCDMAChannelList.Keys.Count.ToString()
            End If
            If TechType = "CDMA" Then
                channelCount = CDMAChannelList.Keys.Count.ToString()
            End If

            If (TechType = "UMTS" Or TechType = "CDMA") And (fileInputObj.FileName.IndexOf("-3030-S") > 0) Then
                channelCount = "0"
                tmpFileName = "None"
            End If

            If TechType = "SPECTRUMSCAN" Then
                channelCount = "0"
            End If

            If TechType <> "SPECTRUMSCAN" Then
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",---------------------------------------------------------------------------------------------------")
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Input:  " + fileInputObj.FileName)
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",File Tech: " + TechType)
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Channel Count: " + channelCount)
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Output: " + tmpFileName)

            End If


        End If





        isUMTSFirstRecord = True
        isLTEFirstRecord = True
        isCDMAFirstRecord = True

        isFirstRecord = True


        newFile = True

        errCtr = 0
        ctr = 0
        Try                                                                             'add error capture if no SQLite3 installed (v12.1.1)
            cnn = New SQLiteConnection
            cnn.ConnectionString = "Data Source=" & fileInputObj.FileName.Replace("\", "\\") & ";Version=3;New=False;Compress=True;"
            cmd = New SQLiteCommand
            cmd.Connection = cnn
            cmd.CommandText = "select Config  from Device where Config like '%<UnitSettings%'"
            cmd.Connection.Open()

            dr = cmd.ExecuteReader()
            While dr.Read()
                Dim ss As String = dr("Config")
                Dim match As Match = Regex.Match(ss, "<Timezone>(\d+)</Timezone>")

                If match.Success Then
                    Dim gg As String = match.Groups(1).Value

                    If Integer.TryParse(gg, tmpInt) Then
                        TimeZoneValue = tmpInt
                    End If
                End If
            End While
            dr.Close()
            dr.Dispose()
            cmd.Dispose()

        Catch
            MsgBox("SQLite3 is not installed on this system" & Chr(10) & Chr(10) & "Install SQLite3 locally by doing the following:" & Chr(10) _
                   & "    1. Run S:\Software\SQLite\SQLite-1.0.62.0-setup.zip" & Chr(10) _
                   & "    2. Run S:\Software\SQLite\dcsqlitefree.zip", MsgBoxStyle.Critical)
            'End
        End Try

        Dim msgID As String
        Dim msgType As String


        cmd = New SQLiteCommand
        cmd.Connection = cnn
        cmd.CommandText = "select ID from message where MessageType = 1001 or MessageType = 1023 order by ID Desc limit 2000 "
        dr = cmd.ExecuteReader()
        Dim count As Integer = 0
        Dim isCorrectedGPS As Boolean = False

        If dr.HasRows Then
            dr.Read()
            ID = Convert.ToInt32(dr("ID"))

            While (dr.Read() And Not isCorrectedGPS)
                If ID - Convert.ToInt32(dr("ID")) = 1 Then
                    count = count + 1
                Else
                    count = 0
                End If
                If count > 10 Then
                    isCorrectedGPS = True
                    Exit While
                End If
                ID = Convert.ToInt32(dr("ID"))
            End While
        End If

        dr.Close()
        cmd.Dispose()

        count_total_message = 0


        '137995
        '100221
        Dim start3030 As DateTime = Nothing
        Dim end3030 As DateTime
        Dim start3040 As DateTime = Nothing
        Dim end3040 As DateTime
        Dim startMerge As DateTime = Nothing
        Dim endMerge As DateTime = Nothing
        Dim isStartMerge As Boolean = False
        Dim isStart3040 As Boolean = False
        Dim isStart3030 As Boolean = False


        If fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip Then
            If fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip Then
                For i = 0 To _DataRowList.Count - 1
                    'If i > 20000 Then
                    '    Exit For
                    'End If

                    If Not isStartMerge Then
                        startMerge = _DataRowList(i).TimeStampCreated
                        isStartMerge = True
                    End If
                    endMerge = _DataRowList(i).TimeStampCreated
                    If _DataRowList(i).FileName.ToLower.EndsWith("3030-s") Then
                        If Not isStart3030 Then
                            start3030 = _DataRowList(i).TimeStampCreated
                            isStart3030 = True
                        End If
                        end3030 = _DataRowList(i).TimeStampCreated
                    End If
                    If _DataRowList(i).FileName.ToLower.EndsWith("3040-s") Then
                        If Not isStart3040 Then
                            start3040 = _DataRowList(i).TimeStampCreated
                            isStart3040 = True
                        End If
                        end3040 = _DataRowList(i).TimeStampCreated
                    End If



                    ID = _DataRowList(i).ID
                    dttm = _DataRowList(i).TimeStampCreated
                    dttmDt = _DataRowList(i).TimeStampCreated
                    dttmOriginal = _DataRowList(i).TimeStampCreatedOriginal
                    _FileName = _DataRowList(i).FileName

                    If _FileName.IndexOf(".") >= 0 Then
                        _FileName = _FileName.Substring(0, _FileName.LastIndexOf("."))
                    End If
                    'modified by My 06/15/2018 task 1261
                    dttmFormat = Convert.ToDateTime(dttm).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)

                    MsgDecoder(False, TechType, _DataRowList(i).MessageData, _DataRowList(i).MessageType, _DataRowList(i).FileName, fileInputObj)
                Next

                _FileDuration3030 = (end3030 - start3030).TotalSeconds
                _FileDuration3040 = (end3040 - start3040).TotalSeconds
                _FileDurationMerge = (endMerge - startMerge).TotalSeconds

                MsgDecoder(True, TechType, _DataRowList(_DataRowList.Count - 1).MessageData, _DataRowList(_DataRowList.Count - 1).MessageType, _DataRowList(_DataRowList.Count - 1).FileName, fileInputObj)

            End If


            GetInputChannelLTE(fileInputObj)

        Else
            cmd = New SQLiteCommand
            cmd.Connection = cnn


            If isCorrectedGPS Then
                cmd.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message ORDER BY TimeStampCreated "
            Else
                cmd.CommandText = "select ID, MessageData, TimeStampCreated, MessageType  from message "
            End If

            If cmd.Connection.State <> ConnectionState.Open Then cmd.Connection.Open()
            dr = cmd.ExecuteReader()
            Try
                While dr.Read
                    If Not (IsDBNull(dr("MessageData")) Or IsDBNull(dr("TimeStampCreated"))) Then

                        msgData = dr("MessageData")

                        dttm = DateTime.FromOADate(CDbl(dr("TimeStampCreated")))

                        If Not isStart3030 Then
                            isStart3030 = True
                            start3030 = dttm
                        End If

                        end3030 = dttm

                        dttmOriginal = dttm

                        dttmDt = DateTime.FromOADate(CDbl(dr("TimeStampCreated")))

                        If TimeZoneValue <> 0 Then
                            dttm = DateTime.FromOADate(CDbl(dr("TimeStampCreated"))).AddMinutes(TimeZoneValue)
                            dttmDt = DateTime.FromOADate(CDbl(dr("TimeStampCreated"))).AddMinutes(TimeZoneValue)

                        End If
                        'modified by My 06/15/2018 task 1261
                        dttmFormat = Convert.ToDateTime(dttm).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)

                        If newFile Then
                            lastCDMATimeStamp = dttm
                            newFile = False
                        End If
                        msgType = Convert.ToInt32(dr("MessageType"))

                        If dr("MessageType") = "12288" Then
                            xmlChannelString = enc.GetString(msgData)
                        End If

                        If msgType = 1001 Then
                            strMsgData = enc.GetString(msgData).Split(",")
                            lat = strMsgData(0)
                            lon = strMsgData(1)
                        End If

                        ID = Convert.ToInt32(dr("ID"))


                        MsgDecoder(False, TechType, msgData, dr("MessageType"), "", fileInputObj)
                    End If
                End While

                _FileDuration3030 = (end3030 - start3030).TotalSeconds

            Catch ex As Exception
                If _isLibMode Then
                    Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
                    dbclass.TrapErrorInDB(ex.ToString, "", fileInputObj.FileName)
                End If

            End Try

            MsgDecoder(True, TechType, msgData, 1, "", fileInputObj)
            dr.Close()
            cmd.Dispose()
            cnn.Close()
            GetChannelInfo(xmlChannelString, FileInfoLog, fileInputObj.FileName)
        End If



        If (TechType = "LTE" And (fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip)) Then

            'Dim myFile As New FileInfo(fileInputObj.FileMergeName)
            'Dim sizeInBytes As Long = myFile.Length

            Dim myFile1 As New FileInfo(outputFileName)
            Dim sizeInByte1 As Long = myFile1.Length


            Dim tmpFileMergeName As String = fileInputObj.FileMergeName.Substring(fileInputObj.FileMergeName.LastIndexOf("\") + 1)

            tmpFileMergeName = tmpFileMergeName.Substring(0, tmpFileMergeName.LastIndexOf("."))

            Dim outputFileMergeName As String = filePath + "\" + tmpFileMergeName + "_PARSE_" + TechType + ".txt"



            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",---------------------------------------------------------------------------------------------------")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Input:  " + fileInputObj.FileName)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",File Tech: " + TechType)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Channel Count: " + LTEChannelList3040.Count.ToString())
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Output: " + outputFileName)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Size: " + _FileSize3040.ToString())

            ScannerCommon.SetStatusRecordCounter(ScannerCommon.GetStatusRecordCounter() + 1)
            status_counter = ScannerCommon.GetStatusRecordCounter()

            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",---------------------------------------------------------------------------------------------------")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Input:  " + fileInputObj.FileMergeName)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",File Tech: " + TechType)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Channel Count: " + LTEChannelList3030.Count.ToString())
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Output: " + outputFileMergeName)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Size: " + (sizeInByte1 - _FileSize3040).ToString())

            ScannerCommon.SetStatusRecordCounter(ScannerCommon.GetStatusRecordCounter() + 1)
            status_counter = ScannerCommon.GetStatusRecordCounter()


            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",---------------------------------------------------------------------------------------------------")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 1 Input: " + fileInputObj.FileName)
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 2 Input: " + fileInputObj.FileMergeName)

            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 1 Duration: " + _FileDuration3040.ToString() + " (sec)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 2 Duration: " + _FileDuration3030.ToString() + " (sec)")

            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 1 ScanRate: " + _ScanRate3040.ToString() + " (messages/sec)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 2 ScanRate: " + _ScanRate3030.ToString() + " (messages/sec)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 1 ScanPeriod: " + _ScanPeriod3040.ToString() + " (sec to scan all channels)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File 2 ScanPeriod: " + _ScanPeriod3030.ToString() + " (sec to scan all channels)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Merging File ABS ScanPeriod: " + Math.Round(_FileDurationMerge / _ScanID, 2).ToString() + " (sec to scan all channels)")

        End If


        If (TechType = "LTE" And (fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip)) Then
            Dim myFile As New FileInfo(outputFileName)
            Dim sizeInBytes As Long = myFile.Length
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Merging File Size: " + sizeInBytes.ToString())
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Merging Completed")
            ScannerCommon.SetStatusRecordCounter(ScannerCommon.GetStatusRecordCounter() + 1)
        ElseIf TechType = "CDMA" Or TechType = "UMTS" Or TechType = "LTE" Then
            Dim myFile As New FileInfo(outputFileName)
            Dim sizeInBytes As Long = myFile.Length
            If TechType = "CDMA" Then
                _ScanRate3030 = Math.Round(_count_CDMA_Record / _FileDuration3030, 2)
            End If

            If TechType = "UMTS" Then
                _ScanRate3030 = Math.Round(_count_UMTS_Record / _FileDuration3030, 2)
            End If


            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File Duration: " + _FileDuration3030.ToString() + " (sec)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File ScanRate: " + _ScanRate3030.ToString() + " (messages/sec)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + status_counter.ToString() + ",Parsing File ScanPeriod: " + Math.Round(_FileDuration3030 / _ScanID, 2).ToString() + " (sec to scan all channels)")
            ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Parsing File Size: " + sizeInBytes.ToString())
            ScannerCommon.SetStatusRecordCounter(ScannerCommon.GetStatusRecordCounter() + 1)
        Else
            'Dim myFile As New FileInfo(outputFileName)
            'Dim sizeInBytes As Long = myFile.Length
            'ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + file_index.ToString() + ",Parsing File Size: " + sizeInBytes.ToString())
            'ScannerCommon.SetStatusRecordCounter(ScannerCommon.GetStatusRecordCounter() + 1)
        End If



        GC.Collect()

        GetChannelFound(TechType, fileInputObj)

        Dim tmpOperatorSet As New Hashtable()
        Dim tmpOperatorBandSet As New Hashtable()

        For Each key In _OperatorSet.Keys
            Dim tmpFileWriter As New OutputFileWriter()
            tmpFileWriter = _OperatorSet.Item(key)
            If IsNothing(tmpFileWriter.FileFullName) Then
                Dim fs As FileStream = tmpFileWriter.writer.BaseStream

                If Not IsNothing(fs) Then
                    tmpFileWriter.FileFullName = fs.Name
                End If

            End If

            tmpFileWriter.writer.Close()
            tmpOperatorSet.Item(key) = tmpFileWriter

        Next

        For Each key In _OperatorBandSet.Keys
            Dim tmpFileWriter As New OutputFileWriter()
            tmpFileWriter = _OperatorBandSet.Item(key)
            If IsNothing(tmpFileWriter.FileFullName) Then
                Dim fs As FileStream = tmpFileWriter.writer.BaseStream
                If Not IsNothing(fs) Then
                    tmpFileWriter.FileFullName = fs.Name
                End If
            End If

            If TechType = "SPECTRUMSCAN" Then

                Dim myFile As New FileInfo(tmpFileWriter.FileFullName)
                Dim sizeInBytes As Long = myFile.Length

                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",---------------------------------------------------------------------------------------------------")
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Parsing File Input: " + fileInputObj.FileName)
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Parsing File Tech:  SPECTRUMSCAN")
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Parsing File Channel Count:  0")
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Parsing File Output: " + tmpFileWriter.FileFullName)
                ScannerCommon.WriteParsingInfoWriter(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "," + ScannerCommon.GetStatusRecordCounter().ToString() + ",Parsing File Size: " + sizeInBytes.ToString)

                ScannerCommon.SetStatusRecordCounter(ScannerCommon.GetStatusRecordCounter() + 1)


            End If

            tmpFileWriter.writer.Close()
            tmpOperatorBandSet.Item(key) = tmpFileWriter
        Next

        ScannerCommon.CloseParsingInfoWriter()

        If _isLibMode Then
            Try
                Dim fs As FileStream = _OutputWriter.BaseStream
                Dim FileFullName As String = fs.Name
                'Insert to tblSCNRParse
                'Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
                Dim propscnrparse As New tblSCNRParse
                propscnrparse.band = ""
                propscnrparse.id_server_trans = fileInputObj.id_server_transaction
                propscnrparse.id_server_file = fileInputObj.id_server_file
                propscnrparse.operator_name = "PARSE"
                propscnrparse.status_flag = "1"
                propscnrparse.tech = TechType


                If TechType = "LTE" Then
                    If fileInputObj.isSkip Or fileInputObj.FileMergeName.Trim() <> "" Then
                        propscnrparse.isMerged = 1
                    Else
                        propscnrparse.isMerged = 2
                    End If
                End If



                If FileFullName.Length > 0 Then
                    propscnrparse.scnr_file_name = FileFullName.Substring(FileFullName.LastIndexOf("\") + 1)
                End If
                propscnrparse.file_path = ScannerCommon.GetGAPPDestPath()
                'dbclass.InserttblSCNRParse(propscnrparse)
                If TechType = "LTE" Then
                    ScannerCommon._tblSCNRParseListLTE.Add(propscnrparse)
                Else
                    ScannerCommon._tblSCNRParseList.Add(propscnrparse)
                End If

                _OutputWriter.Close()

                If TechType <> "SPECTRUMSCAN" Then
                    AddRecNoSType2(FileFullName, TechType)
                End If


                My.Computer.FileSystem.MoveFile(FileFullName, ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name, True)


            Catch ex As Exception

            End Try
        Else


            If TechType <> "SPECTRUMSCAN" Then
                Dim fs As FileStream = _OutputWriter.BaseStream
                Dim FileFullName As String = fs.Name

                _OutputWriter.Close()
                AddRecNoSType2(FileFullName, TechType)
            End If



        End If

        _OperatorSet = tmpOperatorSet
        _OperatorBandSet = tmpOperatorBandSet

        For i = 0 To ScannerCommon.FileLogInfoList.Count - 1
            If ScannerCommon.FileLogInfoList(i).FileName = fileInputObj.FileName Or ScannerCommon.FileLogInfoList(i).FileName = fileInputObj.FileMergeName Then
                ScannerCommon.FileLogInfoList(i).ParseTechList.Add(TechType)
                If _isLibMode Then
                    ScannerCommon.FileLogInfoList(i).path = ScannerCommon.GetGAPPDestPath()
                End If

            End If
        Next

        For Each key In _OperatorSet.Keys
            Dim tmpFileWriter As New OutputFileWriter()
            tmpFileWriter = _OperatorSet.Item(key)


            Dim FileFullName As String = ""
            FileFullName = tmpFileWriter.FileFullName

            If _isLibMode Then

                Try

                    ' dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
                    Dim propscnrparse As New tblSCNRParse
                    propscnrparse.band = "ALL"
                    propscnrparse.id_server_trans = fileInputObj.id_server_transaction
                    propscnrparse.id_server_file = fileInputObj.id_server_file
                    propscnrparse.operator_name = tmpFileWriter.OperatorName
                    propscnrparse.status_flag = "1"
                    propscnrparse.tech = TechType

                    If TechType = "LTE" Then
                        If fileInputObj.isSkip Or fileInputObj.FileMergeName.Trim() <> "" Then
                            propscnrparse.isMerged = 1
                        Else
                            propscnrparse.isMerged = 2
                        End If
                    End If



                    If FileFullName.Length > 0 Then
                        propscnrparse.scnr_file_name = FileFullName.Substring(FileFullName.LastIndexOf("\") + 1)
                    End If
                    propscnrparse.file_path = ScannerCommon.GetGAPPDestPath()
                    'dbclass.InserttblSCNRParse(propscnrparse)

                    If TechType = "LTE" Then
                        ScannerCommon._tblSCNRParseListLTE.Add(propscnrparse)
                    Else
                        ScannerCommon._tblSCNRParseList.Add(propscnrparse)
                    End If

                    'tmpFileWriter.writer.Close()

                    RemoveLastMAXRecord(FileFullName)

                    My.Computer.FileSystem.MoveFile(FileFullName, ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name, True)

                    If ScannerCommon.ChkSpectrumFile(FileFullName) Then

                        Dim gapp_file_name As String = ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name

                        ZipMyFile(ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name)
                        File.Delete(ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name)

                        ScannerCommon._lstSpectrumScanFiles.Add(IO.Path.GetDirectoryName(gapp_file_name) & "\" & IO.Path.GetFileNameWithoutExtension(gapp_file_name) & ".zip")


                    End If
                Catch ex As Exception

                End Try

            Else
                'tmpFileWriter.writer.Close()
                RemoveLastMAXRecord(FileFullName)

            End If




        Next

        For Each key In _OperatorBandSet.Keys
            Dim tmpFileWriter As New OutputFileWriter()
            tmpFileWriter = _OperatorBandSet.Item(key)


            Dim FileFullName As String = ""
            FileFullName = tmpFileWriter.FileFullName

            If _isLibMode Then
                Try
                    'Insert to tblSCNRParse
                    If key.ToString.IndexOf(TechType) >= 0 Or TechType = "SPECTRUMSCAN" Then
                        Dim band As String = ""
                        If TechType = "SPECTRUMSCAN" Then
                            band = key
                        Else
                            band = key.ToString.Replace(TechType, "|").Split("|")(1)
                        End If

                        'Dim dbclass = New SCNR_DB_Layer(ScannerCommon.GetGAPPSQLConnectionString())
                        Dim propscnrparse As New tblSCNRParse
                        propscnrparse.band = band
                        propscnrparse.id_server_trans = fileInputObj.id_server_transaction
                        propscnrparse.id_server_file = fileInputObj.id_server_file
                        Dim OperatorName As String = ""
                        If TechType = "SPECTRUMSCAN" Then
                            OperatorName = "PARSE"
                        Else
                            If tmpFileWriter.OperatorName.IndexOf(TechType) >= 0 Then
                                OperatorName = tmpFileWriter.OperatorName.Substring(0, tmpFileWriter.OperatorName.IndexOf(TechType))
                            End If
                        End If

                        propscnrparse.operator_name = OperatorName
                        propscnrparse.status_flag = "1"
                        propscnrparse.tech = TechType


                        If TechType = "LTE" Then
                            If fileInputObj.isSkip Or fileInputObj.FileMergeName.Trim() <> "" Then
                                propscnrparse.isMerged = 1
                            Else
                                propscnrparse.isMerged = 2
                            End If
                        End If

                        If FileFullName.Length > 0 Then
                            propscnrparse.scnr_file_name = FileFullName.Substring(FileFullName.LastIndexOf("\") + 1)
                        End If
                        propscnrparse.file_path = ScannerCommon.GetGAPPDestPath()

                        If TechType = "LTE" Then
                            ScannerCommon._tblSCNRParseListLTE.Add(propscnrparse)
                        Else
                            ScannerCommon._tblSCNRParseList.Add(propscnrparse)
                        End If
                        'tmpFileWriter.writer.Close()
                        If TechType <> "SPECTRUMSCAN" Then
                            RemoveLastMAXRecord(FileFullName)
                        End If


                        My.Computer.FileSystem.MoveFile(FileFullName, ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name, True)



                        If ScannerCommon.ChkSpectrumFile(FileFullName) Then

                            Dim gapp_file_name As String = ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name

                            ZipMyFile(ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name)
                            File.Delete(ScannerCommon.GetGAPPDestPath() + propscnrparse.scnr_file_name)

                            ScannerCommon._lstSpectrumScanFiles.Add(IO.Path.GetDirectoryName(gapp_file_name) & "\" & IO.Path.GetFileNameWithoutExtension(gapp_file_name) & ".zip")


                        End If
                    End If


                Catch ex As Exception

                End Try
            Else
                'tmpFileWriter.writer.Close()
                If TechType <> "SPECTRUMSCAN" Then
                    RemoveLastMAXRecord(FileFullName)
                End If

            End If

        Next

        Dim EndTime As DateTime = DateTime.Now
        GC.Collect()
        cmd.Dispose()
        cmd = Nothing
        cnn = Nothing
        dr = Nothing
        msgData = Nothing


    End Sub


    Private Function ToByteArray(ByVal HexString As String) As Byte()
        '01B716881301000000000100000000000000000000BCFF
        Dim NumChar As Integer = HexString.Length
        Dim ByteArray As Byte()
        ReDim ByteArray(NumChar / 2)
        Dim i As Integer
        For i = 0 To NumChar - 2 Step 2
            ByteArray(i / 2) = Convert.ToByte(HexString.Substring(i, 2), 16)

        Next i
        Return ByteArray
    End Function

    Private Sub RemoveLastMAXRecord(ByVal fileName As String)
        If Not IsNothing(fileName) Then
            Dim lines As List(Of String) = System.IO.File.ReadAllLines(fileName).ToList()
            If lines(lines.Count - 1).IndexOf(",MAX,") > 0 Then
                File.WriteAllLines(fileName, lines.GetRange(0, lines.Count - 1).ToArray())
            End If

        End If


    End Sub


    Private Sub AddRecNoSType2(ByVal fileName As String, ByVal TechType As String)

        Dim _OperatorSetRow As SortedDictionary(Of Double, String)
        Dim _OperatorBandSetRow As SortedDictionary(Of Double, String)



        Dim _OperatorSet1 As New Hashtable()
        Dim _OperatorBandSet1 As New Hashtable()
        Dim OperatorPosition As New Hashtable()


        Dim tmpFileWriter As New OutputFileWriter()
        Dim tmpFileWriterBand As New OutputFileWriter()

        Dim tmpDouble As Double = -1000

        For Each key In _OperatorSet.Keys
            tmpFileWriter = _OperatorSet.Item(key)

            tmpFileWriter.MapParseToBand = New Hashtable()

            _OperatorSetRow = New SortedDictionary(Of Double, String)
            If Not IsNothing(tmpFileWriter.FileFullName) Then

                Dim tmpStr As String
                Dim count_row As Integer = -1
                For Each tmpStr In System.IO.File.ReadAllLines(tmpFileWriter.FileFullName).ToList()
                    count_row = count_row + 1
                    Dim tmpStrs = tmpStr.Split(",")
                    If tmpStrs(0).IndexOf("|") > 0 Then
                        If tmpStr.IndexOf(",MAX,") < 0 Then
                            _OperatorSetRow.Add(count_row, tmpStr)
                            tmpFileWriter.MapParseToBand.Add(Convert.ToDouble(tmpStrs(0).Split("|")(0)), count_row)
                        Else
                            _OperatorSetRow.Add(count_row, tmpStr)
                            tmpFileWriter.MapParseToBand.Add(Convert.ToDouble(tmpStrs(0).Split("|")(0) + 0.2), count_row)

                        End If
                    Else
                        _OperatorSetRow.Add(count_row, tmpStr)
                    End If


                    tmpFileWriter.OperatorSetRow = _OperatorSetRow

                    _OperatorSet1.Item(key) = tmpFileWriter
                Next
            End If
        Next

        _OperatorSet = _OperatorSet1

        For Each key In _OperatorBandSet.Keys
            tmpFileWriter = _OperatorBandSet.Item(key)
            tmpFileWriter.MapParseToBand = New Hashtable()

            _OperatorBandSetRow = New SortedDictionary(Of Double, String)

            If Not IsNothing(tmpFileWriter.FileFullName) Then

                Dim tmpStr As String
                Dim count_row As Integer = -1
                For Each tmpStr In System.IO.File.ReadAllLines(tmpFileWriter.FileFullName).ToList()
                    count_row = count_row + 1
                    Dim tmpStrs = tmpStr.Split(",")
                    If tmpStrs(0).IndexOf("|") > 0 Then
                        If tmpStr.IndexOf(",MAX,") < 0 Then
                            _OperatorBandSetRow.Add(count_row, tmpStr)
                            tmpFileWriter.MapParseToBand.Add(Convert.ToDouble(tmpStrs(0).Split("|")(0)), count_row)
                        Else
                            _OperatorBandSetRow.Add(count_row, tmpStr)
                            tmpFileWriter.MapParseToBand.Add(Convert.ToDouble(tmpStrs(0).Split("|")(0) + 0.2), count_row)
                        End If
                    Else
                        _OperatorBandSetRow.Add(count_row, tmpStr)

                    End If


                    tmpFileWriter.OperatorBandSetRow = _OperatorBandSetRow

                    _OperatorBandSet1.Item(key) = tmpFileWriter
                Next
            End If
        Next

        _OperatorBandSet = _OperatorBandSet1


        Dim lines As List(Of String) = System.IO.File.ReadAllLines(fileName).ToList()

        _OutputWriter = New StreamWriter(fileName)

        Dim count_temp As Integer = 0

        Dim tmpChannel As String

        Dim ChannelList As New List(Of String)
        Dim ChannelRemoveList As New List(Of String)
        Dim tmpHash As New Hashtable()

        For Each tmpChannel In _AddNoRecChannelList.Keys
            If Not ChannelWithAllRSSINev999.Item(tmpChannel) Then
                ChannelList.Add(tmpChannel)
                Dim tmpObj As New AddNoRecChannel()
                tmpObj.LastScanId = 1
                tmpObj.foundFirstRSSI = False
                tmpObj.firstScanIdRSSI = -1
                tmpObj.ScanIDList = New HashSet(Of Integer)()
                tmpObj.lastIndexFound = 0
                tmpHash.Add(tmpChannel, tmpObj)

            End If
        Next

        _AddNoRecChannelList = tmpHash


        Dim myStr, myStr_next As String

        _LastScanID = 1

        Dim row_count As Integer = -1


        Dim count_to_last As Integer = 0
        Dim i, j As Integer

        For i = 0 To lines.Count - 1

            myStr = lines(i)
            If i < lines.Count - 1 Then
                myStr_next = lines(i + 1)
            Else
                myStr_next = lines(i)
            End If



            row_count = row_count + 1

            If row_count < 2 Then

                _OutputWriter.WriteLine(myStr)
                _TempStringList = New List(Of String)
                Continue For
            End If

            _TempStringList.Add(myStr)

            Dim strs1 = myStr.Split(",")
            Dim strs1_next = myStr_next.Split(",")

            If TechType = "LTE" Then
                _ScanID = Convert.ToInt32(strs1(6))
                _currentRSSIValue = Convert.ToDouble(strs1(12))
            ElseIf TechType = "UMTS" Then
                _ScanID = Convert.ToInt32(strs1(7))
                _currentRSSIValue = Convert.ToDouble(strs1(17))
            Else
                _ScanID = Convert.ToInt32(strs1(7))
                _currentRSSIValue = Convert.ToDouble(strs1(14))
            End If


            Dim tmpObj As New AddNoRecChannel()
            Dim currentChannel As String = ""

            If TechType = "LTE" Then
                tmpObj = _AddNoRecChannelList.Item(strs1(7))
                currentChannel = strs1(7)
            ElseIf TechType = "UMTS" Then
                tmpObj = _AddNoRecChannelList.Item(strs1(8))
                currentChannel = strs1(8)
            Else
                tmpObj = _AddNoRecChannelList.Item(strs1(8))
                currentChannel = strs1(8)
            End If


            If Not tmpObj.isCheckRSSI Then
                If _currentRSSIValue > -999 Then
                    tmpObj.countRSSINev999 = 0
                    tmpObj.isCheckRSSI = True
                End If
            End If

            If _currentRSSIValue > -999 Then
                If tmpObj.firstScanIdRSSI < 0 Then
                    tmpObj.firstScanIdRSSI = _ScanID
                End If

                tmpObj.lastRSSI = _currentRSSIValue
                tmpObj.countRSSINev999 = 0
                tmpObj.isCheckRSSI = True
                tmpObj.LastScanIdRSSI = _ScanID
            End If



            'Find the next RSSI value

            Dim _nextRSSIValue As Double = -999

            For j = i + 1 To lines.Count - 1
                myStr_next = lines(j)
                strs1_next = myStr_next.Split(",")
                If TechType = "LTE" Then
                    If currentChannel = strs1_next(7) Then
                        _nextRSSIValue = Convert.ToDouble(strs1_next(12))

                        Exit For
                    End If
                ElseIf TechType = "UMTS" Then
                    If currentChannel = strs1_next(8) Then
                        _nextRSSIValue = Convert.ToDouble(strs1_next(17))
                        Exit For
                    End If
                Else
                    If currentChannel = strs1_next(8) Then
                        _nextRSSIValue = Convert.ToDouble(strs1_next(14))
                        Exit For
                    End If
                End If
            Next

            'If _ScanID = 3002 And strs1(7) = "8665" Then
            '    Dim tt As String = ""
            'End If

            tmpObj.nextRSSI = _nextRSSIValue

            If TechType = "LTE" Then
                _AddNoRecChannelList.Item(currentChannel) = tmpObj
            ElseIf TechType = "UMTS" Then
                _AddNoRecChannelList.Item(currentChannel) = tmpObj
            Else
                _AddNoRecChannelList.Item(currentChannel) = tmpObj
            End If

            'If _currentRSSIValue > -999 Then
            '    Continue For

            'End If

            If Not (_ScanID = tmpObj.LastScanId And row_count < lines.Count - 1) Then


                count_temp = 0


                For Each tmpChannel1 In ChannelList

                    If tmpChannel1 = currentChannel Then

                        tmpObj = _AddNoRecChannelList.Item(tmpChannel1)

                        If Not tmpObj.isCheckRSSI Then
                            If TechType = "LTE" Then
                                If tmpObj.lastRSSI < -99 Or tmpObj.nextRSSI < -120 Then
                                    tmpObj.countRSSINev999 = tmpObj.countRSSINev999 + 1
                                End If
                            ElseIf TechType = "UMTS" Then
                                If tmpObj.lastRSSI < -99 Or tmpObj.nextRSSI < -115 Then
                                    tmpObj.countRSSINev999 = tmpObj.countRSSINev999 + 1
                                End If
                            Else
                                If tmpObj.lastRSSI < -99 Or tmpObj.nextRSSI < -115 Then
                                    tmpObj.countRSSINev999 = tmpObj.countRSSINev999 + 1
                                End If

                            End If

                        Else
                            tmpObj.countRSSINev999 = 0
                        End If

                        'If _ScanID >= 1255 And tmpChannel1 = "5760" Then
                        '    Dim tt As String = ""
                        'End If

                        _LastScanID = _ScanID

                        tmpObj.isCheckRSSI = False
                        Dim isTypeC As Boolean = (tmpObj.countRSSINev999 > 2 And ((tmpObj.lastRSSI < -120 And tmpObj.lastRSSI > -999) Or (tmpObj.nextRSSI < -120 And tmpObj.nextRSSI > -999)))

                        If TechType = "UMTS" Or TechType = "CDMA" Then
                            isTypeC = (tmpObj.countRSSINev999 > 2 And ((tmpObj.lastRSSI < -115 And tmpObj.lastRSSI > -999) Or (tmpObj.nextRSSI < -115 And tmpObj.nextRSSI > -999)))
                        End If

                        Dim isTypeB As Boolean = (tmpObj.countRSSINev999 > 10 And (tmpObj.lastRSSI < -99 Or (tmpObj.nextRSSI < -99 And tmpObj.nextRSSI > -999)))



                        'If isTypeC Then
                        '    Dim tt As String = ""
                        'End If

                        If isTypeB Or isTypeC Then
                            'If _ScanID >= 443 And tmpChannel1 = "1000" Then
                            '    Dim tt As String = ""
                            'End If
                            Dim _TempStringList1 As New List(Of String)
                            For j = 0 To tmpObj.lastIndexFound - 1
                                If j < _TempStringList.Count Then
                                    _TempStringList1.Add(_TempStringList(j))
                                End If
                            Next

                            Dim myStr_loop As String
                            For j = tmpObj.lastIndexFound To _TempStringList.Count - 1

                                myStr_loop = _TempStringList(j)

                                Dim strs = myStr_loop.Split(",")

                                Dim currentScanId As Integer = 0

                                Dim currentChannelStr As String = ""

                                If TechType = "LTE" Then
                                    currentScanId = Convert.ToInt32(strs(6))
                                    currentChannelStr = strs(7).Trim
                                ElseIf TechType = "UMTS" Then
                                    currentScanId = Convert.ToInt32(strs(7))
                                    currentChannelStr = strs(8).Trim
                                Else
                                    currentScanId = Convert.ToInt32(strs(7))
                                    currentChannelStr = strs(8).Trim
                                End If

                                'If currentScanId = 1288 And currentChannelStr = "425" Then
                                '    Dim tt As String = ""

                                'End If
                                Dim isMod As Boolean = False
                                If (currentScanId > tmpObj.LastScanIdRSSI) Or currentScanId <= tmpObj.firstScanIdRSSI Or tmpObj.ScanIDList.Contains(currentScanId) Then



                                    If Not tmpObj.ScanIDList.Contains(currentScanId) Then
                                        tmpObj.ScanIDList.Add(currentScanId)
                                    End If


                                    If currentChannelStr = tmpChannel1 Then
                                        tmpObj.lastIndexFound = j
                                        Dim OperatorName = ""
                                        Dim OperatorBandName = ""
                                        If TechType = "LTE" Then
                                            OperatorName = strs(53)
                                            OperatorBandName = strs(53) + "LTE" + strs(8)
                                        ElseIf TechType = "UMTS" Then
                                            OperatorName = strs(66)
                                            OperatorBandName = strs(66) + "UMTS" + strs(5)
                                        Else
                                            OperatorName = strs(54)
                                            OperatorBandName = strs(54) + "CDMA" + strs(5)
                                        End If



                                        If TechType = "LTE" Then
                                            If strs(12) = "-999" Or strs(12) = "-130" Then

                                                strs(12) = "-130"
                                                If strs(60).IndexOf("ADDrecNoS") < 0 Then
                                                    strs(60) = IIf(strs(60).Trim <> "", strs(60) + "|ADDrecNoS", "ADDrecNoS")
                                                End If
                                                isMod = True


                                            End If
                                        ElseIf TechType = "UMTS" Then
                                            If strs(17) = "-999" Or strs(17) = "-130" Then
                                                strs(17) = "-130"
                                                If strs(73).IndexOf("ADDrecNoS") < 0 Then
                                                    strs(73) = IIf(strs(73).Trim <> "", strs(73) + "|ADDrecNoS", "ADDrecNoS")
                                                End If
                                                isMod = True
                                            End If
                                        Else
                                            If strs(14) = "-999" Or strs(14) = "-130" Then
                                                strs(14) = "-130"
                                                If strs(61).IndexOf("ADDrecNoS") < 0 Then
                                                    strs(61) = IIf(strs(61).Trim <> "", strs(61) + "|ADDrecNoS", "ADDrecNoS")
                                                End If
                                                isMod = True
                                            End If

                                        End If

                                        If isMod Then
                                            Dim tt1 As Double = Convert.ToInt32(myStr_loop.Substring(0, myStr_loop.IndexOf("|")))
                                            If OperatorName.Trim <> "" Then
                                                tmpFileWriter = _OperatorSet.Item(OperatorName)
                                                If Not IsNothing(tmpFileWriter.MapParseToBand) Then
                                                    If tmpFileWriter.MapParseToBand.ContainsKey(tt1) Then
                                                        tmpFileWriter.OperatorSetRow.Item(tmpFileWriter.MapParseToBand.Item(tt1)) = String.Join(",", strs)
                                                    End If
                                                    If tmpFileWriter.MapParseToBand.ContainsKey(tt1 + 0.2) Then
                                                        tmpFileWriter.OperatorSetRow.Item(tmpFileWriter.MapParseToBand.Item(tt1 + 0.2)) = String.Join(",", strs).Replace(",ALL,", ",MAX,")
                                                    End If
                                                End If


                                            End If

                                            If OperatorBandName.Trim() <> "" Then
                                                tmpFileWriterBand = _OperatorBandSet.Item(OperatorBandName)
                                                If Not IsNothing(tmpFileWriterBand.MapParseToBand) Then
                                                    If tmpFileWriterBand.MapParseToBand.ContainsKey(tt1) Then
                                                        tmpFileWriterBand.OperatorBandSetRow.Item(tmpFileWriterBand.MapParseToBand.Item(tt1)) = String.Join(",", strs)
                                                    End If
                                                    If tmpFileWriterBand.MapParseToBand.ContainsKey(tt1 + 0.2) Then
                                                        tmpFileWriterBand.OperatorBandSetRow.Item(tmpFileWriterBand.MapParseToBand.Item(tt1 + 0.2)) = String.Join(",", strs).Replace(",ALL,", ",MAX,")
                                                    End If

                                                End If

                                            End If



                                        End If



                                    End If

                                End If
                                myStr_loop = String.Join(",", strs)
                                _TempStringList1.Add(myStr_loop)

                            Next

                            _TempStringList = New List(Of String)
                            _TempStringList = _TempStringList1

                        End If

                        If tmpObj.countRSSINev999 = 0 Then
                            count_temp = count_temp + 1
                        End If

                        _AddNoRecChannelList.Item(tmpChannel) = tmpObj
                    End If

                Next


            End If

            tmpObj.LastScanId = _ScanID
            _AddNoRecChannelList.Item(currentChannel) = tmpObj

            If count_temp >= ChannelList.Count Or row_count >= lines.Count - 1 Then
                count_temp = 0

                Dim line As String

                For Each line In _TempStringList
                    Dim str As String = line.Substring(line.IndexOf("|") + 1, line.Length - line.IndexOf("|") - 1)
                    _OutputWriter.WriteLine(str)
                Next
                _TempStringList = New List(Of String)

            End If


        Next

        _OutputWriter.Close()

        For Each tmpFileWriter In _OperatorSet.Values
            If Not IsNothing(tmpFileWriter.FileFullName) Then
                Dim tmpList As New List(Of String)
                Dim count As Integer = 0
                For Each item In tmpFileWriter.OperatorSetRow.Values
                    count = count + 1
                    If count >= 3 Then
                        Dim str As String = item.Substring(item.IndexOf("|") + 1, item.Length - item.IndexOf("|") - 1)
                        tmpList.Add(str)
                    Else
                        tmpList.Add(item)
                    End If
                Next
                File.WriteAllLines(tmpFileWriter.FileFullName, tmpList)

            End If
        Next

        For Each tmpFileWriter In _OperatorBandSet.Values
            If Not IsNothing(tmpFileWriter.FileFullName) Then
                Dim tmpList As New List(Of String)
                Dim count As Integer = 0
                For Each item In tmpFileWriter.OperatorBandSetRow.Values
                    count = count + 1
                    If count >= 3 Then
                        Dim str As String = item.Substring(item.IndexOf("|") + 1, item.Length - item.IndexOf("|") - 1)
                        tmpList.Add(str)
                    Else
                        tmpList.Add(item)
                    End If

                Next
                File.WriteAllLines(tmpFileWriter.FileFullName, tmpList)

            End If
        Next

    End Sub


    Private Function MsgDecoder(ByVal isLast As Boolean, ByVal type As String, ByVal msgData() As Byte, ByVal msgType As Integer, ByVal InputFileName As String, ByVal fileInputObj As ScannerFileInputObject)
        If msgType <> 1001 Then
            'If dttmOriginal.ToString().StartsWith("12/2/2016 12:16:46 AM") Then
            '    Dim tt As String = ""
            'End If
            '12/2/2016  12:16:46 AM
            If type = "LTE" And fileInputObj.FileMergeName.Trim <> "" And InputFileName.Contains("-3030") Then
                GetLatLonFromLinearInterpolation(ListGPSLossLatLon3030, ID, Convert.ToDateTime(dttmOriginal).Ticks)
            Else
                GetLatLonFromLinearInterpolation(ListGPSLossLatLon, ID, Convert.ToDateTime(dttmOriginal).Ticks)
            End If

        End If

        If isLast Then
            If type = "CDMA" Then
                _isOutputCDMA = True
                GoTo lineoutput
            ElseIf type = "UMTS" Then
                _isOutputWCDMA = True
                GoTo lineoutputWCDMA
            ElseIf type = "LTE" Then
                _isOutputLTE = True
                GoTo lineoutputLTE

            End If

        End If

        _isOutputCDMA = False
        _isOutputGSM = False
        _isOutputWCDMA = False
        _isOutputLTE = False


        Dim myStr As String = ""
        Dim strMsgData() As String
        Dim LTEMessageVersion As Integer
        Dim enc As New System.Text.ASCIIEncoding
        strMsgData = enc.GetString(msgData).Split(",")
        If msgType = "10522" Then
            LTEMessageVersion = strMsgData(1).Substring(0, 2)
            strMsgData(1) = strMsgData(1).Substring(2, strMsgData(1).Length - 2)
        End If



        Dim myMsg() As Byte
        Dim x As Integer = 0

        Dim myRaster, myBand As String
        Dim j, k As Long


        Select Case msgType
            Case 1001
                Dim speed, altitude As Single
                Dim altitudeUnit As String
                lat = strMsgData(0)
                lon = strMsgData(1)
                speed = strMsgData(4)
                altitude = strMsgData(9)
                altitudeUnit = strMsgData(10)

                Return True

            Case 10520

                If (type.ToUpper() <> "SPECTRUMSCAN") Then
                    Return False
                End If

                Dim FreqID As Integer = 0

                myMsg = ToByteArray(strMsgData(1))
                Dim bandInfo As String = ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(0)))
                Dim band As Integer
                If IsNothing(bandInfo) Then
                    If ScannerCommon.GetisSkipEmptyBand() Then
                        Return True
                    End If
                    myBand = 0
                    band = 0
                Else
                    myBand = bandInfo.Split("|")(1)
                    band = CInt(bandInfo.Split("|")(2).Trim.Substring(bandInfo.Split("|")(2).Length - 2))
                End If

                Dim Start_Scan_Frequency As String
                Dim End_Scan_Frequency As String
                Dim Num_of_RSSI As Integer
                Dim Freq_Spacing As Integer



                x = 0
                Start_Scan_Frequency = 16777216 * myMsg(x + 3) + 65536 * myMsg(x + 2) + 256 * myMsg(x + 1) + myMsg(x)
                x = x + 4
                End_Scan_Frequency = 16777216 * myMsg(x + 3) + 65536 * myMsg(x + 2) + 256 * myMsg(x + 1) + myMsg(x)
                x = x + 4

                Num_of_RSSI = CInt(256 * myMsg(x + 1) + myMsg(x))
                Dim RSSI_dbm As Decimal
                Dim RSSI_Watt As Decimal
                Dim FreqkHz As Decimal
                Freq_Spacing = CInt(Int32.Parse(End_Scan_Frequency) - Int32.Parse(Start_Scan_Frequency)) / Num_of_RSSI

                Dim tmpFileWriter As New OutputFileWriter()
                If Not _OperatorBandSet.ContainsKey(band) Then
                    '2017-06-10-18-40-20-3040-0011-0006-3040-S_SPECTRUMSCAN_Band40.txt
                    Dim filePath As String = fileInputObj.FileName.Substring(0, fileInputObj.FileName.LastIndexOf("\"))
                    Dim fileName As String = GetFileNameWithOutExt(fileInputObj.FileName) + "_SPECTRUMSCAN_Band" + band.ToString() + ".txt"

                    Dim writer = New StreamWriter(filePath + "\" + fileName)


                    tmpFileWriter.ScanId = 1
                    writer.WriteLine("Time_Stamp,Lat,Lon,MsgID,FreqID,Technology,Band,FreqkHz,RSSI_dBm,RSSI_watts,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                    tmpFileWriter.writer = writer
                    _OperatorBandSet.Item(band) = tmpFileWriter

                Else
                    tmpFileWriter = _OperatorBandSet.Item(band)
                    tmpFileWriter.OperatorName = "SPECTRUMSCAN"
                    tmpFileWriter.ScanId = tmpFileWriter.ScanId + 1
                    _OperatorBandSet.Item(band) = tmpFileWriter
                End If
                'modified by My task #1261
                x = x + 1
                For i = 0 To Num_of_RSSI - 1 '06/01/2018

                    Dim strBuilder As New StringBuilder()


                    'FreqID = FreqID + 1
                    x = x + 1
                    RSSI_dbm = -1 * CInt(myMsg(x)) / 2
                    RSSI_Watt = (10 ^ ((RSSI_dbm - 30) / 10)).ToString()

                    FreqkHz = (CInt(Start_Scan_Frequency) + Freq_Spacing * (i))
                    If band = 66 Then
                        band = 66
                    End If
                    If _SpectrumScanFreqThreshold.thresholds.Keys.Any(Function(f) f.Contains(band)) = False Then


                    ElseIf Not _SpectrumScanFreqThreshold.WithinRange(band.ToString(), FreqkHz) Then
                    Continue For
                    End If
                    FreqID = FreqID + 1          '06/01/2018
                    'FreqkHz = (CInt(Start_Scan_Frequency) + Freq_Spacing * (i))
                    If FreqID = 1 Then GoTo outputRecord
                    If _SpectrumScanLimits.Keys.Count > 0 Then
                        If _SpectrumScanLimits.ContainsKey(band.ToString()) Then
                            If RSSI_dbm < _SpectrumScanLimits(band.ToString()) Then

                                Continue For
                            End If
                        Else
                            If _SpectrumScanLimits.ContainsKey("Default") Then
                                If RSSI_dbm < _SpectrumScanLimits("Default") Then
                                    Continue For
                                End If
                            End If

                        End If
                    End If

                    'If i > 0 Then

                    'FreqkHz = (CInt(Start_Scan_Frequency) + Freq_Spacing * (i))


outputRecord:
                    'End If
                    'FreqID = FreqID + 1          '06/01/2018
                    strBuilder.Append(dttmFormat)

                    strBuilder.Append(",")
                    strBuilder.Append(lat)
                    strBuilder.Append(",")
                    strBuilder.Append(lon)
                    strBuilder.Append(",")
                    strBuilder.Append(tmpFileWriter.ScanId)
                    strBuilder.Append(",")
                    strBuilder.Append(FreqID)
                    strBuilder.Append(",")
                    strBuilder.Append("LTE")
                    strBuilder.Append(",")
                    strBuilder.Append(band.ToString)
                    strBuilder.Append(",")
                    strBuilder.Append((CInt(Start_Scan_Frequency) + Freq_Spacing * (i)).ToString)
                    strBuilder.Append(",")
                    strBuilder.Append(RSSI_dbm)
                    strBuilder.Append(",")
                    strBuilder.Append(RSSI_Watt)
                    strBuilder.Append(",")
                    strBuilder.Append(_FileName)
                    strBuilder.Append(",")
                    strBuilder.Append(_CollectedDateFormat)
                    strBuilder.Append(",")
                    strBuilder.Append(_MarketName)
                    strBuilder.Append(",")
                    strBuilder.Append(_Campaign)
                    strBuilder.Append(",")
                    strBuilder.Append(_ClientName)
                    strBuilder.Append(",")
                    If isGPSInterpolated Then
                        strBuilder.Append("ModGPS")
                    Else
                        strBuilder.Append("")
                    End If
                    If RSSI_dbm < -90 And band = "14" And i > 0 Then
                        Dim tt As String = ""
                    End If
                    tmpFileWriter.writer.WriteLine(strBuilder.ToString())
                    _OperatorBandSet.Item(band) = tmpFileWriter

                    '_OutputWriter.WriteLine(strBuilder.ToString())

                Next



                '_ScanID = _ScanID + 1

            Case 10522
                If (type.ToUpper() <> "LTE") Then
                    Return False
                End If

                _LTESIBInfo = New LTESIBInfo()


                LTEMsgCode = msgType

                x = 0

                Dim mylte As LTEstuff
                Dim refScan, syncScan As LTEScan
                Dim NoRxLev As Single

                Dim RefN, SynN, CarrierRssiN As Integer
                Dim newRec As Boolean = False
                Dim newChan As Boolean = False
                Dim channel As Integer
                Dim varA() As Single = {-999, -999, -999, -999, -999, -999}
                Dim varB() As Single = {-999, -999}
                myRaster = "LTE"

                myMsg = ToByteArray(strMsgData(1))


                With refScan
                    .PhysicalCellId = -999
                    .UniqueCellId = -999
                    .Antennas = -999
                    .TimeOffset = -999
                    .RP = varA
                    .RQ = varA
                    .PSCH_RP = varA
                    .PSCH_RQ = varA
                    .CINR = varA
                    .DelaySpread = -999
                    .NumTxAntennaPortsWideBand = -999 'added by My 05/31/2018 #1278
                    .RSSI = -999
                    .CyclicPrefix = -999
                End With
                With syncScan
                    .PhysicalCellId = -999
                    .UniqueCellId = -999
                    .Antennas = -999
                    .TimeOffset = -999
                    .RP = varA
                    .RQ = varA
                    .PSCH_RP = varA
                    .PSCH_RQ = varA
                    .DelaySpread = -999
                    .NumTxAntennaPortsWideBand = -999 'added by My 05/31/2018 #1278
                    .CINR = varA
                    .RSSI = -999
                    .CyclicPrefix = -999
                End With

                If Convert.ToInt32(strMsgData(0)) = 80 Then
                    channel = CLng(256 * myMsg(1) + myMsg(0))
                Else
                    If ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(0))).ToString().Split("|")(3) = "1" Then
                        channel = CLng(65536 * 1 + 256 * myMsg(1) + myMsg(0)) 'channel =5815, 5230 -6E14   
                    Else
                        channel = CLng(256 * myMsg(1) + myMsg(0)) 'channel =5815, 5230 -6E14   

                    End If
                End If




                currentLTEChannelData.channel = channel

                myBand = ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(0))).ToString().Split("|")(1)
                currentLTEChannelData.Band = myBand

                NoRxLev = IIf(myMsg(7) = 255, (256 - CInt(myMsg(6))) * -0.5, CInt(CLng(256 * myMsg(7) + myMsg(6))) * -0.5)


                With mylte

                    .Latitude = lat
                    .Longitude = lon
                    .Channel = channel
                    .BandWidthKhz = CLng(256 * myMsg(3) + myMsg(2)) '20000           
                    If Not _BandWidthList.ContainsKey(myBand + "|" + channel.ToString() + "|" + InputFileName + "|" + (.BandWidthKhz / 1000).ToString()) Then
                        _BandWidthList.Add(myBand + "|" + channel.ToString() + "|" + InputFileName + "|" + (.BandWidthKhz / 1000).ToString(), (.BandWidthKhz / 1000).ToString())
                    End If
                    _currentNosData.CarrierBW = .BandWidthKhz

                    CarrierRssiN = CInt(myMsg(9)) '1
                    RefN = CInt(myMsg(14)) '13
                    SynN = CInt(myMsg(19)) '20                
                    currentChannel = .Channel
                    currentRefN = RefN

                    ReDim .RefScan(Max(RefN - 1, 5))                            'min # elements=5, v12.1.12

                    If CarrierRssiN > 0 Then
                        .CarrierRSSI = CInt(256 * myMsg(21) + myMsg(20)) - 65536
                        If Math.Abs(.CarrierRSSI) > 999 Then
                            .CarrierRSSI = -999
                        End If
                    End If

                    currentCarrierRSSI = .CarrierRSSI
                    x = 20 + 2 * CarrierRssiN 'The position of First Data Block Ref record - 2 is the length of one Carrier RSSI Record
                    Dim j1, j2 As Integer

                    For j = 0 To RefN - 1


                        Try
                            Dim NumRfDataBlock As Integer = CInt(myMsg(x + 20))
                            With .RefScan(j)

                                ReDim .RQ(0)
                                ReDim .RP(0)
                                ReDim .CINR(0)
                                .PhysicalCellId = CInt(256 * myMsg(x + 5) + myMsg(x + 4))
                                .UniqueCellId = CLng(16777216 * myMsg(x + 9) + 65536 * myMsg(x + 8) + 256 * myMsg(x + 7) + myMsg(x + 6))
                                .CyclicPrefix = CInt(myMsg(x + 10))
                                .DelaySpread = CInt(256 * myMsg(x + 17) + myMsg(x + 16))
                                .NumTxAntennaPortsWideBand = IIf(CInt(myMsg(x + 11)) = 0, -999, CInt(myMsg(x + 11)))  'added by My 05/31/2018 #1278
                            End With

                            If LTEMessageVersion = 2 Then
                                x = x + 2
                            End If

                            Dim RfPathDataBlock_Length As Integer = 6
                            Dim NumSubbands = CInt(256 * myMsg(x + 22) + myMsg(x + 21))


                            For j2 = 0 To NumRfDataBlock - 1

                                With .RefScan(j)

                                    If j2 > 0 Then
                                        Exit For
                                    End If

                                    Try
                                        .RP(j2) = CInt(256 * myMsg(x + 24 + j2 * RfPathDataBlock_Length) + myMsg(x + 23 + j2 * RfPathDataBlock_Length)) - 65536
                                        Dim tt As String = .RP(j2)
                                        If Math.Abs(.RP(j2)) > 999 Then
                                            .RP(j2) = -999
                                        End If

                                        If .RP(j2) <> -999 Then

                                            Dim channelFoundStr = "(" + myBand.ToString + ":" + channel.ToString + ":{" + (mylte.BandWidthKhz / 1000).ToString() + "})"

                                            If InputFileName = "" Then
                                                If _LTEChannelFound.IndexOf(channelFoundStr) < 0 Then
                                                    _LTEChannelFound = _LTEChannelFound + channelFoundStr
                                                End If
                                            Else
                                                If _LTEChannelFoundHash.ContainsKey(InputFileName) Then
                                                    If _LTEChannelFoundHash(InputFileName).IndexOf(channelFoundStr) < 0 Then
                                                        _LTEChannelFoundHash(InputFileName) = _LTEChannelFoundHash(InputFileName) + channelFoundStr
                                                    End If

                                                Else
                                                    _LTEChannelFoundHash.Add(InputFileName, channelFoundStr)
                                                End If
                                            End If

                                        End If



                                        .RQ(j2) = CInt(256 * myMsg(x + 26 + j2 * RfPathDataBlock_Length) + myMsg(x + 25 + j2 * RfPathDataBlock_Length)) - 65536
                                        Dim tt1 As String = .RQ(j2)
                                        If Math.Abs(.RQ(j2)) > 999 Then
                                            .RQ(j2) = -999
                                        End If
                                        .CINR(j2) = CInt(256 * myMsg(x + 28 + j2 * RfPathDataBlock_Length) + myMsg(x + 27 + j2 * RfPathDataBlock_Length))
                                        Dim tt3 As String = .CINR(j2)
                                        If .CINR(j2) > 65000 Then
                                            .CINR(j2) = .CINR(j2) - 65536
                                        End If
                                        If Math.Abs(.CINR(j2)) > 999 Then
                                            .CINR(j2) = -999
                                        End If
                                    Catch

                                    End Try
                                End With

                            Next j2


                            Dim NumChannelCondition As Integer = CInt(myMsg(x + 22))
                            Dim ChannelConditionRecordLength As Integer = 0

                            'Calculate Channel Condition Record Length
                            Dim StartChannelConditionRecord As Integer = x + 23 + NumRfDataBlock * 6
                            For j1 = 0 To NumChannelCondition - 1
                                Try
                                    Dim NumTransDivBlks As Integer = CInt(myMsg(StartChannelConditionRecord + 33))
                                    ChannelConditionRecordLength = ChannelConditionRecordLength + 32 + NumTransDivBlks * 5
                                    StartChannelConditionRecord = ChannelConditionRecordLength
                                Catch

                                End Try

                            Next


                            x = x + 23 + NumRfDataBlock * 6 + ChannelConditionRecordLength
                            x = x + NumSubbands * 6

                        Catch ex As Exception
                            Dim tt = ""

                        End Try

                    Next j

                    'Sort RSRP(0) here
                    Try
                        If RefN >= 2 Then
                            Dim tmpRefScan As LTEScan()
                            ReDim tmpRefScan(RefN - 1)

                            For j = 0 To RefN - 1
                                tmpRefScan(j) = .RefScan(j)
                            Next

                            Dim query As IEnumerable(Of LTEScan) = tmpRefScan.OrderByDescending(Function(pet) pet.RP(0))
                            For j = 0 To RefN - 1
                                .RefScan(j) = query(j)
                            Next

                        End If
                    Catch

                    End Try



                    For j = RefN To 5                                           'add initialization here, v12.1.12
                        .RefScan(j) = refScan
                    Next j

                    If Not isSkipLTESynBlock Then

                        If SynN > 0 Then
                            ReDim .SyncScan(Max(SynN - 1, 5))                           'min # elements=5, v12.1.12                        
                        End If

                        Dim LoopSyN As Integer
                        If SynN > 6 Then
                            LoopSyN = 6
                        Else
                            LoopSyN = SynN
                        End If

                        Dim foundSIB As Boolean = False

                        For j = 0 To LoopSyN - 1
                            Try
                                With .SyncScan(j)

                                    If foundSIB Then
                                        Exit For
                                    End If

                                    ReDim .PSCH_RQ(2)
                                    ReDim .PSCH_RP(2)
                                    ReDim .CINR(2)

                                    If Not isSkipLTESynBlock Then
                                        .PhysicalCellId = CInt(256 * myMsg(x + 5) + myMsg(x + 4))
                                        Dim tt As String = .PhysicalCellId


                                        Try
                                            Dim NumofInformationBlock = CInt(myMsg(x + 36))
                                            x = x + 37
                                            For j1 = 0 To NumofInformationBlock - 1
                                                x = x + 1
                                                Dim SIBString = ""
                                                Dim BLockType = CInt(myMsg(x))

                                                Dim DataLength As Integer = CInt(256 * myMsg(x + 2) + myMsg(x + 1))
                                                If BLockType = 0 Then
                                                    x = x + DataLength + 3
                                                Else

                                                    _LTESIBInfo.SIB_PhysicalCellId = .PhysicalCellId

                                                    For i = 1 To DataLength
                                                        SIBString = SIBString + myMsg(x + i + 2).ToString("X2")
                                                    Next

                                                    If BLockType = 2 Then
                                                        _LTESIBInfo.SIB1 = SIBString
                                                    ElseIf BLockType = 3 Then
                                                        _LTESIBInfo.SIB2 = SIBString
                                                    ElseIf BLockType = 4 Then
                                                        _LTESIBInfo.SIB3 = SIBString
                                                    ElseIf BLockType = 5 Then
                                                        _LTESIBInfo.SIB4 = SIBString
                                                    ElseIf BLockType = 6 Then
                                                        _LTESIBInfo.SIB5 = SIBString
                                                    ElseIf BLockType = 7 Then
                                                        _LTESIBInfo.SIB6 = SIBString
                                                    ElseIf BLockType = 9 Then
                                                        _LTESIBInfo.SIB8 = SIBString
                                                    End If

                                                    x = x + DataLength + 3

                                                    foundSIB = True


                                                End If

                                            Next
                                        Catch

                                        End Try

                                    End If

                                End With
                            Catch ex As Exception
                                Dim tt As String = ""

                            End Try
                        Next j
                    End If



                    ReDim Preserve .SyncScan(5)
                    For j = SynN To 5                                           'add initialization here, v12.1.12
                        .SyncScan(j) = syncScan
                    Next j

                End With


                lastLTETimeStamp = mylte.CollectionTime
                'lat = mylte.Latitude
                'lon = mylte.Longitude
                dttm = dttm
                coldt = dttm

                'filename = myFileInfo.FileName

                myStr = ""



                With mylte

                    For k = 0 To RefN - 1
                        If .RefScan(k).PhysicalCellId = _LTESIBInfo.SIB_PhysicalCellId Then
                            _LTESIBInfo.SIB_PhysicalCellId_Position = k + 1
                            Exit For
                        End If
                    Next

                    For k = RefN To 5
                        .RefScan(k) = refScan
                    Next k
                    For k = SynN To 5
                        .SyncScan(k) = syncScan
                    Next k

                    For k = 0 To 5
                        .SyncScan(k) = syncScan
                        Try
                            'myStr += "," & .RefScan(k).PhysicalCellId & "," & .RefScan(k).RP(0) & "," & .RefScan(k).RQ(0) & "," & .RefScan(k).CINR(0) & "," & .SyncScan(k).PSCH_RP(0) & "," & .SyncScan(k).PSCH_RQ(0) & "," & .RefScan(k).DelaySpread
                            myStr += "," & .RefScan(k).PhysicalCellId & "," & .RefScan(k).RP(0) & "," & .RefScan(k).RQ(0) & "," & .RefScan(k).CINR(0) & "," & .SyncScan(k).PSCH_RP(0) & "," & .SyncScan(k).PSCH_RQ(0) & "," & .RefScan(k).NumTxAntennaPortsWideBand   'modified by My 05/31/2018 #1278

                        Catch

                        End Try
                    Next k

                    _currentRSSIValue = .RefScan(0).RP(0)

                    For k = .RefScan.Length To 5
                        myStr += ",-130,-130,-130,-130,-130,-130"
                    Next k

                    currentLTEChannelData.value = .RefScan(0).RP(0)
                    currentLTEChannelData.value2 = .RefScan(0).RQ(0)
                    currentLTEChannelData.channel = channel
                    currentLTEChannelData.CarrierSignalLevel = mylte.CarrierRSSI

                    myStr = "LTE," & _ScanID.ToString & "," & mylte.Channel & "," & myBand & "," & mylte.BandWidthKhz & "," & mylte.CarrierRSSI & myStr
                    _isOutputLTE = True
                End With

            Case 10103

                Dim i As Integer


                If type.ToUpper() = "CDMA" Then

                    If strMsgData(8).Trim <> "100" Then
                        _isSkippedCDMA = True
                        _isSkippedWCDMA = False
                    Else
                        _isSkippedCDMA = False
                        _isSkippedWCDMA = True
                    End If

                    If _isSkippedCDMA Then
                        Return True
                    End If

                    _count_CDMA_Record = _count_CDMA_Record + 1

                    CDMAPilotInfo.RFBand = strMsgData(1)
                    CDMAPilotInfo.Channel = strMsgData(2)
                    CDMAPilotInfo.ID = ID

                    If Not _CDMAChannelTmp.Contains(CDMAPilotInfo.Channel) Then
                        _CDMAChannelTmp.Add(CDMAPilotInfo.Channel)
                    End If
                    Dim tmpInt As Integer

                    If Double.TryParse(strMsgData(5), tmpInt) Then
                        CDMAPilotInfo.NoRxLev = tmpInt

                    End If


                    Dim tmpBandInfo As String() = ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(1))).ToString().Split("|")

                    myBand = tmpBandInfo(1)

                    If myBand = 800 Then
                        myBand = 850
                    End If
                    CDMAPilotInfo.Band = myBand
                    currentCDMAChannelData.Band = myBand

                    If (type = "CDMA" And tmpBandInfo(0).IndexOf("CDMA") < 0) Then
                        _isSkippedCDMA = True
                        Return True
                    End If



                    If Integer.TryParse(strMsgData(7), tmpInt) Then
                        CDMAPilotInfo.NumRecords = tmpInt
                    End If

                    If CDMAPilotInfo.NumRecords > 0 Then
                        Count_CDMARecord = -1
                        N_CDMA = Max(CDMAPilotInfo.NumRecords, 6)
                        If Integer.TryParse(CDMAPilotInfo.Channel, ch_CDMA) Then
                            ch_CDMA = Convert.ToInt16(CDMAPilotInfo.Channel)
                        End If

                        ReDim scan_CDMA(N_CDMA - 1)
                        ReDim DescSort_CDMA(N_CDMA - 1)
                        ReDim DescSortKeys_CDMA(N_CDMA - 1)

                        For j = 0 To N_CDMA - 1
                            With scan_CDMA(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                            End With
                        Next j
                    End If

                    If CDMAPilotInfo.NumRecords = 0 Then
                        _isOutputCDMA = True
                        x = 0

                        Dim ch, N, DescSortKeys()                          'N = Num of Readings
                        Dim scan(), scanSort() As CDMAstuff

                        Dim NoRxLev As Single
                        Dim DescSort() As Single

                        myStr = ""
                        N = 5
                        ch = strMsgData(2)
                        NoRxLev = Math.Round(Convert.ToDouble(CDMAPilotInfo.NoRxLev))
                        myRaster = "CDMA"
                        ReDim scan(N - 1)
                        ReDim DescSort(N - 1)
                        ReDim DescSortKeys(N - 1)
                        ReDim scan(N - 1)
                        For j = 0 To N - 1
                            With scan(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                                .Base_ID = "-999"
                                .Base_LAT = "-999"
                                .Base_LONG = "-999"

                            End With
                        Next j
                        If ch <> -999 Then
                            'Need to detect empty SID and NID channel
                            Dim SIDNIDList As List(Of SIDNIDStore) = CDMAChannelList(CDMAPilotInfo.Band + ch)

                            If SIDNIDList.Count = 1 Then
                                If SIDNIDList(0).band.Trim = myBand.Trim Then
                                    CDMAPilotInfo.SID = SIDNIDList(0).SID
                                    CDMAPilotInfo.NID = SIDNIDList(0).NID
                                    CDMAPilotInfo.LogTrace = "SIDmodCo"
                                End If
                            Else
                                Dim Min As Double = 100000000
                                Dim jj As Integer = -1
                                For j = 0 To SIDNIDList.Count - 1
                                    If SIDNIDList(0).band.Trim = myBand.Trim Then
                                        Dim dttmDate = Convert.ToDateTime(dttm)
                                        Dim duration As TimeSpan = dttmDate - SIDNIDList(j).time
                                        Dim totalms = duration.TotalMilliseconds
                                        If (Min > Math.Abs(totalms)) Then
                                            Min = totalms
                                            jj = j
                                        End If
                                    End If


                                Next
                                If jj >= 0 Then
                                    CDMAPilotInfo.SID = SIDNIDList(jj).SID
                                    CDMAPilotInfo.NID = SIDNIDList(jj).NID
                                    CDMAPilotInfo.LogTrace = "SIDmodCo"
                                End If

                            End If

                            If IsNothing(CDMAPilotInfo.SID) Then
                                CDMAPilotInfo.SID = ""
                            End If
                            If IsNothing(CDMAPilotInfo.NID) Then
                                CDMAPilotInfo.NID = ""
                            End If


                            If Integer.TryParse(CDMAPilotInfo.SID, tmpInt) Then
                                CDMAPilotInfo.OperatorName = GetOperatorName(CDMAPilotInfo.SID)
                            Else
                                CDMAPilotInfo.OperatorName = ""
                            End If
                            currentCDMAChannelData.OperatorName = CDMAPilotInfo.OperatorName

                            currentCDMAChannelData.value2 = NoRxLev

                            myStr = myBand & "," & myRaster & "," & _ScanID.ToString & "," & ch & "," & CDMAPilotInfo.SID & "," & CDMAPilotInfo.NID & "," & NoRxLev
                        End If

                        For j = 0 To N - 1
                            With scan(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                                DescSortKeys(j) = j
                                DescSort(j) = -999
                            End With
                        Next j

                        currentCDMAChannelData.value = -999
                        _currentRSSIValue = -999
                        currentCDMAChannelData.channel = ch

                        Array.Sort(DescSort, DescSortKeys)
                        ReDim scanSort(UBound(scan))
                        For i = 0 To UBound(DescSortKeys)
                            scanSort(i) = scan(DescSortKeys(UBound(DescSortKeys) - i))
                        Next i
                        ReDim Preserve scanSort(Max(N, 5))
                        For j = N To 5
                            With scanSort(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                            End With
                        Next j

                        lastCDMATimeStamp = dttm



                        If Not IsNothing(ScnrChLstCDMAobj) Then
                            For j = 0 To UBound(ScnrChLstCDMAobj)
                                If ScnrChLstCDMAobj(j).Channel = ch And ScnrChLstCDMAobj(j).Band = myBand Then
                                    If ScnrChLstCDMAobj(j).Scan(0).CPICHEc < scanSort(0).CPICHEc Then
                                        ScnrChLstCDMAobj(j).Io = NoRxLev
                                        ScnrChLstCDMAobj(j).Scan = scanSort
                                        Exit For
                                    End If
                                End If
                            Next j
                        End If
                        For k = 0 To 5
                            With scanSort(k)
                                myStr += "," & .CPICHSC & "," & .SpreadChips & "," & .CPICHEc & "," & .CPICHEcNo & ",-999,-999,-999"
                            End With
                        Next k
                    End If

                End If

                If type.ToUpper() = "UMTS" Then
                    WCDMAPilotInfo.LogTrace = ""

                    If strMsgData(8).Trim <> "100" Then
                        _isSkippedWCDMA = False
                    Else
                        _isSkippedWCDMA = True
                    End If

                    If _isSkippedWCDMA Then
                        Return True
                    End If

                    _count_UMTS_Record = _count_UMTS_Record + 1
                    WCDMAPilotInfo.RFBand = strMsgData(1)
                    WCDMAPilotInfo.Channel = strMsgData(2)

                    Dim tmpBandInfo As String() = ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(1))).ToString().Split("|")
                    myBand = tmpBandInfo(1)
                    myRaster = tmpBandInfo(0)

                    WCDMAPilotInfo.Band = myBand
                    currentWCDMAChannelDaTa.Band = myBand
                    WCDMAPilotInfo.PNThreshold = strMsgData(4)
                    WCDMAPilotInfo.IO = strMsgData(5)
                    WCDMAPilotInfo.DwellingTime = strMsgData(6)

                    currentWCDMAChannelDaTa.value = -999
                    _currentRSSIValue = -999
                    currentWCDMAChannelDaTa.channel = WCDMAPilotInfo.Channel
                    Dim tmpInt As Integer
                    If Integer.TryParse(strMsgData(7), tmpInt) Then
                        WCDMAPilotInfo.NumRecords = tmpInt
                    End If


                    If WCDMAPilotInfo.NumRecords > 0 Then
                        Count_WCDMARecord = -1
                        N_WCDMA = Max(WCDMAPilotInfo.NumRecords, 6)
                        If Integer.TryParse(WCDMAPilotInfo.Channel, ch_WCDMA) Then
                            ch_WCDMA = Convert.ToInt16(WCDMAPilotInfo.Channel)
                        End If

                        ReDim scan_WCDMA(N_WCDMA - 1)
                        ReDim DescSort_WCDMA(N_WCDMA - 1)
                        ReDim DescSortKeys_WCDMA(N_WCDMA - 1)
                        For j = 0 To N_WCDMA - 1
                            With scan_WCDMA(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .CellID = -999
                                .PSCEcNo = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                                .BCCHEcNo = -999
                                .SigInt = -999
                                .LAC = -999

                            End With
                        Next j

                    End If

                    If WCDMAPilotInfo.NumRecords = 0 Then
                        _isOutputWCDMA = True
                        x = 0
                        myMsg = ToByteArray(strMsgData(1))

                        Dim ch, N, DescSortKeys() As Integer                          'N = Num of Readings
                        Dim scan(), scanSort() As WCDMAstuff
                        Dim NoRxLev As Single
                        Dim DescSort() As Single
                        myStr = ""
                        N = 1
                        ch = strMsgData(2)

                        NoRxLev = WCDMAPilotInfo.IO
                        Dim tmpDouble As Double
                        If Double.TryParse(NoRxLev, tmpDouble) Then
                            NoRxLev = Math.Round(NoRxLev)
                        End If


                        myRaster = "UMTS"
                        ReDim scan(N - 1)
                        ReDim DescSort(N - 1)
                        ReDim DescSortKeys(N - 1)
                        ReDim scan(N - 1)
                        For j = 0 To N - 1
                            With scan(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .CellID = -999
                                .PSCEcNo = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                                .BCCHEcNo = -999
                                .SigInt = -999
                                .LAC = -999
                            End With
                        Next j


                        Dim SIDNIDList As List(Of SIDNIDStore) = WCDMAChannelList(WCDMAPilotInfo.Channel)

                        'Need to detect empty MCC and MNC channel

                        If Not IsNothing(SIDNIDList) Then


                            If SIDNIDList.Count = 1 Then
                                WCDMAPilotInfo.MCC = SIDNIDList(0).SID
                                WCDMAPilotInfo.MNC = SIDNIDList(0).NID
                                WCDMAPilotInfo.LogTrace = "SIDmodCo"
                            ElseIf SIDNIDList.Count > 1 Then
                                Dim Min As Double = 100000000
                                Dim jj As Integer = -1
                                For j = 0 To SIDNIDList.Count - 1
                                    Dim dttmDate = Convert.ToDateTime(dttm)
                                    Dim duration As TimeSpan = dttmDate - SIDNIDList(j).time
                                    Dim totalms = duration.TotalMilliseconds
                                    If (Min > Math.Abs(totalms)) Then
                                        Min = totalms
                                        jj = j
                                    End If

                                Next
                                If (jj >= 0) Then
                                    WCDMAPilotInfo.LogTrace = "SIDmodCo"
                                    WCDMAPilotInfo.MCC = SIDNIDList(jj).SID
                                    WCDMAPilotInfo.MNC = SIDNIDList(jj).NID
                                End If

                            End If
                        End If

                        If IsNothing(WCDMAPilotInfo.MCC) Then
                            WCDMAPilotInfo.MCC = ""
                        End If

                        If IsNothing(WCDMAPilotInfo.MNC) Then
                            WCDMAPilotInfo.MNC = ""
                        End If


                        If Not IsNothing(WCDMAPilotInfo.MCC) And Not IsNothing(WCDMAPilotInfo.MNC) Then
                            If Integer.TryParse(WCDMAPilotInfo.MCC.Trim + WCDMAPilotInfo.MNC.Trim, tmpInt) Then
                                WCDMAPilotInfo.OperatorName = GetOperatorName(WCDMAPilotInfo.MCC.Trim + WCDMAPilotInfo.MNC.Trim)
                                currentWCDMAChannelDaTa.OperatorName = WCDMAPilotInfo.OperatorName
                            End If
                        End If
                        currentWCDMAChannelDaTa.SID = WCDMAPilotInfo.MCC
                        currentWCDMAChannelDaTa.NID = WCDMAPilotInfo.MNC


                        myStr = myBand & "," & "UMTS" & "," & _ScanID & "," & ch & "," & WCDMAPilotInfo.MCC & "," & WCDMAPilotInfo.MNC & "," & NoRxLev

                        currentWCDMAChannelDaTa.value2 = NoRxLev


                        Array.Sort(DescSort, DescSortKeys)
                        ReDim scanSort(UBound(scan))
                        For i = 0 To UBound(DescSortKeys)
                            scanSort(i) = scan(DescSortKeys(UBound(DescSortKeys) - i))
                        Next i
                        ReDim Preserve scanSort(Max(N, 5))
                        For j = N To 5
                            With scanSort(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .CellID = -999
                                .PSCEcNo = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                                .BCCHEcNo = -999
                            End With
                        Next j


                        For k = 0 To 5
                            With scanSort(k)
                                If .LAC = 0 Then
                                    .LAC = -999
                                End If
                                If .SigInt = 0 Then
                                    .SigInt = -999
                                End If
                                myStr += "," & .CPICHSC & "," & .SpreadChips & "," & .CellID & "," & .LAC & "," & .PSCEcNo & "," & .CPICHEc & "," & .CPICHEcNo & "," & .BCCHEcNo & "," & .SigInt
                            End With
                        Next k
                    End If
                End If

            Case 10105

                If type.ToUpper() = "CDMA" Then
                    If _isSkippedCDMA Then
                        Return True
                    End If
                    If Not IsNothing(scan_CDMA) Then
                        With scan_CDMA(Count_CDMARecord)
                            If (strMsgData(0) = "1007") And (strMsgData(1).Length = 68) Then

                                Dim tmpCDMASIP As CDMASIB = ScannerCommon.GetCDMASIB(strMsgData(1))

                                With scan_CDMA(Count_CDMARecord)
                                    .Base_ID = tmpCDMASIP.Base_ID
                                    .Base_LAT = tmpCDMASIP.Base_LAT
                                    .Base_LONG = tmpCDMASIP.Base_LONG
                                End With
                            End If
                        End With
                    Else
                        Return False
                    End If
                End If





            Case 10104


                Dim SID, NID

                If type.ToUpper() = "CDMA" Then

                    If _isSkippedCDMA Then
                        Return True
                    End If

                    If CDMAPilotInfo.RFBand = Nothing Then
                        Return False
                    End If
                    Dim i As Integer

                    Count_CDMARecord = Count_CDMARecord + 1

                    x = 0
                    Dim time As String = dttm
                    SID = strMsgData(16)
                    NID = strMsgData(17)
                    If IsNothing(CDMAPilotInfo.SID) Then
                        CDMAPilotInfo.SID = ""
                    End If
                    If SID.Trim <> "" Then
                        CDMAPilotInfo.SID = SID
                        CDMAPilotInfo.NID = NID
                        CDMAPilotInfo.LogTrace = ""

                        'Update the timestamp
                        Dim SIDNIDList As List(Of SIDNIDStore) = CDMAChannelList(CDMAPilotInfo.Band + CDMAPilotInfo.Channel)
                        If SIDNIDList.Count > 1 Then
                            For j = 0 To SIDNIDList.Count - 1
                                If SIDNIDList(j).SID = CDMAPilotInfo.SID And SIDNIDList(j).NID = CDMAPilotInfo.NID And SIDNIDList(j).band = CDMAPilotInfo.Band Then

                                    Dim tmpStore As New SIDNIDStore()
                                    tmpStore.time = Convert.ToDateTime(dttm)
                                    tmpStore.SID = CDMAPilotInfo.SID
                                    tmpStore.NID = CDMAPilotInfo.NID
                                    tmpStore.band = CDMAPilotInfo.Band
                                    SIDNIDList(j) = tmpStore
                                End If
                            Next
                        End If

                        CDMAChannelList(CDMAPilotInfo.Band + CDMAPilotInfo.Channel) = SIDNIDList

                    ElseIf CDMAPilotInfo.SID.Trim = "" Then
                        Dim SIDNIDList As List(Of SIDNIDStore) = CDMAChannelList(CDMAPilotInfo.Band + CDMAPilotInfo.Channel)


                        'Need to detect empty SID and NID channel

                        If SIDNIDList.Count = 1 Then
                            CDMAPilotInfo.SID = SIDNIDList(0).SID
                            CDMAPilotInfo.NID = SIDNIDList(0).NID
                            CDMAPilotInfo.LogTrace = "SIDmodCo"
                        Else
                            Dim Min As Double = 100000000
                            Dim jj As Integer = -1
                            For j = 0 To SIDNIDList.Count - 1
                                Dim dttmDate = Convert.ToDateTime(dttm)
                                Dim duration As TimeSpan = dttmDate - SIDNIDList(j).time
                                Dim totalms = duration.TotalMilliseconds
                                If (Min > Math.Abs(totalms)) Then
                                    Min = totalms
                                    jj = j
                                End If

                            Next
                            If (jj >= 0) Then
                                CDMAPilotInfo.LogTrace = "SIDmodCo"
                                CDMAPilotInfo.SID = SIDNIDList(jj).SID
                                CDMAPilotInfo.NID = SIDNIDList(jj).NID
                            End If

                        End If

                    End If
                    Dim tmpInt As Integer
                    If Integer.TryParse(CDMAPilotInfo.SID, tmpInt) Then
                        CDMAPilotInfo.OperatorName = GetOperatorName(CDMAPilotInfo.SID)
                        currentCDMAChannelData.OperatorName = CDMAPilotInfo.OperatorName
                    Else
                        CDMAPilotInfo.OperatorName = ""
                        currentCDMAChannelData.OperatorName = ""
                    End If


                    If Count_CDMARecord <= CDMAPilotInfo.NumRecords - 1 Then


                        x = 0

                        With scan_CDMA(Count_CDMARecord)
                            .CPICHSC = strMsgData(0)

                            If strMsgData(9) = "" Then
                                .Delay = -999
                            Else
                                .Delay = strMsgData(9)
                                '.Delay = strMsgData(15)
                            End If

                            If strMsgData(9) = "" Then
                                .SpreadChips = -999
                            Else
                                .SpreadChips = strMsgData(9)
                                '.SpreadChips = strMsgData(15)
                            End If
                            .SSCEcNo = -999

                            Dim tmpDouble1 As Double
                            Dim tmpDouble As Double = 0
                            .CPICHEc = 0
                            If Double.TryParse(CDMAPilotInfo.NoRxLev, tmpDouble1) Then
                                .CPICHEc = .CPICHEc + tmpDouble1
                            End If
                            If Double.TryParse(strMsgData(2), tmpDouble1) Then
                                .CPICHEc = .CPICHEc + tmpDouble1
                            End If

                            If .CPICHEc = 0 Then
                                .CPICHEc = -999
                            End If

                            .CPICHEc = Math.Round(.CPICHEc) 'Ec values
                            If .CPICHEc <> -999 Then
                                If Not _CDMAChannelFound.Contains("(" + CDMAPilotInfo.Band + ":" + CDMAPilotInfo.Channel + ")") Then
                                    _CDMAChannelFound = _CDMAChannelFound + "(" + CDMAPilotInfo.Band + ":" + CDMAPilotInfo.Channel + ")"
                                End If
                            End If
                            .CPICHEcNo = strMsgData(2) 'EcIo values
                            If Math.Abs(.CPICHEcNo) > 999 Then
                                .CPICHEcNo = -999
                            End If
                            DescSortKeys_CDMA(Count_CDMARecord) = Count_CDMARecord
                            DescSort_CDMA(Count_CDMARecord) = .CPICHEcNo
                        End With
                    End If

                    Dim total10104 As Integer = _MessageObject.Item(CDMAPilotInfo.ID)
                    If Count_CDMARecord = CDMAPilotInfo.NumRecords - 1 Or Count_CDMARecord = total10104 - 1 Then

                        _isOutputCDMA = True

                        myStr = ""
                        myMsg = ToByteArray(CDMAPilotInfo.RFBand)

                        NoRxLev_CDMA = Math.Round(Convert.ToDouble(CDMAPilotInfo.NoRxLev))

                        myRaster = "CDMA"
                        myStr = CDMAPilotInfo.Band & "," & myRaster & "," & _ScanID.ToString & "," & ch_CDMA & "," & CDMAPilotInfo.SID & "," & CDMAPilotInfo.NID & "," & NoRxLev_CDMA

                        Array.Sort(DescSort_CDMA, DescSortKeys_CDMA)
                        ReDim scanSort_CDMA(UBound(scan_CDMA))
                        For i = 0 To UBound(DescSortKeys_CDMA)
                            scanSort_CDMA(i) = scan_CDMA(i)
                        Next i
                        ReDim Preserve scanSort_CDMA(Max(N_CDMA, 5))
                        For j = N_CDMA To 5
                            With scanSort_CDMA(j)
                                .Delay = -999
                                .CPICHSC = -999
                                .SpreadChips = -999
                                .SSCEcNo = -999
                                .CPICHEc = -999
                                .CPICHEcNo = -999
                                .Base_ID = "-999"
                                .Base_LAT = "-999"
                                .Base_LONG = "-999"
                            End With
                        Next j


                        currentCDMAChannelData.value = scanSort_CDMA(0).CPICHEc
                        _currentRSSIValue = scanSort_CDMA(0).CPICHEc
                        currentCDMAChannelData.value2 = NoRxLev_CDMA
                        currentCDMAChannelData.channel = ch_CDMA

                        'Command	Time_Stamp	GPS_Time	Lat	Lon	Band	Tech	ScanID	Channel	SID	NID	NoRxLev	PN_1	Spread_1	Ec_1_dbm	EcIo_1	BaseId_1	BaseLat_1	BaseLon_1	PN_2	Spread_2	Ec_2_dbm	EcIo_2	BaseId_2	BaseLat_2	BaseLon_2	PN_3	Spread_3	Ec_3_dbm	EcIo_3	BaseId_3	BaseLat_3	BaseLon_3	PN_4	Spread_4	Ec_4_dbm	EcIo_4	BaseId_4	BaseLat_4	BaseLon_4	PN_5	Spread_5	Ec_5_dbm	EcIo_5	BaseId_5	BaseLat_5	BaseLon_5	PN_6	Spread_6	Ec_6_dbm	EcIo_6	BaseId_6	BaseLat_6	BaseLon_6	Operator	DataType	FileName	Collected_Date	MarketName	Campaign	ClientName	LogTrace

                        For k = 0 To 5
                            With scanSort_CDMA(k)
                                Try
                                    If IsNothing(.Base_ID) Then
                                        .Base_ID = "-999"
                                    End If
                                    If IsNothing(.Base_LAT) Then
                                        .Base_LAT = "-999"
                                    End If
                                    If IsNothing(.Base_LONG) Then
                                        .Base_LONG = "-999"
                                    End If
                                    If .Base_ID.Trim = "" Then
                                        .Base_ID = "-999"
                                    End If
                                    If .Base_LAT.Trim = "" Then
                                        .Base_LAT = "-999"
                                    End If
                                    If .Base_LONG.Trim = "" Then
                                        .Base_LONG = "-999"
                                    End If
                                Catch ex As Exception

                                End Try

                                myStr += "," & .CPICHSC & "," & .SpreadChips & "," & .CPICHEc & "," & .CPICHEcNo & "," & .Base_ID & "," & .Base_LAT & "," & .Base_LONG
                            End With
                        Next k
                        myStr1 = myStr
                    End If
                End If



                If type.ToUpper() = "UMTS" Then

                    If _isSkippedWCDMA Then
                        Return True
                    End If

                    Dim i As Integer
                    Dim LAC, SIGINT As String
                    SIGINT = ""

                    If WCDMAPilotInfo.RFBand = Nothing Then
                        Return False
                    End If
                    Count_WCDMARecord = Count_WCDMARecord + 1

                    x = 0

                    myBand = WCDMAPilotInfo.Band

                    myRaster = "UMTS"


                    If Count_WCDMARecord <= WCDMAPilotInfo.NumRecords - 1 Then
                        With scan_WCDMA(Count_WCDMARecord)
                            If strMsgData(9) = "" Then
                                .Delay = -999
                            Else
                                .Delay = strMsgData(9)

                            End If

                            .CPICHSC = strMsgData(0)
                            If strMsgData(9) = "" Then
                                .SpreadChips = -999
                            Else
                                .SpreadChips = strMsgData(9)

                            End If
                            If strMsgData(19) = "" Then
                                .CellID = -999
                            Else
                                .CellID = strMsgData(19)

                            End If

                            If strMsgData(11) = "" Then
                                .SigInt = -999
                            Else
                                .SigInt = strMsgData(11)
                            End If

                            If strMsgData(18) = "" Then
                                .LAC = -999
                            Else
                                .LAC = strMsgData(18)
                            End If
                            If IsNothing(WCDMAPilotInfo.MCC) Then
                                WCDMAPilotInfo.MCC = ""
                            End If
                            If strMsgData(16).Trim <> "" Then
                                WCDMAPilotInfo.MCC = strMsgData(16)
                                WCDMAPilotInfo.MNC = strMsgData(17)
                                WCDMAPilotInfo.LogTrace = ""

                                'Update the timestamp
                                Dim SIDNIDList As List(Of SIDNIDStore) = WCDMAChannelList(WCDMAPilotInfo.Channel)
                                If SIDNIDList.Count > 1 Then
                                    For j = 0 To SIDNIDList.Count - 1
                                        If SIDNIDList(j).SID = WCDMAPilotInfo.MCC And SIDNIDList(j).NID = WCDMAPilotInfo.MNC And SIDNIDList(j).band - WCDMAPilotInfo.Band Then
                                            Dim tmpStore As New SIDNIDStore()
                                            tmpStore.time = Convert.ToDateTime(dttm)
                                            tmpStore.SID = WCDMAPilotInfo.MCC
                                            tmpStore.NID = WCDMAPilotInfo.MNC
                                            tmpStore.band = WCDMAPilotInfo.Band
                                            SIDNIDList(j) = tmpStore
                                        End If
                                    Next
                                End If

                                WCDMAChannelList(WCDMAPilotInfo.Channel) = SIDNIDList

                            ElseIf WCDMAPilotInfo.MCC.Trim = "" Then
                                Dim SIDNIDList As List(Of SIDNIDStore) = WCDMAChannelList(WCDMAPilotInfo.Channel)

                                'Need to detect empty MCC and MNC channel


                                If Not IsNothing(SIDNIDList) Then


                                    If SIDNIDList.Count = 1 Then
                                        WCDMAPilotInfo.MCC = SIDNIDList(0).SID
                                        WCDMAPilotInfo.MNC = SIDNIDList(0).NID
                                        WCDMAPilotInfo.LogTrace = "SIDmodCo"
                                    ElseIf SIDNIDList.Count > 1 Then
                                        Dim Min As Double = 100000000
                                        Dim jj As Integer = 0
                                        For j = 0 To SIDNIDList.Count - 1
                                            Dim dttmDate = Convert.ToDateTime(dttm)
                                            Dim duration As TimeSpan = dttmDate - SIDNIDList(j).time
                                            Dim totalms = duration.TotalMilliseconds
                                            If (Min > Math.Abs(totalms)) Then
                                                Min = totalms
                                                jj = j
                                            End If

                                        Next
                                        If (jj >= 0) Then
                                            WCDMAPilotInfo.LogTrace = "SIDmodCo"
                                            WCDMAPilotInfo.MCC = SIDNIDList(jj).SID
                                            WCDMAPilotInfo.MNC = SIDNIDList(jj).NID
                                        End If

                                    End If
                                End If



                            End If

                            currentWCDMAChannelDaTa.SID = WCDMAPilotInfo.MCC
                            currentWCDMAChannelDaTa.NID = WCDMAPilotInfo.MNC

                            Dim tmpInt As Integer
                            If Not IsNothing(WCDMAPilotInfo.MCC) And Not IsNothing(WCDMAPilotInfo.MNC) Then
                                If Integer.TryParse(WCDMAPilotInfo.MCC.Trim + WCDMAPilotInfo.MNC.Trim, tmpInt) Then
                                    WCDMAPilotInfo.OperatorName = GetOperatorName(WCDMAPilotInfo.MCC.Trim + WCDMAPilotInfo.MNC.Trim)

                                    currentWCDMAChannelDaTa.OperatorName = WCDMAPilotInfo.OperatorName
                                End If
                            End If

                            .SSCEcNo = -999

                            Dim tmpDouble1 As Double
                            Dim tmpDouble As Double = 0
                            .CPICHEc = 0
                            If Double.TryParse(WCDMAPilotInfo.IO, tmpDouble1) Then
                                .CPICHEc = .CPICHEc + tmpDouble1
                            End If
                            If Double.TryParse(strMsgData(2), tmpDouble1) Then
                                .CPICHEc = .CPICHEc + tmpDouble1
                            End If
                            If .CPICHEc = 0 Then
                                .CPICHEc = -999
                            End If

                            .CPICHEc = Math.Round(.CPICHEc) 'Ec values

                            If .CPICHEc <> -999 Then
                                If Not _WCDMAChannelFound.Contains("(" + WCDMAPilotInfo.Band + ":" + WCDMAPilotInfo.Channel + ")") Then
                                    _WCDMAChannelFound = _WCDMAChannelFound + "(" + WCDMAPilotInfo.Band + ":" + WCDMAPilotInfo.Channel + ")"
                                End If

                            End If

                            .CPICHEcNo = strMsgData(2)
                            If Math.Abs(.CPICHEcNo) > 999 Then
                                .CPICHEcNo = -999
                            End If
                            .BCCHEcNo = -999
                            DescSortKeys_WCDMA(Count_WCDMARecord) = Count_WCDMARecord
                            DescSort_WCDMA(Count_WCDMARecord) = .CPICHEcNo
                        End With

                    End If



                    If Count_WCDMARecord = WCDMAPilotInfo.NumRecords - 1 Then

                        _isOutputWCDMA = True
                        myStr = ""
                        myMsg = ToByteArray(WCDMAPilotInfo.RFBand)

                        NoRxLev_WCDMA = Math.Round(Convert.ToDouble(WCDMAPilotInfo.IO))

                        myStr = myBand & "," & "UMTS" & "," & _ScanID & "," & WCDMAPilotInfo.Channel & "," & WCDMAPilotInfo.MCC & "," & WCDMAPilotInfo.MNC & "," & NoRxLev_WCDMA


                        Array.Sort(DescSort_WCDMA, DescSortKeys_WCDMA)
                        ReDim Preserve scanSort_WCDMA(UBound(scan_WCDMA))
                        For i = 0 To UBound(DescSortKeys_WCDMA)
                            Dim tmpInt As Integer = DescSortKeys_WCDMA(UBound(DescSortKeys_WCDMA) - i)
                            scanSort_WCDMA(i) = scan_WCDMA(i)

                        Next i
                        ReDim Preserve scanSort_WCDMA(Max(N_WCDMA, 5))
                        If (WCDMAPilotInfo.NumRecords <= 5) Then
                            For j = WCDMAPilotInfo.NumRecords To 5
                                With scanSort_WCDMA(j)
                                    .Delay = -999
                                    .CPICHSC = -999
                                    .SpreadChips = -999
                                    .CellID = -999
                                    .PSCEcNo = -999
                                    .SSCEcNo = -999
                                    .CPICHEc = -999
                                    .CPICHEcNo = -999
                                    .BCCHEcNo = -999
                                    .SigInt = -999
                                    .LAC = -999
                                End With
                            Next
                        End If

                        Select Case myBand.Length
                            Case Is > 0

                                If Not IsNothing(ScnrChLstWCDMAobj) Then
                                    For j = 0 To UBound(ScnrChLstWCDMAobj)
                                        If ScnrChLstWCDMAobj(j).Channel = ch_WCDMA Then
                                            If N_WCDMA > 0 Then ScnrChLstWCDMAobj(j).lastFoundTime = dttmDt
                                            If ScnrChLstWCDMAobj(j).Scan(0).CPICHEc < scanSort_WCDMA(0).CPICHEc Then
                                                ScnrChLstWCDMAobj(j).Io = NoRxLev_WCDMA
                                                ScnrChLstWCDMAobj(j).Band = myBand
                                                ScnrChLstWCDMAobj(j).Scan = scanSort_WCDMA
                                            End If
                                        End If
                                    Next j
                                End If
                            Case Else
                                myStr = ""
                        End Select

                        currentWCDMAChannelDaTa.value = scanSort_WCDMA(0).CPICHEc
                        _currentRSSIValue = scanSort_WCDMA(0).CPICHEc
                        currentWCDMAChannelDaTa.value2 = NoRxLev_WCDMA
                        currentWCDMAChannelDaTa.channel = ch_WCDMA
                        For k = 0 To 5
                            With scanSort_WCDMA(k)
                                myStr += "," & .CPICHSC & "," & .SpreadChips & "," & .CellID & "," & .LAC & "," & .PSCEcNo & "," & .CPICHEc & "," & .CPICHEcNo & "," & .BCCHEcNo & "," & .SigInt
                            End With
                        Next k
                        myStr1 = myStr
                    End If

                End If

        End Select



        If msgType <> 10010 Then
            If msgType <> 10011 Or GSM_BCCHInfo.NumChannels = Count_GSMRecord + 1 Then

                strMSp12am = dttm
                If dttm.IndexOf(" ") > 0 Then
                    coldt = dttm.Substring(0, dttm.IndexOf(" "))
                End If

                If _isOutputLTE Then

                    If _LastScanID <> _ScanID Then
                        _FirstScanIDData.gps_time_stamp = dttmDt
                        _FirstScanIDData.time_stamp = dttm
                        _FirstScanIDData.lat = lat
                        _FirstScanIDData.lon = lon

                    End If



                    Dim blankPolygonLTE As String = ""
                    Dim blankPolygonLTEMax As String = ""
                    Dim LogTrace As String = ""
                    currentLTEChannelData.OperatorName = ""

                    Dim MCC As String = ""
                    Dim MNC As String = ""
                    Dim SIB1 As New LTESIB1()
                    If _EnableSipDecoding Then

                        If _LTESIBInfo.SIB1 <> "" Then
                            SIB1 = ScannerCommon.GetSIB1(_LTESIBInfo.SIB1)
                        End If

                        If Not IsNothing(SIB1.TAC) And SIB1.TAC.Trim <> "" Then
                            SIB1.TAC = Convert.ToInt32(SIB1.TAC, 16)
                        End If
                        MCC = SIB1.MCC0 + SIB1.MCC1 + SIB1.MCC2
                        MNC = SIB1.MNC0 + SIB1.MNC1 + SIB1.MNC2

                        If MCC.Trim() = "" Then

                            Dim SIDNIDList As List(Of SIDNIDStore) = LTEChannelList(currentLTEChannelData.channel + "|" + currentLTEChannelData.Band)


                            'Need to detect empty SID and NID channel

                            If SIDNIDList.Count = 1 Then
                                MCC = SIDNIDList(0).SID
                                MNC = SIDNIDList(0).NID
                                LogTrace = "SIDmodCo"
                            Else
                                Dim Min As Double = 100000000
                                Dim jj As Integer = -1
                                For j = 0 To SIDNIDList.Count - 1
                                    Dim dttmDate = Convert.ToDateTime(dttm)
                                    Dim duration As TimeSpan = dttmDate - SIDNIDList(j).time
                                    Dim totalms = duration.TotalMilliseconds
                                    If (Min > Math.Abs(totalms)) Then
                                        Min = totalms
                                        jj = j
                                    End If

                                Next
                                If (jj >= 0) Then
                                    LogTrace = "SIDmodCo"
                                    MCC = SIDNIDList(jj).SID
                                    MNC = SIDNIDList(jj).NID
                                End If

                            End If
                        Else
                            'Update the timestamp
                            Dim SIDNIDList As List(Of SIDNIDStore) = LTEChannelList(currentLTEChannelData.channel + "|" + currentLTEChannelData.Band)
                            If SIDNIDList.Count > 1 Then
                                For j = 0 To SIDNIDList.Count - 1
                                    If SIDNIDList(j).SID = MCC And SIDNIDList(j).NID = MNC Then
                                        Dim tmpStore As New SIDNIDStore()
                                        tmpStore.time = Convert.ToDateTime(dttm)
                                        tmpStore.SID = MCC
                                        tmpStore.NID = MNC
                                        tmpStore.band = currentLTEChannelData.Band
                                        SIDNIDList(j) = tmpStore
                                    End If
                                Next
                            End If

                            LTEChannelList(currentLTEChannelData.channel + "|" + currentLTEChannelData.Band) = SIDNIDList
                        End If

                        If MCC.Trim() <> "" Then
                            Dim tmpInt As Integer
                            If Integer.TryParse(MCC.Trim + MNC.Trim, tmpInt) Then
                                currentLTEChannelData.OperatorName = GetOperatorName(MCC.Trim + MNC.Trim)
                            End If

                        End If

                        'modified by My 05/31/2018 #1277
                        If SIB1.SIB1cellBarred.Trim <> "" Or SIB1.SIB1intraFreqReselection.Trim <> "" Or SIB1.SIB1qRxLevMin.Trim <> "" Or SIB1.SIB1pMax.Trim <> "" Or SIB1.SIB1tddSubFrameAssignment.Trim <> "" Or SIB1.SIB1tddSpecialSubFramePatterns.Trim <> "" Then
                            _LTESIBInfo.SIB1 = SIB1.SIB1cellBarred + "|" + SIB1.SIB1intraFreqReselection + "|" + SIB1.SIB1qRxLevMin + "|" + SIB1.SIB1pMax + "|" + SIB1.SIB1tddSubFrameAssignment + "|" + SIB1.SIB1tddSpecialSubFramePatterns
                        End If


                        If _LTESIBInfo.SIB2 <> "" Then
                            _LTESIBInfo.SIB2 = ScannerCommon.GetSIB2(_LTESIBInfo.SIB2)
                        End If
                        If _LTESIBInfo.SIB3 <> "" Then
                            _LTESIBInfo.SIB3 = ScannerCommon.GetSIB3(_LTESIBInfo.SIB3)
                        End If
                        If _LTESIBInfo.SIB4 <> "" Then
                            _LTESIBInfo.SIB4 = ScannerCommon.GetSIB4(_LTESIBInfo.SIB4)
                        End If
                        If _LTESIBInfo.SIB5 <> "" Then
                            _LTESIBInfo.SIB5 = ScannerCommon.GetSIB5(_LTESIBInfo.SIB5)
                        End If
                        If _LTESIBInfo.SIB6 <> "" Then
                            _LTESIBInfo.SIB6 = ScannerCommon.GetSIB6(_LTESIBInfo.SIB6)
                        End If
                        If _LTESIBInfo.SIB8 <> "" Then
                            _LTESIBInfo.SIB8 = ScannerCommon.GetSIB8(_LTESIBInfo.SIB8)
                        End If

                    End If
                    If isGPSInterpolated Then
                        If LogTrace.Trim = "" Then
                            LogTrace = "ModGPS"
                        Else
                            LogTrace = LogTrace + "|ModGPS"
                        End If
                    End If
                    Dim strBuilder As New StringBuilder()
                    strBuilder.Append(msgType)
                    strBuilder.Append(",")
                    strBuilder.Append(strMSp12am)
                    strBuilder.Append(",")
                    strBuilder.Append(dttm)
                    strBuilder.Append(",")
                    strBuilder.Append(FormatNumber(lat, 6))
                    strBuilder.Append(",")
                    strBuilder.Append(FormatNumber(lon, 6))
                    strBuilder.Append(",")
                    strBuilder.Append(myStr)
                    strBuilder.Append(",")
                    strBuilder.Append(currentLTEChannelData.OperatorName + ",ALL," + _FileName + "," + _CollectedDate)
                    strBuilder.Append(",")
                    strBuilder.Append(_MarketName + "," + _Campaign + "," + _ClientName + "," + LogTrace)
                    strBuilder.Append("," & _LTESIBInfo.SIB_PhysicalCellId & "," & _LTESIBInfo.SIB_PhysicalCellId_Position & "," & MCC & "," & MNC & "," & SIB1.CI & "," & SIB1.TAC & ",")
                    strBuilder.Append(_LTESIBInfo.SIB1 + "," + _LTESIBInfo.SIB2 + "," + _LTESIBInfo.SIB3 + "," + _LTESIBInfo.SIB4 + "," + _LTESIBInfo.SIB5 + "," + _LTESIBInfo.SIB6 + "," + _LTESIBInfo.SIB8)

                    _rowCount = _rowCount + 1

                    myStr = _rowCount.ToString() + "|" + strBuilder.ToString()

                    If type = "LTE" And fileInputObj.FileMergeName.Trim <> "" And InputFileName.Contains(fileInputObj.MergePattern2) Then
                        _FileSize3030 = _FileSize3030 + System.Text.ASCIIEncoding.ASCII.GetByteCount(myStr)
                    ElseIf type = "LTE" And fileInputObj.FileMergeName.Trim <> "" And InputFileName.Contains(fileInputObj.MergePattern1) Then
                        _FileSize3040 = _FileSize3040 + System.Text.ASCIIEncoding.ASCII.GetByteCount(myStr)
                    End If

                    Dim isMerged As String = ""
                    If fileInputObj.FileMergeName.Trim <> "" Then
                        isMerged = "|MERGED FILE"
                    End If

                    'modified by My 05/31/2018 #1277
                    Dim header As String = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",LTE,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion() + isMerged + ",-999,-999,-999,-999,-999,-999,SIB1cellBarred|SIB1intraFreqReselection|SIB1qRxLevMin(dbm)|SIB1pMax|SIB1tddSubFrameAssignment|SIB1tddSpecialSubFramePatterns,SIB2RAPreambles|SIB2PwrRampStep|SIB2InitRecdTrgPwr|SIB2PreambleTransMax|SIB2RARespWinSize|SIB2MACContResTimer|SIB2maxHARQMsg3Tx|SIB2PDSCHRefSigPwr|SIB2PDSCHPb|SIB2SRSbwConfig|SIB2SRSsubframeConfig|SIB2ACKNACKsimultTx|SIB2P0NominalPUSCH|SIB2Alpha|SIB2P0NomPUCCH|SIB2t300|SIB2t301|SIB2t310|SIB2n310|SIB2t311|SIB2n311,SIB3qHyst|SIB3sNonIntraSearch(db)|SIB3ThreshServLow(db)|SIB3cellReselPriority|SIB3qRxLevMin(dbm)|SIB3pMax(dbm)|SIB3sIntraSearch(db),SIB4IntraFreqCellList|SIB4PCIs|SIB4OffsetPCIs|SIB4BlackCellList|SIB4BlackCells,SIB5qRxLevMin(dbm)|SIB5pMax(dbm)|SIB5tReslectEUTRA(s)|SIB5threshXhi(db)|SIB5threshXlo(db),SIB6ThreshXHi(db)|SIB6ThreshXLo(db)|SIB6qRxLevMin(dbm)|SIB6pMaxUTRA|SIB6qQualMin(db),SIB8cdmaEUTRASynch|SIB8synchSysTime|SIB8searchWinSize"
                    'Dim header As String = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",LTE,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion() + isMerged + ",-999,-999,-999,-999,-999,-999,SIB1cellBarred|SIB1intraFreqReselection|SIB1qRxLevMin(dbm)|SIB1pMax,SIB2RAPreambles|SIB2PwrRampStep|SIB2InitRecdTrgPwr|SIB2PreambleTransMax|SIB2RARespWinSize|SIB2MACContResTimer|SIB2maxHARQMsg3Tx|SIB2PDSCHRefSigPwr|SIB2PDSCHPb|SIB2SRSbwConfig|SIB2SRSsubframeConfig|SIB2ACKNACKsimultTx|SIB2P0NominalPUSCH|SIB2Alpha|SIB2P0NomPUCCH|SIB2t300|SIB2t301|SIB2t310|SIB2n310|SIB2t311|SIB2n311,SIB3qHyst|SIB3sNonIntraSearch(db)|SIB3ThreshServLow(db)|SIB3cellReselPriority|SIB3qRxLevMin(dbm)|SIB3pMax(dbm)|SIB3allowedMeasBandwidth|SIB3sIntraSearch(db),SIB4IntraFreqCellList|SIB4PCIs|SIB4OffsetPCIs|SIB4BlackCellList|SIB4BlackCells,SIB5qRxLevMin(dbm)|SIB5pMax(dbm)|SIB5tReslectEUTRA(s)|SIB5threshXhi(db)|SIB5threshXlo(db),SIB6ThreshXHi(db)|SIB6ThreshXLo(db)|SIB6qRxLevMin(dbm)|SIB6pMaxUTRA|SIB6qQualMin(db),SIB8cdmaEUTRASynch|SIB8synchSysTime|SIB8searchWinSize"

                    If isLTEFirstRecord Then
                        'Write Header
                        'Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7
                        isLTEFirstRecord = False
                        _OutputWriter.WriteLine(header)
                    End If

                    Dim isWritePARSE As Boolean = False
                    Dim isWriteOperator As Boolean = False
                    Dim isWriteBandOperator As Boolean = False


                    If currentLTEChannelData.OperatorName.Trim = "" Then
                        If _RECaddNoSCondition.lastScanId <> _ScanID Then
                            _RECaddNoSCondition.operatorEmptyCount = _RECaddNoSCondition.operatorEmptyCount + 1

                        End If
                    Else
                        _RECaddNoSCondition.operatorEmptyCount = 0
                    End If

                    Dim operators = _OperatorSet.Keys

                    Dim i As Integer


                    _TempStringList.Add(myStr)


lineoutputLTE:
                    operators = _OperatorSet.Keys

                    _canWrite = True


                    If isLast Or _canWrite Then


                        Dim count_to_last As Integer = 0

                        For Each myStr In _TempStringList
                            count_to_last = count_to_last + 1

                            Dim strs = myStr.Split(",")
                            _FirstScanIDData.time_stamp = strs(1)
                            _FirstScanIDData.gps_time_stamp = strs(2)
                            _FirstScanIDData.lat = strs(3)
                            _FirstScanIDData.lon = strs(4)

                            For i = 0 To operators.Count - 1
                                Dim tmpFileWriter As New OutputFileWriter()
                                tmpFileWriter = _OperatorSet.Item(operators(i))

                                If tmpFileWriter.isDoNotExist Then

                                    If Not _ScanIDSet.Contains(_ScanID.ToString() + tmpFileWriter.OperatorName) Then
                                        _ScanIDSet.Add(_ScanID.ToString() + tmpFileWriter.OperatorName)

                                        If tmpFileWriter.isFirstRecrod Then
                                            tmpFileWriter.isFirstRecrod = False
                                            tmpFileWriter.writer.WriteLine(header)
                                        End If

                                        'Dim LTEBandList As String() = {"700", "850", "1900", "2100", "2300", "2500"}

                                        Dim bandStr = DoNotExistList.Item(tmpFileWriter.OperatorName).ToString()
                                        Dim tmps As String() = bandStr.Split(")")
                                        Dim tmpStr As String
                                        For Each tmpStr In tmps
                                            If tmpStr.IndexOf("LTE") >= 0 Then
                                                bandStr = tmpStr
                                            End If
                                        Next
                                        bandStr = bandStr.Replace("(", "")
                                        bandStr = bandStr.Replace("LTE", "")
                                        bandStr = bandStr.Trim

                                        Dim LTEBandList As String() = bandStr.Split(",")



                                        blankPolygonLTEMax = msgType & "," & _FirstScanIDData.time_stamp & "," & _FirstScanIDData.gps_time_stamp & "," & _FirstScanIDData.lat & "," & _FirstScanIDData.lon & ",LTE," & _ScanID & ",-999,-999,-999" &
                                          ",-130,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," &
                                           tmpFileWriter.OperatorName & ",MAX," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS,,," & tmpFileWriter.currentNosData.MCC & "," & tmpFileWriter.currentNosData.MNC & ",,,,,,,,,"

                                        For Each band In LTEBandList
                                            If band.Trim <> "" Then
                                                'All Band TXT File
                                                blankPolygonLTE = msgType & "," & _FirstScanIDData.time_stamp & "," & _FirstScanIDData.gps_time_stamp & "," & _FirstScanIDData.lat & "," & _FirstScanIDData.lon & ",LTE," & _ScanID & ",-999," & band & ",-999" &
                                                  ",-130,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," &
                                                   tmpFileWriter.OperatorName & ",ALL," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS,,," & tmpFileWriter.currentNosData.MCC & "," & tmpFileWriter.currentNosData.MNC & ",,,,,,,,,"
                                                tmpFileWriter.writer.WriteLine(blankPolygonLTE)

                                                'Per Band TXT Files
                                                Dim tmpFileWriter1 As New OutputFileWriter()
                                                tmpFileWriter1 = _OperatorBandSet.Item(operators(i) + "LTE" + band)
                                                If tmpFileWriter1.isFirstRecrod Then
                                                    tmpFileWriter1.isFirstRecrod = False
                                                    tmpFileWriter1.writer.WriteLine(header)
                                                End If
                                                tmpFileWriter1.writer.WriteLine(blankPolygonLTE)
                                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                                    tmpFileWriter1.writer.WriteLine(blankPolygonLTE.Replace(",ALL,", ",MAX,"))

                                                End If

                                                _OperatorBandSet.Item(operators(i) + "LTE" + band) = tmpFileWriter1
                                            End If

                                        Next

                                        'All Band MAX record
                                        tmpFileWriter.writer.WriteLine(blankPolygonLTEMax)
                                        _OperatorSet.Item(operators(i)) = tmpFileWriter
                                    End If

                                End If
                            Next

                            'Dim strs = myStr.Split(",")

                            Dim count_input_chanel As Integer = 0
                            Dim count_input_chanel3030 As Integer = 0
                            Dim count_input_chanel3040 As Integer = 0


                            For Each tmpStr In LTEChannelList.Keys
                                Dim tmpObj As InputChannelStore = LTEChannelListWithTime.Item(tmpStr)
                                If Not ((Convert.ToDateTime(dttm) - tmpObj.StartTime).TotalMilliseconds < 0 And tmpObj.isStartLate) Then
                                    count_input_chanel = count_input_chanel + 1
                                    If (fileInputObj.FileMergeName.Trim() <> "") Then
                                        If tmpObj.inputFileName.IndexOf(fileInputObj.MergePattern1) > 0 Then
                                            count_input_chanel3040 = count_input_chanel3040 + 1
                                        End If
                                        If tmpObj.inputFileName.IndexOf(fileInputObj.MergePattern2) > 0 Then
                                            count_input_chanel3030 = count_input_chanel3030 + 1
                                        End If
                                    End If
                                End If

                            Next

                            Dim shouldEndRound As Boolean = False

                            If LTEChannelList.Count = 1 Then
                                shouldEndRound = True
                            End If
                            If LTEChannelList.Count > 1 And strs(7) <> _lastChannel Then
                                shouldEndRound = True
                            End If

                            Dim isChannelRepeatedCountMet As Boolean = False
                            If (fileInputObj.FileMergeName.Trim() <> "") Then
                                isChannelRepeatedCountMet = _LTEScannerData.channelRepeatedCount3030 > Int(count_input_chanel3030 * 2.5) - 1 And _LTEScannerData.channelRepeatedCount3040 > Int(count_input_chanel3040 * 2.5) - 1
                            Else
                                isChannelRepeatedCountMet = _LTEScannerData.channelRepeatedCount > Int(count_input_chanel * 2.5) - 1
                            End If

                            Dim isTimeLimitReached As Boolean = False

                            If InputFileName.IndexOf(fileInputObj.MergePattern2) > 0 Then
                                _isNo3030Message = False
                            End If

                            If InputFileName.IndexOf(fileInputObj.MergePattern1) > 0 Then
                                _isNo3040Message = False
                            End If

                            If Not isSetlastEndRoundTimeStamp Then
                                _lastEndRoundTimeStamp = Convert.ToDateTime(dttm)
                                isSetlastEndRoundTimeStamp = True
                            End If

                            If (Convert.ToDateTime(strs(1)) - _lastEndRoundTimeStamp).TotalSeconds > _LTE_ScanID_Maximum_Duration1 Then
                                isTimeLimitReached = True

                            End If

                            If (Convert.ToDateTime(strs(1)) - _lastEndRoundTimeStamp).TotalSeconds > _LTE_ScanID_Maximum_Duration And ((_LTEScannerData.channelRepeatedCount3030 > Int(count_input_chanel3030 * 2.5) - 1 And _isNo3040Message) Or (_LTEScannerData.channelRepeatedCount3040 > Int(count_input_chanel3040 * 2.5) - 1) And _isNo3030Message) Then
                                isTimeLimitReached = True

                            End If

                            If Convert.ToDateTime(strs(1)) > _lastLTETimeStamp3030 Then
                                If InputFileName.IndexOf(fileInputObj.MergePattern1) > 0 Then
                                    isChannelRepeatedCountMet = _LTEScannerData.channelRepeatedCount3040 > Int(count_input_chanel3040 * 2.5) - 1
                                    _LTEScannerData.inputChannelList.IntersectWith(LTEChannelList3040)
                                End If
                            End If

                            If Convert.ToDateTime(strs(1)) > _lastLTETimeStamp3040 Then
                                If InputFileName.IndexOf(fileInputObj.MergePattern2) > 0 Then
                                    isChannelRepeatedCountMet = _LTEScannerData.channelRepeatedCount3030 > Int(count_input_chanel3030 * 2.5) - 1

                                    _LTEScannerData.inputChannelList.IntersectWith(LTEChannelList3030)

                                End If
                            End If

                            If ((_LTEScannerData.inputChannelList.Count = 0 And shouldEndRound) Or (isLast And count_to_last = _TempStringList.Count) Or isChannelRepeatedCountMet Or isTimeLimitReached) Then

                                _lastEndRoundTimeStamp = Convert.ToDateTime(strs(1))
                                _isNo3030Message = True
                                _isNo3040Message = True

                                'Find Max 

                                Dim keyTmp As String
                                Dim tmpList As List(Of ChannelData)

                                For Each keyTmp In _OperatorSet.Keys

                                    Dim tmpFileWriter As New OutputFileWriter()

                                    tmpFileWriter = _OperatorSet.Item(keyTmp)


                                    If Not tmpFileWriter.isDoNotExist Then
                                        Dim result = From v In _LTEScannerData.channelData Order By v.value Descending
                                        tmpList = result.ToList()
                                        Dim isALl999 = True
                                        For i = 0 To tmpList.Count - 1
                                            If keyTmp = tmpList(i).OperatorName Then
                                                If tmpList(i).value <> -999 Then
                                                    isALl999 = False
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If isALl999 Then
                                            result = From v In _LTEScannerData.channelData Order By v.value2 Descending
                                            tmpList = result.ToList()
                                        End If


                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName = keyTmp Then
                                                If Not tmpFileWriter.isDoNotExist Then
                                                    If Not (isLast And count_to_last >= _TempStringList.Count) Then
                                                        If _ScanID = 1 Or _writeFirstMaxRecordList.Contains(keyTmp) Then
                                                            tmpFileWriter.writer.WriteLine((tmpList(i).outputStrData.Replace(",ALL,", ",MAX,")))
                                                        End If
                                                        _writeFirstMaxRecordList.Add(keyTmp)

                                                    End If

                                                End If
                                                Exit For
                                            End If
                                        Next
                                    End If

                                Next

                                For Each tmpKey In _OperatorBandSet.Keys

                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorBandSet.Item(tmpKey)

                                    If Not tmpFileWriter.isDoNotExist Then
                                        Dim result = From v In _LTEScannerData.channelData Order By v.value Descending
                                        tmpList = result.ToList()
                                        Dim isALl999 = True
                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName + "LTE" + tmpList(i).Band = tmpKey Then
                                                If tmpList(i).value <> -999 Then
                                                    isALl999 = False
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If isALl999 Then

                                            result = From v In _LTEScannerData.channelData Order By v.value2 Descending
                                            tmpList = result.ToList()
                                        End If

                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName + "LTE" + tmpList(i).Band = tmpKey Then

                                                If Not tmpFileWriter.isDoNotExist Then
                                                    If isLast Then
                                                        Dim tt As String = ""

                                                    End If
                                                    If Not (isLast And count_to_last = _TempStringList.Count) Then
                                                        tmpFileWriter.writer.WriteLine((tmpList(i).outputStrData.Replace(",ALL,", ",MAX,")))
                                                    End If

                                                End If
                                                Exit For
                                            End If
                                        Next
                                    End If

                                Next

                                _RECaddNoSCondition.lastScanId = _ScanID

                                _LastScanID = _ScanID

                                _LTEScannerData = New ScannerData()

                                _LTEScannerData.channelRepeatedCount = 0
                                _LTEScannerData.channelRepeatedCount3030 = 0
                                _LTEScannerData.channelRepeatedCount3040 = 0
                                _LTEScannerData.consecutiveChannelRepeatedCount = 0
                                Dim tmpStr As String
                                _LTEScannerData.inputChannelList = New HashSet(Of String)
                                _LTEScannerData.inputChannelList3030 = New HashSet(Of String)
                                _LTEScannerData.inputChannelList3040 = New HashSet(Of String)

                                _LTEScannerData.channelData = New List(Of ChannelData)
                                For Each tmpStr In LTEChannelList.Keys
                                    Dim tmpObj As InputChannelStore = LTEChannelListWithTime.Item(tmpStr)
                                    If Not ((Convert.ToDateTime(dttm) - tmpObj.StartTime).TotalMilliseconds < 0 And tmpObj.isStartLate) Then
                                        _LTEScannerData.inputChannelList.Add(tmpStr.Split("|")(0))
                                    End If
                                Next

                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                    _ScanID = _ScanID + 1
                                End If

                            End If
                            strs(6) = _ScanID.ToString

                            Dim tmpChannelInt As Integer
                            If Integer.TryParse(strs(7).Trim, tmpChannelInt) Then

                                If (ChannelWithAllRSSINev999.Item(tmpChannelInt.ToString()) = True) Then
                                    If strs(12) = "-999" Or strs(12) = "-130" Then
                                        strs(12) = "-130"
                                        If strs(60).IndexOf("ADDrecNoS") < 0 Then
                                            strs(60) = IIf(strs(60).Trim <> "", strs(60) + "|ADDrecNoS", "ADDrecNoS")
                                        End If
                                    End If
                                End If
                            End If

                            myStr = Join(strs, ",")


                            _OutputWriter.WriteLine(myStr)
                            Dim currentOperatorName As String = strs(53)
                            Dim currentBand As String = strs(8)
                            If currentOperatorName <> "" Then

                                If Not IsNothing(_OperatorSet.Item(currentOperatorName)) Then

                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorSet.Item(currentOperatorName)
                                    If Not tmpFileWriter.isDoNotExist Then
                                        If tmpFileWriter.isFirstRecrod Then
                                            tmpFileWriter.isFirstRecrod = False

                                            _OperatorSet.Item(currentOperatorName) = tmpFileWriter

                                            tmpFileWriter.writer.WriteLine(header)
                                        End If
                                        tmpFileWriter.writer.WriteLine(myStr)
                                        _OperatorSet.Item(currentOperatorName) = tmpFileWriter
                                    End If

                                End If

                                Dim key As String = currentOperatorName + "LTE" + currentBand
                                If Not IsNothing(_OperatorBandSet.Item(key)) And Not isWriteBandOperator Then
                                    If (_OperatorBandSet.ContainsKey(key)) Then
                                        Dim tmpFileWriter As New OutputFileWriter()
                                        tmpFileWriter = _OperatorBandSet.Item(key)
                                        If Not tmpFileWriter.isDoNotExist Then
                                            If tmpFileWriter.isFirstRecrod Then
                                                tmpFileWriter.isFirstRecrod = False
                                                _OperatorBandSet.Item(key) = tmpFileWriter
                                                tmpFileWriter.writer.WriteLine(header)

                                            End If
                                            tmpFileWriter.writer.WriteLine(myStr)
                                            _OperatorBandSet.Item(key) = tmpFileWriter
                                        End If
                                    End If
                                End If

                            End If

                            currentLTEChannelData.outputStrData = myStr
                            currentLTEChannelData.OperatorName = strs(53)
                            currentLTEChannelData.Band = strs(8)
                            currentLTEChannelData.value = Double.Parse(strs(12))
                            currentLTEChannelData.value2 = Double.Parse(strs(13))



                            If _LTEScannerData.inputChannelList.Contains(strs(7)) Then
                                _LTEScannerData.inputChannelList.Remove(strs(7))
                            End If

                            If _lastChannel <> strs(7) Then
                                If InputFileName.IndexOf(fileInputObj.MergePattern1) > 0 Then
                                    _LTEScannerData.channelRepeatedCount = _LTEScannerData.channelRepeatedCount + 1
                                Else
                                    _LTEScannerData.channelRepeatedCount = _LTEScannerData.channelRepeatedCount + 1
                                End If

                                If LTEChannelList3030.Contains(strs(7)) Then
                                    _LTEScannerData.channelRepeatedCount3030 = _LTEScannerData.channelRepeatedCount3030 + 1
                                End If

                                If LTEChannelList3040.Contains(strs(7)) Then
                                    _LTEScannerData.channelRepeatedCount3040 = _LTEScannerData.channelRepeatedCount3040 + 1
                                End If

                            End If

                            _LTEScannerData.channelData.Add(currentLTEChannelData)


                            _lastChannel = strs(7)



                            _isOutputLTE = False
                            isLTEFirstRecord = False
                            LogTrace = ""
                            currentLTEChannelData = New ChannelData()
                        Next

                        _TempStringList = New List(Of String)


                    End If

                End If

                If _isOutputWCDMA Then

                    count_total_message = count_total_message + 1

                    currentWCDMAChannelDaTa.OperatorName = WCDMAPilotInfo.OperatorName
                    If isGPSInterpolated Then
                        If WCDMAPilotInfo.LogTrace.Trim = "" Then
                            WCDMAPilotInfo.LogTrace = "ModGPS"
                        Else
                            WCDMAPilotInfo.LogTrace = WCDMAPilotInfo.LogTrace + "|ModGPS"
                        End If
                    End If
                    Dim strBuilder As New StringBuilder()
                    strBuilder.Append(msgType)
                    strBuilder.Append(",")
                    strBuilder.Append(strMSp12am)
                    strBuilder.Append(",")
                    strBuilder.Append(dttm)
                    strBuilder.Append(",")
                    strBuilder.Append(FormatNumber(lat, 6))
                    strBuilder.Append(",")
                    strBuilder.Append(FormatNumber(lon, 6))
                    strBuilder.Append(",")
                    strBuilder.Append(myStr)
                    strBuilder.Append(",")
                    strBuilder.Append(WCDMAPilotInfo.OperatorName + ",ALL," + _FileName + "," + _CollectedDate)
                    strBuilder.Append(",")
                    strBuilder.Append(_MarketName + "," + _Campaign + "," + _ClientName + "," + WCDMAPilotInfo.LogTrace)
                    strBuilder.Append(",,,,,,")

                    _rowCount = _rowCount + 1
                    myStr = _rowCount.ToString() + "|" + strBuilder.ToString()
                    If _LastScanID <> _ScanID Then
                        _FirstScanIDData.gps_time_stamp = dttmDt
                        _FirstScanIDData.time_stamp = dttm
                        _FirstScanIDData.lat = lat
                        _FirstScanIDData.lon = lon

                    End If


                    If IsNothing(currentWCDMAChannelDaTa.OperatorName) Then
                        currentWCDMAChannelDaTa.OperatorName = ""
                    End If


                    Dim header As String = ""
                    header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,UMTS,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion() + ",,,,,"
                    If isUMTSFirstRecord Then
                        'Write Header
                        'Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7

                        _OutputWriter.WriteLine(header)

                    End If

                    Dim isWritePARSE As Boolean = False
                    Dim isWriteOperator As Boolean = False

                    '10103,3/29/2017 6:11:59 PM,3/29/2017 6:11:59 PM,44.916596,-107.151660,850,UMTS,291,1007,310,410,-89,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,AT&T,ALL,2017-03-29-12-06-24-3040-0027-0006-3040-S,03/29/2017,Gilette WY,17D1,AT&T,SIDmodCo,,,,,,
                    Dim blankPolygonUMTS As String = ""
                    Dim blankPolygonUMTSMAX As String = ""

                    If currentWCDMAChannelDaTa.OperatorName.Trim = "" Then
                        If _RECaddNoSCondition.lastScanId <> _ScanID Then
                            _RECaddNoSCondition.operatorEmptyCount = _RECaddNoSCondition.operatorEmptyCount + 1

                        End If
                    Else
                        _RECaddNoSCondition.operatorEmptyCount = 0
                    End If

                    Dim operators = _OperatorSet.Keys
                    Dim i As Integer
                    Dim count_temp As Integer = 0

                    _TempStringList.Add(myStr)
                    operators = _OperatorSet.Keys
                    _canWrite = True


lineoutputWCDMA:

                    If _canWrite Or isLast Then

                        Dim count_to_last As Integer = 0
                        For Each myStr In _TempStringList

                            count_to_last = count_to_last + 1

                            operators = _OperatorSet.Keys

                            Dim strs = myStr.Split(",")
                            _FirstScanIDData.time_stamp = strs(1)
                            _FirstScanIDData.gps_time_stamp = strs(2)
                            _FirstScanIDData.lat = strs(3)
                            _FirstScanIDData.lon = strs(4)

                            For i = 0 To operators.Count - 1
                                Dim tmpFileWriter As New OutputFileWriter()
                                tmpFileWriter = _OperatorSet.Item(operators(i))

                                If tmpFileWriter.isDoNotExist Then
                                    If Not _ScanIDSet.Contains(_ScanID.ToString() + tmpFileWriter.OperatorName) Then
                                        _ScanIDSet.Add(_ScanID.ToString() + tmpFileWriter.OperatorName)

                                        If tmpFileWriter.isFirstRecrod Then
                                            tmpFileWriter.isFirstRecrod = False
                                            tmpFileWriter.writer.WriteLine(header)

                                        End If
                                        'Dim UMTSBandList As String() = {"700", "850", "1900", "2100"}

                                        Dim bandStr = DoNotExistList.Item(tmpFileWriter.OperatorName).ToString()
                                        Dim tmps As String() = bandStr.Split(")")
                                        Dim tmpStr As String
                                        For Each tmpStr In tmps
                                            If tmpStr.IndexOf("UMTS") >= 0 Then
                                                bandStr = tmpStr
                                            End If
                                        Next
                                        bandStr = bandStr.Replace("(", "")
                                        bandStr = bandStr.Replace("UMTS", "")
                                        bandStr = bandStr.Trim

                                        Dim UMTSBandList As String() = bandStr.Split(",")


                                        'Write PARSE TXT File
                                        'blankPolygonUMTS = "10103," & dttm & "," & dttmDt & "," & lat & "," & lon & "," & currentWCDMAChannelDaTa.Band & ",UMTS," & _ScanID & "-999,-999,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," & operators(i) & ",ALL," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS,,,,,,"
                                        blankPolygonUMTSMAX = "10103," & _FirstScanIDData.time_stamp & "," & _FirstScanIDData.gps_time_stamp & "," & _FirstScanIDData.lat & "," & _FirstScanIDData.lon & ",-999,UMTS," & _ScanID & ",-999,-999,-999,-130,-999,-999,-999,-999,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," & tmpFileWriter.OperatorName & ",MAX," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS,,,,,,"

                                        For Each band In UMTSBandList
                                            If band.Trim <> "" Then
                                                'All Band UMTS
                                                blankPolygonUMTS = "10103," & _FirstScanIDData.time_stamp & "," & _FirstScanIDData.gps_time_stamp & "," & _FirstScanIDData.lat & "," & _FirstScanIDData.lon & "," & band & ",UMTS," & _ScanID & ",-999,-999,-999,-130,-999,-999,-999,-999,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," & operators(i) & ",ALL," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS,,,,,,"
                                                tmpFileWriter.writer.WriteLine(blankPolygonUMTS)

                                                'Per Band TXT Files                                
                                                Dim tmpFileWriter1 As New OutputFileWriter()
                                                tmpFileWriter1 = _OperatorBandSet.Item(operators(i) + "UMTS" + band)
                                                If tmpFileWriter1.isFirstRecrod Then
                                                    tmpFileWriter1.isFirstRecrod = False
                                                    tmpFileWriter1.writer.WriteLine(header)
                                                End If
                                                tmpFileWriter1.writer.WriteLine(blankPolygonUMTS)
                                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                                    tmpFileWriter1.writer.WriteLine(blankPolygonUMTS.Replace(",ALL,", ",MAX,"))
                                                End If

                                                _OperatorBandSet.Item(operators(i) + "UMTS" + band) = tmpFileWriter1

                                            End If

                                        Next

                                        'All Band Max Record
                                        tmpFileWriter.writer.WriteLine(blankPolygonUMTSMAX)
                                        _OperatorSet.Item(operators(i)) = tmpFileWriter
                                    End If

                                End If

                            Next

                            Dim shouldEndRound As Boolean = False

                            If WCDMAChannelList.Count = 1 Then
                                shouldEndRound = True
                            End If
                            If WCDMAChannelList.Count > 1 And strs(8) <> _lastChannel Then
                                shouldEndRound = True
                            End If

                            Dim count_input_chanel As Integer = 0
                            For Each tmpStr In WCDMAChannelList.Keys
                                Dim tmpObj As InputChannelStore = WCDMAChannelListWithTime.Item(tmpStr)
                                If Not ((Convert.ToDateTime(dttm) - tmpObj.StartTime).TotalMilliseconds < 0 And tmpObj.isStartLate) Then
                                    count_input_chanel = count_input_chanel + 1
                                End If

                            Next

                            If ((_WCDMAScannerData.inputChannelList.Count = 0 And shouldEndRound) Or (isLast And count_to_last = _TempStringList.Count) Or _WCDMAScannerData.channelRepeatedCount > Int(count_input_chanel * 2.5) - 1) Then

                                Dim keyTmp As String

                                For Each keyTmp In _OperatorSet.Keys

                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorSet.Item(keyTmp)

                                    If Not tmpFileWriter.isDoNotExist Then
                                        Dim result = From v In _WCDMAScannerData.channelData Order By v.value Descending
                                        Dim tmpList As List(Of ChannelData) = result.ToList()
                                        Dim isALl999 = True
                                        For i = 0 To tmpList.Count - 1
                                            If keyTmp = tmpList(i).OperatorName Then
                                                If tmpList(i).value <> -999 Then
                                                    isALl999 = False
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If isALl999 Then
                                            result = From v In _WCDMAScannerData.channelData Order By v.value2 Descending
                                            tmpList = result.ToList()
                                        End If

                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName = keyTmp Then

                                                If Not (isLast And count_to_last >= _TempStringList.Count) Then

                                                    If _ScanID = 1 Or _writeFirstMaxRecordList.Contains(keyTmp) Then
                                                        tmpFileWriter.writer.WriteLine((tmpList(i).outputStrData.Replace(",ALL,", ",MAX,")))
                                                    End If
                                                    _writeFirstMaxRecordList.Add(keyTmp)

                                                End If

                                                Exit For
                                            End If
                                        Next
                                    End If

                                Next

                                For Each tmpKey In _OperatorBandSet.Keys
                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorBandSet.Item(tmpKey)

                                    If Not tmpFileWriter.isDoNotExist Then
                                        Dim result = From v In _WCDMAScannerData.channelData Order By v.value Descending
                                        Dim tmpList As List(Of ChannelData) = result.ToList()

                                        Dim isALl999 = True
                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName + "UMTS" + tmpList(i).Band = tmpKey Then
                                                If tmpList(i).value <> -999 Then
                                                    isALl999 = False
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If isALl999 Then

                                            result = From v In _WCDMAScannerData.channelData Order By v.value2 Descending
                                            tmpList = result.ToList()
                                        End If

                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName + "UMTS" + tmpList(i).Band = tmpKey Then
                                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                                    tmpFileWriter.writer.WriteLine((tmpList(i).outputStrData.Replace(",ALL,", ",MAX,")))
                                                End If


                                                Exit For
                                            End If
                                        Next
                                    End If

                                Next

                                _RECaddNoSCondition.lastScanId = _ScanID
                                _LastScanID = _ScanID

                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                    _ScanID = _ScanID + 1
                                End If

                                _WCDMAScannerData = New ScannerData()

                                _WCDMAScannerData.channelRepeatedCount = 0
                                _WCDMAScannerData.consecutiveChannelRepeatedCount = 0

                                _WCDMAScannerData.inputChannelList = New HashSet(Of String)
                                _WCDMAScannerData.channelData = New List(Of ChannelData)
                                For Each tmpStr In WCDMAChannelList.Keys

                                    Dim tmpObj As InputChannelStore = WCDMAChannelListWithTime.Item(tmpStr)
                                    If Not ((Convert.ToDateTime(dttm) - tmpObj.StartTime).TotalMilliseconds < 0 And tmpObj.isStartLate) Then
                                        _WCDMAScannerData.inputChannelList.Add(tmpStr.Split("|")(0))

                                    End If

                                Next
                            End If

                            strs(7) = _ScanID.ToString

                            Dim tmpChannelInt As Integer
                            If Integer.TryParse(strs(8).Trim, tmpChannelInt) Then

                                If (ChannelWithAllRSSINev999.Item(tmpChannelInt.ToString()) = True) Then

                                    If strs(17) = "-999" Or strs(17) = "-130" Then
                                        strs(17) = "-130"

                                        If strs(73).IndexOf("ADDrecNoS") < 0 Then
                                            strs(73) = IIf(strs(73).Trim <> "", strs(73) + "|ADDrecNoS", "ADDrecNoS")
                                        End If
                                    End If

                                End If
                            End If


                            myStr = Join(strs, ",")
                            _OutputWriter.WriteLine(myStr)
                            Dim currentOperatorName As String = strs(66)
                            Dim currentBand As String = strs(5)

                            If currentOperatorName <> "" Then
                                If Not IsNothing(_OperatorSet.Item(currentOperatorName)) Then
                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorSet.Item(currentOperatorName)

                                    If tmpFileWriter.isFirstRecrod Then

                                        tmpFileWriter.isFirstRecrod = False
                                        _OperatorSet.Item(currentOperatorName) = tmpFileWriter
                                        'Write Header                                    
                                        header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,UMTS,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion() + ",,,,,"
                                        tmpFileWriter.writer.WriteLine(header)
                                    End If

                                    tmpFileWriter.writer.WriteLine(myStr)
                                    _OperatorSet.Item(currentOperatorName) = tmpFileWriter
                                End If



                                Dim key As String = currentOperatorName + "UMTS" + currentBand
                                If Not IsNothing(_OperatorBandSet.Item(key)) Then
                                    If (_OperatorBandSet.ContainsKey(key)) Then
                                        Dim tmpFileWriter As New OutputFileWriter()
                                        tmpFileWriter = _OperatorBandSet.Item(key)

                                        If tmpFileWriter.isFirstRecrod Then

                                            tmpFileWriter.isFirstRecrod = False
                                            _OperatorBandSet.Item(key) = tmpFileWriter
                                            'Write Header                                    
                                            header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,UMTS,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion() + ",,,,,"
                                            tmpFileWriter.writer.WriteLine(header)

                                        End If
                                        tmpFileWriter.writer.WriteLine(myStr)
                                        _OperatorBandSet.Item(key) = tmpFileWriter
                                    End If
                                End If

                            End If


                            currentWCDMAChannelDaTa.outputStrData = myStr
                            currentWCDMAChannelDaTa.Band = strs(5)
                            currentWCDMAChannelDaTa.OperatorName = strs(66)
                            currentWCDMAChannelDaTa.value = Double.Parse(strs(17))
                            currentWCDMAChannelDaTa.value2 = Double.Parse(strs(11))

                            If _WCDMAScannerData.inputChannelList.Contains(strs(8)) Then
                                _WCDMAScannerData.inputChannelList.Remove(strs(8))
                            End If

                            If _lastChannel <> strs(8) Then
                                _WCDMAScannerData.channelRepeatedCount = _WCDMAScannerData.channelRepeatedCount + 1
                            End If
                            _WCDMAScannerData.channelData.Add(currentWCDMAChannelDaTa)
                            _lastChannel = strs(8)

                        Next

                        _TempStringList = New List(Of String)

                    End If

                    _isOutputWCDMA = False
                    WCDMAPilotInfo.MCC = ""
                    WCDMAPilotInfo.MNC = ""
                    WCDMAPilotInfo.LogTrace = ""
                    WCDMAPilotInfo.OperatorName = ""
                    isUMTSFirstRecord = False
                    currentWCDMAChannelDaTa = New ChannelData()

                End If


                If (_isOutputCDMA) Then


                    count_total_message = count_total_message + 1

                    Dim strBuilder As New StringBuilder()
                    strBuilder.Append(msgType)
                    strBuilder.Append(",")
                    strBuilder.Append(strMSp12am)
                    strBuilder.Append(",")
                    strBuilder.Append(dttm)
                    strBuilder.Append(",")
                    strBuilder.Append(FormatNumber(lat, 6))
                    strBuilder.Append(",")
                    strBuilder.Append(FormatNumber(lon, 6))
                    strBuilder.Append(",")
                    strBuilder.Append(myStr)
                    strBuilder.Append(",")
                    strBuilder.Append(CDMAPilotInfo.OperatorName + ",ALL," + _FileName + "," + _CollectedDate)
                    strBuilder.Append(",")


                    Dim header As String = ""
                    header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,CDMA,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion()

                    If isCDMAFirstRecord Then

                        _OutputWriter.WriteLine(header)

                    End If

                    'HEADER	8/18/2017 17:33:24	8/18/2017 17:33:24	47.254964	-122.516317	-999	CDMA			HEADER	2017-08-18-17-33-25-3040-0031-0006-3040-S	8/18/2017	Aberdeen WA	17D2	AT&T BM	SCNRdll: 17.1.1 09/29/2017 12:57:52 PM

                    If isCDMAFirstRecord Then
                        CDMAPilotInfo.LogTrace = ScannerCommon.GetDLLVersion()
                    End If

                    If isGPSInterpolated Then
                        If CDMAPilotInfo.LogTrace.Trim = "" Then
                            CDMAPilotInfo.LogTrace = "ModGPS"
                        Else
                            CDMAPilotInfo.LogTrace = CDMAPilotInfo.LogTrace + "|ModGPS"
                        End If
                    End If

                    strBuilder.Append(_MarketName + "," + _Campaign + "," + _ClientName + "," + CDMAPilotInfo.LogTrace)


                    _rowCount = _rowCount + 1

                    myStr = _rowCount.ToString() + "|" + strBuilder.ToString()

                    If isCDMAFirstRecord Then
                        myStr = myStr.Replace(CDMAPilotInfo.LogTrace, "")
                        isCDMAFirstRecord = False
                    End If

                    'NoS File
                    If _LastScanID <> _ScanID Then
                        _FirstScanIDData.gps_time_stamp = dttmDt
                        _FirstScanIDData.time_stamp = dttm
                        _FirstScanIDData.lat = lat
                        _FirstScanIDData.lon = lon

                    End If


                    Dim isWritePARSE As Boolean = False
                    Dim isWriteOperator As Boolean = False

                    Dim blankPolygonCDMA As String = ""
                    Dim blankPolygonCDMAMAX As String = ""

                    If currentCDMAChannelData.OperatorName.Trim = "" Then
                        If _RECaddNoSCondition.lastScanId <> _ScanID Then
                            _RECaddNoSCondition.operatorEmptyCount = _RECaddNoSCondition.operatorEmptyCount + 1

                        End If
                    Else
                        _RECaddNoSCondition.operatorEmptyCount = 0
                    End If

                    Dim operators = _OperatorSet.Keys
                    Dim i As Integer

                    _TempStringList.Add(myStr)
                    _canWrite = True


lineoutput:
                    If _canWrite Or isLast Then

                        Dim count_to_last As Integer = 0

                        For Each myStr In _TempStringList

                            operators = _OperatorSet.Keys

                            Dim strs = myStr.Split(",")
                            _FirstScanIDData.time_stamp = strs(1)
                            _FirstScanIDData.gps_time_stamp = strs(2)
                            _FirstScanIDData.lat = strs(3)
                            _FirstScanIDData.lon = strs(4)


                            For i = 0 To operators.Count - 1


                                Dim tmpFileWriter As New OutputFileWriter()
                                tmpFileWriter = _OperatorSet.Item(operators(i))



                                If tmpFileWriter.isDoNotExist Then
                                    If Not _ScanIDSet.Contains(_ScanID.ToString() + tmpFileWriter.OperatorName) Then
                                        _ScanIDSet.Add(_ScanID.ToString() + tmpFileWriter.OperatorName)

                                        'Dim CDMABandList As String() = {"850", "1900", "2100"}

                                        Dim bandStr = DoNotExistList.Item(tmpFileWriter.OperatorName).ToString()
                                        Dim tmps As String() = bandStr.Split(")")
                                        Dim tmpStr As String
                                        For Each tmpStr In tmps
                                            If tmpStr.IndexOf("CDMA") >= 0 Then
                                                bandStr = tmpStr
                                            End If
                                        Next
                                        bandStr = bandStr.Replace("(", "")
                                        bandStr = bandStr.Replace("CDMA", "")
                                        bandStr = bandStr.Trim

                                        Dim CDMABandList As String() = bandStr.Split(",")

                                        If tmpFileWriter.isFirstRecrod Then
                                            header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,CDMA,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion()
                                            tmpFileWriter.isFirstRecrod = False
                                            tmpFileWriter.writer.WriteLine(header)

                                        End If
                                        'Write PARSE TXT File

                                        blankPolygonCDMAMAX = "10103," & _FirstScanIDData.time_stamp & "," & _FirstScanIDData.gps_time_stamp & "," & _FirstScanIDData.lat & "," & _FirstScanIDData.lon & ",-999,CDMA," & _ScanID & ",-999,-999,-999,-130,-999,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," & tmpFileWriter.OperatorName & ",MAX," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS"


                                        For Each band In CDMABandList

                                            If band.Trim <> "" Then
                                                'All Band CDMA
                                                blankPolygonCDMA = "10103," & _FirstScanIDData.time_stamp & "," & _FirstScanIDData.gps_time_stamp & "," & _FirstScanIDData.lat & "," & _FirstScanIDData.lon & "," & band & ",CDMA," & _ScanID & ",-999,-999,-999,-130,-999,-999,-130,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999," & tmpFileWriter.OperatorName & ",ALL," & _FileName & "," & _CollectedDate & "," & _MarketName & "," & _Campaign & "," & _ClientName & ",ADDrecNoS"
                                                tmpFileWriter.writer.WriteLine(blankPolygonCDMA)

                                                'Per Band TXT Files                                
                                                Dim tmpFileWriter1 As New OutputFileWriter()

                                                Dim keyStr As String = operators(i) + "CDMA" + band
                                                tmpFileWriter1 = _OperatorBandSet.Item(operators(i) + "CDMA" + band)

                                                If tmpFileWriter1.isFirstRecrod Then
                                                    header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,CDMA,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion()
                                                    tmpFileWriter1.isFirstRecrod = False
                                                    tmpFileWriter1.writer.WriteLine(header)
                                                End If

                                                tmpFileWriter1.writer.WriteLine(blankPolygonCDMA)
                                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                                    tmpFileWriter1.writer.WriteLine(blankPolygonCDMA.Replace(",ALL,", ",MAX,"))
                                                End If

                                                _OperatorBandSet.Item(operators(i) + "CDMA" + band) = tmpFileWriter1
                                            End If


                                        Next

                                        'All Band Max Record
                                        tmpFileWriter.writer.WriteLine(blankPolygonCDMAMAX)
                                        _OperatorSet.Item(operators(i)) = tmpFileWriter
                                    End If
                                End If


                            Next

                            Dim count_input_chanel As Integer = 0
                            For Each tmpStr In CDMAChannelList.Keys
                                Dim tmpObj As InputChannelStore = CDMAChannelListWithTime.Item(tmpStr)
                                If Not ((Convert.ToDateTime(dttm) - tmpObj.StartTime).TotalMilliseconds < 0 And tmpObj.isStartLate) Then
                                    count_input_chanel = count_input_chanel + 1
                                End If

                            Next

                            Dim shouldEndRound As Boolean = False

                            If CDMAChannelList.Count = 1 Then
                                shouldEndRound = True
                            End If
                            If CDMAChannelList.Count > 1 And strs(8) <> _lastChannel Then
                                shouldEndRound = True
                            End If



                            If ((_CDMAScannerData.inputChannelList.Count = 0 And shouldEndRound) Or (isLast And count_to_last = _TempStringList.Count) Or _CDMAScannerData.channelRepeatedCount > Int(count_input_chanel * 2.5) - 1) Then

                                'Find Max 

                                Dim keyTmp As String
                                Dim tmpList As List(Of ChannelData)


                                For Each keyTmp In _OperatorSet.Keys

                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorSet.Item(keyTmp)

                                    If Not tmpFileWriter.isDoNotExist Then
                                        Dim result = From v In _CDMAScannerData.channelData Order By v.value Descending
                                        tmpList = result.ToList()
                                        Dim isALl999 = True
                                        For i = 0 To tmpList.Count - 1
                                            If keyTmp = tmpList(i).OperatorName Then
                                                If tmpList(i).value <> -999 Then
                                                    isALl999 = False
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If isALl999 Then
                                            result = From v In _CDMAScannerData.channelData Order By v.value2 Descending
                                            tmpList = result.ToList()
                                        End If

                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName = keyTmp Then

                                                If Not (isLast And count_to_last >= _TempStringList.Count) Then

                                                    If _ScanID = 1 Or _writeFirstMaxRecordList.Contains(keyTmp) Then
                                                        tmpFileWriter.writer.WriteLine((tmpList(i).outputStrData.Replace(",ALL,", ",MAX,")))
                                                    End If
                                                    _writeFirstMaxRecordList.Add(keyTmp)
                                                End If

                                                Exit For
                                            End If
                                        Next
                                    End If

                                Next

                                For Each tmpKey In _OperatorBandSet.Keys
                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorBandSet.Item(tmpKey)

                                    If Not tmpFileWriter.isDoNotExist Then
                                        Dim result = From v In _CDMAScannerData.channelData Order By v.value Descending
                                        tmpList = result.ToList()
                                        Dim isALl999 = True
                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName + "CDMA" + tmpList(i).Band = tmpKey Then
                                                If tmpList(i).value <> -999 Then
                                                    isALl999 = False
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If isALl999 Then

                                            result = From v In _CDMAScannerData.channelData Order By v.value2 Descending
                                            tmpList = result.ToList()
                                        End If

                                        For i = 0 To tmpList.Count - 1
                                            If tmpList(i).OperatorName + "CDMA" + tmpList(i).Band = tmpKey Then
                                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                                    tmpFileWriter.writer.WriteLine((tmpList(i).outputStrData.Replace(",ALL,", ",MAX,")))
                                                End If

                                                Exit For
                                            End If
                                        Next
                                    End If


                                Next

                                _RECaddNoSCondition.lastScanId = _ScanID

                                _LastScanID = _ScanID

                                If Not (isLast And count_to_last = _TempStringList.Count) Then
                                    _ScanID = _ScanID + 1
                                End If

                                _CDMAScannerData = New ScannerData()
                                _CDMAScannerData.channelRepeatedCount = 0
                                _CDMAScannerData.consecutiveChannelRepeatedCount = 0
                                Dim tmpStr As String
                                _CDMAScannerData.inputChannelList = New HashSet(Of String)
                                _CDMAScannerData.channelData = New List(Of ChannelData)
                                For Each tmpStr In CDMAChannelList.Keys
                                    _CDMAScannerData.inputChannelList.Add(tmpStr)
                                Next

                                For Each tmpStr In CDMAChannelList.Keys

                                    Dim tmpObj As InputChannelStore = CDMAChannelListWithTime.Item(tmpStr)
                                    If Not ((Convert.ToDateTime(dttm) - tmpObj.StartTime).TotalMilliseconds < 0 And tmpObj.isStartLate) Then
                                        _CDMAScannerData.inputChannelList.Add(tmpStr.Split("|")(0))

                                    End If

                                Next

                            End If

                            strs(7) = _ScanID.ToString

                            Dim tmpChannelInt As Integer
                            If Integer.TryParse(strs(8).Trim, tmpChannelInt) Then

                                If (ChannelWithAllRSSINev999.Item(tmpChannelInt.ToString()) = True) Then

                                    If strs(14) = "-999" Or strs(14) = "-130" Then
                                        strs(14) = "-130"
                                        If strs(61).IndexOf("ADDrecNoS") < 0 Then
                                            strs(61) = IIf(strs(61).Trim <> "", strs(61) + "|ADDrecNoS", "ADDrecNoS")
                                        End If
                                    End If


                                End If
                            End If






                            myStr = Join(strs, ",")
                            _OutputWriter.WriteLine(myStr)
                            Dim currentOperatorName As String = strs(54)
                            Dim currentBand As String = strs(5)


                            If (currentOperatorName.Trim <> "") Then
                                If Not IsNothing(_OperatorSet.Item(currentOperatorName)) Then
                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorSet.Item(currentOperatorName)

                                    If tmpFileWriter.isFirstRecrod Then

                                        header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,CDMA,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion()
                                        tmpFileWriter.writer.WriteLine(header)

                                        Dim strs1 As String() = myStr.Split(",")
                                        strs1(61) = ScannerCommon.GetDLLVersion()
                                        myStr = String.Join(",", strs1)

                                        tmpFileWriter.isFirstRecrod = False
                                        _OperatorSet.Item(currentOperatorName) = tmpFileWriter

                                    End If

                                    tmpFileWriter.writer.WriteLine(myStr)
                                    _OperatorSet.Item(currentOperatorName) = tmpFileWriter
                                End If

                            End If

                            Dim key As String = currentOperatorName + "CDMA" + currentBand

                            If Not IsNothing(_OperatorBandSet.Item(key)) Then
                                If (_OperatorBandSet.ContainsKey(key)) Then
                                    Dim tmpFileWriter As New OutputFileWriter()
                                    tmpFileWriter = _OperatorBandSet.Item(key)

                                    If tmpFileWriter.isFirstRecrod Then

                                        header = "HEADER," & strMSp12am & "," & dttm & "," & FormatNumber(lat, 6) & "," & FormatNumber(lon, 6) & ",-999,CDMA,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,-999,,HEADER," & _FileName & "," & _CollectedDate & "," & _MarketName & "," + _Campaign & "," + _ClientName & "," + ScannerCommon.GetDLLVersion()
                                        tmpFileWriter.writer.WriteLine(header)


                                        Dim strs1 As String() = myStr.Split(",")
                                        strs1(61) = ScannerCommon.GetDLLVersion()
                                        myStr = String.Join(",", strs1)

                                        tmpFileWriter.isFirstRecrod = False
                                        _OperatorBandSet.Item(key) = tmpFileWriter

                                    End If
                                    tmpFileWriter.writer.WriteLine(myStr)
                                    _OperatorBandSet.Item(key) = tmpFileWriter
                                End If
                            End If


                            currentCDMAChannelData.outputStrData = myStr
                            currentCDMAChannelData.Band = strs(5)
                            currentCDMAChannelData.OperatorName = strs(54)
                            currentCDMAChannelData.value = Double.Parse(strs(14))
                            currentCDMAChannelData.value2 = Double.Parse(strs(11))


                            If _CDMAScannerData.inputChannelList.Contains(strs(5) + strs(8)) Then
                                _CDMAScannerData.inputChannelList.Remove(strs(5) + strs(8))

                            End If

                            If _lastChannel <> strs(8) Then
                                _CDMAScannerData.channelRepeatedCount = _CDMAScannerData.channelRepeatedCount + 1
                            End If
                            _CDMAScannerData.channelData.Add(currentCDMAChannelData)
                            _lastChannel = strs(8)


                        Next

                        _TempStringList = New List(Of String)
                    End If


                    _isOutputCDMA = False
                    CDMAPilotInfo.SID = ""
                    CDMAPilotInfo.OperatorName = ""
                    CDMAPilotInfo.LogTrace = ""
                    CDMAPilotInfo.NID = ""
                    currentCDMAChannelData = New ChannelData()


                End If

            End If
        End If

        Return True
    End Function



    Private Function GetOperatorName(ByVal SID As String) As String
        Dim tmpInt As Integer
        Dim OperatorName As String = ""
        If Integer.TryParse(SID, tmpInt) Then
            OperatorName = _SIDLookupList(tmpInt)
            If IsNothing(OperatorName) Or OperatorName = "" Then
                If SID <> "" Then
                    OperatorName = "SID" + SID
                End If

            End If
        End If

        Return OperatorName

    End Function

    Private Function GetCarrierFrequency(ByVal channel As String) As String
        Dim CarrierFrequency As String = "-999"
        Try
            Dim i, j As Integer
            Dim len As Integer = TechChOpArray.GetLength(0)
            For i = 0 To len - 1
                Dim tmpArray As Integer() = TechChOpArray(i, 3)
                If Not IsNothing(tmpArray) Then
                    For j = 0 To tmpArray.Length - 1
                        If tmpArray(j) = channel Then
                            Return TechChOpArray(i, 2)
                        End If
                    Next
                End If
            Next
        Catch

        End Try

        Return CarrierFrequency
    End Function


    Function GetHex(ByVal myDec As Integer) As String
        GetHex = IIf(Len(Hex(myDec)) = 2, Hex(myDec), "0" & Hex(myDec))
    End Function
    Function GetInt(ByVal myByte As Byte) As Integer
        If myByte >= 128 Then
            GetInt = Convert.ToInt16(-((255 - Convert.ToInt16(myByte)) + 1))
        Else
            GetInt = Convert.ToInt16(myByte)
        End If
    End Function

    Private Function GetBandFromChannelList(ByVal channel As String) As String

        Dim tmp As String
        For Each tmp In LTEChannelList1.Keys
            If tmp.StartsWith(channel + "|") Then
                Return tmp.Split("|")(1)
            End If
        Next
        Return ""
    End Function



    Private Sub GetChannelInfo(ByVal xmlStr As String, ByRef FileInfoLog As System.IO.TextWriter, ByVal FileName As String)

        Try
            Dim i As Integer
            Dim xmlDoc As New XmlDocument
            xmlDoc.LoadXml(xmlStr)

            Dim nodeList As XmlNodeList

            nodeList = xmlDoc.SelectNodes("ScanConfiguration/Scans/GenericScan")
            Dim CDMAChannelList As New List(Of Channel)
            Dim WCDMAChannelList As New List(Of Channel)
            Dim LTEChannelList As New List(Of Channel)
            Dim GSMChannelListBSIC As String = "GSM Input channel (BSIC): "
            Dim GSMChannelListRSSI As String = "GSM Input channel (RSSI): "
            Dim CDMAChannelStr As String = "CDMA Input Channel: "
            Dim WCDMAChannelStr As String = "WCDMA Input Channel: "
            Dim LTEChannelStr As String = "LTE Input Channel: "

            Dim tmpInt As Integer


            Dim xmlNode As XmlNode
            For Each xmlNode In nodeList
                Dim tmpStr As String = xmlNode.Value
                Dim childNode As XmlNode = xmlNode.FirstChild
                Dim childNodeList As XmlNodeList = childNode.ChildNodes

                Dim nodeChild As XmlNode
                Dim band As String = ""
                Dim protocol As String = ""
                Dim caption As String = ""
                Dim startChannel As String = ""
                Dim stopChannel As String = ""

                For Each nodeChild In childNodeList
                    If nodeChild.Name = "Band" Then
                        band = nodeChild.InnerText
                    End If
                    If nodeChild.Name = "Protocol" Then
                        protocol = nodeChild.InnerText
                    End If
                    If nodeChild.Name = "StartChannel" Then
                        startChannel = nodeChild.InnerText
                    End If
                    If nodeChild.Name = "StopChannel" Then
                        stopChannel = nodeChild.InnerText
                    End If
                    If nodeChild.Name = "Scantype" Then
                        caption = nodeChild.Attributes("Caption").Value
                    End If
                Next

                If startChannel = "66661" Then
                    Dim tt As String = ""
                End If

                If protocol = "GSM" And caption.ToUpper.Contains("BSIC") Then
                    GSMChannelListBSIC = GSMChannelListBSIC + "(" + band + ":" + startChannel + "-" + stopChannel + ");"
                End If
                If protocol = "GSM" And caption.ToUpper.Contains("RSSI") Then
                    GSMChannelListRSSI = GSMChannelListRSSI + "(" + band + ":" + startChannel + "-" + stopChannel + ");"
                End If

                If protocol.ToUpper.Contains("IS95_") Then
                    Dim channel As New Channel()
                    If band.Contains("800_") Then
                        band = "850"
                    End If
                    If band.Contains("600_") Then
                        band = "600"
                    End If
                    channel = New Channel()
                    channel.band = band
                    channel.startChanel = startChannel
                    If Int32.TryParse(band, tmpInt) Then
                        channel.bandInt = tmpInt
                    Else
                        channel.bandInt = 100000000
                    End If

                    CDMAChannelList.Add(channel)
                End If


                If protocol.ToUpper.Contains("UMTS") Or protocol.ToUpper.Contains("WCDMA") Then
                    Dim channel As New Channel()
                    If band = "1700" Then
                        band = "2100"
                    End If
                    channel = New Channel()
                    channel.band = band
                    channel.startChanel = startChannel
                    If Int32.TryParse(band, tmpInt) Then
                        channel.bandInt = tmpInt
                    Else
                        channel.bandInt = 100000000
                    End If
                    WCDMAChannelList.Add(channel)
                End If

                If protocol.ToUpper.Contains("LTE") Then
                    Dim channel As New Channel()
                    'Dim tmpBand As String
                    'For i = 14 To 200
                    '    If band.Contains((i * 50).ToString) Then
                    '        tmpBand = (i * 50).ToString()
                    '    End If
                    'Next
                    'band = tmpBand
                    'If band = "1700" Then
                    '    band = "2100"
                    'End If
                    'If band = "900" Then
                    '    band = "1900"
                    'End If
                    'If band.Contains("600_") Then
                    '    band = "600"
                    'End If

                    band = GetBandFromChannelList(startChannel)
                    If band = "" Then
                        If Integer.TryParse(startChannel, tmpInt) Then
                            band = ScannerCommon.GetLTEBandByChannel(tmpInt)
                        End If

                    End If

                    If band = "" Then
                        band = "0"
                    End If

                    channel = New Channel()
                    channel.band = band
                    channel.startChanel = startChannel
                    If Int32.TryParse(band, tmpInt) Then
                        channel.bandInt = tmpInt
                    Else
                        channel.bandInt = 100000000
                    End If
                    LTEChannelList.Add(channel)
                End If
            Next


            'Sort value

            Dim query As IEnumerable(Of Channel) = CDMAChannelList.OrderBy(Function(pet) pet.bandInt).ThenBy(Function(pet) pet.startChanel)
            CDMAChannelList = New List(Of Channel)()
            For i = 0 To query.Count - 1
                CDMAChannelList.Add(query(i))
            Next


            query = WCDMAChannelList.OrderBy(Function(pet) pet.bandInt).ThenBy(Function(pet) pet.startChanel)
            WCDMAChannelList = New List(Of Channel)()
            For i = 0 To query.Count - 1
                WCDMAChannelList.Add(query(i))
            Next

            query = LTEChannelList.OrderBy(Function(pet) pet.bandInt).ThenBy(Function(pet) pet.startChanel)

            LTEChannelList = New List(Of Channel)()
            For i = 0 To query.Count - 1
                LTEChannelList.Add(query(i))
            Next


            For i = 0 To CDMAChannelList.Count - 1
                CDMAChannelStr = CDMAChannelStr + "(" + CDMAChannelList(i).band + ":" + CDMAChannelList(i).startChanel + ");"
            Next
            CDMAChannelStr = "<" + CDMAChannelStr + ">"

            For i = 0 To WCDMAChannelList.Count - 1
                WCDMAChannelStr = WCDMAChannelStr + "(" + WCDMAChannelList(i).band + ":" + WCDMAChannelList(i).startChanel + ");"
            Next
            WCDMAChannelStr = "<" + WCDMAChannelStr + ">"
            Dim LTEBand As String = ""

            Dim FileNameOnly As String = FileName.Substring(FileName.LastIndexOf("\") + 1, FileName.Length - FileName.LastIndexOf("\") - 1)
            FileNameOnly = FileNameOnly.Substring(0, FileNameOnly.IndexOf("."))
            Dim keyStr As String

            For i = 0 To LTEChannelList.Count - 1
                Dim LTEBandList As New List(Of String)
                For Each keyStr In _BandWidthList.Keys
                    If keyStr.StartsWith(LTEChannelList(i).band + "|" + LTEChannelList(i).startChanel + "|" + FileNameOnly) Then
                        LTEBandList.Add(_BandWidthList.Item(keyStr))
                    ElseIf keyStr.StartsWith(LTEChannelList(i).band + "|" + LTEChannelList(i).startChanel + "|") Then
                        LTEBandList.Add(_BandWidthList.Item(keyStr))
                    End If
                Next

                'If _BandWidthList.ContainsKey(LTEChannelList(i).band + "|" + FileNameOnly) Then
                '    LTEBand = _BandWidthList.Item(LTEChannelList(i).band + "|" + FileNameOnly)
                'ElseIf _BandWidthList.ContainsKey(LTEChannelList(i).band + "|") Then
                '    LTEBand = _BandWidthList.Item(LTEChannelList(i).band + "|")
                'End If
                If LTEBandList.Count > 0 Then
                    For Each band In LTEBandList
                        If LTEChannelStr.IndexOf("(" + LTEChannelList(i).band + ":" + LTEChannelList(i).startChanel + ":{" + band + "});") < 0 Then
                            LTEChannelStr = LTEChannelStr + "(" + LTEChannelList(i).band + ":" + LTEChannelList(i).startChanel + ":{" + band + "});"
                        End If

                    Next
                Else
                    If LTEChannelStr.IndexOf("(" + LTEChannelList(i).band + ":" + LTEChannelList(i).startChanel + ":{});") < 0 Then
                        LTEChannelStr = LTEChannelStr + "(" + LTEChannelList(i).band + ":" + LTEChannelList(i).startChanel + ":{});"
                    End If

                End If

            Next

            LTEChannelStr = "<" + LTEChannelStr + ">"

            GSMChannelListBSIC = "<" + GSMChannelListBSIC + ">"
            GSMChannelListRSSI = "<" + GSMChannelListRSSI + ">"

            For i = 0 To ScannerCommon.FileLogInfoList.Count - 1
                If ScannerCommon.FileLogInfoList(i).FileName = FileName.Replace("\\", "\") Then
                    If Not ScannerCommon.FileLogInfoList(i).isGetInputChannel Then
                        ScannerCommon.FileLogInfoList(i).isGetInputChannel = True
                        ScannerCommon.FileLogInfoList(i).GSM_RSSI_InputChannel = GSMChannelListRSSI
                        ScannerCommon.FileLogInfoList(i).GSM_BSIC_InputChannel = GSMChannelListBSIC
                        ScannerCommon.FileLogInfoList(i).CDMAInputChannel = CDMAChannelStr
                        ScannerCommon.FileLogInfoList(i).WCDMAInputChannel = WCDMAChannelStr
                        ScannerCommon.FileLogInfoList(i).LTEInputChannel = LTEChannelStr
                    End If
                End If
            Next

        Catch ex As Exception
            Dim tt As String = ""
        End Try
    End Sub


    Private Sub GetSID(ByVal date_time As DateTime, ByVal TechType As String, ByVal msgData As Byte(), ByVal MessageType As String, ByVal OriginalFileNameStr As String, ByVal fileInputObj As ScannerFileInputObject)
        Dim MCC As String = ""
        Dim MNC As String = ""
        Dim BW As String = ""
        Dim strMsgData() As String
        Dim enc As New System.Text.ASCIIEncoding
        Dim myMsg() As Byte

        Dim tmpInt As Integer
        Dim channel As String

        Dim currentCDMANo As Double

        Dim isMessage10001Exist As Boolean = False
        Dim FileNameStr As String = ""
        'Get the list of LTE Channel
        FileNameStr = fileInputObj.FileName.Replace("\", "\\")

        If MessageType = "10522" Then


            strMsgData = enc.GetString(msgData).Split(",")

            strMsgData(1) = strMsgData(1).Substring(2, strMsgData(1).Length - 2)
            _myBand = ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(0))).ToString().Split("|")(1)


            myMsg = ToByteArray(strMsgData(1))

            If Convert.ToInt32(strMsgData(0)) = 80 Then
                channel = CLng(256 * myMsg(1) + myMsg(0))
            Else
                If ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(0))).ToString().Split("|")(3) = "1" Then
                    channel = CLng(65536 * 1 + 256 * myMsg(1) + myMsg(0)) 'channel =5815, 5230 -6E14   
                Else
                    channel = CLng(256 * myMsg(1) + myMsg(0)) 'channel =5815, 5230 -6E14   

                End If
            End If
            If Not ChannelWithAllRSSINev999.Contains(channel) Then
                ChannelWithAllRSSINev999.Add(channel, True)
            End If

            If Not _AddNoRecChannelList.Contains(channel) Then
                Dim recChannel As New AddNoRecChannel()
                recChannel.lastRSSI = -998


                _AddNoRecChannelList.Add(channel, recChannel)
            End If


            BW = CLng(256 * myMsg(3) + myMsg(2)) '20000 

            If Integer.TryParse(channel.Trim, tmpInt) Then



                If TechType = "LTE" And Not LTEChannelList.ContainsKey(channel + "|" + _myBand) Then
                    Dim SIDNID As New SIDNIDStore()
                    Dim SIDNIDList = New List(Of SIDNIDStore)
                    Dim ChannelStore As New InputChannelStore()
                    ChannelStore.isSetStartTime = False
                    ChannelStore.isFound = False
                    ChannelStore.channel = channel
                    ChannelStore.isStartLate = False
                    ChannelStore.isEndEarly = False
                    ChannelStore.band = _myBand
                    ChannelStore.inputFileName = OriginalFileNameStr
                    LTEChannelList.Add(channel + "|" + _myBand, SIDNIDList)
                    LTEChannelListWithTime.Add(channel + "|" + _myBand, ChannelStore)

                    _LTEScannerData.inputChannelList.Add(channel)
                    If fileInputObj.FileMergeName.Trim <> "" And OriginalFileNameStr.IndexOf(fileInputObj.MergePattern2) > 0 Then
                        _LTEScannerData.inputChannelList3030.Add(channel)
                    End If
                    If fileInputObj.FileMergeName.Trim <> "" And OriginalFileNameStr.IndexOf(fileInputObj.MergePattern1) > 0 Then
                        _LTEScannerData.inputChannelList3040.Add(channel)
                    End If
                End If
            End If

            If LTEChannelListWithTime.ContainsKey(channel + "|" + _myBand) Then
                Dim ChannelStore As InputChannelStore = LTEChannelListWithTime.Item(channel + "|" + _myBand)
                If Not ChannelStore.isSetStartTime Then
                    ChannelStore.StartTime = date_time
                    ChannelStore.StartTimeTick = date_time.Ticks
                    ChannelStore.isSetStartTime = True
                End If
                ChannelStore.EndTime = date_time
                ChannelStore.EndTimeTick = date_time.Ticks

                LTEChannelListWithTime.Item(channel + "|" + _myBand) = ChannelStore
            End If

            _LTESIBInfo = New LTESIBInfo()
            LTEMsgCode = MessageType

            Dim x As Integer = 0

            Dim RefN, SynN, CarrierRssiN As Integer



            CarrierRssiN = CInt(myMsg(9)) '1
            RefN = CInt(myMsg(14)) '13
            SynN = CInt(myMsg(19)) '20                

            x = 20 + 2 * CarrierRssiN 'The position of First Data Block Ref record - 2 is the length of one Carrier RSSI Record
            Dim j1 As Integer

            For j = 0 To RefN - 1

                Try
                    Dim NumRfDataBlock As Integer = CInt(myMsg(x + 20))

                    x = x + 2 'LTEMessageVersion 2

                    Dim LTERSSI As Double = CInt(256 * myMsg(x + 24) + myMsg(x + 23)) - 65536
                    If Math.Abs(LTERSSI) > 999 Then
                        LTERSSI = -999
                    End If
                    If LTERSSI > -999 And ChannelWithAllRSSINev999.Item(channel) = True Then
                        ChannelWithAllRSSINev999.Item(channel) = False
                    End If
                    Dim RfPathDataBlock_Length As Integer = 6
                    Dim NumSubbands = CInt(256 * myMsg(x + 22) + myMsg(x + 21))
                    Dim NumChannelCondition As Integer = CInt(myMsg(x + 22))
                    Dim ChannelConditionRecordLength As Integer = 0

                    'Calculate Channel Condition Record Length
                    Dim StartChannelConditionRecord As Integer = x + 23 + NumRfDataBlock * 6
                    For j1 = 0 To NumChannelCondition - 1
                        Try
                            Dim NumTransDivBlks As Integer = CInt(myMsg(StartChannelConditionRecord + 33))
                            ChannelConditionRecordLength = ChannelConditionRecordLength + 32 + NumTransDivBlks * 5
                            StartChannelConditionRecord = ChannelConditionRecordLength
                        Catch

                        End Try

                    Next
                    x = x + 23 + NumRfDataBlock * 6 + ChannelConditionRecordLength
                    x = x + NumSubbands * 6

                Catch

                End Try

            Next j

            Dim LoopSyN As Integer
            If SynN > 6 Then
                LoopSyN = 6
            Else
                LoopSyN = SynN
            End If

            Dim foundSIB As Boolean = False

            For j = 0 To LoopSyN - 1
                Try
                    If foundSIB Then
                        Exit For
                    End If

                    If Not isSkipLTESynBlock Then

                        'Try
                        Dim NumofInformationBlock = CInt(myMsg(x + 36))
                        x = x + 37
                        For j1 = 0 To NumofInformationBlock - 1
                            x = x + 1
                            Dim SIBString = ""
                            Dim BLockType = CInt(myMsg(x))

                            Dim DataLength As Integer = CInt(256 * myMsg(x + 2) + myMsg(x + 1))
                            If BLockType = 0 Then
                                x = x + DataLength + 3
                            Else

                                For i = 1 To DataLength
                                    SIBString = SIBString + myMsg(x + i + 2).ToString("X2")
                                Next

                                If BLockType = 2 Then
                                    _LTESIBInfo.SIB1 = SIBString
                                    Dim SIB1 As LTESIB1 = ScannerCommon.GetSIB1(SIBString)
                                    'Dim timestamp As String = strMsgData(1)

                                    Dim SIDNID As New SIDNIDStore()
                                    SIDNID.SID = SIB1.MCC0 + SIB1.MCC1 + SIB1.MCC2
                                    MCC = SIDNID.SID
                                    SIDNID.NID = SIB1.MNC0 + SIB1.MNC1 + SIB1.MNC2

                                    MNC = SIDNID.NID
                                    SIDNID.band = _myBand
                                    SIDNID.time = Convert.ToDateTime(dttm)



                                    If LTEChannelList.ContainsKey(channel + "|" + _myBand) Then
                                        If Not LTESIDHashSet.Contains(SIDNID.SID.Trim + SIDNID.NID.Trim + channel.Trim) Then
                                            If channel = "5035" Then
                                                Dim tt As String = ""
                                            End If
                                            Dim OpName As String = GetOperatorName(Convert.ToInt32(SIDNID.SID.Trim + SIDNID.NID.Trim))
                                            If OpName = "T-Mobile" Then
                                                Dim tt As String = ""
                                            End If
                                            Dim key As String = OpName + TechType.Trim + _myBand.Trim

                                            If Not _OperatorBandSet.ContainsKey(key) Then
                                                Dim filePath As String = FileNameStr.Substring(0, FileNameStr.LastIndexOf("\"))
                                                Dim FileNameOnly As String = FileNameStr.Substring(FileNameStr.LastIndexOf("\") + 1)

                                                Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_" + _myBand + "_HOME.txt"
                                                Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                                                'modified by My 05/31/2018 #1278
                                                'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,DelaySpread_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,DelaySpread_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,DelaySpread_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,DelaySpread_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,DelaySpread_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,DelaySpread_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                                                tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,NumTxAntennaPortsWideBand_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,NumTxAntennaPortsWideBand_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,NumTxAntennaPortsWideBand_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,NumTxAntennaPortsWideBand_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,NumTxAntennaPortsWideBand_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,NumTxAntennaPortsWideBand_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")

                                                Dim tmpFileWriter As New OutputFileWriter()
                                                tmpFileWriter.isFirstRecrod = True
                                                tmpFileWriter.countNoOperator = 0
                                                tmpFileWriter.writer = tmpStreamWriter
                                                tmpFileWriter.OperatorName = key
                                                tmpFileWriter.countRSSINev999 = 0
                                                tmpFileWriter.FileFullName = tmpOutFile
                                                _OperatorBandSet.Add(key, tmpFileWriter)

                                            End If


                                            If (Not _OperatorSet.ContainsKey(OpName)) Then
                                                Dim filePath As String = FileNameStr.Substring(0, FileNameStr.LastIndexOf("\"))
                                                Dim FileNameOnly As String = FileNameStr.Substring(FileNameStr.LastIndexOf("\") + 1)

                                                Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_AllBands_HOME.txt"
                                                Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)

                                                If TechType = "LTE" Then
                                                    'modified by My 05/31/2018 #1278
                                                    tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,NumTxAntennaPortsWideBand_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,NumTxAntennaPortsWideBand_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,NumTxAntennaPortsWideBand_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,NumTxAntennaPortsWideBand_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,NumTxAntennaPortsWideBand_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,NumTxAntennaPortsWideBand_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                                                    'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,DelaySpread_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,DelaySpread_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,DelaySpread_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,DelaySpread_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,DelaySpread_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,DelaySpread_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                                                End If

                                                Dim tmpFileWriter As New OutputFileWriter()
                                                tmpFileWriter.isFirstRecrod = True
                                                tmpFileWriter.countNoOperator = 0
                                                tmpFileWriter.writer = tmpStreamWriter
                                                tmpFileWriter.OperatorName = OpName
                                                tmpFileWriter.countRSSINev999 = 0
                                                tmpFileWriter.isDoNotExist = False
                                                tmpFileWriter.FileFullName = tmpOutFile
                                                _OperatorSet.Add(OpName, tmpFileWriter)

                                            End If

                                            LTESIDHashSet.Add(SIDNID.SID.Trim + SIDNID.NID.Trim + channel.Trim)
                                            LTEChannelList.Item(channel + "|" + _myBand).Add(SIDNID)
                                        End If

                                    End If



                                End If
                                x = x + DataLength + 3
                                foundSIB = True

                            End If

                        Next
                        'Catch

                        'End Try

                    End If
                Catch ex As Exception
                    Dim tt As String = ""

                End Try
            Next j

            If isLTEFirstRecord Then
                _currentNosData.Band = _myBand
                _currentNosData.Channel = channel
                _currentNosData.EARFCN = channel
                _currentNosData.MCC = MCC
                _currentNosData.MNC = MNC
                _currentNosData.CarrierBW = BW

                isLTEFirstRecord = False


            End If

        End If


        'Get the list of WCDMA  and CDMA Channel - TSMW scanner
        If MessageType = "10103" Then
       
            If Not _MessageObject.ContainsKey(ID) Then
                _MessageObject.Add(ID, 0)
            End If

            strMsgData = enc.GetString(msgData).Split(",")
            If (Not IsNothing(strMsgData)) Then
                If (Not IsNothing(strMsgData(2))) Then
                    channel = strMsgData(2)
                    _channel = channel

                    If Not ChannelWithAllRSSINev999.Contains(channel) Then
                        ChannelWithAllRSSINev999.Add(channel, True)
                    End If
                    If Not _AddNoRecChannelList.Contains(channel) Then
                        _AddNoRecChannelList.Add(channel, New AddNoRecChannel())
                    End If

                    Dim tmpBandInfo As String() = ScannerCommon._BandLookUp.Item(Convert.ToInt32(strMsgData(1))).Split("|")


                    _myBand = tmpBandInfo(1)
                    If _myBand = 800 Then
                        _myBand = 850
                    End If

                    If strMsgData(8).Trim <> "100" Then

                        _isSkippedCDMA = True
                        _isSkippedWCDMA = False
                    Else

                        _isSkippedCDMA = False
                        _isSkippedWCDMA = True

                    End If




                    If (TechType = "CDMA" And tmpBandInfo(0).IndexOf("CDMA") < 0) Then
                        _isSkippedCDMA = True
                    End If

                    If (TechType = "UMTS" And tmpBandInfo(0).IndexOf("UMTS") < 0) Then
                        _isSkippedWCDMA = True
                    End If


                    If (_isSkippedWCDMA And TechType = "UMTS") Then
                        Return
                    End If

                    If (_isSkippedCDMA And TechType = "CDMA") Then
                        Return
                    End If


                    If Double.TryParse(strMsgData(5), tmpInt) Then
                        currentCDMANo = tmpInt
                    End If

                    currentCDMAChannel = channel
                    currentWCDMAChannel = channel

                    Dim ChannelStore As New InputChannelStore()
                    ChannelStore.isSetStartTime = False
                    ChannelStore.isFound = False
                    ChannelStore.channel = channel
                    ChannelStore.isStartLate = False
                    ChannelStore.isEndEarly = False
                    ChannelStore.band = _myBand

                    If Integer.TryParse(channel.Trim, tmpInt) And TechType = "UMTS" Then
                        If tmpInt > 0 And Not WCDMAChannelList.ContainsKey(channel) Then
                            Dim SIDNID As New SIDNIDStore()
                            Dim SIDNIDList = New List(Of SIDNIDStore)
                            WCDMAChannelList.Add(channel, SIDNIDList)
                            _WCDMAScannerData.inputChannelList.Add(channel)

                            WCDMAChannelListWithTime.Add(channel, ChannelStore)

                        End If

                        If WCDMAChannelListWithTime.ContainsKey(channel) Then
                            ChannelStore = WCDMAChannelListWithTime.Item(channel)

                            If Not ChannelStore.isSetStartTime Then
                                ChannelStore.StartTime = date_time
                                ChannelStore.StartTimeTick = date_time.Ticks
                                ChannelStore.isSetStartTime = True
                            End If
                            ChannelStore.EndTime = date_time
                            ChannelStore.EndTimeTick = date_time.Ticks

                            WCDMAChannelListWithTime.Item(channel) = ChannelStore
                        End If

                    End If

                    If Integer.TryParse(channel.Trim, tmpInt) And TechType = "CDMA" Then
                        If tmpInt > 0 And Not CDMAChannelList.ContainsKey(_myBand + channel) Then
                            Dim SIDNID As New SIDNIDStore()
                            Dim SIDNIDList = New List(Of SIDNIDStore)

                            CDMAChannelList.Add(_myBand + channel, SIDNIDList)
                            _CDMAScannerData.inputChannelList.Add(_myBand + channel)
                            CDMAChannelListWithTime.Add(_myBand + channel, ChannelStore)
                        End If

                        If CDMAChannelListWithTime.ContainsKey(_myBand + channel) Then
                            ChannelStore = CDMAChannelListWithTime.Item(_myBand + channel)

                            If Not ChannelStore.isSetStartTime Then
                                ChannelStore.StartTime = date_time
                                ChannelStore.StartTimeTick = date_time.Ticks
                                ChannelStore.isSetStartTime = True
                            End If
                            ChannelStore.EndTime = date_time
                            ChannelStore.EndTimeTick = date_time.Ticks

                            CDMAChannelListWithTime.Item(_myBand + channel) = ChannelStore
                        End If

                    End If

                End If

            End If



        End If



        If MessageType = "10104" Then
            _MessageObject.Item(ID) = _MessageObject.Item(ID) + 1
            If _isSkippedCDMA And TechType = "CDMA" Then

                Return
            End If

            If _isSkippedWCDMA And TechType = "UMTS" Then

                Return
            End If
            strMsgData = enc.GetString(msgData).Split(",")
            Try
                If (Not IsNothing(strMsgData)) Then
                    Dim timestamp As String = strMsgData(1)
                    Dim tmpDouble1 As Double

                    If TechType = "UMTS" Then
                        If Double.TryParse(strMsgData(2), tmpDouble1) Then
                            If tmpDouble1 <> 0 And tmpDouble1 > -999 And ChannelWithAllRSSINev999.Item(currentCDMAChannel) = True Then
                                ChannelWithAllRSSINev999.Item(currentCDMAChannel) = False
                            End If
                        End If
                    Else
                        'currentCDMANo
                        If Double.TryParse(strMsgData(2), tmpDouble1) Then
                            Dim tt As Double = 0
                            tt = tt + currentCDMANo + tmpDouble1
                            If tt = 0 Then
                                tt = -999
                            End If
                            If Math.Abs(tt) > 999 Then
                                tt = -999

                            End If
                            If tt <> 0 And tt > -999 And ChannelWithAllRSSINev999.Item(currentCDMAChannel) = True Then
                                ChannelWithAllRSSINev999.Item(currentCDMAChannel) = False
                            End If
                        End If


                    End If


                    Dim SIDNID As New SIDNIDStore()
                    SIDNID.SID = strMsgData(16).Trim
                    SIDNID.NID = strMsgData(17).Trim
                    SIDNID.band = _myBand
                    SIDNID.time = Convert.ToDateTime(dttm)

                    If SIDNID.SID.Trim <> "" And SIDNID.NID.Trim <> "" And SIDNID.SID.Trim.Length + SIDNID.SID.Trim.Length > 1 Then
                        If TechType = "CDMA" Then
                            If CDMAChannelList.ContainsKey(_myBand + currentCDMAChannel) Then
                                If Not CDMASIDHashSet.Contains(SIDNID.SID.Trim + currentCDMAChannel.Trim) Then
                                    Dim OpName As String = GetOperatorName(Convert.ToInt32(SIDNID.SID.Trim))
                                    Dim key As String = OpName + TechType.Trim + _myBand.Trim

                                    If Not _OperatorBandSet.ContainsKey(key) Then
                                        Dim filePath As String = FileNameStr.Substring(0, FileNameStr.LastIndexOf("\"))
                                        Dim FileNameOnly As String = FileNameStr.Substring(FileNameStr.LastIndexOf("\") + 1)

                                        Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_" + _myBand + "_HOME.txt"
                                        Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                                        tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,Spread_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,Spread_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,Spread_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,Spread_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,Spread_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,Spread_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                                        'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,ULInterference_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,ULInterference_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,ULInterference_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,ULInterference_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,ULInterference_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,ULInterference_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")

                                        Dim tmpFileWriter As New OutputFileWriter()
                                        tmpFileWriter.isFirstRecrod = True
                                        tmpFileWriter.countNoOperator = 0
                                        tmpFileWriter.writer = tmpStreamWriter
                                        tmpFileWriter.OperatorName = key
                                        tmpFileWriter.countRSSINev999 = 0
                                        tmpFileWriter.FileFullName = tmpOutFile
                                        _OperatorBandSet.Add(key, tmpFileWriter)

                                    End If


                                    If (Not _OperatorSet.ContainsKey(OpName)) Then
                                        Dim filePath As String = FileNameStr.Substring(0, FileNameStr.LastIndexOf("\"))
                                        Dim FileNameOnly As String = FileNameStr.Substring(FileNameStr.LastIndexOf("\") + 1)

                                        Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_AllBands_HOME.txt"
                                        Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)

                                        tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,Spread_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,Spread_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,Spread_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,Spread_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,Spread_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,Spread_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                                        'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,ULInterference_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,ULInterference_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,ULInterference_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,ULInterference_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,ULInterference_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,ULInterference_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                                        Dim tmpFileWriter As New OutputFileWriter()
                                        tmpFileWriter.isFirstRecrod = True
                                        tmpFileWriter.countNoOperator = 0
                                        tmpFileWriter.writer = tmpStreamWriter
                                        tmpFileWriter.OperatorName = OpName
                                        tmpFileWriter.countRSSINev999 = 0
                                        tmpFileWriter.FileFullName = tmpOutFile
                                        _OperatorSet.Add(OpName, tmpFileWriter)

                                    End If

                                    CDMASIDHashSet.Add(SIDNID.SID.Trim + currentCDMAChannel.Trim)
                                    CDMAChannelList.Item(_myBand + currentCDMAChannel).Add(SIDNID)
                                End If

                                If isCDMAFirstRecord Then
                                    _currentNosData.Band = _myBand
                                    _currentNosData.Channel = _channel

                                    _currentNosData.MCC = SIDNID.SID
                                    _currentNosData.MNC = SIDNID.NID
                                    isCDMAFirstRecord = False
                                End If

                            End If

                        ElseIf TechType = "UMTS" Then

                            If WCDMAChannelList.ContainsKey(currentWCDMAChannel) Then
                                If Not WCDMASIDHashSet.Contains(SIDNID.SID.Trim + SIDNID.NID.Trim + currentWCDMAChannel.Trim) Then

                                    Dim OpName As String = GetOperatorName(Convert.ToInt32(SIDNID.SID.Trim + SIDNID.NID.Trim))

                                    If Not IsNothing(OpName) Then
                                        Dim key As String = OpName + TechType.Trim + _myBand.Trim

                                        If Not _OperatorBandSet.ContainsKey(key) Then
                                            Dim filePath As String = FileNameStr.Substring(0, FileNameStr.LastIndexOf("\"))
                                            Dim FileNameOnly As String = FileNameStr.Substring(FileNameStr.LastIndexOf("\") + 1)

                                            Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_" + _myBand + "_HOME.txt"
                                            Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                                            tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                                            'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,ULInterference_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,ULInterference_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,ULInterference_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,ULInterference_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,ULInterference_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,ULInterference_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")

                                            Dim tmpFileWriter As New OutputFileWriter()
                                            tmpFileWriter.isFirstRecrod = True
                                            tmpFileWriter.countNoOperator = 0
                                            tmpFileWriter.writer = tmpStreamWriter
                                            tmpFileWriter.OperatorName = key
                                            tmpFileWriter.countRSSINev999 = 0
                                            tmpFileWriter.FileFullName = tmpOutFile
                                            _OperatorBandSet.Add(key, tmpFileWriter)

                                        End If


                                        If (Not _OperatorSet.ContainsKey(OpName)) Then
                                            Dim filePath As String = FileNameStr.Substring(0, FileNameStr.LastIndexOf("\"))
                                            Dim FileNameOnly As String = FileNameStr.Substring(FileNameStr.LastIndexOf("\") + 1)

                                            Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_AllBands_HOME.txt"
                                            Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                                            If TechType = "CDMA" Then
                                                tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,Spread_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,Spread_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,Spread_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,Spread_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,Spread_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,Spread_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                                                'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,ULInterference_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,ULInterference_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,ULInterference_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,ULInterference_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,ULInterference_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,ULInterference_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                                            ElseIf TechType = "UMTS" Then
                                                tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                                                'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,ULInterference_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,ULInterference_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,ULInterference_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,ULInterference_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,ULInterference_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,ULInterference_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                                            End If

                                            Dim tmpFileWriter As New OutputFileWriter()
                                            tmpFileWriter.isFirstRecrod = True
                                            tmpFileWriter.countNoOperator = 0
                                            tmpFileWriter.writer = tmpStreamWriter
                                            tmpFileWriter.OperatorName = OpName
                                            tmpFileWriter.countRSSINev999 = 0
                                            tmpFileWriter.FileFullName = tmpOutFile
                                            _OperatorSet.Add(OpName, tmpFileWriter)

                                        End If

                                        WCDMASIDHashSet.Add(SIDNID.SID.Trim + SIDNID.NID.Trim + currentWCDMAChannel.Trim)
                                        WCDMAChannelList.Item(currentWCDMAChannel).Add(SIDNID)
                                    End If

                                End If

                            End If

                            If isUMTSFirstRecord Then
                                _currentNosData.Band = _myBand
                                _currentNosData.Channel = _channel

                                _currentNosData.MCC = SIDNID.SID
                                _currentNosData.MNC = SIDNID.NID
                                isUMTSFirstRecord = False
                            End If
                        End If

                    End If


                End If
            Catch ex As Exception
                Dim tt As String = ""

            End Try

        End If


    End Sub

    Private Function GetFileNameOnly(ByVal FileName As String) As String
        Return FileName.Substring(FileName.LastIndexOf(".") + 1)
    End Function

    Private Function GetFileNameWithOutExt(ByVal FileName As String) As String
        Dim FileNameOnly As String = FileName.Substring(FileName.LastIndexOf("\") + 1)

        Return FileNameOnly.Substring(0, FileNameOnly.IndexOf("."))

    End Function

    Private Function GetLatLon(ByVal fileInputObj As ScannerFileInputObject, ByVal FileName As String) As List(Of GPSLossLatLon)

        Dim ListGPSLossLatLon As New List(Of GPSLossLatLon)()

        Dim enc As New System.Text.ASCIIEncoding
        Dim cnn As SQLiteConnection
        Dim cmd As SQLiteCommand
        Dim dr As SQLiteDataReader
        Dim msgData As Byte()
        Dim msgType As Integer
        Dim strMsgData As String()
        Dim tmpID As Integer
        Dim channelList As New Hashtable()

        cnn = New SQLiteConnection
        cnn.ConnectionString = "Data Source=" & FileName.Replace("\", "\\") & ";Version=3;New=False;Compress=True;"
        cmd = New SQLiteCommand
        cmd.Connection = cnn
        cmd.CommandText = "select ID, MessageData, TimeStampCreated,MessageType  from message WHERE MessageType = 1001 order by ID "

        If cmd.Connection.State <> ConnectionState.Open Then cmd.Connection.Open()
        dr = cmd.ExecuteReader()

        Dim lastTmpTimeStamp As DateTime = Nothing
        Dim currentTimeStamp As DateTime
        Dim tmpGPSLossLatLon As New GPSLossLatLon()
        tmpGPSLossLatLon.FileName = FileName

        Dim isSet As Boolean = False
        While dr.Read
            tmpID = Convert.ToInt32(dr("ID"))
            currentTimeStamp = DateTime.FromOADate(CDbl(dr("TimeStampCreated")))
            msgData = dr("MessageData")
            strMsgData = enc.GetString(msgData).Split(",")
            lat = strMsgData(0)
            lon = strMsgData(1)
            If Not isSet Then
                lastTmpTimeStamp = currentTimeStamp
                isSet = True
            End If

            If (currentTimeStamp - lastTmpTimeStamp).TotalSeconds < 10 Then
                tmpGPSLossLatLon.StartID = tmpID
                tmpGPSLossLatLon.StartLat = lat
                tmpGPSLossLatLon.StartLon = lon
                tmpGPSLossLatLon.startTime1 = currentTimeStamp
                tmpGPSLossLatLon.StartTime = currentTimeStamp.Ticks
            Else
                tmpGPSLossLatLon.EndID = tmpID
                tmpGPSLossLatLon.EndLat = lat
                tmpGPSLossLatLon.EndLon = lon
                tmpGPSLossLatLon.EndTime = currentTimeStamp.Ticks
                tmpGPSLossLatLon.endTime1 = currentTimeStamp

                ListGPSLossLatLon.Add(tmpGPSLossLatLon)

            End If



            lastTmpTimeStamp = currentTimeStamp

        End While

        dr.Close()
        cmd.Dispose()
        cnn.Close()

        Return ListGPSLossLatLon

    End Function

    Private Sub GetLatLonFromLinearInterpolation(ByVal ListGPSLossLatLon As List(Of GPSLossLatLon), ByVal tmpID As Integer, ByVal time As Long)
        isGPSInterpolated = False
        For Each gpsLatLon In ListGPSLossLatLon
            If Math.Abs(gpsLatLon.StartLat - gpsLatLon.EndLat) > 0.001 Or Math.Abs(gpsLatLon.StartLon - gpsLatLon.EndLon) > 0.001 Then
                If gpsLatLon.StartID < tmpID And tmpID < gpsLatLon.EndID Then
                    isGPSInterpolated = True
                    lat = (time - gpsLatLon.StartTime) * (gpsLatLon.EndLat - gpsLatLon.StartLat) / (gpsLatLon.EndTime - gpsLatLon.StartTime) + gpsLatLon.StartLat
                    lon = (time - gpsLatLon.StartTime) * (gpsLatLon.EndLon - gpsLatLon.StartLon) / (gpsLatLon.EndTime - gpsLatLon.StartTime) + gpsLatLon.StartLon
                    Return
                End If
            End If

        Next

    End Sub



    Private Sub GetChannelList(ByVal TechType As String, ByVal fileInputObj As ScannerFileInputObject)

        Dim enc As New System.Text.ASCIIEncoding

        CDMASIDHashSet = New HashSet(Of String)
        WCDMASIDHashSet = New HashSet(Of String)
        LTESIDHashSet = New HashSet(Of String)

        Dim msgData As Byte()
        _LTEChannelFoundHash = New Hashtable()

        If fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip Then
            GetFileData(fileInputObj)
            Dim rowDataInput As DataRow

            Dim isFirstRecord3030 As Boolean = True
            Dim isFirstRecord3040 As Boolean = True


            For Each rowDataInput In _DataRowList

                If rowDataInput.FileFullName.IndexOf("-3030-") > 0 Then
                    _lastLTETimeStamp3030 = rowDataInput.TimeStampCreated
                Else
                    _lastLTETimeStamp3040 = rowDataInput.TimeStampCreated
                End If

                GetSID(rowDataInput.TimeStampCreated, TechType, rowDataInput.MessageData, Convert.ToString(rowDataInput.MessageType), rowDataInput.FileFullName, fileInputObj)
            Next

            If _DataRowList1.Count > 0 Then
                For Each rowDataInput In _DataRowList1
                    GetSID(rowDataInput.TimeStampCreated, TechType, rowDataInput.MessageData, Convert.ToString(rowDataInput.MessageType), rowDataInput.FileFullName, fileInputObj)
                Next
            End If
        Else
            Dim cnn As SQLiteConnection
            Dim cmd As SQLiteCommand
            Dim dr As SQLiteDataReader

            Dim channelList As New Hashtable()

            cnn = New SQLiteConnection
            cnn.ConnectionString = "Data Source=" & fileInputObj.FileName.Replace("\", "\\") & ";Version=3;New=False;Compress=True;"
            cmd = New SQLiteCommand
            cmd.Connection = cnn

            Dim totalCountRecord As Integer = 0

            If TechType.ToUpper() = "LTE" Then
                cmd.CommandText = "select ID, MessageData, TimeStampCreated,MessageType  from message WHERE MessageType = 10522 order by ID "
            ElseIf TechType.ToUpper() = "UMTS" Then
                cmd.CommandText = "select ID, MessageData, TimeStampCreated,MessageType  from message where MessageType = 10103 or MessageType =10104 order by ID "
            ElseIf TechType.ToUpper() = "CDMA" Then
                cmd.CommandText = "select ID, MessageData, TimeStampCreated,MessageType  from message where MessageType =10103 or MessageType =10104 order by ID "
            ElseIf TechType.ToUpper() = "GSM" Then
                cmd.CommandText = "select ID, MessageData, TimeStampCreated,MessageType  from message where MessageType =10010 or MessageType =10011 " _
                                    + "or MessageType = 12288 or MessageType = 10001 order by ID"
            End If

            If cmd.Connection.State <> ConnectionState.Open Then cmd.Connection.Open()
            dr = cmd.ExecuteReader()
            Dim strMsgData As String()

            Dim isFirstTimeStamp As Boolean = False


            Dim firstTimeStamp As DateTime
            Dim lastTimeStamp As DateTime


            While (dr.Read)
                Try
                    If Not (IsDBNull(dr("MessageData"))) Then

                        If TechType = "LTE" Then
                            totalCountRecord = totalCountRecord + 1
                        End If

                        dttm = DateTime.FromOADate(CDbl(dr("TimeStampCreated")))

                        Dim MessageType As String = Convert.ToString(dr("MessageType"))

                        If Not isFirstTimeStamp Then
                            TimeZoneValue = GetTimeOffset(fileInputObj.FileName, dttm)
                            isFirstTimeStamp = True
                            firstTimeStamp = dttm
                        End If

                        lastTimeStamp = dttm

                        msgData = dr("MessageData")

                        strMsgData = enc.GetString(msgData).Split(",")

                        'If MessageType.ToString() = "10104" Then
                        '    If TechType = "UMTS" And strMsgData(8).Trim = "100" Then
                        '        totalCountRecord = totalCountRecord + 1
                        '    End If
                        '    If TechType = "CDMA" And strMsgData(8).Trim <> "100" Then
                        '        totalCountRecord = totalCountRecord + 1
                        '    End If
                        'End If



                        dttm = Convert.ToDateTime(dttm).AddMinutes(TimeZoneValue)


                        If MessageType.ToString() = "10103" Then

                            ID = dr("ID")
                        End If

                        GetSID(dttm, TechType, msgData, MessageType, fileInputObj.FileName, fileInputObj)

                    End If
                Catch
                    Dim tt As String = ""
                End Try
            End While

            dr.Close()
            cmd.Dispose()
            cnn.Close()

            _ScanRate3030 = totalCountRecord / (lastTimeStamp - firstTimeStamp).TotalSeconds

        End If


        'Sort Channel List by Time

        If TechType = "LTE" Then
            If fileInputObj.FileMergeName.Trim <> "" And Not fileInputObj.isSkip Then
                _ScanRate3030 = Math.Round(_ScanRate3030, 2)
                _ScanRate3040 = Math.Round(_ScanRate3040, 2)
                _ScanPeriod3030 = Math.Round(_LTEScannerData.inputChannelList3030.Count / _ScanRate3030, 2)
                _ScanPeriod3040 = Math.Round(_LTEScannerData.inputChannelList3040.Count / _ScanRate3040, 2)

            Else
                _ScanRate3030 = 1
                _ScanRate3040 = 1
                _ScanPeriod3030 = Math.Round(_LTEScannerData.inputChannelList3030.Count / _ScanRate3030, 2)
                _ScanPeriod3040 = Math.Round(_LTEScannerData.inputChannelList3040.Count / _ScanRate3040, 2)
            End If


        Else
            _ScanRate3030 = Math.Round(_ScanRate3030, 2)

        End If


        If TechType = "LTE" Then
            Dim tmpList As New List(Of Object)

            Dim result = From v In LTEChannelListWithTime.Values Order By v.StartTimeTick Descending

            tmpList = result.ToList()
            Dim inputChannelObj As InputChannelStore

            If tmpList.Count > 1 Then
                For i = 0 To tmpList.Count - 2
                    Dim date1 As DateTime = tmpList(i).StartTime
                    Dim date2 As DateTime = tmpList(tmpList.Count - 2).StartTime
                    Dim tt As Integer = (date1 - date2).TotalSeconds
                    If tt > 5 Or tt < -5 Then
                        inputChannelObj = tmpList(i)
                        inputChannelObj.isStartLate = True
                        _LTEScannerData.inputChannelList.Remove(inputChannelObj.channel)

                        tmpList(i) = inputChannelObj
                        LTEChannelListWithTime.Item(inputChannelObj.channel + "|" + inputChannelObj.band) = inputChannelObj
                    End If
                Next
            End If



            result = From v In LTEChannelListWithTime.Values Order By v.EndTimeTick

            tmpList = result.ToList()


            If tmpList.Count > 1 Then
                For i = 0 To tmpList.Count - 2
                    Dim date1 As DateTime = tmpList(i).EndTime
                    Dim date2 As DateTime = tmpList(tmpList.Count - 2).EndTime
                    Dim tt As Integer = (date1 - date2).TotalSeconds
                    If tt > 5 Or tt < -5 Then
                        inputChannelObj = tmpList(i)
                        inputChannelObj.isEndEarly = True
                        tmpList(i) = inputChannelObj
                        LTEChannelListWithTime.Item(inputChannelObj.channel + "|" + inputChannelObj.band) = inputChannelObj
                    End If
                Next
            End If

            LTEChannelList3030 = _LTEScannerData.inputChannelList3030
            LTEChannelList3040 = _LTEScannerData.inputChannelList3040
        ElseIf TechType = "UMTS" Then
            Dim tmpList As New List(Of Object)

            Dim result = From v In WCDMAChannelListWithTime.Values Order By v.StartTimeTick Descending

            tmpList = result.ToList()
            Dim inputChannelObj As InputChannelStore

            If tmpList.Count > 1 Then
                For i = 0 To tmpList.Count - 2
                    Dim date1 As DateTime = tmpList(i).StartTime
                    Dim date2 As DateTime = tmpList(tmpList.Count - 2).StartTime
                    Dim tt As Integer = (date1 - date2).TotalSeconds
                    If tt > 5 Or tt < -5 Then
                        inputChannelObj = tmpList(i)
                        inputChannelObj.isStartLate = True
                        _WCDMAScannerData.inputChannelList.Remove(inputChannelObj.channel)

                        tmpList(i) = inputChannelObj
                        WCDMAChannelListWithTime.Item(inputChannelObj.channel + "|" + inputChannelObj.band) = inputChannelObj
                    End If
                Next
            End If


            result = From v In WCDMAChannelListWithTime.Values Order By v.EndTimeTick

            tmpList = result.ToList()


            If tmpList.Count > 1 Then
                For i = 0 To tmpList.Count - 2
                    Dim date1 As DateTime = tmpList(i).EndTime
                    Dim date2 As DateTime = tmpList(tmpList.Count - 2).EndTime
                    Dim tt As Integer = (date1 - date2).TotalSeconds
                    If tt > 5 Or tt < -5 Then
                        inputChannelObj = tmpList(i)
                        inputChannelObj.isEndEarly = True
                        tmpList(i) = inputChannelObj
                        WCDMAChannelListWithTime.Item(inputChannelObj.channel + "|" + inputChannelObj.band) = inputChannelObj
                    End If
                Next
            End If
        ElseIf TechType = "CDMA" Then
            Dim tmpList As New List(Of Object)

            Dim result = From v In CDMAChannelListWithTime.Values Order By v.StartTimeTick Descending

            tmpList = result.ToList()
            Dim inputChannelObj As InputChannelStore

            If tmpList.Count > 1 Then
                For i = 0 To tmpList.Count - 2
                    Dim date1 As DateTime = tmpList(i).StartTime
                    Dim date2 As DateTime = tmpList(tmpList.Count - 2).StartTime
                    Dim tt As Integer = (date1 - date2).TotalSeconds
                    If tt > 5 Or tt < -5 Then
                        inputChannelObj = tmpList(i)
                        inputChannelObj.isStartLate = True
                        _CDMAScannerData.inputChannelList.Remove(inputChannelObj.band + inputChannelObj.channel)

                        tmpList(i) = inputChannelObj
                        CDMAChannelListWithTime.Item(inputChannelObj.channel + "|" + inputChannelObj.band) = inputChannelObj
                    End If
                Next
            End If


            result = From v In CDMAChannelListWithTime.Values Order By v.EndTimeTick

            tmpList = result.ToList()


            If tmpList.Count > 1 Then
                For i = 0 To tmpList.Count - 2
                    Dim date1 As DateTime = tmpList(i).EndTime
                    Dim date2 As DateTime = tmpList(tmpList.Count - 2).EndTime
                    Dim tt As Integer = (date1 - date2).TotalSeconds
                    If tt > 5 Or tt < -5 Then
                        inputChannelObj = tmpList(i)
                        inputChannelObj.isEndEarly = True
                        tmpList(i) = inputChannelObj
                        CDMAChannelListWithTime.Item(inputChannelObj.channel + "|" + inputChannelObj.band) = inputChannelObj
                    End If
                Next
            End If
        End If

        LTEChannelList1 = LTEChannelList
    End Sub

    Private Sub GetChannelFound(ByVal TechType As String, ByVal fileInputObj As ScannerFileInputObject)


        Try

            For kk = 0 To ScannerCommon.FileLogInfoList.Count - 1

                If fileInputObj.FileMergeName.Trim <> "" Then
                    If ScannerCommon.FileLogInfoList(kk).FileName = fileInputObj.FileName Then
                        ScannerCommon.FileLogInfoList(kk).LTEChannelFound.Add(" LTE Channels Found:" & _LTEChannelFoundHash.Item(GetFileNameWithOutExt(fileInputObj.FileName)))
                    End If

                    If ScannerCommon.FileLogInfoList(kk).FileName = fileInputObj.FileMergeName Then
                        ScannerCommon.FileLogInfoList(kk).LTEChannelFound.Add(" LTE Channels Found:" & _LTEChannelFoundHash.Item(GetFileNameWithOutExt(fileInputObj.FileMergeName)))
                    End If

                Else
                    If ScannerCommon.FileLogInfoList(kk).FileName = fileInputObj.FileName Then
                        If ScannerCommon.FileLogInfoList(kk).LTEChannelFound.IndexOf(_LTEChannelFound) < 0 Then
                            ScannerCommon.FileLogInfoList(kk).LTEChannelFound.Add(" LTE Channels Found:" & _LTEChannelFound)
                        End If
                    End If

                End If

                If ScannerCommon.FileLogInfoList(kk).FileName = fileInputObj.FileName Then
                    If ScannerCommon.FileLogInfoList(kk).CDMAChannelFound.IndexOf(_CDMAChannelFound) < 0 Then
                        ScannerCommon.FileLogInfoList(kk).CDMAChannelFound.Add(" CDMA Channels Found:" & _CDMAChannelFound)
                    End If
                    If ScannerCommon.FileLogInfoList(kk).WCDMAChannelFound.IndexOf(_WCDMAChannelFound) < 0 Then
                        ScannerCommon.FileLogInfoList(kk).WCDMAChannelFound.Add(" WCDMA Channels Found:" & _WCDMAChannelFound)
                    End If

                End If
            Next

        Catch ex As Exception

        End Try
    End Sub

    Private Sub GetDoNotExistList(ByVal fileName As String, ByVal TechType As String)

        'Do Not Exist Operator
        Dim filePath As String = fileName.Substring(0, fileName.LastIndexOf("\"))
        Dim FileNameOnly As String = fileName.Substring(fileName.LastIndexOf("\") + 1)
        DoNotExistList = New Hashtable()

        Dim tmpHashtable As New Tier1_Operator
        tmpHashtable = Tier1_Operator_List.Item("Default")


        If Tier1_Operator_List.ContainsKey(_ClientName) Then
            tmpHashtable = Tier1_Operator_List.Item(_ClientName)
        End If
        For Each key In tmpHashtable.Operator_List.Keys
            If Not _OperatorSet.ContainsKey(key.Trim) And tmpHashtable.Operator_List.Item(key.Trim).ToString().IndexOf(TechType) >= 0 Then
                DoNotExistList.Add(key, tmpHashtable.Operator_List.Item(key))
            End If
        Next


        If TechType = "LTE" Then

            If DoNotExistList.Count > 0 Then
                Dim OpName As String
                For Each OpName In DoNotExistList.Keys
                    Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_AllBands_HOME_NoS.txt"
                    Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                    'modified by My 05/31/2018 #1278
                    tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,NumTxAntennaPortsWideBand_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,NumTxAntennaPortsWideBand_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,NumTxAntennaPortsWideBand_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,NumTxAntennaPortsWideBand_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,NumTxAntennaPortsWideBand_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,NumTxAntennaPortsWideBand_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                    'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,DelaySpread_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,DelaySpread_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,DelaySpread_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,DelaySpread_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,DelaySpread_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,DelaySpread_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                    Dim tmpFileWriter As New OutputFileWriter()
                    tmpFileWriter.isFirstRecrod = True
                    tmpFileWriter.countNoOperator = 0
                    tmpFileWriter.writer = tmpStreamWriter
                    tmpFileWriter.OperatorName = OpName
                    tmpFileWriter.countRSSINev999 = 0
                    tmpFileWriter.FileFullName = tmpOutFile
                    tmpFileWriter.isDoNotExist = True
                    _OperatorSet.Add(OpName, tmpFileWriter)

                    'Adding Operator Band Set
                    'a.	700, 850, 1900, 2100, 2300, 2500
                    Dim bandStr = DoNotExistList.Item(OpName).ToString()
                    Dim tmps As String() = bandStr.Split(")")
                    Dim tmpStr As String
                    For Each tmpStr In tmps
                        If tmpStr.IndexOf(TechType) >= 0 Then
                            bandStr = tmpStr
                        End If
                    Next
                    bandStr = bandStr.Replace("(", "")
                    bandStr = bandStr.Replace(TechType, "")
                    bandStr = bandStr.Trim


                    'LTE(700,850,1900,2100,2300),UMTS(700,850,1900,2100)
                    Dim LTEBandList As String() = bandStr.Split(",")
                    'Dim LTEBandList As String() = {"700", "850", "1900", "2100", "2300", "2500"}
                    For Each band In LTEBandList
                        If band.Trim <> "" Then
                            Dim key As String = OpName + "LTE" + band

                            Dim tmpOutFile1 As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_" + band + "_HOME_NoS.txt"
                            Dim tmpStreamWriter1 As StreamWriter = New StreamWriter(tmpOutFile1)
                            'modified by My 05/31/2018 #1278
                            tmpStreamWriter1.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,NumTxAntennaPortsWideBand_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,NumTxAntennaPortsWideBand_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,NumTxAntennaPortsWideBand_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,NumTxAntennaPortsWideBand_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,NumTxAntennaPortsWideBand_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,NumTxAntennaPortsWideBand_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                            'tmpStreamWriter1.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Tech,ScanID,EARFCN,CarrierFrequencyMHz,CarrierBW,CarrierSignalLevel,PhysicalCellId_1,RSRP_1,RSRQ_1,CINR_1,PSCH_RP_1,PSCH_RQ_1,DelaySpread_1,PhysicalCellId_2,RSRP_2,RSRQ_2,CINR_2,PSCH_RP_2,PSCH_RQ_2,DelaySpread_2,PhysicalCellId_3,RSRP_3,RSRQ_3,CINR_3,PSCH_RP_3,PSCH_RQ_3,DelaySpread_3,PhysicalCellId_4,RSRP_4,RSRQ_4,CINR_4,PSCH_RP_4,PSCH_RQ_4,DelaySpread_4,PhysicalCellId_5,RSRP_5,RSRQ_5,CINR_5,PSCH_RP_5,PSCH_RQ_5,DelaySpread_5,PhysicalCellId_6,RSRP_6,RSRQ_6,CINR_6,PSCH_RP_6,PSCH_RQ_6,DelaySpread_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB_PhysicalCellId,SIB_PhysicalCellId_Position,MCC,MNC,CellIdentity,TAC,SIB1,SIB2,SIB3,SIB4,SIB5,SIB6,SIB8")
                            Dim tmpFileWriter1 As New OutputFileWriter()
                            tmpFileWriter1.isFirstRecrod = True
                            tmpFileWriter1.countNoOperator = 0
                            tmpFileWriter1.writer = tmpStreamWriter1
                            tmpFileWriter1.OperatorName = key
                            tmpFileWriter1.FileFullName = tmpOutFile1
                            tmpFileWriter1.countRSSINev999 = 0
                            _OperatorBandSet.Add(key, tmpFileWriter1)
                        End If

                    Next
                Next
            End If

        ElseIf TechType = "UMTS" Then

            'If Not _OperatorSet.ContainsKey("AT&T") Then
            '    DoNotExistList.Add("AT&T")
            'End If
            'If Not _OperatorSet.ContainsKey("T-Mobile") Then
            '    DoNotExistList.Add("T-Mobile")
            'End If

            If DoNotExistList.Count > 0 Then
                Dim OpName As String
                For Each OpName In DoNotExistList.Keys
                    Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_AllBands_HOME_NoS.txt"
                    Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                    tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                    'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,ULInterference_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,ULInterference_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,ULInterference_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,ULInterference_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,ULInterference_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                    Dim tmpFileWriter As New OutputFileWriter()
                    tmpFileWriter.isFirstRecrod = True
                    tmpFileWriter.countNoOperator = 0
                    tmpFileWriter.writer = tmpStreamWriter
                    tmpFileWriter.OperatorName = OpName
                    tmpFileWriter.countRSSINev999 = 0
                    tmpFileWriter.FileFullName = tmpOutFile
                    tmpFileWriter.isDoNotExist = True
                    _OperatorSet.Add(OpName, tmpFileWriter)

                    'Adding Operator Band Set
                    '700, 850, 1900, 2100
                    'Dim BandList As String() = {"700", "850", "1900", "2100"}

                    Dim bandStr = DoNotExistList.Item(OpName).ToString()

                    Dim tmps As String() = bandStr.Split(")")
                    Dim tmpStr As String
                    For Each tmpStr In tmps
                        If tmpStr.IndexOf(TechType) >= 0 Then
                            bandStr = tmpStr
                        End If
                    Next
                    bandStr = bandStr.Replace("(", "")
                    bandStr = bandStr.Replace(TechType, "")
                    bandStr = bandStr.Trim


                    'LTE(700,850,1900,2100,2300),UMTS(700,850,1900,2100)
                    Dim BandList As String() = bandStr.Split(",")


                    For Each band In BandList
                        If band.Trim <> "" Then
                            Dim key As String = OpName + "UMTS" + band
                            Dim tmpOutFile1 As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_" + band + "_HOME_NoS.txt"
                            Dim tmpStreamWriter1 As StreamWriter = New StreamWriter(tmpOutFile1)
                            tmpStreamWriter1.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,Spread_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,Spread_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,Spread_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,Spread_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,Spread_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                            'tmpStreamWriter1.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,MCC,MNC,NoRxLev,CPICHSC_1,ULInterference_1,CellID_1,LAC_1,PSCEc_1,CPICHEc_1,CPICHEcNo_1,BCCHEc_1,SigIntRatio_1,CPICHSC_2,ULInterference_2,CellID_2,LAC_2,PSCEc_2,CPICHEc_2,CPICHEcNo_2,BCCHEc_2,SigIntRatio_2,CPICHSC_3,ULInterference_3,CellID_3,LAC_3,PSCEc_3,CPICHEc_3,CPICHEcNo_3,BCCHEc_3,SigIntRatio_3,CPICHSC_4,ULInterference_4,CellID_4,LAC_4,PSCEc_4,CPICHEc_4,CPICHEcNo_4,BCCHEc_4,SigIntRatio_4,CPICHSC_5,ULInterference_5,CellID_5,LAC_5,PSCEc_5,CPICHEc_5,CPICHEcNo_5,BCCHEc_5,SigIntRatio_5,CPICHSC_6,Spread_6,CellID_6,LAC_6,PSCEc_6,CPICHEc_6,CPICHEcNo_6,BCCHEc_6,SigIntRatio_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace,SIB1,SIB2,SIB3,SIB5,SIB7")
                            Dim tmpFileWriter1 As New OutputFileWriter()
                            tmpFileWriter1.isFirstRecrod = True
                            tmpFileWriter1.countNoOperator = 0
                            tmpFileWriter1.writer = tmpStreamWriter1
                            tmpFileWriter1.OperatorName = key
                            tmpFileWriter1.FileFullName = tmpOutFile1
                            tmpFileWriter1.countRSSINev999 = 0
                            _OperatorBandSet.Add(key, tmpFileWriter1)
                        End If
                    
                    Next
                Next
            End If

        ElseIf TechType = "CDMA" Then
            'If Not _OperatorSet.ContainsKey("Verizon") Then
            '    DoNotExistList.Add("Verizon")
            'End If
            'If Not _OperatorSet.ContainsKey("Sprint") Then
            '    DoNotExistList.Add("Sprint")
            'End If

            If DoNotExistList.Count > 0 Then
                Dim OpName As String
                For Each OpName In DoNotExistList.Keys
                    Dim tmpOutFile As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_AllBands_HOME_NoS.txt"
                    Dim tmpStreamWriter As StreamWriter = New StreamWriter(tmpOutFile)
                    tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,Spread_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,Spread_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,Spread_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,Spread_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,Spread_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,Spread_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                    'tmpStreamWriter.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,ULInterference_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,ULInterference_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,ULInterference_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,ULInterference_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,ULInterference_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,ULInterference_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                    Dim tmpFileWriter As New OutputFileWriter()
                    tmpFileWriter.isFirstRecrod = True
                    tmpFileWriter.countNoOperator = 0
                    tmpFileWriter.writer = tmpStreamWriter
                    tmpFileWriter.OperatorName = OpName
                    tmpFileWriter.countRSSINev999 = 0
                    tmpFileWriter.FileFullName = tmpOutFile
                    tmpFileWriter.isDoNotExist = True
                    _OperatorSet.Add(OpName, tmpFileWriter)
                    ':  850, 1900, 2100
                    'Dim BandList As String() = {"850", "1900", "2100"}

                    Dim bandStr = DoNotExistList.Item(OpName).ToString()
                    Dim tmps As String() = bandStr.Split(")")
                    Dim tmpStr As String
                    For Each tmpStr In tmps
                        If tmpStr.IndexOf(TechType) >= 0 Then
                            bandStr = tmpStr
                        End If
                    Next
                    bandStr = bandStr.Replace("(", "")
                    bandStr = bandStr.Replace(TechType, "")
                    bandStr = bandStr.Trim


                    'LTE(700,850,1900,2100,2300),UMTS(700,850,1900,2100)
                    Dim BandList As String() = bandStr.Split(",")

                    For Each band In BandList
                        If band.Trim <> "" Then
                            Dim key As String = OpName + "CDMA" + band
                            Dim tmpOutFile1 As String = filePath + "\" + FileNameOnly.Substring(0, FileNameOnly.IndexOf(".")) + "_" + OpName.Replace("-", "").Replace("&", "").Replace(" ", "") + "_" + TechType + "_" + band + "_HOME_NoS.txt"
                            Dim tmpStreamWriter1 As StreamWriter = New StreamWriter(tmpOutFile1)
                            tmpStreamWriter1.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,Spread_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,Spread_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,Spread_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,Spread_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,Spread_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,Spread_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                            'tmpStreamWriter1.WriteLine("Command,Time_Stamp,GPS_Time,Lat,Lon,Band,Tech,ScanID,Channel,SID,NID,NoRxLev,PN_1,ULInterference_1,Ec_1_dbm,EcIo_1,BaseId_1,BaseLat_1,BaseLon_1,PN_2,ULInterference_2,Ec_2_dbm,EcIo_2,BaseId_2,BaseLat_2,BaseLon_2,PN_3,ULInterference_3,Ec_3_dbm,EcIo_3,BaseId_3,BaseLat_3,BaseLon_3,PN_4,ULInterference_4,Ec_4_dbm,EcIo_4,BaseId_4,BaseLat_4,BaseLon_4,PN_5,ULInterference_5,Ec_5_dbm,EcIo_5,BaseId_5,BaseLat_5,BaseLon_5,PN_6,ULInterference_6,Ec_6_dbm,EcIo_6,BaseId_6,BaseLat_6,BaseLon_6,Operator,DataType,FileName,Collected_Date,MarketName,Campaign,ClientName,LogTrace")
                            Dim tmpFileWriter1 As New OutputFileWriter()
                            tmpFileWriter1.isFirstRecrod = True
                            tmpFileWriter1.countNoOperator = 0
                            tmpFileWriter1.writer = tmpStreamWriter1
                            tmpFileWriter1.OperatorName = key
                            tmpFileWriter1.countRSSINev999 = 0
                            tmpFileWriter1.FileFullName = tmpOutFile1
                            _OperatorBandSet.Add(key, tmpFileWriter1)
                        End If
                        
                    Next
                Next
            End If
        End If

    End Sub


    Private Sub GetSidLooup()
        _SIDLookupList = New Hashtable()

        Try

            Try
                If System.IO.File.Exists("T:\Market Operators\GWS_PCS_Cellular_SID_lookup.txt") Then
                    Dim netFileTimeStamp = System.IO.File.GetLastWriteTime("T:\Market Operators\GWS_PCS_Cellular_SID_lookup.txt")
                    Dim localFileTimeStamp = System.IO.File.GetLastWriteTime(_SIDFile)
                    If netFileTimeStamp <> localFileTimeStamp Then
                        System.IO.File.Copy("T:\Market Operators\GWS_PCS_Cellular_SID_lookup.txt", _SIDFile, True)
                    End If
                ElseIf System.IO.File.Exists("\\GWSFS2\TEMP\Market Operators\GWS_PCS_Cellular_SID_lookup.txt") Then

                    Dim netFileTimeStamp = System.IO.File.GetLastWriteTime("\\GWSFS2\TEMP\Market Operators\GWS_PCS_Cellular_SID_lookup.txt")
                    Dim localFileTimeStamp = System.IO.File.GetLastWriteTime(_SIDFile)
                    If netFileTimeStamp <> localFileTimeStamp Then
                        System.IO.File.Copy("\\GWSFS2\TEMP\Market Operators\GWS_PCS_Cellular_SID_lookup.txt", _SIDFile, True)
                    End If

                    '\\GWSFS2\TEMP\Market Operators\GWS_PCS_Cellular_SID_lookup.txt

                End If
            Catch ex As Exception

            End Try


            Using sr As New StreamReader(_SIDFile)
                Dim line As String
                Do While sr.Peek() >= 0
                    line = sr.ReadLine()
                    Dim strs As String() = line.Split(",")
                    If strs(0).Trim <> "SID" Then

                        Dim tmpInt As Integer
                        If Integer.TryParse(strs(0), tmpInt) Then

                            If Not _SIDLookupList.ContainsKey(tmpInt) Then
                                If tmpInt = 5035 Then
                                    Dim tt As String = ""
                                End If
                                _SIDLookupList.Add(tmpInt, strs(1))
                            End If

                        End If

                    End If
                Loop


            End Using
        Catch e As Exception
            Console.WriteLine("The file could not be read:")
            Console.WriteLine(e.Message)
        End Try
    End Sub

    Private Sub ZipMyFile(filename As String)
        Dim fileZip As ZipFile
        fileZip = New ZipFile()
        fileZip.AddFile(filename, "")
        fileZip.Save(IO.Path.GetDirectoryName(filename) & "\" & IO.Path.GetFileNameWithoutExtension(filename) & ".zip")
        fileZip.Dispose()
    End Sub

End Class
