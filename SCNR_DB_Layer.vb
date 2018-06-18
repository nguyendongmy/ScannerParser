Imports System.Data.SqlClient

Public Class SCNR_DB_Layer
    Private _DBConnStr As String
    Public Sub New(DBConnectionString As String)
        _DBConnStr = DBConnectionString
    End Sub
    Public Function getSCNRFiles(ByVal lteamname As String) As DataTable
        Dim daSCNRFiles As SqlDataAdapter = Nothing
        Dim dtSCNRFilesTable As DataTable = Nothing
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdGetFilesForSCNR = New SqlCommand
                    cmdGetFilesForSCNR.Connection = gappsqlconnection
                    cmdGetFilesForSCNR.CommandText = " SELECT file_name,file_name_only,id_server_file,b.id_server_transaction, " & _
                    " isnull(status_flag,0) status_flag,isnull(file_status,0) file_status, " & _
                    " date_collected,isnull(isScanner,0) isScanner ,isnull(is_finished,0) is_finished, date_finished, isnull (isASideOrphan,0) isASideOrphan, " & _
                    " bsidedevicetype,b.campaign,b.market_name,b.project,b.username,b.transaction_status,a.time_scnr FROM tblFTPFile a, tblftptransaction b " & _
                    " WHERE isscanner = 1 and file_status in (3,4) and isnull(a.file_stor_state,0) <> 6 and a.time_scnr is null and b.time_scnr is null and " & _
                    " not (isnull(status_flag,0) > 0) and a.id_server_transaction = b.id_server_transaction and b.transaction_status <> 99 " & _
                    " /*and b.transaction_status > 0*/ and b.username in (" & lteamname & ") " & _
                    " order by date_finished asc "
                    daSCNRFiles = New SqlDataAdapter
                    dtSCNRFilesTable = New DataTable
                    daSCNRFiles.SelectCommand = cmdGetFilesForSCNR
                    daSCNRFiles.Fill(dtSCNRFilesTable)
                    Return dtSCNRFilesTable
                End Using
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in getSCNRFiles", lteamname, "")
            Return Nothing
        End Try
    End Function

    Public Function getNoScnrTrans(lteamname As String) As DataTable
        Dim daSCNRFiles As SqlDataAdapter = Nothing
        Dim dtSCNRFilesTable As DataTable = Nothing
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdGetFilesForSCNR = New SqlCommand
                    cmdGetFilesForSCNR.Connection = gappsqlconnection
                    cmdGetFilesForSCNR.CommandText = " select count(1) filecnt, total_file_uploaded,a.id_server_transaction  from tblftptransaction a, tblftpfile b " & _
                    " where a.id_server_transaction = b.id_server_transaction " & _
                    " and a.username in (" & lteamname & ") " & _
                    " and (isnull(isscanner,0) = 0 or (isnull(isscanner,0) = 1 and status_flag > 0) or (isnull(isscanner,0) = 1 and b.time_scnr is not null))  " & _
                    " and transaction_status >= 1 and transaction_status <> 99 and a.time_scnr is null  " & _
                    " and exists (select 1 from tblftpfile where id_server_transaction = a.id_server_transaction and (file_name_only like '%.sqz%' or file_name_only like '%.mf%')) " & _
                    " group by total_file_uploaded,a.id_server_transaction having count(1) = total_file_uploaded " & _
                    " order by a.id_server_transaction asc "
                    daSCNRFiles = New SqlDataAdapter
                    dtSCNRFilesTable = New DataTable
                    daSCNRFiles.SelectCommand = cmdGetFilesForSCNR
                    daSCNRFiles.Fill(dtSCNRFilesTable)
                    Return dtSCNRFilesTable
                End Using
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in getSCNRFiles", lteamname, "")
            Return Nothing
        End Try
    End Function

    Public Function getSCNRPair(id_server_transaction As Long, ModuleNumber As String) As DataTable
        Dim daSCNRFiles As SqlDataAdapter = Nothing
        Dim dtSCNRFilesTable As DataTable = Nothing
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdGetFilesForSCNR = New SqlCommand
                    cmdGetFilesForSCNR.Connection = gappsqlconnection
                    cmdGetFilesForSCNR.CommandText = " SELECT file_name,file_name_only,id_server_file,a.id_server_transaction, " & _
                    " isnull(status_flag,0) status_flag,isnull(file_status,0) file_status, " & _
                    " date_collected,isnull(isScanner,0) isScanner ,isnull(is_finished,0) is_finished, date_finished, isnull (isASideOrphan,0) isASideOrphan, " & _
                    " bsidedevicetype  FROM tblFTPFile a " & _
                    " WHERE " & _
                    " id_server_transaction = @id_server_transaction " & _
                    " and isscanner = 1 and file_status = 3 and a.time_scnr is null and " & _
                    " not (isnull(status_flag,0) in (1,2)) " & _
                    " and file_name_only like '%" & ModuleNumber & "%'" & _
                    " order by date_finished asc "
                    daSCNRFiles = New SqlDataAdapter
                    dtSCNRFilesTable = New DataTable
                    cmdGetFilesForSCNR.Parameters.AddWithValue("@id_server_transaction", id_server_transaction)
                    daSCNRFiles.SelectCommand = cmdGetFilesForSCNR
                    daSCNRFiles.Fill(dtSCNRFilesTable)
                    Return dtSCNRFilesTable
                End Using
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in getSCNRPair", id_server_transaction, "")
            Return Nothing
        End Try

    End Function

    Public Function InserttblSCNRParse(scnrparse As tblSCNRParse) As Long
        Dim id_scnr_parse As Long
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdInsertSCNRFiles = New SqlCommand
                    cmdInsertSCNRFiles.Connection = gappsqlconnection
                    cmdInsertSCNRFiles.CommandText = "Insert into tblSCNRParse (id_server_transaction,id_server_file," & _
                        " file_path,scnr_file_name,operator_name,technology,band,date_created,status_flag,isMerged)" & _
                        " OUTPUT inserted.id_scnr_parse " & _
                        " values(@id_server_transaction,@id_server_file,@file_path,@scnr_file_name,@operator_name,@technology,@band,getdate()," & _
                        " @status_flag,@isMerged)"
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@id_server_transaction", scnrparse.id_server_trans)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@id_server_file", scnrparse.id_server_file)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@file_path", scnrparse.file_path)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@scnr_file_name", scnrparse.scnr_file_name)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@operator_name", scnrparse.operator_name)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@technology", scnrparse.tech)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@band", scnrparse.band)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@status_flag", scnrparse.status_flag)
                    cmdInsertSCNRFiles.Parameters.AddWithValue("@isMerged", scnrparse.isMerged)
                    id_scnr_parse = cmdInsertSCNRFiles.ExecuteScalar()
                End Using
            End Using
            Return id_scnr_parse
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in InserttblSCNRParse", scnrparse.id_server_file, "")
            'Trap Error in DB
            Return 0
        End Try
    End Function

    Public Sub Update_hasSIBL3(id_scnr_parse As Long)
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdUpdateSIB3_L3 = New SqlCommand
                    cmdUpdateSIB3_L3.Connection = gappsqlconnection
                    cmdUpdateSIB3_L3.CommandText = "Update [tblSCNRParse] set hasSIBL3_flag = 1 where id_scnr_parse = @id_scnr_parse"
                    cmdUpdateSIB3_L3.Parameters.AddWithValue("@id_scnr_parse", id_scnr_parse)
                    cmdUpdateSIB3_L3.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in Update_hasSIBL3", id_scnr_parse, "")
        End Try
    End Sub

    Public Sub UpdateFTPFile_timescnr(id_server_file As Long)
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdUpdateFTPFile = New SqlCommand
                    cmdUpdateFTPFile.Connection = gappsqlconnection
                    cmdUpdateFTPFile.CommandText = "Update [tblftpfile] set time_scnr = getdate() where id_server_file = @id_server_file"
                    cmdUpdateFTPFile.Parameters.AddWithValue("@id_server_file", id_server_file)
                    cmdUpdateFTPFile.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in UpdateFTPFile", id_server_file, "")
        End Try
    End Sub

    Public Sub UpdateFTPFile_status_flag(id_server_file As Long, status_flag As Integer)
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdUpdateFTPFile = New SqlCommand
                    cmdUpdateFTPFile.Connection = gappsqlconnection
                    cmdUpdateFTPFile.CommandText = "Update [tblftpfile] set status_flag = @status_flag where id_server_file = @id_server_file"
                    cmdUpdateFTPFile.Parameters.AddWithValue("@id_server_file", id_server_file)
                    cmdUpdateFTPFile.Parameters.AddWithValue("@status_flag", status_flag)
                    cmdUpdateFTPFile.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in UpdateFTPFile_status_flag", id_server_file, "")
        End Try
    End Sub

    Public Sub UpdateFTPTrans(id_server_trans As Long)
        Dim iRowsAffected As Integer
        Try
            Using gappsqlconnection = New SqlConnection
                gappsqlconnection.ConnectionString = _DBConnStr
                gappsqlconnection.Open()
                Using cmdUpdateFTPFile = New SqlCommand
                    cmdUpdateFTPFile.Connection = gappsqlconnection
                    cmdUpdateFTPFile.CommandText = " Update tblftptransaction set time_scnr = getdate() where id_server_transaction = @id_server_trans and " & _
                    " transaction_status >= 1 and not exists(select 1 from tblftpfile where id_server_transaction = @id_server_trans1 and isscanner = 1 and time_scnr is null and isnull(status_flag,0) <= 0) "
                    cmdUpdateFTPFile.Parameters.AddWithValue("@id_server_trans", id_server_trans)
                    cmdUpdateFTPFile.Parameters.AddWithValue("@id_server_trans1", id_server_trans)
                    iRowsAffected = cmdUpdateFTPFile.ExecuteNonQuery()
                End Using
                If iRowsAffected > 0 Then
                    Using cmdUpdateFTPFile = New SqlCommand
                        cmdUpdateFTPFile.Connection = gappsqlconnection
                        cmdUpdateFTPFile.CommandText = " Update tblftpfile set time_scnr = getdate() where id_server_transaction = @id_server_trans "
                        cmdUpdateFTPFile.Parameters.AddWithValue("@id_server_trans", id_server_trans)
                        cmdUpdateFTPFile.ExecuteNonQuery()
                    End Using
                End If
            End Using
        Catch ex As Exception
            TrapErrorInDB(ex.Message & "=> Error occured in UpdateFTPTrans", id_server_trans, "")
        End Try
    End Sub

    Public Sub TrapErrorInDB(ByVal ErrorText As String, ByVal ErrTeamName As String, ByVal ErrFileNameWithPath As String)
        Using gappsqlconnection = New SqlConnection
            gappsqlconnection.ConnectionString = _DBConnStr
            gappsqlconnection.Open()
            Using cmdError = New SqlCommand
                cmdError.Connection = gappsqlconnection
                cmdError.CommandText = "Insert into tblError(error_text,username,machine_name,file_cause_error,time_error,app_id) " & _
                    "values(@Error_Text,@UserName,@Machine_Name,@File_Cause_Error,getdate(),17)"
                cmdError.Parameters.AddWithValue("@Error_Text", IIf(ErrorText <> "", ErrorText, DBNull.Value))
                cmdError.Parameters.AddWithValue("@UserName", IIf(ErrTeamName <> "", ErrTeamName, DBNull.Value))
                cmdError.Parameters.AddWithValue("@Machine_Name", My.Computer.Name)
                cmdError.Parameters.AddWithValue("@File_Cause_Error", IIf(ErrFileNameWithPath <> "", ErrFileNameWithPath, DBNull.Value))
                cmdError.ExecuteNonQuery()
            End Using
        End Using
    End Sub

End Class
