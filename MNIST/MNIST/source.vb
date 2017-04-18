Public Class Source_mnist



    Dim Pos As Integer ' текущая позиция

    Public N_examples As Integer = 30000
    Dim p_size = 28
    Dim pics()(,) As Byte
    Dim labels() As Integer

    Public pic(,) As Byte
    Public label As Integer

    Public Sub New()

        Dim s As Byte()
        s = IO.File.ReadAllBytes("train-images.idx3-ubyte")

        ReDim pics(N_examples - 1)

        Dim p As Integer = 16

        For i = 0 To N_examples - 1

            ReDim pics(i)(p_size - 1, p_size - 1)

            For y = 0 To p_size - 1

                For x = 0 To p_size - 1

                    pics(i)(x, y) = s(p + x + y * p_size)

                Next

            Next

            p += p_size * p_size

        Next

        s = IO.File.ReadAllBytes("train-labels.idx1-ubyte")

        ReDim labels(N_examples - 1)

        p = 8

        For i = 0 To N_examples - 1

            labels(i) = s(p + i)

        Next

        Pos = 0


    End Sub


    Public Sub get_next()

        pic = pics(Pos)

        label = labels(Pos)

        Pos += 1

        If Pos >= N_examples Then
            Pos = 0
        End If

    End Sub


End Class
