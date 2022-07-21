using Xunkong.Hoyolab.TravelNotes;

namespace Xunkong.Desktop.Models;

public class TravelNotesPage_DayOrMonthStats
{
    public int CurrentPrimogems { get; set; }


    public int CurrentMora { get; set; }


    public int LastPrimogems { get; set; }


    public int LastMora { get; set; }


    public string PrimogemsChange => IntToString(CurrentPrimogems - LastPrimogems);

    public string PrimogemsChangeRatio => DoubleToPercentString((CurrentPrimogems - LastPrimogems) / (double)LastPrimogems);

    public string MoraChange => IntToString(CurrentMora - LastMora);

    public string MoraChangeRatio => DoubleToPercentString((CurrentMora - LastMora) / (double)LastMora);

    public double CurrentPrimogemsProgressValue => 100 * (CurrentPrimogems >= LastPrimogems ? 1 : (double)CurrentPrimogems / LastPrimogems);

    public double LastPrimogemsProgressValue => 100 * (LastPrimogems >= CurrentPrimogems ? 1 : (double)LastPrimogems / CurrentPrimogems);

    public double CurrentMoraProgressValue => 100 * (CurrentMora >= LastMora ? 1 : (double)CurrentMora / LastMora);

    public double LastMoraProgressValue => 100 * (LastMora >= CurrentMora ? 1 : (double)LastMora / CurrentMora);


    private static string IntToString(int value)
    {
        return value switch
        {
            > 0 => $"+{value}",
            0 => "0",
            <= 0 => value.ToString(),
        };
    }

    private static string DoubleToPercentString(double value)
    {
        return value switch
        {
            > 0 => $"+{value:P0}",
            0 => "0%",
            <= 0 => value.ToString("P0"),
            _ => "NAN",
        };
    }

}


public class TravelNotesPage_MonthData : TravelNotesMonthData
{
    public string YearMonth => $"{Year % 100:D2}-{Month:D2}";

    public string PrimogemsChangeRateString => PrimogemsChangeRate switch
    {
        > 0 => $"+{PrimogemsChangeRate}%",
        0 => "0%",
        < 0 => $"{PrimogemsChangeRate}%",
    };

    public string MoraChangeRateString => MoraChangeRate switch
    {
        > 0 => $"+{MoraChangeRate}%",
        0 => "0%",
        < 0 => $"{MoraChangeRate}%",
    };
}



public class TravelNotesPage_DayData
{
    public TravelNotesPage_DayData(string date, int primogems, int mora)
    {
        Date = date;
        Primogems = primogems;
        Mora = mora;
    }

    public string Date { get; set; }

    public int Primogems { get; set; }

    public int Mora { get; set; }
}


public record TravelNotesPage_VersionData(string Version, int Primogems, int Mora, int PrimogemsChangeRate, int MoraChangeRate)
{
    public string PrimogemsChangeRateString => PrimogemsChangeRate switch
    {
        > 0 => $"+{PrimogemsChangeRate}%",
        0 => "0%",
        < 0 => $"{PrimogemsChangeRate}%",
    };

    public string MoraChangeRateString => MoraChangeRate switch
    {
        > 0 => $"+{MoraChangeRate}%",
        0 => "0%",
        < 0 => $"{MoraChangeRate}%",
    };
}



public class TravelNotesPage_TravelNotesMonthData
{

    public string Uid { get; set; }

    public string Year { get; set; }

    public string Month { get; set; }

    public string CurrentPrimogems { get; set; }

    public string CurrentMora { get; set; }

    public string LastPrimogems { get; set; }

    public string LastMora { get; set; }

}


public class TravelNotesPage_TravelNotesAwardItem
{

    public string Uid { get; set; }

    public string Time { get; set; }

    public string ActionName { get; set; }

    public string Number { get; set; }

}