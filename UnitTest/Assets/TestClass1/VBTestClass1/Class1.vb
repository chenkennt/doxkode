''' <summary>
''' This is summary from vb class...
''' </summary>
Public Class Class1

    Private _value As Integer

    ''' <summary>
    ''' This is a *Value* type
    ''' </summary>
    Public ValueClass As Class1

    ''' <summary>
    ''' What is **Sub**?
    ''' </summary>
    Public Sub New()
        _value = 2
    End Sub

    ''' <summary>
    ''' This is a *Function*
    ''' </summary>
    ''' <param name="name">Name as the **String**
    ''' value</param>
    ''' <returns>**Returns**
    ''' Ahooo</returns>
    Public Function Value(ByVal name As String) As Integer
        Return _value * 2
    End Function
End Class
