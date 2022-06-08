using System;
using System.Collections.Generic;
using System.Text;

namespace EquipTracking
{
    public enum TypeOfArgMach
    {
        Tractor,
        CombineHarvester
    }
    public class AgrMachinery
    {
        public AgrMachinery(int id, string name, string point, TypeOfArgMach type, DateTime st, DateTime et)
        {
            WorkSpace = point;
            Id = id;
            OperatorName = name;
            PointOfDislocation = point;
            Type = type;
            StartTime = st;
            EndTime = et;
            if (Type == TypeOfArgMach.Tractor)
            {
                TypeStr = "Трактор";
                MaxTankCapacity = 120;
                FuelTank = 120;
            }
            else if (Type == TypeOfArgMach.CombineHarvester)
            {
                TypeStr = "Комбайн";
                MaxTankCapacity = 420;
                FuelTank = 420;
            }
            ConvertDataToTimeStr();
            isWorkingNowCheck();

        }
        public string OperatorName { get; set; } //ФИО машиниста
        public int Id { get; set; } // id техники

        public string PointOfDislocation { get; set; } //Текущее местоположение
        public String WorkSpace { get; set; } // Место, в котором должна быть техника

        public int HoursOfWorkTD { get; set; } //Часы работы сегодня
        public int AllHoursOfWork { get; set; } //Общее кол-во отработанных часов

        private double fuelTank;
        public double FuelTank
        {
            get { return fuelTank; }
            set {
                if (value < MaxTankCapacity) fuelTank = value;
                else fuelTank = MaxTankCapacity;
            }
        } //Оставшееся топливо
        public double SpentFuel { get; set; } //Потрачено топлива за все время
        public int MaxTankCapacity { get; set; } //Вместимось топливного бака

        public  TypeOfArgMach Type { get; set; } //Тип техники
        public string TypeStr { get; set; } //Тип техники в виде строки

        public DateTime StartTime { get; set; } //Время начала смены
        public DateTime EndTime { get; set; } //Время конца смены
        public string StartTimeStr { get; set; } //Время начала смены (строка)
        public string EndTimeStr { get; set; }//Время конца смены (строка)

        public bool IsOnTheWork { get; set; }//Флаг - техника на работе?
        public bool IsNotOnThePoint { get; set; } = false;//Флаг - техника на месте работы?

        public bool TechnicalCondition { get; set; } = true; //Флаг - техника в надлежащем состоянии?
        public string DescriptionOfCondition { get; set; } //Описание поломки

        public DateTime dateOfDebiting { get; set; } //Дата последнего события, связанного с этой техникой

        public void Clone(AgrMachinery mach)
        {
            PointOfDislocation = mach.PointOfDislocation;
            Id = mach.Id;
            OperatorName = mach.OperatorName;
            HoursOfWorkTD = mach.HoursOfWorkTD;
            AllHoursOfWork = mach.AllHoursOfWork;
            WorkSpace = mach.WorkSpace;
            Type = mach.Type;
            TypeStr = mach.TypeStr;
            SpentFuel = mach.SpentFuel;
            FuelTank = mach.FuelTank;
            MaxTankCapacity = mach.MaxTankCapacity;
            StartTime = mach.StartTime;
            StartTimeStr = mach.StartTimeStr;
            EndTime = mach.EndTime;
            EndTimeStr = mach.EndTimeStr;
            IsOnTheWork = mach.IsOnTheWork;
            IsNotOnThePoint = mach.IsNotOnThePoint;
            TechnicalCondition = mach.TechnicalCondition;
            DescriptionOfCondition = mach.DescriptionOfCondition;
            dateOfDebiting = mach.dateOfDebiting;
        }

