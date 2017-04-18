Public Class mnist_img

    Structure L
        Dim Lx As Integer
        Dim Ly As Integer
        Dim alfa As Double ' -pi...pi
        Dim A As Double
    End Structure

    Structure det
        Dim x_source As Integer
        Dim y_source As Integer
        Dim alfa As Double
        Dim A As Double
        Dim out As Boolean
    End Structure

    Dim p_size As Integer = 28

    Public pic(,) As Byte

    Public label As Integer

    Public pic_gr1(,) As L

    Public N_alfa_size As Integer = 4
    Public N_alfa As Integer = N_alfa_size * N_alfa_size
    Public out_size As Integer = p_size * N_alfa_size
    Public R_det As Double = 2.0
    Public max_A As Double = 1200
    Public out_set(out_size - 1, out_size - 1) As det
    Public out_bin As New BitArray(out_size * out_size)

    Dim out_range_alfa As Double = 0.3 ' от единицы
    Dim out_range_A As Double = 0.3


    Dim N_colors As Integer = 100
    Dim spec(N_colors - 1) As Color



    Public Sub New()

        Dim a As Double = (My.Resources.Resource1.spectrum.Width - 1) / N_colors

        For i = 0 To N_colors - 1

            spec(i) = My.Resources.Resource1.spectrum.GetPixel(Int(a * i), 0)

        Next


        ' Создание выходных детекторов

        Dim fx, fy, k As Integer

        For x = 0 To p_size - 1

            For y = 0 To p_size - 1

                For x1 = 0 To N_alfa_size - 1
                    For y1 = 0 To N_alfa_size - 1

                        fx = x * N_alfa_size + x1
                        fy = y * N_alfa_size + y1

                        k = x1 + y1 * N_alfa_size

                        With out_set(fx, fy)

                            .x_source = x
                            .y_source = y
                            .alfa = ((k / N_alfa) * 2 - 1) * Math.PI
                            .A = max_A * 0.7

                        End With

                    Next
                Next

            Next
        Next

    End Sub



    Public Function i_det(x As Integer, y As Integer) As Integer
        i_det = y * out_size + x
    End Function

    Public Sub Sobel()

        Dim Sx(,) As Integer = {{-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1}}
        Dim Sy(,) As Integer = {{1, 2, 1}, {0, 0, 0}, {-1, -2, -1}}

        Dim s1, s2 As Integer

        ReDim pic_gr1(p_size - 1, p_size - 1)

        For x = 1 To p_size - 2
            For y = 1 To p_size - 2


                s1 = 0
                s2 = 0

                For x1 = 0 To 2
                    For y1 = 0 To 2

                        s1 += pic(x + x1 - 1, y + y1 - 1) * Sx(x1, y1)
                        s2 += pic(x + x1 - 1, y + y1 - 1) * Sy(x1, y1)

                    Next
                Next


                With pic_gr1(x, y)

                    .Lx = s1
                    .Ly = s2
                    .A = Math.Sqrt(.Lx * .Lx + .Ly * .Ly)

                    If .A > 0 Then
                        .alfa = Math.Atan2(.Lx, .Ly)
                    Else
                        .alfa = 0
                    End If


                End With

            Next
        Next

    End Sub

    Public Sub make_det_out()

        Dim R As Integer = Int(R_det + 1)
        Dim x, y As Integer

        Dim b As Double

        For fx = 0 To out_size - 1
            For fy = 0 To out_size - 1

                With out_set(fx, fy)

                    For x1 = -R To R
                        For y1 = -R To R

                            x = .x_source + x1
                            y = .y_source + y1

                            If x >= 0 And x < p_size And y >= 0 And y < p_size Then

                                If (x1 * x1 + y1 * y1) <= R_det * R_det Then

                                    If Math.Abs(.A - pic_gr1(x, y).A) < max_A * out_range_A Then

                                        b = Math.Abs(.alfa - pic_gr1(x, y).alfa)
                                        b = Math.Min(b, 2 * Math.PI - b)

                                        If b / Math.PI < out_range_alfa Then

                                            .out = True

                                            GoTo m1

                                        End If

                                    End If

                                End If

                            End If


                        Next
                    Next

                    .out = False

m1:

                    out_bin(fx + fy * out_size) = .out

                End With

            Next

        Next
    End Sub

    Public Function draw_pic() As Bitmap

        draw_pic = New Bitmap(p_size, p_size)
        Dim Y As Integer

        For i = 0 To p_size - 1
            For j = 0 To p_size - 1

                Y = pic(i, j)

                draw_pic.SetPixel(i, j, Color.FromArgb(Y, Y, Y))

            Next
        Next
    End Function

    Public Function draw_gr1() As Bitmap

        draw_gr1 = New Bitmap(p_size, p_size)

        Dim Y As Integer
        Dim k As Double
        Dim c As Color

        For i = 0 To p_size - 1
            For j = 0 To p_size - 1

                Y = pic_gr1(i, j).A

                k = Y / max_A
                c = spec((pic_gr1(i, j).alfa / Math.PI + 1) * (N_colors - 1) / 2)
                draw_gr1.SetPixel(i, j, Color.FromArgb(c.R * k, c.G * k, c.B * k))

            Next
        Next
    End Function

    Public Function draw_out() As Bitmap

        draw_out = New Bitmap(p_size * N_alfa_size, p_size * N_alfa_size)

        For x = 0 To out_size - 1
            For y = 0 To out_size - 1


                If out_set(x, y).out Then
                    draw_out.SetPixel(x, y, Color.White)
                Else
                    draw_out.SetPixel(x, y, Color.Black)
                End If


            Next
        Next
    End Function

End Class
