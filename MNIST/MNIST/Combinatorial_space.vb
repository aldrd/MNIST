﻿
Public Class Combinatorial_space

    Public Enum Learning_mode
        unsupervised
        supervised
    End Enum


    Public internal_time As Integer

    Public points_size As Integer
    Public N_points As Integer

    Public N_bits_in_point As Integer
    Public out_code_size As Integer
    Public N_bits_in_out_code As Integer

    Public learn_type As Learning_mode

    Dim N_fix As Integer ' кол-во точек фиксации на одно воспоминание


    Dim N_bit_main As Integer


    Dim N_1_low_range As Integer

    Dim N_concepts As Integer ' Общее число используемых понятий

    Dim N_conf As Integer ' Требуемое число повторений перед переводом кластера в устойчивую форму
    Dim N_learn_1 As Integer ' Стадии обучения
    Dim N_learn_2 As Integer

    Dim points() As comb_point
    Dim sigma_point As Double = 10 ' разброс бит на входном векторе для формирования точки
    Dim sigma_out As Double = 4

    Dim common_memory As New List(Of BitArray)
    Dim cur_source_bin As BitArray

    Dim NN() As Integer

    Public lvl_stats() As stats

    Public top_lvl As Integer
    Public main_lvl As Integer
    Public part_lvl As Integer
    Public A_lvl_N_p As Integer ' Количество активных точек
    Public A_lvl_N_m As Integer ' Максимальное количество воспоминаний в активной точке


    Friend min_N_sign_p As Integer
    Friend min_N_sign_create As Integer

    Dim max_M_in_point As Integer
    Dim T_consol As Integer

    Public out_code As BitArray
    Public out_code_part As BitArray
    Public out_code_N() As Integer
    Public out_code_N_part() As Integer

    Public profile_A_complit()() As Integer  ' профиль активности выходного бита
    Public profile_A_part()() As Integer  ' 

    Dim out_code_set_of_hashs() As Dictionary(Of Integer, mem_recept_cluster)


    Dim observed_claster As mem_recept_cluster = Nothing


    Friend N_cl_total As Integer

    Public N_perm As Integer
    Public N_perm_ok, N_perm_ex, N_perm_notex, N_perm_ex_ok As Integer

    Public N_cut_ex, N_cut_bad, N_broken, N_good_del As Integer
    Public N_potenc_cut_ex, N_potenc_cut_bad, N_potenc_good, N_potenc_bad, N_potenc_broken, N_potenc_good_del As Integer
    Public N_ok, N_ok_del, N_ok_del_TO, N_ok_del_doub As Integer

    Structure stats
        Dim avg_all As Single
        Dim sigma As Single

        Dim avg_1 As Single

        Dim N As Integer
    End Structure

    Public Sub New(ByRef m As mnist_img, type As Learning_mode)

        internal_time = 0

        N_bit_main = m.out_set.Length


        Select Case type

            Case Learning_mode.unsupervised

                Me.learn_type = Learning_mode.unsupervised
                N_bits_in_point = 100

                out_code_size = 30
                N_bits_in_out_code = out_code_size * out_code_size

                points_size = 100
                N_points = points_size * points_size
                N_conf = 3

                max_M_in_point = 150
                min_N_sign_p = 20
                min_N_sign_create = 40

                N_1_low_range = min_N_sign_p - 1

                T_consol = 1000 * N_conf

                N_learn_1 = 6
                N_learn_2 = 16


            Case Learning_mode.supervised

                Me.learn_type = Learning_mode.supervised
                N_bits_in_point = 32
                N_bits_in_out_code = N_bit_main
                N_points = 60000
                max_M_in_point = 200
                min_N_sign_p = 4
                min_N_sign_create = 6

                N_learn_1 = 6
                N_learn_2 = 16

                'T_consol = N_concepts * N_conf * 4
        End Select


        ReDim points(N_points - 1)

        ReDim NN(0 To N_bits_in_point - 1)

        ReDim lvl_stats(N_bits_in_point - 1)

        out_code = New BitArray(N_bits_in_out_code)
        out_code_part = New BitArray(N_bits_in_out_code)

        ReDim out_code_set_of_hashs(N_bits_in_out_code - 1)

        For i = 0 To N_bits_in_out_code - 1
            out_code_set_of_hashs(i) = New Dictionary(Of Integer, mem_recept_cluster)
        Next

        ReDim out_code_N(N_bits_in_out_code - 1)
        ReDim out_code_N_part(N_bits_in_out_code - 1)

        Dim i_det, i_p, i_out As Integer
        Dim p As Drawing.Point

        Dim x_mapping, y_mapping As Integer

        ' Создаем случайные сочетания с учетом топографичности отображения
        For x = 0 To points_size - 1
            For y = 0 To points_size - 1

                i_p = y * points_size + x

                points(i_p) = New comb_point(x, y, N_bit_main, N_bits_in_point)

                x_mapping = Math.Round(x / points_size * m.out_size)
                y_mapping = Math.Round(y / points_size * m.out_size)

                For j = 1 To N_bits_in_point

                    Do

                        p = coord_gaus(x_mapping, y_mapping, m.out_size, sigma_point)

                        i_det = m.i_det(p.X, p.Y)

                    Loop Until points(i_p).bin(i_det) = False

                    points(i_p).bin(i_det) = True

                Next

            Next

        Next

        ' Создаем структуру хеша для получения выходного кода из кода точек

        For i = 0 To N_points - 1

            points(i).out_bit = Int(Rnd() * N_bits_in_out_code)

        Next

        For x = 0 To points_size - 1
            For y = 0 To points_size - 1

                i_p = y * points_size + x

                x_mapping = Math.Round(x / points_size * out_code_size)
                y_mapping = Math.Round(y / points_size * out_code_size)


                p = coord_gaus(x_mapping, y_mapping, out_code_size, sigma_out)

                i_out = p.X + p.Y * out_code_size

                points(i_p).out_bit = i_out

            Next

        Next
    End Sub

    Private Function coord_gaus(xc As Integer, yc As Integer, size As Integer, sigma As Double) As Drawing.Point

        ' Преобразование Бокса — Мюллера для получения нормально распределенной величины

        Dim s, x, y, dx, dy As Double
        Dim lx, ly As Integer

        Do
            Do
                x = Rnd() * 2 - 1
                y = Rnd() * 2 - 1

                s = x * x + y * y

            Loop Until s < 1 And s > 0

            s = Math.Sqrt(-2 * Math.Log(s) / s)

            dx = x * s
            dy = y * s
            lx = xc + dx * sigma
            ly = yc + dy * sigma


            ' определяем, что не вышли за границы поля 

        Loop Until lx >= 0 And ly >= 0 And lx < size And ly < size

        coord_gaus = New Drawing.Point(lx, ly)

    End Function

    Public Function info() As String

        info = "всего кластеров " & N_cl_total & vbCrLf

        ' info &= "ok (правильный выходной бит) " & N_ok & vbCrLf

        info &= "perm: всего " & N_perm '& " ok " & N_perm_ok & " ok точные " & N_perm_ex_ok & " точные " & N_perm_ex & vbCrLf

        'info &= "точные после cut +-broken (" & N_cut_ex & " , " & N_cut_bad & " , " & N_broken & ")" & vbCrLf


        'info &= "потенциальные после cut +- broken (" & N_potenc_cut_ex & " , " & N_potenc_cut_bad & " , " & N_potenc_broken & ")   потенциальные new +- (" & N_potenc_good & " , " & N_potenc_bad & ")" & vbCrLf


        'info &= "del точно пот.точно  ok (ok_timeout, ok_doub) (" & N_good_del & " , " & N_potenc_good_del & " , " & N_ok_del & " (" & N_ok_del_TO & "," & N_ok_del_doub & ")" & " )" & vbCrLf



    End Function



    ' Выбор случайного кластера
    Friend Sub obs_cl_select(type As status_type, Optional good As Boolean = False)

        If (internal_time > 0 And type = status_type.tmp) Or (N_perm > 0 And type <> status_type.tmp) Then

            observed_claster = Nothing

            Do

                Dim np As Integer = Int(Rnd() * N_points)

                If points(np).memory.Count > 0 Then

                    observed_claster = points(np).memory(Int(Rnd() * points(np).memory.Count))
                    If observed_claster.status = type Then

                        If good Then
                            observed_claster = Nothing
                        End If

                    Else

                        observed_claster = Nothing

                    End If

                End If

            Loop Until observed_claster IsNot Nothing

        End If

    End Sub

    ' Информация о кластере
    Friend Function obs_cl_info(concepts_str_to_bin As Dictionary(Of String, BitArray)) As String

        Dim s As Integer

        obs_cl_info = ""

        If observed_claster IsNot Nothing Then

            With observed_claster

                obs_cl_info = "cross     = " & .bits_set.Count & vbCrLf
                obs_cl_info &= "status    = " & .status.ToString & vbCrLf
                obs_cl_info &= "T         = " & (internal_time - .start_time) & vbCrLf
                obs_cl_info &= "N_act     = " & .N_activate & vbCrLf
                obs_cl_info &= "N_act_part= " & .N_activate_part & vbCrLf
                obs_cl_info &= "N_error   = " & .N_error_complit & vbCrLf & vbCrLf

                For Each ch In concepts_str_to_bin

                    s = 0

                    For Each b In .bits_set
                        s -= ch.Value(b)
                    Next

                    If s > 0 Then
                        obs_cl_info &= ch.Key & vbTab & s & vbCrLf
                    End If


                Next


            End With

        End If

    End Function

    Private Sub del_claster(ByRef cl As mem_recept_cluster, ByRef p As comb_point, reason As String)

        With cl

            N_cl_total -= 1


            If .status <> status_type.tmp Then
                N_perm -= 1
            End If



            out_code_set_of_hashs(p.out_bit).Remove(hc(.bits_set))
            .status = status_type.on_deliting


        End With

    End Sub



    Friend Sub consolidate_memory_sl()

        Dim T As Single


        For Each p In points

            If p.memory.Count > 0 Then


                For i = p.memory.Count - 1 To 0 Step -1

                    With p.memory(i)

                        T = internal_time - .start_time


                        If .N_error_complit / .N_activate > 0.05 Then

                            del_claster(p.memory(i), p, ".N_error")

                            p.memory.RemoveAt(i)

                            Continue For
                        End If

                        If .N_error_part / .N_activate_part > 0.3 Then

                            del_claster(p.memory(i), p, ".N_error_part")

                            p.memory.RemoveAt(i)

                            Continue For
                        End If


                        Select Case .status

                            Case status_type.tmp

                                If .N_learn >= N_learn_1 Then

                                    .status = status_type.permanent_1


                                End If



                            Case status_type.permanent_1

                                If .N_learn >= N_learn_1 Then

                                    .status = status_type.permanent_2
                                    N_perm += 1

                                End If


                        End Select


                    End With

                Next
            End If

        Next

    End Sub

    Friend Sub consolidate_memory_unsl()

        Dim T As Single


        For Each p In points

            If p.memory.Count > 0 Then


                For i = p.memory.Count - 1 To 0 Step -1

                    With p.memory(i)

                        T = internal_time - .start_time








                        Select Case .status

                            Case status_type.tmp

                                If .N_learn >= N_learn_1 Then

                                    .status = status_type.permanent_1


                                End If



                            Case status_type.permanent_1

                                If .N_learn >= N_learn_2 Then

                                    .status = status_type.permanent_2
                                    N_perm += 1

                                End If


                        End Select


                    End With

                Next
            End If

        Next

    End Sub


    Friend Sub modify_clasters_F()

        Dim F As Single()
        'Dim g, pg As Boolean

        Dim h As String

        For Each p In points


            For i = p.memory.Count - 1 To 0 Step -1

                With p.memory(i)

                    ' Подрезание кластера
                    If .status <> status_type.permanent_2 AndAlso .N_learn = N_learn_1 Or .N_learn = N_learn_2 Then

                        F = .F_main_iter()


                        Dim New_bits As New List(Of Integer)
                        Dim new_W As New List(Of Integer)
                        Dim new_N As New List(Of Integer)

                        For j = 0 To .bits_set.Count - 1

                            If F(j) > 0.75 Then
                                New_bits.Add(.bits_set(j))
                                new_W.Add(.W(j))
                                new_N.Add(j)
                            End If

                        Next


                        If New_bits.Count <> .bits_set.Count Then


                            If New_bits.Count >= min_N_sign_p Then


                                h = hc(New_bits)

                                If Not out_code_set_of_hashs(p.out_bit).ContainsKey(h) Then

                                    out_code_set_of_hashs(p.out_bit).Remove(hc(.bits_set))
                                    .bits_set = New_bits.ToArray
                                    .W = new_W.ToArray


                                    out_code_set_of_hashs(p.out_bit).Add(h, p.memory(i))


                                Else

                                    del_claster(p.memory(i), p, "duplicate after cut")

                                    p.memory.RemoveAt(i)

                                    Continue For

                                End If

                            Else

                                del_claster(p.memory(i), p, "short after cut")

                                p.memory.RemoveAt(i)

                                Continue For

                            End If

                        End If

                    End If

                End With

            Next


        Next
    End Sub



    Friend Sub activate_clasters(ByRef source_bin As BitArray, Optional ByRef target_bin As BitArray = Nothing)


        cur_source_bin = source_bin.Clone

        common_memory.Add(cur_source_bin)


        Dim s1, s2 As Integer

        For Each p In points

            s1 = 0
            s2 = 0

            For Each m In p.memory

                m.A_complit = False
                m.A_part = False


                m.set_P(source_bin)

                If m.P = m.bits_set.Count Then

                    m.A_complit = True

                    m.N_activate += 1

                    If target_bin IsNot Nothing AndAlso Not target_bin(p.out_bit) Then
                        m.N_error_complit += 1
                    End If


                    If m.status = status_type.permanent_2 Then

                        's1 += 2 ^ (m.P - min_N_sign_p)
                        s1 += (m.P - min_N_sign_p) + 1

                    End If

                End If


                If m.P >= min_N_sign_p Then

                    m.A_part = True

                    m.N_activate_part += 1

                    Dim s As String = ""

                    If target_bin IsNot Nothing AndAlso Not target_bin(p.out_bit) Then
                        m.N_error_part += 1
                        s = "- "
                    Else
                        s = "+ "

                    End If



                    If m.status <> status_type.permanent_2 Then

                            m.history_part_A_bin.Add(cur_source_bin)

                            m.N_learn += 1

                    End If



                    's2 += 2 ^ (m.P - min_N_sign_p)
                    s2 += (m.P - min_N_sign_p) + 1


                    For Each b In m.bits_set
                        If source_bin(b) Then
                            s &= "1"
                        Else
                            s &= "0"
                        End If
                    Next

                    m.history_part_A.Add(s)

                End If

            Next

            p.P = s1
            p.P_part = s2


        Next

    End Sub



    Friend Sub make_out_code_lvl(Optional ByVal lvl As Integer = 1)

        Array.Clear(out_code_N, 0, N_bits_in_out_code)

        For Each p In points

            out_code_N(p.out_bit) += p.P

        Next

        For i = 0 To N_bits_in_out_code - 1

            out_code(i) = (out_code_N(i) >= lvl)

        Next


    End Sub

    Friend Sub make_out_code_A()

        out_code.SetAll(False)

        For Each p In points

            If p.A Then

                out_code(p.out_bit) = True

            End If

        Next

    End Sub

    Friend Sub make_out_code_unsl(ByRef bin As BitArray)

        'Array.Clear(out_code_N, 0, N_bits_in_out_code)

        make_profile(bin)

        find_main_lvl()
        make_points_A(min_N_sign_p, main_lvl)

        make_out_code_A()

    End Sub


    ' Вычисляем количество попаданий в точках
    Public Sub calc_cross(ByRef bin As BitArray)


        Array.Clear(NN, 0, N_bits_in_point)

        Dim s As Integer

        For Each p In points

            s = 0

            For j = 0 To N_bit_main - 1
                s += bin(j) * p.bin(j)
            Next

            p.cross = s

            NN(p.cross) += 1

        Next

    End Sub


    Public Sub make_profile(ByRef bin As BitArray)

        part_lvl = 0

        calc_cross(bin)

        top_lvl = 0

        For Each p In points

            Array.Clear(p.profile_A_complit, 0, N_bits_in_point)
            Array.Clear(p.profile_A_part, 0, N_bits_in_point)

            If p.cross >= min_N_sign_p Then

                ' создание профилей активности точки
                For Each m In p.memory

                    m.set_P(bin)

                    If m.P = m.bits_set.Length Then
                        p.profile_A_complit(m.P) += 1
                    End If

                    If m.P >= min_N_sign_p Then

                        p.profile_A_part(m.P) += m.N_activate_part

                        part_lvl += m.N_activate_part

                    End If

                Next


                ' накопление
                For i = N_bits_in_point - 2 To 0 Step -1

                    If i > top_lvl AndAlso p.profile_A_complit(i) > 0 Then
                        top_lvl = i
                    End If

                    p.profile_A_complit(i) += p.profile_A_complit(i + 1)
                    p.profile_A_part(i) += p.profile_A_part(i + 1)

                Next

            End If

        Next

    End Sub


    Private Function hc(ByRef s As List(Of Integer)) As Integer

        Dim str As String = ""
        For Each n In s
            str &= "." & n
        Next

        hc = str.GetHashCode

    End Function

    Private Function hc(ByRef s() As Integer) As Integer

        Dim str As String = ""
        For Each n In s
            str &= "." & n
        Next

        hc = str.GetHashCode

    End Function


    Public Sub add_new_clusters(ByRef source_bin As BitArray, Optional ByRef target_bin As BitArray = Nothing)

        calc_cross(source_bin)

        Dim h As Integer

        For Each p In points

            If p.cross >= min_N_sign_create AndAlso
                (learn_type = Learning_mode.unsupervised OrElse (target_bin IsNot Nothing AndAlso target_bin(p.out_bit))) Then

                Dim m = New mem_recept_cluster(source_bin, p.bin, N_learn_1)

                m.start_time = internal_time

                h = hc(m.bits_set)

                If Not out_code_set_of_hashs(p.out_bit).ContainsKey(h) And p.memory.Count <= max_M_in_point Then

                    out_code_set_of_hashs(p.out_bit).Add(h, m)
                    p.memory.Add(m)

                    N_cl_total += 1

                End If

            End If

        Next


    End Sub



    Public Sub find_main_lvl()

        main_lvl = min_N_sign_p

        For l = top_lvl To 0 Step -1
            make_points_A(l, 1)
            If A_lvl_N_p >= min_N_sign_p Then
                main_lvl = l
                Exit For
            End If
        Next


    End Sub

    ' Активация по превышению заданного порога
    ' L_lvl - уровень точности (кол-во совпавших бит)
    ' L_N - уровень количества (кол-во воспоминаний)
    Public Sub make_points_A(L_lvl As Integer, L_N As Integer)


        For Each p In points

            'If p.profile_A_complit(L_lvl) >= L_N Then
            If p.profile_A_part(L_lvl) >= L_N Then ' активация по частичному совпадению

                p.A = True

            Else
                p.A = False
            End If

        Next

    End Sub

    Friend Function draw_points_A() As Bitmap

        Dim N As Integer = Math.Sqrt(N_points) + 1
        Dim k As Integer

        draw_points_A = New Bitmap(N, N)

        For i = 0 To N - 1
            For j = 0 To N - 1

                k = i + j * N

                If k < N_points AndAlso points(k).A Then
                    draw_points_A.SetPixel(i, j, Color.White)
                Else
                    draw_points_A.SetPixel(i, j, Color.Black)
                End If

            Next
        Next

    End Function



    Friend Function draw_mem() As Bitmap

        Dim S As Integer

        draw_mem = New Bitmap(points_size, points_size)

        For x = 0 To points_size - 1
            For y = 0 To points_size - 1

                S = points(x + y * points_size).memory.Count

                S = S * 255 / max_M_in_point * 2

                If S > 255 Then S = 255

                draw_mem.SetPixel(x, y, Color.FromArgb(S, S, S))



            Next
        Next

    End Function

    Friend Function draw_in_mem(ByRef bin As BitArray, mode As String, ByRef NN As Integer) As Bitmap
        ' mode "in", "eq", "contain"

        NN = 0

        Dim N As Integer = Math.Sqrt(N_points) + 1
        Dim k, S As Integer

        draw_in_mem = New Bitmap(N, N)

        For i = 0 To N - 1
            For j = 0 To N - 1

                k = i + j * N

                If k < N_points Then

                    S = points(k).memory.Count
                    NN += S

                    For Each m In points(k).memory

                    Next


                    If S > 255 Then S = 255

                    draw_in_mem.SetPixel(i, j, Color.FromArgb(S, S, S))
                Else
                    draw_in_mem.SetPixel(i, j, Color.Black)
                End If


            Next
        Next

    End Function


