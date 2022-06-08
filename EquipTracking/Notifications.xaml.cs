using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EquipTracking
{
    public partial class Notifications : Window
    {
        public Notifications()
        {
            InitializeComponent();

        }

        private void Table_Loaded(object sender, RoutedEventArgs e)
        {

            NotTable.ItemsSource = Container.EventsList;


        }

        #region basic actions with the window

        private void Mouse_Drag_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
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
        private void Mouse_Enter(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Colors.Gray) { Opacity = 0.2 };
        }
        private void Mouse_Leave(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Colors.Gray) { Opacity = 0 };
        }

        private void Mouse_Enter_Close(object sender, RoutedEventArgs e)
        {
            ((Label)sender).Background = Brushes.Red;
        }
        private void Mouse_Leave_Close(object sender, RoutedEventArgs e)
        {
            lbl_Close.Background = new SolidColorBrush(Colors.Gray) { Opacity = 0 };
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Container.EndFlag = true;
            this.Close();
        }



        private void ButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Label)sender).Background = (Brush)Application.Current.MainWindow.FindResource("ButtonBrush2");

        }



        #endregion
        private DateTime StringToDate(string dateStr)
        {
            dateStr = System.Text.RegularExpressions.Regex.Replace(dateStr, @"\s+", " ");
            if (dateStr[0] == ' ') dateStr = dateStr.Remove(0, 1);
            if (dateStr[dateStr.Length - 1] == ' ') dateStr = dateStr.Remove(dateStr.Length - 1, 1);
            string tmp = "";
            int iter = 0;
            int step = 0;
            int year = 0;
            int month = 0;
            int day = 0;
            try
            {
                while (step != 3)
                {
                    switch (step)
                    {
                        case 0:
                            {
                                if (iter > 2) throw new Exception();
                                if (dateStr[iter] != '.')
                                {
                                    tmp += dateStr[iter];
                                }
                                else
                                {
                                    day = int.Parse(tmp);
                                    if (day > 31) day = 1;
                                    tmp = "";
                                    step = 1;
                                }
                            }
                            break;
                        case 1:
                            {
                                if (iter > 7) throw new Exception();
                                if (dateStr[iter] != '.')
                                {
                                    tmp += dateStr[iter];
                                }
                                else
                                {
                                    month = int.Parse(tmp);
                                    if (month > 12) month = 12;
                                    tmp = "";
                                    step = 2;
                                }
                            }
                            break;
                        case 2:
                            {
                                if (iter != dateStr.Length)
                                {
                                    if (iter > 10) throw new Exception();
                                    if (dateStr[iter] != ' ')
                                    {
                                        tmp += dateStr[iter];
                                    }
                                }
                                else
                                {
                                    if (tmp.Length > 4) year = 2022;
                                    else year = int.Parse(tmp);
                                    tmp = "";
                                    step = 3;

                                }
                            }
                            break;
                        default:
                            break;
                    }
                    iter++;
                }
            }
            catch (Exception)
            {

                return new DateTime();
            }

            return new DateTime(year, month, day, 0, 0, 0);
        }

        private void DataPick_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".")))
            {
                e.Handled = true;
            }
        }


        private void SortBtn_Click(object sender, RoutedEventArgs e)
        {
            BindingList<AgrMachinery> SortedMach = new BindingList<AgrMachinery>();
            if (DateTxb.Text != "")
            {
                DateTime SDate = StringToDate(DateTxb.Text);
                if (ThisDate == null) return;
                bool thisDate = (bool)ThisDate.IsChecked;
                bool aDate = (bool)AfterDate.IsChecked;
                bool bDate = (bool)BeforeDate.IsChecked;

                foreach (AgrMachinery eventMach in Container.EventsList)
                {
                    if ((thisDate) && (SDate.Date == eventMach.dateOfDebiting.Date))
                    {
                        SortedMach.Add(eventMach);
                    }
                    else if ((aDate) && (SDate.Date >= eventMach.dateOfDebiting.Date))
                    {
                        SortedMach.Add(eventMach);
                    }
                    else if ((bDate) && (SDate.Date <= eventMach.dateOfDebiting.Date))
                    {
                        SortedMach.Add(eventMach);
                    }
                }

                notification.Text = "Сортировка завершена";
                NotTable.ItemsSource = SortedMach;
                notification.Visibility = Visibility.Visible;

            }
            else
            {
                notification.Text = "Введите дату!";
                notification.Visibility = Visibility.Visible;
            }
        }

        private void ChB_Checked(object sender, RoutedEventArgs e)
        {
            if (BeforeDate == null) return;
            if ((((CheckBox)sender).Name == "ThisDate") && (ThisDate.IsChecked == true))
            {
                BeforeDate.IsChecked = false;
                AfterDate.IsChecked = false;
            }
            else if ((((CheckBox)sender).Name == "BeforeDate") && (BeforeDate.IsChecked == true))
            {
                ThisDate.IsChecked = false;
                AfterDate.IsChecked = false;
            }
            else if ((((CheckBox)sender).Name == "AfterDate") && (AfterDate.IsChecked == true))
            {
                BeforeDate.IsChecked = false;
                ThisDate.IsChecked = false;
            }
            else
            {
                if ((((CheckBox)sender).Name == "ThisDate"))
                {
                    AfterDate.IsChecked = true;
                }
                else ThisDate.IsChecked = true;
            }
        }

        private void AllList_Click(object sender, RoutedEventArgs e)
        {
            NotTable.ItemsSource = Container.EventsList;
            notification.Text = "Список восстановлен";
            notification.Visibility = Visibility.Visible;
        }

        private void BackToTableBtn_Click(object sender, RoutedEventArgs e)
        {
            notification.Visibility = Visibility.Hidden;
            NotificationLbl.Visibility = Visibility.Visible;
            NotTable.Visibility = Visibility.Visible;
            SortNotPanel.Visibility = Visibility.Hidden;
        }

        private void MachTabl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            notification.Visibility = Visibility.Hidden;
            NotificationLbl.Visibility = Visibility.Hidden;
            NotTable.Visibility = Visibility.Hidden;
            SortNotPanel.Visibility = Visibility.Visible;
        }

        private void ClearTabl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Container.EventsList = new LinkedList<AgrMachinery>();
            NotTable.ItemsSource = Container.EventsList;
        }

        private void BackToMainWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        
    }
}
