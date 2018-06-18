Imports System.IO
Public Class SendScnrSpectrum
    Public Sub Move3030Files(lst3030Files As List(Of String))
        lst3030Files = (From f In lst3030Files
                        Where f.ToUpper.Contains("SPECTRUMSCAN") AndAlso f.Contains("3030")).ToList()
        If lst3030Files.Count = 0 Then Exit Sub
        Dim TotFileSize = CalcFilesSize(lst3030Files)
        Dim TotRepoSize = GetFolderSize(ScannerCommon.server3030Path)
        Dim SizeToDel As Long
        Dim deletedSize As Long
        If (TotFileSize + TotRepoSize) > ScannerCommon.Max_Size_SpectrumScan_Repository Then
            SizeToDel = (TotFileSize + TotRepoSize) - (ScannerCommon.Max_Size_SpectrumScan_Repository)
        End If
        If SizeToDel > 0 Then
            'Dim RepoFiles = Directory.GetFiles(ScannerCommon.server3030Path, "*-3030-*SPECTRUMSCAN*")
            Dim RepoFiles = Directory.GetFiles(ScannerCommon.server3030Path, "*SPECTRUMSCAN*")
            'Sort with ASC order
            RepoFiles = (From f In RepoFiles Select f Order By File.GetLastWriteTime(f) Ascending).ToArray()
            For Each f In RepoFiles
                If deletedSize < SizeToDel Then
                    deletedSize += My.Computer.FileSystem.GetFileInfo(f).Length
                    File.Delete(f)
                Else
                    Exit For
                End If
            Next
        End If
        'Copy Files now
        For Each f In lst3030Files
            File.Copy(f, ScannerCommon.server3030Path & Path.GetFileName(f), True)
        Next
    End Sub

    Public Sub Move3040Files(lst3040Files As List(Of String))
        lst3040Files = (From f In lst3040Files
                        Where f.ToUpper.Contains("SPECTRUMSCAN") AndAlso f.Contains("3040")).ToList()
        If lst3040Files.Count = 0 Then Exit Sub
        Dim TotFileSize = CalcFilesSize(lst3040Files)
        Dim TotRepoSize = GetFolderSize(ScannerCommon.server3040Path)
        Dim SizeToDel As Long
        Dim deletedSize As Long
        If (TotFileSize + TotRepoSize) > ScannerCommon.Max_Size_SpectrumSxms_Repository Then
            SizeToDel = (TotFileSize + TotRepoSize) - (ScannerCommon.Max_Size_SpectrumSxms_Repository)
        End If
        If SizeToDel > 0 Then
            'Dim RepoFiles = Directory.GetFiles(ScannerCommon.server3040Path, "*-3040-*SPECTRUMSCAN*")
            Dim RepoFiles = Directory.GetFiles(ScannerCommon.server3040Path, "*SPECTRUMSCAN*")
            'Sort with ASC order
            RepoFiles = (From f In RepoFiles Select f Order By File.GetLastWriteTime(f) Ascending).ToArray()
            For Each f In RepoFiles
                If deletedSize < SizeToDel Then
                    deletedSize += My.Computer.FileSystem.GetFileInfo(f).Length
                    File.Delete(f)
                Else
                    Exit For
                End If
            Next
        End If
        'Copy Files now
        For Each f In lst3040Files
            File.Copy(f, ScannerCommon.server3040Path & Path.GetFileName(f), True)
        Next
    End Sub

    Private Function CalcFilesSize(lstFiles As List(Of String)) As Long
        Dim tsize As Double
        For Each myfile In lstFiles
            If File.Exists(myfile) Then
                tsize += My.Computer.FileSystem.GetFileInfo(myfile).Length
            End If
        Next
        Return tsize
    End Function

    Function GetFolderSize(ByVal DirPath As String,
   Optional IncludeSubFolders As Boolean = True) As Long

        Dim lngDirSize As Long
        Dim objFileInfo As FileInfo
        Dim objDir As DirectoryInfo = New DirectoryInfo(DirPath)
        Dim objSubFolder As DirectoryInfo

        Try

            'add length of each file
            For Each objFileInfo In objDir.GetFiles()
                lngDirSize += objFileInfo.Length
            Next

            'call recursively to get sub folders
            'if you don't want this set optional
            'parameter to false 
            If IncludeSubFolders Then
                For Each objSubFolder In objDir.GetDirectories()
                    lngDirSize += GetFolderSize(objSubFolder.FullName)
                Next
            End If

        Catch Ex As Exception


        End Try

        Return lngDirSize
    End Function
End Class
