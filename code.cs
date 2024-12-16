
using System;
using System.Collections.Generic;
using System.Linq;

public interface IObserver
{
    void Update(string message);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void NotifyObservers(string message);
}

public class Поликлиника : ISubject
{
    public List<Пациент> Пациенты { get; set; }
    private Dictionary<DateTime, List<TimeSpan>> доступныеВремена;
    private List<IObserver> observers;

    public Поликлиника()
    {
        Пациенты = new List<Пациент>();
        доступныеВремена = new Dictionary<DateTime, List<TimeSpan>>();
        observers = new List<IObserver>();
    }

    public void ПостановкаНаУчет(Администратор администратор, string имя, string фамилия, string отчество, DateTime датаРождения, int id)
    {
        var новыйПациент = new Пациент
        {
            ID = id,
            Имя = имя,
            Фамилия = фамилия,
            Отчество = отчество,
            ДатаРождения = датаРождения
        };
        Пациенты.Add(новыйПациент);
        Console.WriteLine($"{новыйПациент.Фамилия} {новыйПациент.Имя} {новыйПациент.Отчество} добавлен(а) на учет.");
        администратор.УведомитьОДобавлении();
    }

    public void ПоказатьПациентов()
    {
        Console.WriteLine("Список пациентов:");
        foreach (var пациент in Пациенты)
        {
            Console.WriteLine($"{пациент.ID}: {пациент.Фамилия} {пациент.Имя} {пациент.Отчество}, Дата рождения: {пациент.ДатаРождения.ToShortDateString()}");
        }
    }

    public void СнятьСУчета(Администратор администратор)
    {
        ПоказатьПациентов();

        Console.WriteLine("Введите ID пациента для снятия с учета:");
        if (int.TryParse(Console.ReadLine(), out int пациентID))
        {
            Пациент пациент = Пациенты.FirstOrDefault(p => p.ID == пациентID);

            if (пациент != null)
            {
                Пациенты.Remove(пациент);
                Console.WriteLine($"Пациент {пациент.Фамилия} {пациент.Имя} {пациент.Отчество} снят с учета.");
                администратор.УведомитьОСнятии();
            }
            else
            {
                Console.WriteLine("Пациент не найден.");
                администратор.УведомитьОНеверномСнятии();
            }
        }
        else
        {
            Console.WriteLine("Некорректный ввод. Пожалуйста, введите правильный ID.");
        }
    }

    public bool ЗаписьНаПрием(Администратор администратор, Пациент пациент, Врач врач, DateTime дата, TimeSpan время)
    {
        if (ПроверитьДоступность(дата, время))
        {
            var запись = new ЗаписьНаПрием(пациент, врач, дата, время);
            запись.Attach(пациент);
            запись.Attach(врач);
            запись.ПодтвердитьЗапись();

            ОбновитьДоступностьПослеЗаписи(дата, время);
            return true;
        }
        else
        {
            администратор.УведомитьОНедоступности();
            return false;
        }
    }

    private bool ПроверитьДоступность(DateTime дата, TimeSpan время)
    {
        if (доступныеВремена.ContainsKey(дата))
        {
            return доступныеВремена[дата].Contains(время);
        }
        return true;
    }

    private void ОбновитьДоступностьПослеЗаписи(DateTime дата, TimeSpan время)
    {
        if (доступныеВремена.ContainsKey(дата))
        {
            доступныеВремена[дата].Remove(время);
        }
    }

    public void ОбработатьЗапросНаУчетФактаПриема(Врач врач, Пациент пациент, МедКарта медКарта, string запись)
    {
        if (ПроверитьЗаписьПациента(пациент))
        {
            врач.ДобавитьЗаписьВКарту(медКарта, $"{DateTime.Now}: {запись}");
            Console.WriteLine("Учет факта приема завершен.");
        }
        else
        {
            Console.WriteLine("Пациент не записан.");
        }
    }
    private bool ПроверитьЗаписьПациента(Пациент пациент)
    {
        return Пациенты.Contains(пациент);
    }

