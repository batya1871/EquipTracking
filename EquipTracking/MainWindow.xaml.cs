using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace EquipTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        private readonly string AgrMachPath = $"{Environment.CurrentDirectory}\\AgrMach.json";
        private readonly string eventsPath = $"{Environment.CurrentDirectory}\\Events.json";
        public BindingList<AgrMachinery> AllArgMachineryList;
        public BindingList<AgrMachinery> AgrMachineryOnTheWorkList = new BindingList<AgrMachinery>();
        public BindingList<AgrMachinery> MachWithoutGasolineList = new BindingList<AgrMachinery>();
        private SaveAndLoadService fileIOService;
        bool isFromTxb = false;
        bool UserInActiveTable = true;
        DispatcherTimer FuelTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            fileIOService = new SaveAndLoadService(AgrMachPath, eventsPath);
            try
            {
                AllArgMachineryList = fileIOService.LoadAllAgrMachinery();
                Container.EventsList = fileIOService.LoadEventsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных!");
                Close();
            }
            if (AllArgMachineryList != null)
            {
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if ((machinery.IsOnTheWork)&& (machinery.HoursOfWorkTD == 0) && (machinery.FuelTank == machinery.MaxTankCapacity))
                    {
                        machinery.CalculationOfHoursSpentAndFuel();
                    }
                }
            }
            CheckActiveMachine(AllArgMachineryList);
            if (AllArgMachineryList == null) AllArgMachineryList = new BindingList<AgrMachinery>();
            MainTable.ItemsSource = AgrMachineryOnTheWorkList;

            FuelTimer.Interval = TimeSpan.FromMinutes(1);
            FuelTimer.Tick += time_tick;
            FuelTimer.Start();

        }

        private void time_tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Minute % 5 == 0)
            {
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if ((machinery.IsOnTheWork)&& (!machinery.IsNotOnThePoint) && (machinery.TechnicalCondition))
                    {
                        bool isLeaveWorkSpace = (rnd.Next(1, 21) == 1);
                        if (isLeaveWorkSpace)
                        {
                            machinery.IsNotOnThePoint = true;
                            machinery.dateOfDebiting = DateTime.Now;
                            machinery.DescriptionOfCondition = "Машина покинула место работы!";
                            AgrMachinery EventMach = new AgrMachinery(int.MinValue, null, null, TypeOfArgMach.Tractor, DateTime.MinValue, DateTime.MinValue);
                            EventMach.Clone(machinery);
                            Container.EventsList.AddFirst(EventMach);
                            NoteInfo.Content = "Новые оповещения!";
                        }
                        if (machinery.Type == TypeOfArgMach.CombineHarvester)
                        {
                            if (machinery.FuelTank - 0.7 < 0)
                            {
                                machinery.FuelTank = 0;
                                machinery.SpentFuel += (0.7 - machinery.FuelTank);
                            }
                            else
                            {
                                machinery.FuelTank -= 0.7;
                                machinery.SpentFuel += 0.7;
                            }
                        }
                        else if (machinery.Type == TypeOfArgMach.Tractor)
                        {
                            if (machinery.FuelTank - 0.4 < 0)
                            {
                                machinery.FuelTank = 0;
                                machinery.SpentFuel += (0.4 - machinery.FuelTank);
                            }
                            else
                            {
                                machinery.FuelTank -= 0.4;
                                machinery.SpentFuel += 0.4;
                            }
                        }
                        machinery.FuelTank = Math.Round(machinery.FuelTank, 3);
                        machinery.SpentFuel = Math.Round(machinery.SpentFuel, 3);

                        if (machinery.FuelTank == 0)
                        {
                            machinery.dateOfDebiting = DateTime.Now;
                            machinery.DescriptionOfCondition = "Закончилось топливо";
                            AgrMachinery EventMach = new AgrMachinery(int.MinValue, null, null, TypeOfArgMach.Tractor, DateTime.MinValue, DateTime.MinValue);
                            EventMach.Clone(machinery);
                            Container.EventsList.AddFirst(EventMach);
                            NoteInfo.Content = "Новые оповещения!";
                        }

                    }
                    CheckActiveMachine(AllArgMachineryList);
                    CheckMachineWithoutGasoline(AllArgMachineryList);
                    AllMachTable.ItemsSource = null;
                    AllMachTable.ItemsSource = AllArgMachineryList;
                    MainTable.ItemsSource = null;
                    if (UserInActiveTable) { MainTable.ItemsSource = AgrMachineryOnTheWorkList; }
                    else { MainTable.ItemsSource = MachWithoutGasolineList; }



                }
            }
            if (DateTime.Now.Minute == 0)
            {
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if ((machinery.IsOnTheWork) && (!machinery.IsNotOnThePoint) && (machinery.TechnicalCondition))
                    {
                        machinery.HoursOfWorkTD += 1;
                        machinery.AllHoursOfWork += 1;
                    }
                    CheckActiveMachine(AllArgMachineryList);
                    CheckMachineWithoutGasoline(AllArgMachineryList);
                    AllMachTable.ItemsSource = null;
                    AllMachTable.ItemsSource = AllArgMachineryList;
                    MainTable.ItemsSource = null;
                    if (UserInActiveTable) { MainTable.ItemsSource = AgrMachineryOnTheWorkList; }
                    else { MainTable.ItemsSource = MachWithoutGasolineList; }
                }
            }
        }

        private void CheckActiveMachine(BindingList<AgrMachinery> list)
        {
            if (list != null)
            {
                AgrMachineryOnTheWorkList = new BindingList<AgrMachinery>();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].isWorkingNowCheck();
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].IsOnTheWork)
                    {
                       
                        AgrMachineryOnTheWorkList.Add(list[i]);
                    }
                }
            }
        }
        private void CheckMachineWithoutGasoline(BindingList<AgrMachinery> list)
        {
            if (list != null)
            {
                MachWithoutGasolineList = new BindingList<AgrMachinery>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].FuelTank == 0)
                    {
                        MachWithoutGasolineList.Add(list[i]);
                    }
                }
            }
        }

        private DateTime StringToTime(string dateStr)
        {
           
            int hour = int.Parse("" + dateStr[0] + dateStr[1]);
            int minute = int.Parse("" + dateStr[3] + dateStr[4]);


            return new DateTime(1, 1, 1, hour, minute, 0);
        }


        #region Find Machine

        private void FindMach_Click(object sender, RoutedEventArgs e)
        {
            HiddenAllPanels();
            FindMachPanel.Visibility = Visibility.Visible;
            FindMachNotificationTb.Visibility = Visibility.Hidden;
            FindMach_Warning.Visibility = Visibility.Hidden;
        }
        private void FindMachBtn_click(object sender, RoutedEventArgs e)
        {
            FindMachNotificationTb.Visibility = Visibility.Hidden;
            FindMach_Warning.Visibility = Visibility.Hidden;
            if (FindHiddenMachIdTxb.Text != "")
            {
                bool isFounded = false;
                int machId = int.Parse(FindHiddenMachIdTxb.Text);
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if ((machinery.Id == machId))
                    {
                        if (machinery.IsNotOnThePoint == false)
                        {
                            FindMachNotificationTb.Text = "Техника находится на месте работы!";
                            FindMachNotificationTb.Visibility = Visibility.Visible;
                            return;
                        }
                        isFounded = true;
                        machinery.IsNotOnThePoint = false;
                        machinery.DescriptionOfCondition = "Техника возвращена";
                        machinery.dateOfDebiting = DateTime.Now;
                        AgrMachinery EventMach = new AgrMachinery(int.MinValue, null, null, TypeOfArgMach.Tractor, DateTime.MinValue, DateTime.MinValue);
                        EventMach.Clone(machinery);
                        Container.EventsList.AddFirst(EventMach);
                        NoteInfo.Content = "Новые оповещения!";
                        machinery.DescriptionOfCondition = "Эксплуатируется";
                        machinery.dateOfDebiting = DateTime.MinValue;
                        FindMachNotificationTb.Text = "Техника возвращена!";
                        break;
                    }
                }
                if (!isFounded)
                {
                    FindMachNotificationTb.Text = " id отсутствует в системе!";
                }
                ClearAllTbx();
                FindMachNotificationTb.Visibility = Visibility.Visible;
            }
            else
            {
                FindMach_Warning.Visibility = Visibility.Visible;
            }

        }
        #endregion
        #region Write Off Machine
        private void Mach_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (breakDownPanel.Visibility == Visibility.Visible)
            {
                breakDownPanel.Visibility = Visibility.Hidden;
                breakdownDescPanel.Visibility = Visibility.Hidden;
                isBrokenMachChB.IsChecked = false;
                breakDownDescTxb.Clear();
            }
        }
        private void Mach_PreviewTextInputDesc(object sender, TextCompositionEventArgs e)
        {
            if (breakDownPanel.Visibility == Visibility.Visible)
            {
                breakDownPanel.Visibility = Visibility.Hidden;
                breakdownDescPanel.Visibility = Visibility.Hidden;
                isBrokenMachChB.IsChecked = false;
                breakDownDescTxb.Clear();
            }
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ",")))
            {
                e.Handled = true;
            }
        }
        private void isBrokenMachChB_Click(object sender, RoutedEventArgs e)
        {
            if (isBrokenMachChB.IsChecked == true)
            {
                breakdownDescPanel.Visibility = Visibility.Visible;
            }
            else
            {
                breakDownDescTxb.Clear();
                breakdownDescPanel.Visibility = Visibility.Hidden;
            }
        }

        private void WOMach_Click(object sender, RoutedEventArgs e)
        {
            HiddenAllPanels();
            WOMachIdTxb.Clear();
            isBrokenMachChB.IsChecked = false;
            writeOffMachPanel.Visibility = Visibility.Visible;
            WOMachNotificationTb1.Visibility = Visibility.Hidden;
            WOMachNotificationTb2.Visibility = Visibility.Hidden;
            BOMach_Warning.Visibility = Visibility.Hidden;
            WOMach_Warning.Visibility = Visibility.Hidden;

        }
        private void WOMachBtn_click(object sender, RoutedEventArgs e)
        {
            WOMachNotificationTb1.Visibility = Visibility.Hidden;
            WOMach_Warning.Visibility = Visibility.Hidden;
            if (WOMachIdTxb.Text != "")
            {
                bool alreadyWO = false;
                bool isInRoute = false;
                bool isFounded = false;
                int SearchedId = int.Parse(WOMachIdTxb.Text);
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if ((machinery.Id == SearchedId))
                    {
                        isFounded = true;
                        if (!machinery.TechnicalCondition)
                        {
                            alreadyWO = true;
                            WOMachNotificationTb1.Text = "Техника уже списана!";
                            WOMachNotificationTb1.Visibility = Visibility.Visible;
                        }
                        break;
                    }
                }
                if (isFounded)
                {
                    if ((!isInRoute) && (!alreadyWO))
                    {

                        breakDownPanel.Visibility = Visibility.Visible;
                        infoTb.Text = "id - " + SearchedId;
                    }
                }
                else
                {
                    WOMachNotificationTb1.Text = "Техника с данным id отсутствует в системе!";

                    WOMachNotificationTb1.Visibility = Visibility.Visible;
                    ClearAllTbx();
                }


            }
            else
            {
                WOMach_Warning.Visibility = Visibility.Visible;
            }

        }
        private void breakdownDescBtn_Click(object sender, RoutedEventArgs e)
        {
            WOMachNotificationTb1.Visibility = Visibility.Hidden;
            BOMach_Warning.Visibility = Visibility.Hidden;
            int SearchedId = int.Parse(WOMachIdTxb.Text);
            if (breakDownDescTxb.Text != "")
            {
                string Desc = breakDownDescTxb.Text;
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.Id == SearchedId)
                    {
                        machinery.TechnicalCondition = false;
                        machinery.dateOfDebiting = DateTime.Now;
                        machinery.DescriptionOfCondition = "Cписана с эксплуатации. " + Desc;
                        AgrMachinery EventMach = new AgrMachinery(int.MinValue, null,null,TypeOfArgMach.Tractor, DateTime.MinValue, DateTime.MinValue);
                        EventMach.Clone(machinery);
                        Container.EventsList.AddFirst(EventMach);
                        NoteInfo.Content = "Новые оповещения!";
                        WOMachNotificationTb2.Visibility = Visibility.Visible;
                        breakDownDescTxb.Clear();
                    }
                }
            }
            else
            {
                WOMachNotificationTb2.Visibility = Visibility.Hidden;
                BOMach_Warning.Visibility = Visibility.Visible;
            }
        }
        #endregion
        #region Write On Machine

        private void writeOnMach_Click(object sender, RoutedEventArgs e)
        {
            HiddenAllPanels();
            breakDownPanel.Visibility = Visibility.Hidden;
            breakdownDescPanel.Visibility = Visibility.Hidden;
            writeOnMachPanel.Visibility = Visibility.Visible;
        }

        private void WOnMachBtn_click(object sender, RoutedEventArgs e)
        {
            WOnMachNotificationTb.Visibility = Visibility.Hidden;
            WOnMach_Warning.Visibility = Visibility.Hidden;
            if (WOnMachIdTxb.Text != "")
            {
                bool isFounded = false;
                int machId = int.Parse(WOnMachIdTxb.Text);
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if ((machinery.Id == machId))
                    {
                        isFounded = true;
                        machinery.TechnicalCondition = true;
                        machinery.DescriptionOfCondition = "Техника введена в эскплуатацию.";
                        machinery.dateOfDebiting = DateTime.Now;
                        AgrMachinery EventMach = new AgrMachinery(int.MinValue, null, null, TypeOfArgMach.Tractor, DateTime.MinValue, DateTime.MinValue);
                        EventMach.Clone(machinery);
                        Container.EventsList.AddFirst(EventMach);
                        NoteInfo.Content = "Новые оповещения!";
                        machinery.DescriptionOfCondition = "Эксплуатируется";
                        machinery.dateOfDebiting = DateTime.MinValue;
                        WOnMachNotificationTb.Text = "Техника восстановлена!";
                        break;
                    }
                }
                if (!isFounded)
                {
                    WOnMachNotificationTb.Text = " id отсутствует в системе!";
                }
                ClearAllTbx();
                WOnMachNotificationTb.Visibility = Visibility.Visible;
            }
            else
            {
                WOnMach_Warning.Visibility = Visibility.Visible;
            }

        }

        #endregion
        #region basic actions with the window
        private void Header_Leave(object sender, MouseEventArgs e)
        {
           if (e.MouseDevice.DirectlyOver != ListActZone)  ListActZone.Visibility = Visibility.Hidden;
            if (e.MouseDevice.DirectlyOver != MachActZone) MachActZone.Visibility = Visibility.Hidden;
            return;
        }


        private void MachActDown(object sender, MouseButtonEventArgs e)
        {
            if (ListActZone.Visibility == Visibility.Visible) ListActZone.Visibility = Visibility.Hidden;

            MachActZone.Visibility = Visibility.Visible;

        }

        private void ButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Label)sender).Background = (Brush)Application.Current.MainWindow.FindResource("ButtonBrush2");

        }

        private void MachActZone_MouseLeave(object sender, MouseEventArgs e)
        {
            MachActZone.Visibility = Visibility.Hidden;
        }

        private void Mouse_Drag_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void ListActDown(object sender, MouseButtonEventArgs e)
        {
            if (MachActZone.Visibility == Visibility.Visible) MachActZone.Visibility = Visibility.Hidden;

            ListActZone.Visibility = Visibility.Visible;

        }
        private void ListActZone_MouseLeave(object sender, MouseEventArgs e)
        {
            ListActZone.Visibility = Visibility.Hidden;
        }



        private void Header_Mouse_Enter(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Foreground = Brushes.White;
        }
        private void Header_Mouse_Leave(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Foreground = Brushes.Gray;
        }
        private void Resize_Wondow(object sender, RoutedEventArgs e)
        {

            if (WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }
        private void Roll_Up(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fileIOService.SaveAllAgrMachinery(AllArgMachineryList);
            fileIOService.SaveEventsList(Container.EventsList);
            FuelTimer.Stop();
            e.Cancel = true; // Отмена закрытия окна 
        }
        private void Mouse_Enter(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Colors.Gray) { Opacity = 0.2 };
        }
        private void Mouse_Leave(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Colors.Gray) { Opacity = 0 };
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            
            Application.Current.Shutdown();
        }
        private void Mouse_Enter_Close(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Background = Brushes.Red;
        }
        private void Mouse_Leave_Close(object sender, RoutedEventArgs e)
        {
            lbl_Close.Background = new SolidColorBrush(Colors.Gray) { Opacity = 0 };
        }

        //Открытие окна для входа в аккаунт\регистрации
        private void LoginWindow_Open(object sender, MouseButtonEventArgs e)
        {
            
        }


        #endregion
        private void NewId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0)))
            {
                e.Handled = true;
            }
        }
        private void NewTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            if ((!(Char.IsDigit(e.Text, 0))) || (((TextBox)sender).Text.Length > 4))
            {
                e.Handled = true;
                return;
            }
            if ((((TextBox)sender).Text.Length == 0)&&(int.Parse(e.Text) > 2)) e.Handled = true;
            if ((((TextBox)sender).Text.Length == 3) && (int.Parse(e.Text) > 5)) e.Handled = true;
            isFromTxb = false;

        }
        private void NewStartTimeTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Back) && (((TextBox)sender).Text.Length == 3))
            {
                isFromTxb = true;
            }
            else if(e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void NewStartTimeTB_TextChanged(object sender, TextChangedEventArgs e)
       {
            if (!isFromTxb)
            {
                isFromTxb = true;
                string tmp = ((TextBox)sender).Text;
                if ((((TextBox)sender).Text.Length == 2)) ((TextBox)sender).Text += ":";
                else if (((TextBox)sender).Text.Length == 3) ((TextBox)sender).Text = "" + tmp[0] + tmp[1];
                ((TextBox)sender).CaretIndex = Int32.MaxValue;
                isFromTxb = false;
            }
        }
        private void SelectTypeChB_Click(object sender, RoutedEventArgs e)
        {
            if (isTRChB == null) return;
            if ((((CheckBox)sender).Name == "isTRChB") && (isTRChB.IsChecked == true))
            {
                isCMChB.IsChecked = false;
            }
            else if ((((CheckBox)sender).Name == "isCMChB") && (isCMChB.IsChecked == true))
            {
                isTRChB.IsChecked = false;
            }
            else
            {
                if ((((CheckBox)sender).Name == "isTRChB"))
                {
                    isCMChB.IsChecked = true;
                }
                else isTRChB.IsChecked = true;
            }
        }

        private void AddMachBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((NewId.Text == "") || (NewName.Text == "") || (NewPoint.Text == "") || (NewStartTimeTB.Text == "") || (NewEndTimeTB.Text == ""))
            {
                AddNot.Text = "Введены не все данные!";
                AddNot.Visibility = Visibility.Visible;
            }
            else
            {
                AgrMachinery newMach;
                int id = int.Parse(NewId.Text);
                DateTime st = StringToTime(NewStartTimeTB.Text);
                DateTime et = StringToTime(NewEndTimeTB.Text);
                string name = NewName.Text;
                string point = NewPoint.Text;
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.Id == id)
                    {
                        AddNot.Text = "Техника с данным id  уже существует!";
                        AddNot.Visibility = Visibility.Visible;
                        return;
                    }
                }
                if ((bool)isTRChB.IsChecked)
                {
                    newMach = new AgrMachinery(id, name, point, TypeOfArgMach.Tractor, st, et);
                }
                else
                {
                    newMach = new AgrMachinery(id, name, point, TypeOfArgMach.CombineHarvester, st, et);
                }
                newMach.isWorkingNowCheck();
                AllArgMachineryList.Add(newMach);
                CheckActiveMachine(AllArgMachineryList);
                AddNot.Text = "Техника успешно добавлена в список!";
                AddNot.Visibility = Visibility.Visible;
                ClearAllTbx();


            }
        }


        private void ClearAllTbx()
        {
            NewId.Clear();
            NewName.Clear();
            NewPoint.Clear();
            NewStartTimeTB.Clear();
            NewEndTimeTB.Clear();

            DelId.Clear();

            ChangeId.Clear();
            ChangeName.Clear();
            ChangePoint.Clear();
            ChangeStartTimeTB.Clear();
            ChangeEndTimeTB.Clear();

            TopUpId.Clear();

            WOnMachIdTxb.Clear();
            breakDownDescTxb.Clear();
            WOMachIdTxb.Clear();

        }

        private void DelMachBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DelId.Text == "")
            {
                DelNot.Text = "Введите id!";
                DelNot.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                int delId = int.Parse(DelId.Text);

                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.Id == delId)
                    {
                        AllArgMachineryList.Remove(machinery);
                        DelNot.Text = "Техника успешно удалена!";
                        DelNot.Visibility = Visibility.Visible;
                        CheckActiveMachine(AllArgMachineryList);
                        return;
                    }
                }
                DelNot.Text = "Техника с данным id не найдена!";
                DelNot.Visibility = Visibility.Visible;
                ClearAllTbx();
                return;
            }
        }

        
        private void FindIdBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeNot2.Visibility = Visibility.Hidden;
            ChangeNot1.Visibility = Visibility.Hidden;
            if (ChangeId.Text == "")
            {
                ChangeNot1.Text = "Введите id!";
                ChangeNot1.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                int FindId = int.Parse(ChangeId.Text);
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.Id == FindId)
                    {
                        ChangeMachZone2.Visibility = Visibility.Visible;
                        ChangeName.Text = machinery.OperatorName;
                        ChangePoint.Text = machinery.PointOfDislocation;
                        ChangeStartTimeTB.Text = machinery.StartTimeStr;
                        ChangeEndTimeTB.Text = machinery.EndTimeStr;
                        return;
                    }
                }
                ChangeNot1.Text = "Техника с данным id не найдена!";
                ChangeNot1.Visibility = Visibility.Visible;
                ClearAllTbx();
                return;
            }
        }

        private void ChangeId_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeMachZone2.Visibility = Visibility.Hidden;
            ChangeName.Clear();
            ChangePoint.Clear();
            ChangeStartTimeTB.Clear();
            ChangeEndTimeTB.Clear();
        }


        private void ChangeMachBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeNot1.Visibility = Visibility.Hidden;
            if ( (ChangeName.Text == "") || (ChangePoint.Text == "") || (ChangeStartTimeTB.Text == "") || (ChangeEndTimeTB.Text == ""))
            {
                ChangeNot2.Text = "Введены не все данные!";
                ChangeNot2.Visibility = Visibility.Visible;
            }
            else
            {
                int id = int.Parse(ChangeId.Text);
                DateTime st = StringToTime(ChangeStartTimeTB.Text);
                DateTime et = StringToTime(ChangeEndTimeTB.Text);
                string name = ChangeName.Text;
                string point = ChangePoint.Text;
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.Id == id)
                    {
                        machinery.OperatorName = name;
                        machinery.PointOfDislocation = point;
                        machinery.WorkSpace = point;
                        machinery.StartTime = st;
                        machinery.EndTime = et;

                        machinery.isWorkingNowCheck();
                        machinery.ConvertDataToTimeStr();

                        CheckActiveMachine(AllArgMachineryList);

                        ChangeNot2.Text = "Новые данные успешно внесены!";
                        ChangeNot2.Visibility = Visibility.Visible;
                        ClearAllTbx();
                        return;
                    }
                }
                


            }
        }


        private void btnWorkingMach_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HiddenAllPanels();
            UserInActiveTable = true;
            ClearAllTbx();
            CheckActiveMachine(AllArgMachineryList);
            MainTable.ItemsSource = null;
            MainTable.ItemsSource = AgrMachineryOnTheWorkList;
            MainTable.Visibility = Visibility.Visible;
            TableTitlelbl.Content = "Техника на вывозе";
            TableTitlelbl.FontSize = 18;
            TableTitlelbl.Margin = new Thickness(5, -9, 0, 0);
            TableTitlelbl.Visibility = Visibility.Visible;

        }

        private void btnAddMach_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HiddenAllPanels();
            AddMachZone.Visibility = Visibility.Visible;
            AddNot.Visibility = Visibility.Hidden;
        }

        private void btnDeleteMach_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HiddenAllPanels();
            DeleteMachZone.Visibility = Visibility.Visible;
            DelNot.Visibility = Visibility.Hidden;
        }

        private void btnEditMach_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HiddenAllPanels();

            ChangeMachZone.Visibility = Visibility.Visible;
        }

        private void   TopUpChB_Click(object sender, RoutedEventArgs e)
        {
            if (isTRChB == null) return;
            TopUpTanksNot.Visibility = Visibility.Hidden;
            TopUpOneTankNot.Visibility = Visibility.Hidden;
            if ((((CheckBox)sender).Name == "TopUpAllTanksChB") && (TopUpAllTanksChB.IsChecked == true))
            {
                TopUpOneTanksChB.IsChecked = false;
                TopUpAllTanksBtn.Visibility = Visibility.Visible;
                TopUpOneTankZone.Visibility = Visibility.Hidden;
                TopUpId.Clear();
            }
            else if ((((CheckBox)sender).Name == "TopUpOneTanksChB") && (TopUpOneTanksChB.IsChecked == true))
            {
                TopUpAllTanksChB.IsChecked = false;
                TopUpAllTanksBtn.Visibility = Visibility.Hidden;
                TopUpOneTankZone.Visibility = Visibility.Visible;
            }
            else
            {
                if ((((CheckBox)sender).Name == "TopUpAllTanksChB"))
                {
                    TopUpOneTanksChB.IsChecked = true;
                    TopUpAllTanksBtn.Visibility = Visibility.Hidden;
                    TopUpOneTankZone.Visibility = Visibility.Visible;

                }
                else
                {
                    TopUpAllTanksChB.IsChecked = true;
                    TopUpAllTanksBtn.Visibility = Visibility.Visible;
                    TopUpOneTankZone.Visibility = Visibility.Hidden;
                    TopUpId.Clear();
                }
            }
        }

        private void TopUpAllTanksBtn_Click(object sender, RoutedEventArgs e)
        {
            bool topUpFlag = false;
            if ((AllArgMachineryList == null)|| (AllArgMachineryList.Count == 0))
            {
                TopUpTanksNot.Text = "Список техники пуск!";
                TopUpTanksNot.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.FuelTank == 0)
                    {
                        topUpFlag = true;
                        if (machinery.Type == TypeOfArgMach.Tractor)
                        {
                            machinery.FuelTank = 120;
                        }
                        else if (machinery.Type == TypeOfArgMach.CombineHarvester)
                        {
                            machinery.FuelTank = 420;
                        }
                    }
                }

                if (topUpFlag) { TopUpTanksNot.Text = "Вся техника без бензина заправлена!"; }
                else { TopUpTanksNot.Text = "Техника без бензина не обнаружена!"; }
                TopUpTanksNot.Visibility = Visibility.Visible;
            }
            
        }

        private void TopUpIdBtn_Click(object sender, RoutedEventArgs e)
        {
            TopUpOneTankNot.Visibility = Visibility.Hidden;
            if (TopUpId.Text == "")
            {
                TopUpOneTankNot.Text = "Введите id!";
                TopUpOneTankNot.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                int topUpId = int.Parse(TopUpId.Text);
                foreach (AgrMachinery machinery in AllArgMachineryList)
                {
                    if (machinery.Id == topUpId)
                    {
                        if(machinery.FuelTank == 0)
                        {
                            if (machinery.Type == TypeOfArgMach.Tractor)
                            {
                                machinery.FuelTank = 120;
                            }
                            else if (machinery.Type == TypeOfArgMach.CombineHarvester)
                            {
                                machinery.FuelTank = 420;
                            }
                            TopUpOneTankNot.Text = "Техника с данным id успешно заправлена!";
                        }
                        else TopUpOneTankNot.Text = "Данная машина уже  заправлена!";
                        TopUpOneTankNot.Visibility = Visibility.Visible;
                        return;
                    }
                }
                TopUpOneTankNot.Text = "Техника с данным id не найдена!";
                TopUpOneTankNot.Visibility = Visibility.Visible;
                ClearAllTbx();
                return;
            }
        }

        private void btnRefill_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HiddenAllPanels();
            TopUpTanksZone.Visibility = Visibility.Visible;

        }

        private void btnMachWithoutGasoline_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HiddenAllPanels();
            UserInActiveTable = false;
            CheckMachineWithoutGasoline(AllArgMachineryList);
            MainTable.ItemsSource = MachWithoutGasolineList;
            MainTable.Visibility = Visibility.Visible;
            TableTitlelbl.Content = "Техника без бензина";
            TableTitlelbl.FontSize = 18;
            TableTitlelbl.Margin = new Thickness(5, -9, 0, 0);
            TableTitlelbl.Visibility = Visibility.Visible;
        }

        private void AllMachInTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckMachineWithoutGasoline(AllArgMachineryList);
            CheckActiveMachine(AllArgMachineryList);
            HiddenAllPanels();
            AllMachTable.ItemsSource = null;
            AllMachTable.ItemsSource = AllArgMachineryList;
            AllMachTable.Visibility = Visibility.Visible;
            TableTitlelbl.Content = "Вся техника";
            TableTitlelbl.FontSize = 18;
            TableTitlelbl.Margin = new Thickness(5, -9, 0, 0);
            TableTitlelbl.Visibility = Visibility.Visible;
        }
        private void Mach_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ",")))
            {
                e.Handled = true;
            }
        }

        private void TOActiveTableBtn_Click(object sender, RoutedEventArgs e)
        {
            btnWorkingMach_MouseDown(null, null);
        }

        private void HiddenAllPanels()
        {
            ClearAllTbx();
            breakDownPanel.Visibility = Visibility.Hidden;
            breakdownDescPanel.Visibility = Visibility.Hidden;
            WOnMachNotificationTb.Visibility = Visibility.Hidden;
            MainTable.Visibility = Visibility.Hidden;
            AllMachTable.Visibility = Visibility.Hidden;
            TableTitlelbl.Visibility = Visibility.Hidden;
            AddNot.Visibility = Visibility.Hidden;
            DeleteMachZone.Visibility = Visibility.Hidden;
            ChangeMachZone.Visibility = Visibility.Hidden;
            ChangeMachZone2.Visibility = Visibility.Hidden;
            ChangeNot1.Visibility = Visibility.Hidden;
            ChangeNot2.Visibility = Visibility.Hidden;
            TopUpTanksZone.Visibility = Visibility.Hidden;
            TopUpOneTankNot.Visibility = Visibility.Hidden;
            TopUpTanksNot.Visibility = Visibility.Hidden;
            writeOffMachPanel.Visibility = Visibility.Hidden;
            writeOnMachPanel.Visibility = Visibility.Hidden;
            AddMachZone.Visibility = Visibility.Hidden;
            FindMachPanel.Visibility = Visibility.Hidden;
        }

        private void NotInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Notifications w = new Notifications();
            this.Visibility = Visibility.Hidden;
            w.ShowDialog();
            NoteInfo.Content = "Оповещения";
            if(Container.EndFlag) Application.Current.Shutdown();
            this.Visibility = Visibility.Visible;
            
        }
    }



}
