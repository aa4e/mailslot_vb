Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text

Namespace Mailslots

    ''' <summary>
    ''' Клиент Mailslot.
    ''' </summary>
    <SuppressUnmanagedCodeSecurity()>
    Public Class MailslotClient
        Implements IDisposable

        Private Handle As SafeMailslotHandle
        Private ReadOnly Name As String
        Private ReadOnly Machine As String

#Region "ENUMS"

        <Flags()>
        Public Enum FileDesiredAccess As UInteger
            GenericRead = &H80000000UI
            GenericWrite = &H40000000UI
            GenericExecute = &H20000000UI
            GenericAll = &H10000000UI
        End Enum

        <Flags()>
        Public Enum FileShareMode As UInteger
            Zero = 0UI
            FileShareRead = 1UI
            FileShareWrite = 2UI
            FileShareDelete = 4UI
        End Enum

        Public Enum FileCreationDisposition As UInteger
            CreateNew = 1UI
            CreateAlways
            OpenExisting
            OpenAlways
            TruncateExisting
        End Enum

#End Region '/ENUMS

#Region "NATIVE"

        <DllImport("kernel32.dll")>
        Private Shared Function CreateFile(fileName As String, desiredAccess As FileDesiredAccess, shareMode As FileShareMode, securityAttributes As IntPtr, creationDisposition As FileCreationDisposition, flagsAndAttributes As Integer, hTemplateFile As IntPtr) As SafeMailslotHandle
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function WriteFile(handle As SafeMailslotHandle, bytes As Byte(), numBytesToWrite As Integer, <Out()> ByRef numBytesWritten As Integer, overlapped As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region '/NATIVE

#Region "CTOR"

        Public Sub New(name As String)
            Me.New(name, ".")
        End Sub

        Public Sub New(name As String, machine As String)
            Me.Name = name
            Me.Machine = machine
        End Sub

        Private Sub CreateHandle()
            Dim fileName As String = $"\\{Machine}\MAILSLOT\{Name}"
            Handle = CreateFile(fileName, FileDesiredAccess.GenericWrite, FileShareMode.FileShareRead, IntPtr.Zero, FileCreationDisposition.OpenExisting, 0, IntPtr.Zero)
            If Handle.IsInvalid Then
                Throw New Win32Exception("Ошибка создания указателя CreateHandle()")
            End If
        End Sub

        Protected Sub Dispose() Implements IDisposable.Dispose
            If (Handle IsNot Nothing) Then
                Handle.Close()
                Handle = Nothing
            End If
        End Sub

#End Region '/CTOR

#Region "METHODS"

        ''' <summary>
        ''' Отправляет сообщение.
        ''' </summary>
        ''' <param name="msg">Текст сообщения.</param>
        Public Overridable Sub SendMessage(msg As String)
            Dim message As Byte() = Encoding.UTF8.GetBytes(msg)
            SendMessage(message)
        End Sub

        Private ReadOnly SyncObj As New Object()

        ''' <summary>
        ''' Отправляет сообщение.
        ''' </summary>
        ''' <param name="msg">Текст сообщения.</param>
        Protected Sub SendMessage(msg As Byte())
            SyncLock SyncObj
                If (Handle Is Nothing) Then
                    CreateHandle()
                End If
                Dim bytesWritten As Integer
                Dim succeeded As Boolean = WriteFile(Handle, msg, msg.Length, bytesWritten, IntPtr.Zero)
                If (Not succeeded) OrElse (msg.Length <> bytesWritten) Then
                    Me.Dispose()
                    Throw New Win32Exception("Ошибка отправки сообщения SendMessage()")
                End If
            End SyncLock
        End Sub

#End Region '/METHODS

    End Class '/MailslotClient

End Namespace