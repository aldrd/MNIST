Public Class Info_Source_mnist



    Dim Pos As Integer ' текущая позиция

    Dim N_ex As Integer = 60000
    Dim p_size = 28 * 28
    Dim pics()() As Byte
    Dim labels() As Integer

    Public pic() As Byte
    Public label As Integer

    Public Sub New(N As Integer)

        Dim s As Byte()
        s = IO.File.ReadAllBytes("train-images.idx3-ubyte")

        ReDim pics(N_ex - 1)

        Dim p As Integer = 16

        For i = 0 To N_ex - 1

            ReDim pics(i)(p_size - 1)

            For j = 0 To p_size - 1

                pics(i)(j) = s(p + j)

            Next

            p += p_size

        Next

        s = IO.File.ReadAllBytes("train-labels.idx1-ubyte")

        ReDim labels(N_ex - 1)

        p = 8

        For i = 0 To N_ex - 1

            labels(i) = s(p + i)

        Next

        Pos = 0


    End Sub


    Public Sub get_next()

        pic = pics(Pos)

        label = labels(Pos)

        Pos += 1

        If Pos >= N_ex Then
            Pos = 0
        End If

    End Sub


End Class