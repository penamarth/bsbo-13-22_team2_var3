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
    public List<Пациент> Пациенты { get; private set; }
    private Dictionary<DateTime, List<TimeSpan>> доступныеВремена;
    private List<IObserver> observers;

    public Врач Врач { get; private set; }
    public Администратор Администратор { get; private set; }

    public Поликлиника()
    {
        Пациенты = new List<Пациент>();
        доступныеВремена = new Dictionary<DateTime, List<TimeSpan>>();
        observers = new List<IObserver>();
    }

    public void НазначитьВрача(string фио)
    {
        Врач = new Врач { ФИО = фио };
        Attach(Врач);
        Console.WriteLine($"Назначен врач: {Врач.ФИО}");
    }

    public void НазначитьАдминистратора(string фио)
    {
        Администратор = new Администратор { ФИО = фио };
        Attach(Администратор);
        Console.WriteLine($"Назначен администратор: {Администратор.ФИО}");
    }

    public void ПостановкаНаУчет(string имя, string фамилия, string отчество, DateTime датаРождения)
    {
        int id = Пациенты.Count + 1;
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
        NotifyObservers("Добавлен новый пациент.");
    }

    public void ПоказатьПациентов()
    {
        Console.WriteLine("Список пациентов:");
        foreach (var пациент in Пациенты)
        {
            Console.WriteLine($"{пациент.ID}: {пациент.Фамилия} {пациент.Имя} {пациент.Отчество}, Дата рождения: {пациент.ДатаРождения.ToShortDateString()}");
        }
    }

    public void СнятьСУчета()
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
                NotifyObservers("Пациент снят с учета.");
            }
            else
            {
                Console.WriteLine("Пациент не найден.");
            }
        }
        else
        {
            Console.WriteLine("Некорректный ввод. Пожалуйста, введите правильный ID.");
        }
    }

    public void ЗаписьНаПрием(int patientId, DateTime дата, TimeSpan время)
    {
        Пациент пациент = Пациенты.FirstOrDefault(p => p.ID == patientId);
        if (пациент == null)
        {
            Console.WriteLine("Пациент не найден.");
            return;
        }

        if (Врач == null)
        {
            Console.WriteLine("Врач не назначен.");
            return;
        }

        if (ПроверитьДоступность(дата, время))
        {
            var запись = new ЗаписьНаПрием(пациент, Врач, дата, время);
            запись.Attach(пациент);
            запись.Attach(Врач);
            запись.ПодтвердитьЗапись();
            ОбновитьДоступностьПослеЗаписи(дата, время);
        }
        else
        {
            Console.WriteLine("Выбранное время недоступно.");
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

    public void Update(string message)
    {
        Console.WriteLine($"[Уведомление для пациента] {Имя} {Фамилия}: {message}");
    }
}

public class Врач : IObserver
{
    public string ФИО { get; set; }

    public void Update(string message)
    {
        Console.WriteLine($"[Уведомление для врача] {ФИО}: {message}");
    }
}

public class Администратор : IObserver
{
    public string ФИО { get; set; }

    public void Update(string message)
    {
        Console.WriteLine($"[Уведомление для администратора] {ФИО}: {message}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Поликлиника поликлиника = new Поликлиника();

        поликлиника.НазначитьВрача("Иванов Иван Иванович");
        поликлиника.НазначитьАдминистратора("Сидоров Алексей Петрович");

        while (true)
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Добавить пациента на учет");
            Console.WriteLine("2. Показать пациентов");
            Console.WriteLine("3. Снять пациента с учета");
            Console.WriteLine("4. Записать пациента на прием");
            Console.WriteLine("0. Выход");

            string выбор = Console.ReadLine();
            switch (выбор)
            {
                case "1":
                    Console.WriteLine("Введите имя:");
                    string имя = Console.ReadLine();
                    Console.WriteLine("Введите фамилию:");
                    string фамилия = Console.ReadLine();
                    Console.WriteLine("Введите отчество:");
                    string отчество = Console.ReadLine();
                    Console.WriteLine("Введите дату рождения (гггг,мм,дд):");
                    DateTime датаРождения = DateTime.Parse(Console.ReadLine());
                    поликлиника.ПостановкаНаУчет(имя, фамилия, отчество, датаРождения);
                    break;

                case "2":
                    поликлиника.ПоказатьПациентов();
                    break;

                case "3":
                    поликлиника.СнятьСУчета();
                    break;

                case "4":
                    Console.WriteLine("Введите ID пациента:");
                    int patientId = int.Parse(Console.ReadLine());
                    Console.WriteLine("Введите дату приема (гггг,мм,дд):");
                    DateTime дата = DateTime.Parse(Console.ReadLine());
                    Console.WriteLine("Введите время приема (чч:мм):");
                    TimeSpan время = TimeSpan.Parse(Console.ReadLine());
                    поликлиника.ЗаписьНаПрием(patientId, дата, время);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
