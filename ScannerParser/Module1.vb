Imports System.IO

Module Module1
    Private _databaseConnectionString As String
    Public Property DatabaseConnectionString As String
        Get
            Return _databaseConnectionString
        End Get
        Set(ByVal value As String)
            _databaseConnectionString = value
        End Set
    End Property
    Sub Main()
        Dim dbclass = New SCNR_DB_Layer(DatabaseConnectionString)
        'Dim tech_id As Dictionary(Of String, Long)
        'Dim opscnr As List(Of String)
        Dim opname As String = "", tech As String = "", band As String = ""
        Dim id_scnr_parse As Long
        Dim propscnrparse As New tblSCNRParse
        'Add This Section while creating each file for scanner
        'One Scanner input has multiple output right?, use this each time
        'Each file should have individual entries

        'Dim scnrmethods As New SCNR_Methods
        'tech_id = New Dictionary(Of String, Long)
        'opscnr = New List(Of String)
        opname = "" : tech = "" : band = ""

        'You can remove this method if are able to get operator name, technology and band directly while generating scanner files
        'Please look at this method, what this method does is:
        '1) it tries to get opname, band and tech directly based on filename 
        '2) if it gets from them, it just exit sub
        '3) if not then it opens file and read header and first line
        '4) It gets all values of Operator, Tech and Band from first line itself
        '5) try to get rid of this method by getting these values internally
        getInfoFromFile("[Parsed File Name with Path]", opname, tech, band)

        propscnrparse.id_server_trans = "[id_server_transaction]"
        propscnrparse.id_server_file = "[id_server_file]"
        propscnrparse.file_path = "[Scanner File Path, NO File Name]"
        propscnrparse.scnr_file_name = "[Name of the Scanner File ONLY, NO PATH]"
        propscnrparse.operator_name = opname
        propscnrparse.tech = tech
        propscnrparse.band = band
        propscnrparse.status_flag = "[If there is an error, status flag will be 0 else status flag will be 1]"
        'move these files to destination location
        'scnrmethods.MoveFile(sparse, propscnrparse.file_path)
        id_scnr_parse = dbclass.InserttblSCNRParse(propscnrparse)

        'TUAN you dont need to include this in your code
        '' ''If (opname & "").ToUpper = "PARSE" Then
        '' ''    If Not tech_id.ContainsKey(tech) Then
        '' ''        tech_id.Add(tech, id_scnr_parse)
        '' ''    End If
        '' ''Else
        '' ''    If Not opscnr.Contains(tech) Then
        '' ''        opscnr.Add(tech)
        '' ''    End If
        '' ''End If



        'TUAN you dont need to include this in your code
        'Call this part AFTER all output files is done. 
        'Once all files are processed
        'Update hasScanner Flag
        '' ''For Each mytech In tech_id.Keys
        '' ''    If opscnr.Contains(mytech) Then
        '' ''        'Update DB with hasSIBL3_flag
        '' ''        dbclass.Update_hasSIBL3(tech_id(mytech))
        '' ''    End If
        '' ''Next
    End Sub


    Public Sub getInfoFromFile(parsedfilename As String, ByRef opname As String, ByRef tech As String, ByRef band As String)
        Dim lineno As Integer = 0
        Dim firstline As String = ""
        Dim headerline As String = ""
        Dim posopname As Integer = 0, postech As Integer = 0, posband As Integer = 0
        Dim file_name_only As String = Path.GetFileName(parsedfilename)

        'Check if we can get these parameters without opening file
        If file_name_only.ToUpper.Contains("PARSE") Then
            opname = "PARSE"
            band = "ALL"
        End If
        If file_name_only.ToUpper.Contains("ALLBANDS") Then
            band = "ALL"
        End If
        If file_name_only.ToUpper.Contains("LTE") Then
            tech = "LTE"
        ElseIf file_name_only.ToUpper.Contains("GSM") Then
            tech = "GSM"
        ElseIf file_name_only.ToUpper.Contains("CDMA") Then
            tech = "CDMA"
        ElseIf file_name_only.ToUpper.Contains("UMTS") Then
            tech = "UMTS"
        Else
            tech = "LTE"
        End If
        If opname <> "" AndAlso band <> "" AndAlso tech <> "" Then
            Exit Sub
        End If

        'if we do not find any one of the above parameters then open file to read the values from first line
        Using srscnr = New StreamReader(parsedfilename)
            Do While srscnr.Peek <> -1
                lineno += 1
                If lineno = 1 Then
                    headerline = srscnr.ReadLine
                ElseIf lineno = 2 Then
                    firstline = srscnr.ReadLine
                    Exit Do
                End If
            Loop
        End Using
        If headerline = "" OrElse firstline = "" Then
            Exit Sub
        Else
            posopname = Get_Field_Location(headerline, "Operator", ",")
            postech = Get_Field_Location(headerline, "Tech", ",")
            posband = Get_Field_Location(headerline, "Band", ",")
            opname = Get_Field(firstline, posopname, , ",")
            'tech = Get_Field(firstline, postech, , ",")
            band = Get_Field(firstline, posband, , ",")
        End If
    End Sub
    Public Function Get_Field_Location(ByVal DataString As String, ByVal FindFieldName As String, _
    Optional ByVal Delimiter As String = ",", Optional ByVal ExactMatch As Boolean = True) As Integer

        ' Given a String containing Field Names (DataString), and a String of a field Name,
        '   find the fieldnumber

        'This Function is modified by JT to removed looping and use .NET capabilities

        'Mod Function to permit matches that are not exact, but default to ExactMatch=True  (mod v15.1.1.apg)

        'Dim vlcol As Integer
        'Dim vLi As Integer
        Dim arrFieldlist As ArrayList
        Dim vLi As Integer

        Get_Field_Location = 0
        DataString = DataString.Trim.ToUpper
        DataString = DataString.Replace(Delimiter & " ", Delimiter)
        arrFieldlist = New ArrayList(DataString.Split(New Char() {Delimiter}))
        If ExactMatch Then                                                              'mod with condition, v15.1.1.apg
            Get_Field_Location = arrFieldlist.IndexOf(FindFieldName.ToUpper) + 1
        Else
            For vLi = 0 To arrFieldlist.ToArray.Length
                If arrFieldlist.Item(vLi).ToString.ToUpper.Contains(FindFieldName.ToUpper) Then
                    Get_Field_Location = vLi + 1
                    Exit For
                End If
            Next
        End If
        arrFieldlist = Nothing
        'vlcol = InStrOccur(1, DataString, Delimiter) + 1
        'For vLi = 1 To vlcol
        '    If UCase(FindFieldName) = UCase(Get_Field(DataString, vLi, , Delimiter)) Then
        '        Get_Field_Location = vLi
        '        Exit For
        '    End If
        'Next vLi

    End Function

    Public Function Get_Field(ByVal DataString As String, ByVal fieldid As Object, _
        Optional ByVal TotalNumberFields As Integer = 0, _
        Optional ByVal Delimiter As String = ",", Optional ByVal ExtractLastSubField As Boolean = False) As Object
        ' Given a Line of data (DataString), return the data contained within the FieldID listed:
        '   FieldID is either a # detailing the field number, or a String within the sought-after field
        '   Delimiter can be any length
        ' Mod 3/31/11 (v11.0.10.apg)
        '  Optional Criteria ExtractLastSubField will extract the msec of the LAST subfield meeting criteria

        'Dim vLi As Integer
        Dim vLFldStart As Integer, vLFldLen As Integer
        Dim vLFldNum As Integer
        Dim arrFieldlist As ArrayList
        Dim strGetField As Object
        'If Delimiter DNE in DataString, Output ""

        If InStr(1, DataString, Delimiter) = 0 Then
            Return ""
            Exit Function
        End If

        'If TotalNumberFields not provided, determine as = # Delimiters +1
        If TotalNumberFields = 0 Then TotalNumberFields = InStrOccur(1, DataString, Delimiter) + 1


        'If FieldID = Invalid #, exit function; else determine field length
        If IsNumeric(fieldid) Then
            If Val(fieldid) <= 0 Or Val(fieldid) > TotalNumberFields Then 'Modified by JT to take care of 0th position of array in .NET
                Return ""
                Exit Function
            Else
                'vLFldStart = 1
                'vLFldLen = 0
                'For vLi = 0 To fieldid - 1              'Get Starting Point of Field 'Modified by JT 1 --> 0 as field array is starting from 0 and not 1
                '    vLFldStart = InStr(vLFldStart, DataString, Delimiter) + Len(Delimiter)
                'Next vLi
                'vLFldLen = InStr(vLFldStart, DataString, Delimiter) - vLFldStart        'Get Len of Field
                'vLFldNum = fieldid

                DataString = DataString.Trim
                DataString = DataString.Replace(Delimiter & " ", Delimiter)
                'arrFieldlist = New ArrayList(DataString.Split(New Char() {Delimiter}))     'commented by JPT as each character is considered as single char and not actual string
                arrFieldlist = New ArrayList(Split(DataString, Delimiter, , CompareMethod.Text))    'added to use each char as string or char
                If arrFieldlist.Count > fieldid - 1 Then
                    strGetField = arrFieldlist.Item(fieldid - 1)
                Else
                    strGetField = ""
                End If
                arrFieldlist = Nothing
                Return strGetField
            End If

        Else
            'If FieldID DNE in DataString, exit function; else determine field length
            If InStr(1, DataString, fieldid, CompareMethod.Binary) = 0 Then
                Return ""
                Exit Function
            Else
                If ExtractLastSubField Then                     'extract last sub field meeting criteria (v11.0.10.apg)
                    vLFldStart = InStrRev(DataString, fieldid, , CompareMethod.Binary)
                Else                                            'extract last sub field meeting criteria
                    vLFldStart = InStr(1, DataString, fieldid, CompareMethod.Binary)
                End If
                vLFldLen = InStr(vLFldStart, DataString, Delimiter, CompareMethod.Binary) - vLFldStart        'Get Len of Field
                vLFldNum = InStrOccur(1, Left(DataString, vLFldStart), Delimiter) + 1
            End If
        End If

        'Get Field Data --------------------------------------------------------------------------
        If vLFldLen = 0 Then
            Return ""
        ElseIf vLFldNum = TotalNumberFields Then
            strGetField = Mid(DataString, vLFldStart, Len(DataString))
            If Right(strGetField, Len(Delimiter)) = Delimiter Then strGetField = Left(strGetField, Len(strGetField) - 1)
            Return strGetField
        Else
            Return Mid(DataString, vLFldStart, vLFldLen)
        End If

    End Function

    Public Function InStrOccur(ByVal Start As Integer, ByVal String1 As String, ByVal String2 As String) As Integer

        ' This function determines the number of occurances of String2 within String1
        '   Start       Numeric expression that sets the starting position for each search.
        '   String1     String expression being searched.
        '   String2     String expression sought.

        'Declare Function Variables (vF)
        Dim vFInStrLocation As Long              'mod Int->Long, v9.7.6.apg
        Dim strActualString As String = ""
        Dim lInttrOccur As Integer = 0
        Try                                     'add v13.2.3.jpt


            '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            If String2 = "" Then
                Return 0
                Exit Function
            End If

            vFInStrLocation = InStr(Start, String1, String2)
            If vFInStrLocation > 0 Then
                lInttrOccur = 0
                'Modified by JT to use .NET Methods
                strActualString = String1.Substring(Start - 1)
                'lInttrOccur = UBound(strActualString.Split(String2))
                lInttrOccur = UBound(Split(strActualString, String2))
                'Do
                '    InStrOccur = InStrOccur + 1
                '    'Dim length = Len(String2)
                '    Dim startposition As Integer = vFInStrLocation + Len(String2)
                '    vFInStrLocation = InStr(startposition, String1, String2)
                'Loop Until vFInStrLocation = 0      'Zero when String2 no longer found within String1
            Else                                    'If String2 not found within String1
                lInttrOccur = 0

            End If
            Return lInttrOccur

        Catch ex As Exception
            Return 0
        End Try

    End Function
End Module