End Class

Class comb_point

    Public bin As BitArray

    Public x As Integer
    Public y As Integer

    Public cross As Integer ' число попадания бит сигнала в точку
    Public memory As New List(Of mem_recept_cluster)
    Public profile_A_complit() As Integer  ' профиль активности точки
    Public profile_A_part() As Integer  ' профиль активности точки по частичным совпадениям
    Public A As Boolean
    Public P As Integer ' потенциал точки
    Public P_part As Integer ' потенциал точки по частичным совпадениям
    Public out_bit As Integer


    Public Sub New(x As Integer, y As Integer, N_bit_main As Integer, N_bits_in_point As Integer)

        Me.x = x
        Me.y = y

        bin = New BitArray(N_bit_main)
        ReDim profile_A_complit(N_bits_in_point - 1)
        ReDim profile_A_part(N_bits_in_point - 1)

    End Sub

End Class


Public Enum status_type
    tmp
    permanent_1
    permanent_2
    on_deliting
End Enum

Friend Class mem_recept_cluster

    Public status As status_type

    Public F_need_check As Boolean = False

    Public Const L_speed As Single = 0.3

    Public N As Integer

    'Public mem_link As common_memory_item


    Public history_part_A As New List(Of String)
    Public history_part_A_bin As New List(Of BitArray)


    Public bits_set() As Integer
    Public bits_set_final() As Integer

    Public W() As Integer

    Public N_learn As Integer
    Public N_learn_next As Integer

    Public N_activate As Integer
    Public N_activate_part As Integer

    Public N_error_complit As Integer
    Public N_error_part As Integer

    Public N_cut As Integer

    Public start_time As Integer

    Public A_complit As Boolean
    Public A_part As Boolean

    Public P As Integer



    Public Sub New(ByRef source_bin As BitArray, ByRef p_bin As BitArray, N_learn_step As Integer)



        make_bits_set(source_bin, p_bin)

        Dim N As Integer = bits_set.Count - 1

        ReDim W(N)

        For i = 0 To N

            W(i) = 1

        Next

        N_learn = 0

        N_learn_next = N_learn_step

        N_activate = 0
        N_activate_part = 0

        status = status_type.tmp



    End Sub


    Public Sub make_bits_set(ByRef source_bin As BitArray, ByRef p_bin As BitArray)

        Dim bs As New List(Of Integer)

        For i = 0 To source_bin.Length - 1

            If source_bin(i) And p_bin(i) Then
                bs.Add(i)
            End If

        Next

        bits_set = bs.ToArray

    End Sub

    ' Расчет величины совпадения в кластере
    Public Sub set_P(ByRef bin As BitArray)

        P = 0

        For Each b In bits_set

            If bin(b) Then P += 1

        Next

    End Sub


    Friend Function F_main_iter() As Single()

        Dim max As Single = 0
        Dim A As Integer = 0

        Dim F(bits_set.Length - 1) As Single

        Dim nu As Single = 1 / bits_set.Count

        For i = 0 To bits_set.Count - 1
            F(i) = 1.0
        Next

        For j = 0 To 2


            For Each bin In history_part_A_bin


                A = 0

                For i = 0 To bits_set.Count - 1

                    If bin(bits_set(i)) Then

                        A += F(i)

                    End If

                Next

                For i = 0 To bits_set.Count - 1

                    If bin(bits_set(i)) Then

                        F(i) += A * nu

                    End If

                Next


                max = F.Max

                For i = 0 To bits_set.Length - 1
                    F(i) = F(i) / max
                Next

                nu = nu * 0.8

            Next
        Next
        F_main_iter = F

    End Function

    Private Function Norma2(f() As Single) As Single

        Norma2 = 0

        For Each x In f
            Norma2 += x * x
        Next

    End Function



End Class

Class points_cl

    Public size As Integer
    Public p() As comb_point

    Public Sub New(size As Integer)
        Me.size = size
        ReDim p(size * size - 1)
    End Sub

    Public Function point(i As Integer) As comb_point
        point = p(i)
    End Function

    Public Function point(x As Integer, y As Integer) As comb_point
        point = p(x + y * size)
    End Function
End Class




