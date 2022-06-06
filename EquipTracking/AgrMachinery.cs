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
        public string OperatorName { get; set; }
        public int Id { get; set; }
        public string PointOfDislocation { get; set; }
        public int HoursOfWorkTD { get; set; }
        public int AllHoursOfWork { get; set; }
        private double fuelTank;
        public double FuelTank
        {
            get { return fuelTank; }
            set {
                if (value < MaxTankCapacity) fuelTank = value;
                else fuelTank = MaxTankCapacity;
            }
        }
        public double SpentFuel { get; set; }
        public int MaxTankCapacity { get; }
        public  TypeOfArgMach Type { get; set; }
        public string TypeStr { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string StartTimeStr { get; set; }
        public string EndTimeStr { get; set; }

        public bool IsOnTheWork { get; set; }

        public void isWorkingNowCheck()
        {
            if (FuelTank != 0)
            {
                DateTime Now = DateTime.Now;
                if (StartTime < EndTime)
                {
                    if ((StartTime.Hour < Now.Hour) && (EndTime.Hour > Now.Hour))
                    {
                        IsOnTheWork = true;
                        return;

                    }
                    else if (StartTime.Hour == Now.Hour)
                    {
                        if (StartTime.Minute < Now.Minute)
                        {
                            IsOnTheWork = true;
                            return;
                        }
                    }
                    else if (EndTime.Hour == Now.Hour)
                    {
                        if (EndTime.Minute > Now.Minute) 
                        {
                            IsOnTheWork = true;
                            return;
                        }
                    }
                }
                else
                {
                    if ((StartTime.Hour > Now.Hour) && (EndTime.Hour < Now.Hour))
                    {
                        IsOnTheWork = false;
                    }
                    else if (StartTime.Hour == Now.Hour)
                    {
                        if (StartTime.Minute < Now.Minute)
                        {
                            IsOnTheWork = true;
                            return;
                        }
                    }
                    else if (EndTime.Hour == Now.Hour)
                    {
                        if (EndTime.Minute > Now.Minute)
                        {
                            IsOnTheWork = true;
                            return;
                        }
                    }
                    else
                    {
                        IsOnTheWork = true;
                        return;
                    }

                }

            }
            IsOnTheWork = false;


        }

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