    public void Attach(IObserver observer)
    {
        observers.Add(observer);
    }

    public void Detach(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void NotifyObservers(string message)
    {
        foreach (var observer in observers)
        {
            observer.Update(message);
        }
    }
}

public class ЗаписьНаПрием : ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    private Пациент пациент;
    private Врач врач;
    private DateTime дата;
    private TimeSpan время;

    public ЗаписьНаПрием(Пациент пациент, Врач врач, DateTime дата, TimeSpan время)
    {
        this.пациент = пациент;
        this.врач = врач;
        this.дата = дата;
        this.время = время;
    }


    public void ПодтвердитьЗапись()
    {
        string message = $"Запись на прием: Пациент {пациент.Имя} {пациент.Фамилия} к врачу {врач.ФИО} на {дата.ToShortDateString()} в {время}.";
        NotifyObservers(message);
    }

    public void Attach(IObserver observer)
    {
        observers.Add(observer);
    }

    public void Detach(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void NotifyObservers(string message)
    {
        foreach (var observer in observers)
        {
            observer.Update(message);
        }
    }
}


public class Пациент : IObserver
{
    public int ID { get; set; }
    public string Имя { get; set; }
    public string Фамилия { get; set; }
    public string Отчество { get; set; }
    public DateTime ДатаРождения { get; set; }

    public void ЗапросНаПостановкуНаУчет()
    {
        Console.WriteLine($"{Фамилия} {Имя} {Отчество} желает встать на учет.");
    }

    public void Update(string message)
    {
        Console.WriteLine($"[Уведомление для пациента] {Имя} {Фамилия}: {message}");
    }
}

public class Врач : IObserver
{
    public string ФИО { get; set; }

    public void ДобавитьЗаписьВКарту(МедКарта медКарта, string запись)
    {
        медКарта.ДобавитьЗапись(запись);
    }

    public void Update(string message)
    {
        Console.WriteLine($"[Уведомление для врача] {ФИО}: {message}");
    }
}

public class Администратор
{
    public string ФИО { get; set; }

    public void ПодтвердитьЗапись(Пациент пациент, DateTime дата, TimeSpan время)
    {
        Console.WriteLine($"Запись подтверждена для пациента {пациент.Имя} {пациент.Фамилия} на {дата.ToShortDateString()} в {время}.");
    }

    public void УведомитьОНедоступности()
    {
        Console.WriteLine("Выбранное время недоступно.");
    }

    public void УведомитьОДобавлении()
    {
        Console.WriteLine("Пациент успешно добавлен.");
    }

    public void УведомитьОСнятии()
    {
        Console.WriteLine("Пациент успешно снят с учета.");
    }

    public void УведомитьОНеверномСнятии()
    {
        Console.WriteLine("Не удалось снять с учета. Пациент не найден.");
    }
}

public class МедКарта
{
    public List<string> Записи { get; set; }

    public МедКарта()
    {
        Записи = new List<string>();
    }

    public void ДобавитьЗапись(string запись)
    {
        Записи.Add(запись);
        Console.WriteLine($"Добавлена запись: {запись}");
    }

