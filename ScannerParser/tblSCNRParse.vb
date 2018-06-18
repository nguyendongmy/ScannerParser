Public Class tblSCNRParse
    Private _id_server_trans As Long
    Public Property id_server_trans() As Long
        Get
            Return _id_server_trans
        End Get
        Set(ByVal value As Long)
            _id_server_trans = value
        End Set
    End Property
    Private _id_server_file As Long
    Public Property id_server_file() As Long
        Get
            Return _id_server_file
        End Get
        Set(ByVal value As Long)
            _id_server_file = value
        End Set
    End Property
    Private _file_path As String
    Public Property file_path() As String
        Get
            Return _file_path
        End Get
        Set(ByVal value As String)
            _file_path = value
        End Set
    End Property
    Private _scnr_file_name As String
    Public Property scnr_file_name() As String
        Get
            Return _scnr_file_name
        End Get
        Set(ByVal value As String)
            _scnr_file_name = value
        End Set
    End Property
    Private _operator_name As String
    Public Property operator_name() As String
        Get
            Return _operator_name
        End Get
        Set(ByVal value As String)
            _operator_name = value
        End Set
    End Property
    Private _tech As String
    Public Property tech() As String
        Get
            Return _tech
        End Get
        Set(ByVal value As String)
            _tech = value
        End Set
    End Property
    Private _band As String
    Public Property band() As String
        Get
            Return _band
        End Get
        Set(ByVal value As String)
            _band = value
        End Set
    End Property
    Private _status_flag As String
    Public Property status_flag() As String
        Get
            Return _status_flag
        End Get
        Set(ByVal value As String)
            _status_flag = value
        End Set
    End Property
    Private _error_id As String
    Public Property error_id() As String
        Get
            Return _error_id
        End Get
        Set(ByVal value As String)
            _error_id = value
        End Set
    End Property
    Private _hasSIB3_flag As String
    Public Property hasSIB3_flag() As String
        Get
            Return _hasSIB3_flag
        End Get
        Set(ByVal value As String)
            _hasSIB3_flag = value
        End Set
    End Property
    Private _time_SCNRLoad As String
    Public Property time_SCNRLoad() As String
        Get
            Return _time_SCNRLoad
        End Get
        Set(ByVal value As String)
            _time_SCNRLoad = value
        End Set
    End Property
    Private _Team As String
    Public Property Team() As String
        Get
            Return _Team
        End Get
        Set(ByVal value As String)
            _Team = value
        End Set
    End Property
    Private _isMerged As Integer
    Public Property isMerged() As Integer
        Get
            Return _isMerged
        End Get
        Set(ByVal value As Integer)
            _isMerged = value
        End Set
    End Property

End Class
