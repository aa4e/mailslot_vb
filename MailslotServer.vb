Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text

Namespace Mailslots

    ''' <summary>
    ''' Сервер Mailslot.
    ''' </summary>
    <SuppressUnmanagedCodeSecurity()>
    Public Class MailslotServer
        Implements IDisposable

#Region "NATIVE"

        <DllImport("kernel32.dll")>
        Private Shared Function CreateMailslot(mailslotName As String, nMaxMessageSize As UInteger, lReadTimeout As Integer, securityAttributes As IntPtr) As SafeMailslotHandle
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function GetMailslotInfo(hMailslot As SafeMailslotHandle, lpMaxMessageSize As IntPtr, <Out()> ByRef lpNextSize As Integer, <Out()> ByRef lpMessageCount As Integer, lpReadTimeout As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function ReadFile(handle As SafeMailslotHandle, bytes As Byte(), numBytesToRead As Integer, <Out()> ByRef numBytesRead As Integer, overlapped As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region '/NATIVE

        ''' <summary>
        ''' Число ожидающих сообщений. Обновляется только после вызова <see cref="GetNextMessage()"/>.
        ''' </summary>
        Public ReadOnly Property NumberMessages As Integer
            Get
                Return _NumberMessages
            End Get
        End Property
        Private _NumberMessages As Integer = 0

#Region "CTOR"

        Private Handle As SafeMailslotHandle

        Public Sub New(name As String)
            Handle = CreateMailslot($"\\.\mailslot\{name}", 0, 0, IntPtr.Zero)
            If Handle.IsInvalid Then
                Throw New Win32Exception("Ошибка создания Mailslot.CreateMailslot()")
            End If
        End Sub

        Private Sub Dispose() Implements IDisposable.Dispose
            If (Handle IsNot Nothing) Then
                Handle.Close()
                Handle = Nothing
            End If
        End Sub

#End Region '/CTOR

#Region "METHODS"

        Private Const NO_MESSAGE As Integer = -1

        ''' <summary>
        ''' Получает следующее сообщение.
        ''' </summary>
        Public Function GetNextMessage() As String
            Dim buffer As Byte() = GetNextMessageBytes()
            Dim result As String = Encoding.Unicode.GetString(buffer)
            Return result
        End Function

        ''' <summary>
        ''' Возвращает массив байтов следующего сообщения.
        ''' </summary>
        Public Function GetNextMessageBytes() As Byte()
            Dim messageBytes As Integer
            If (Not GetMailslotInfo(Handle, IntPtr.Zero, messageBytes, _NumberMessages, IntPtr.Zero)) Then
                Throw New Win32Exception("Ошибка в Mailslot.GetMailslotInfo()")
            End If
            If (messageBytes = NO_MESSAGE) Then
                Return {}
            End If
            Dim buffer As Byte() = New Byte(messageBytes - 1) {}
            Dim bytesRead As Integer
            Dim succeeded As Boolean = ReadFile(Handle, buffer, messageBytes, bytesRead, IntPtr.Zero)
            If (Not succeeded) OrElse (bytesRead = 0) Then
                Throw New Win32Exception("Ошибка в Mailslot.ReadFile()")
            End If
            Return buffer
        End Function

#End Region '/METHODS

    End Class '/MailslotServer

End Namespace