    public void ПоказатьЗаписи()
    {
        Console.WriteLine("Записи в медицинской карте:");
        foreach (var запись in Записи)
        {
            Console.WriteLine(запись);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        int id = 0;
        Поликлиника поликлиника = new Поликлиника();
        Врач врач = new Врач { ФИО = "Иванов Иван" };
        Пациент пациент = new Пациент { Имя = "Иван", Фамилия = "Иванов", Отчество = "Иванович" };
        Администратор администратор = new Администратор { ФИО = "Сидоров Алексей" };

        поликлиника.Пациенты.Add(пациент);

        while (true)
        {
            Console.WriteLine("Выберите пользователя:");
            Console.WriteLine("1. Администратор");
            Console.WriteLine("2. Пациент");
            Console.WriteLine("3. Врач");
            Console.WriteLine("0. Выход");

            string выбор = Console.ReadLine();
            switch (выбор)
            {
                case "1":
                    Console.WriteLine("Выбрано: Администратор.");
                    Console.WriteLine("1. Поставить на учет");
                    Console.WriteLine("2. Снять с учета");
                    Console.WriteLine("3. Запись пациента на приём");
                    Console.WriteLine("4. Показать список пациентов");
                    string выборАдм = Console.ReadLine();
                    if (выборАдм == "1")
                    {
                        Console.WriteLine("Введите имя:");
                        string имя = Console.ReadLine();
                        Console.WriteLine("Введите фамилию:");
                        string фамилия = Console.ReadLine();
                        Console.WriteLine("Введите отчество:");
                        string отчество = Console.ReadLine();
                        Console.WriteLine("Введите дату рождения (гггг,мм,дд):");
                        DateTime датаРождения = DateTime.Parse(Console.ReadLine());
                        id = id + 1;
                        поликлиника.ПостановкаНаУчет(администратор, имя, фамилия, отчество, датаРождения, id);
                    }
                    else if (выборАдм == "2")
                    {
                        поликлиника.СнятьСУчета(администратор);
                    }
                    else if (выборАдм == "3")
                    {
                        Console.WriteLine("Введите дату приёма (гггг,мм,дд):");
                        DateTime желаемаяДата = DateTime.Parse(Console.ReadLine());

                        Console.WriteLine("Введите время приёма (чч:мм):");
                        TimeSpan желаемоеВремя = TimeSpan.Parse(Console.ReadLine());

                        Console.WriteLine("Введите ID пациента:");
                        int patientId = int.TryParse(Console.ReadLine(), out patientId) ? patientId : -1;
                        var выбранныйПациент = поликлиника.Пациенты.FirstOrDefault(p => p.ID == patientId);

                        if (выбранныйПациент != null)
                        {
                            if (!поликлиника.ЗаписьНаПрием(администратор, выбранныйПациент, врач, желаемаяДата, желаемоеВремя))
                            {
                                Console.WriteLine("Попробуйте другое время.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Пациент с таким ID не найден.");
                        }
                    }
                    else if (выборАдм == "4")
                    {
                        поликлиника.ПоказатьПациентов();
                    }
                    break;

                case "2":
                    Console.WriteLine("Выбрано: Пациент.");
                    Console.WriteLine("1. Запрос на постановку на учет");
                    string выборПац = Console.ReadLine();
                    if (выборПац == "1")
                    {
                        Console.WriteLine("Введите имя:");
                        string имя = Console.ReadLine();
                        Console.WriteLine("Введите фамилию:");
                        string фамилия = Console.ReadLine();
                        Console.WriteLine("Введите отчество:");
                        string отчество = Console.ReadLine();
                        Console.WriteLine("Введите дату рождения (гггг,мм,дд):");
                        DateTime датаРождения = DateTime.Parse(Console.ReadLine());

                        Пациент новыйПациент = new Пациент { Имя = имя, Фамилия = фамилия, Отчество = отчество, ДатаРождения = датаРождения };
                        новыйПациент.ЗапросНаПостановкуНаУчет();
                    }
                    break;
                case "3":
                    Console.WriteLine("Выбрано: Врач.");
                    Console.WriteLine("1. Учет факта приема пациента");
                    string выборВрач = Console.ReadLine();
                    if (выборВрач == "1")
                    {
                        Console.WriteLine("Введите ID пациента:");
                        int patientId = int.TryParse(Console.ReadLine(), out patientId) ? patientId : -1;
                        var выбралПациент = поликлиника.Пациенты.FirstOrDefault(p => p.ID == patientId);

                        if (выбралПациент != null)
                        {
                            Console.WriteLine("Введите данные осмотра:");
                            string запись = Console.ReadLine();
                            МедКарта медКарта = new МедКарта();
                            поликлиника.ОбработатьЗапросНаУчетФактаПриема(врач, выбралПациент, медКарта, запись);
                        }
                        else
                        {
                            Console.WriteLine("Пациент с таким ID не найден.");
                        }
                    }
                    break;

                case "0":
                    Console.WriteLine("Выход.");
                    return;

                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в меню...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}