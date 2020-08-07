Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions

Namespace Mailslots

    <SuppressUnmanagedCodeSecurity()>
    <SecurityCritical(SecurityCriticalScope.Everything)>
    <HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort:=True), SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode:=True)>
    Public Class SafeMailslotHandle
        Inherits SafeHandleZeroOrMinusOneIsInvalid

#Region "NATIVE"

        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <DllImport("kernel32.dll")>
        Private Shared Function CloseHandle(handle As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region '/NATIVE

#Region "CTOR"

        Private Sub New()
            MyBase.New(True)
        End Sub

        Public Sub New(preexistingHandle As IntPtr, ownsHandle As Boolean)
            MyBase.New(ownsHandle)
            MyBase.SetHandle(preexistingHandle)
        End Sub

#End Region '/CTOR

        Protected Overrides Function ReleaseHandle() As Boolean
            Return SafeMailslotHandle.CloseHandle(Me.handle)
        End Function

    End Class

End Namespace