        /// <summary>
        /// Метод проверяет работает ли сейчас техника
        /// и находится ли она на нужном месте
        /// </summary>
        public void isWorkingNowCheck()
        {
            
            if ((FuelTank != 0)&&(!IsNotOnThePoint) && (TechnicalCondition))
            {
                DateTime Now = DateTime.Now;
                if (StartTime < EndTime)
                {
                    if ((StartTime.Hour < Now.Hour) && (EndTime.Hour > Now.Hour))
                    {
                        IsOnTheWork = true;
                        PointOfDislocation = WorkSpace;
                        return;

                    }
                    else if (StartTime.Hour == Now.Hour)
                    {
                        if (StartTime.Minute < Now.Minute)
                        {
                            IsOnTheWork = true;
                            PointOfDislocation = WorkSpace;
                            return;
                        }
                    }
                    else if (EndTime.Hour == Now.Hour)
                    {
                        if (EndTime.Minute > Now.Minute) 
                        {
                            IsOnTheWork = true;
                            PointOfDislocation = WorkSpace;
                            return;
                        }
                    }
                }
                else
                {
                    if ((StartTime.Hour > Now.Hour) && (EndTime.Hour < Now.Hour))
                    {
                        IsOnTheWork = false;
                        PointOfDislocation = "На стоянке";
                    }
                    else if (StartTime.Hour == Now.Hour)
                    {
                        if (StartTime.Minute < Now.Minute)
                        {
                            IsOnTheWork = true;
                            PointOfDislocation = WorkSpace;
                            return;
                        }
                    }
                    else if (EndTime.Hour == Now.Hour)
                    {
                        if (EndTime.Minute > Now.Minute)
                        {
                            IsOnTheWork = true;
                            PointOfDislocation = WorkSpace;
                            return;
                        }
                    }
                    else
                    {
                        IsOnTheWork = true;
                        PointOfDislocation = WorkSpace;
                        return;
                    }

                }

            }
            if (IsNotOnThePoint) PointOfDislocation = "Не на точке";
            else if  (!TechnicalCondition)  PointOfDislocation = "В СТО";
            else PointOfDislocation = "На стоянке";
            IsOnTheWork = false;


        }
        /// <summary>
        /// Метод для конвертации объекта DataTime в строку
        /// </summary>
        public void ConvertDataToTimeStr()
        {
            StartTimeStr = "";
            EndTimeStr = "";
            if (StartTime.Hour < 10) StartTimeStr += "" + 0 + StartTime.Hour.ToString() + ":";
            else StartTimeStr += StartTime.Hour + ":";
            if (EndTime.Hour < 10) EndTimeStr += "" + 0 + EndTime.Hour.ToString() + ":";
            else EndTimeStr += EndTime.Hour + ":";

            if (StartTime.Minute < 10) StartTimeStr += 0 + StartTime.Minute.ToString();
            else StartTimeStr += StartTime.Minute.ToString();
            if (EndTime.Minute < 10) EndTimeStr += 0 + EndTime.Minute.ToString();
            else EndTimeStr += EndTime.Minute.ToString();
        }
        /// <summary>
        /// Метод, высчитывающий потроченное топливо
        /// и кол-во отработанных часов за время, что программа провела
        /// без работы
        /// </summary>
        public void CalculationOfHoursSpentAndFuel()
        {
            if (IsOnTheWork)
            {
                int hours = 0;
                double spentFuel = 0;
                if (EndTime > StartTime) hours = DateTime.Now.Hour - StartTime.Hour;
                else
                {
                    if (DateTime.Now.Hour >= StartTime.Hour) hours = DateTime.Now.Hour - StartTime.Hour;
                    else hours = 24 - StartTime.Hour + DateTime.Now.Hour;
                }
                if (Type == TypeOfArgMach.Tractor)
                {
                    if (FuelTank - 4.8 < 0)
                    {
                        FuelTank = 0;
                        SpentFuel += (4.8 - fuelTank);
                    }
                    else
                    {
                        FuelTank -= 4.8;
                        SpentFuel += 4.8;
                    }
                }
                else if (Type == TypeOfArgMach.CombineHarvester)
                {
                    if (FuelTank - 8.4 < 0)
                    {
                        FuelTank = 0;
                        spentFuel += (8.4 - FuelTank);
                    }
                    else
                    {
                        FuelTank -= 8.4;
                        spentFuel += 8.4;
                    }
                }
                FuelTank = Math.Round(FuelTank, 3);
                SpentFuel = Math.Round(SpentFuel, 3);
            }
        }
    }
}
