Public Class Form1

    Dim s_mnist As New Source_mnist()

    Dim c_space As Combinatorial_space

    Dim m As mnist_img

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        m = New mnist_img()

        c_space = New Combinatorial_space(m, Combinatorial_space.Learning_mode.unsupervised)



    End Sub

    Public Sub mnist_step(show As Boolean)

        s_mnist.get_next()

        m.pic = s_mnist.pic
        m.label = s_mnist.label

        m.Sobel()

        m.make_det_out()

        If show Then

            PictureBox1.Image = m.draw_pic
            PictureBox1.Update()

            PictureBox2.Image = m.draw_gr1
            PictureBox2.Update()


            PictureBox3.Image = m.draw_out
            PictureBox3.Update()

            Label1.Text = s_mnist.label
            Label1.Update()

        End If



    End Sub

    Private Sub draw_pic(m As mnist_img, ByRef pb As PictureBox)

        pb.Image = m.draw_pic
        pb.Update()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        mnist_step(True)
    End Sub

    ' шаг обучения
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        learn_step()

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        For i = 1 To 500

            learn_step()

            Label3.Text = i
            Label3.Update()

        Next
    End Sub

    Public Sub learn_step()

        mnist_step(True)

        c_space.activate_clasters(m.out_bin)
        c_space.add_new_clusters(m.out_bin)

        c_space.modify_clasters_F()

        c_space.consolidate_memory_unsl()

        c_space.internal_time += 1

        Label2.Text = c_space.info()
        Label2.Update()

        PictureBox4.Image = c_space.draw_mem()
        PictureBox4.Update()

    End Sub

    Structure dig_item
        Dim bin As BitArray
        Dim label As Integer
    End Structure
    ' тест 
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

        Dim items As New List(Of dig_item)

        Dim s, s1, s2 As Integer
        Dim c As Double

        Dim max As Double
        Dim max_N As Integer
        Dim min_c As Double = 0.85

        Dim p_label As Integer
        Dim N_yes, N_no As Integer

        Dim answers(s_mnist.N_examples) As Boolean

        For i = 1 To s_mnist.N_examples

            mnist_step(False)


            max = 0
            max_N = 0

            If items.Count > 0 Then

                For j = 0 To items.Count - 1

                    mult(m.out_bin, items(j).bin, c)

                    If c > max Then
                        max = c
                        max_N = j
                    End If

                Next

                p_label = items(max_N).label

                If p_label = m.label Then
                    N_yes += 1
                    answers(i) = True
                Else
                    N_no += 1
                    PictureBox1.Image = m.draw_pic
                    PictureBox1.Update()
                    Label1.Text = p_label
                    Label1.Update()
                End If

                s = 0
                If i > 100 Then
                    For j = i - 99 To i
                        If answers(j) Then s += 1
                    Next
                End If

                s1 = 0
                If i > 1000 Then
                    For j = i - 999 To i
                        If answers(j) Then s1 += 1
                    Next
                End If

                Label2.Text = i & " y=" & N_yes & " n=" & N_no & " items.Count" & items.Count & " last 100 yes=" & s & " last 1000 yes=" & s1
                Label2.Update()


            End If

            If max < min_c Then

                items.Add(New dig_item With {.bin = m.out_bin.Clone, .label = m.label})

            End If

        Next



    End Sub

    ' тест с бустингом
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        Dim items As New List(Of dig_item)

        Dim s, s1, s2 As Integer
        Dim c As Double

        Dim max, max1 As Double
        Dim max_N As Integer
        Dim min_c As Double = 0.75

        Dim p_label As Integer
        Dim N_yes, N_no As Integer

        Dim N_Top As Integer = 10
        Dim vote(9) As Double
        Dim m_res As New List(Of Double)
        Dim m_lbl As New List(Of Integer)

        Dim m_res_array() As Double
        Dim m_lbl_array() As Integer

        Dim answers(s_mnist.N_examples) As Boolean

        For i = 1 To s_mnist.N_examples

            mnist_step(False)


            max = 0
            max_N = 0

            If items.Count > 0 Then

                m_res.Clear()
                m_lbl.Clear()

                For j = 0 To items.Count - 1

                    mult(m.out_bin, items(j).bin, c)

                    If c > max Then
                        max = c
                        max_N = j
                    End If

                    If c > 0.7 Then

                        m_res.Add(c)
                        m_lbl.Add(items(j).label)

                    End If

                Next

                m_res_array = m_res.ToArray
                m_lbl_array = m_lbl.ToArray

                Array.Sort(m_res_array, m_lbl_array)

                If m_lbl_array.Length >= 1 Then

                    Array.Clear(vote, 0, 10)

                    For j = 1 To Math.Min(N_Top, m_lbl_array.Length)

                        vote(m_lbl_array(m_res_array.Length - j)) += m_res_array(m_res_array.Length - j)

                    Next

                    max1 = 0

                    max_N = -1

                    For j = 0 To 9
                        If vote(j) > max1 Then
                            max1 = vote(j)
                            max_N = j
                        End If
                    Next


                    p_label = max_N

                    If p_label = m.label Then
                        N_yes += 1
                        answers(i) = True
                    Else
                        N_no += 1
                        PictureBox1.Image = m.draw_pic
                        PictureBox1.Update()
                        Label1.Text = p_label
                        Label1.Update()
                    End If

                Else
                    N_no += 1
                End If





                s = 0
                If i > 100 Then
                    For j = i - 99 To i
                        If answers(j) Then s += 1
                    Next
                End If


                If i Mod 10 = 0 Then

                    s1 = 0
                    If i > 1000 Then
                        For j = i - 999 To i
                            If answers(j) Then s1 += 1
                        Next
                    End If

                End If

                Label2.Text = i & " y=" & N_yes & " n=" & N_no & " items.Count" & items.Count & " last 100 yes=" & s & " last 1000 yes=" & s1
                Label2.Update()


            End If

            If max < min_c Then

                items.Add(New dig_item With {.bin = m.out_bin.Clone, .label = m.label})

            End If

        Next

    End Sub

    Private Sub mult(ByRef bin1 As BitArray, ByRef bin2 As BitArray, ByRef c As Double)

        Dim s, s1, s2 As Integer

        For i = 0 To bin1.Length - 1

            s1 -= bin1(i)
            s2 -= bin2(i)

            s += bin1(i) * bin2(i)

        Next

        If s1 > 0 And s2 > 0 Then
            c = s / Math.Sqrt(s1 * s2)
        Else
            c = 0
        End If



    End Sub


End Class
