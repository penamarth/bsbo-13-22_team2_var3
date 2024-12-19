
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

public class Поликлиника
{
    public List<Пациент> Пациенты { get; private set; }
    private Dictionary<DateTime, List<TimeSpan>> доступныеВремена;
    private List<ЗаписьНаПрием> записиНаПрием;

    public Врач Врач { get; private set; }
    public Администратор Администратор { get; private set; }

    public Поликлиника()
    {
        Пациенты = new List<Пациент>();
        доступныеВремена = new Dictionary<DateTime, List<TimeSpan>>();
        записиНаПрием = new List<ЗаписьНаПрием>();
    }

    public void НазначитьВрача(string фио)
    {
        Врач = new Врач { ФИО = фио };
        Console.WriteLine($"Назначен врач: {Врач.ФИО}");
    }

    public void НазначитьАдминистратора(string фио)
    {
        Администратор = new Администратор { ФИО = фио };
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
            запись.Attach(Администратор);
            запись.ПодтвердитьЗапись();
            ОбновитьДоступностьПослеЗаписи(дата, время);
            записиНаПрием.Add(запись);
        }
        else
        {
            Console.WriteLine("Выбранное время недоступно.");
        }
    }


    public void ИзменитьВремяЗаписи(string имяПациента, DateTime новаяДата, TimeSpan новоеВремя)
    {
        var запись = записиНаПрием.FirstOrDefault(z => z.Пациент.Имя == имяПациента);
        if (запись != null && ПроверитьДоступность(новаяДата, новоеВремя))
        {
            обновитьДоступностьПослеОтмены(запись.Дата, запись.Время);
            запись.ИзменитьВремя(новаяДата, новоеВремя);
            ОбновитьДоступностьПослеЗаписи(новаяДата, новоеВремя);
        }
        else
        {
            Console.WriteLine("Запись не найдена или выбранное время недоступно.");
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
        if (!доступныеВремена.ContainsKey(дата))
        {
            доступныеВремена[дата] = new List<TimeSpan>();
        }

        доступныеВремена[дата].Remove(время);
    }

    private void обновитьДоступностьПослеОтмены(DateTime дата, TimeSpan время)
    {
        if (!доступныеВремена.ContainsKey(дата))
        {
            доступныеВремена[дата] = new List<TimeSpan>();
        }

        доступныеВремена[дата].Add(время);
    }
}

public class ЗаписьНаПрием : ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    public Пациент Пациент { get; private set; }
    public Врач Врач { get; private set; }
    public DateTime Дата { get; private set; }
    public TimeSpan Время { get; private set; }

    public ЗаписьНаПрием(Пациент пациент, Врач врач, DateTime дата, TimeSpan время)
    {
        this.Пациент = пациент;
        this.Врач = врач;
        this.Дата = дата;
        this.Время = время;
    }

    public void ПодтвердитьЗапись()
    {
        string message = $"Запись на прием: Пациент {Пациент.Имя} {Пациент.Фамилия} к врачу {Врач.ФИО} на {Дата.ToShortDateString()} в {Время}.";
        NotifyObservers(message);
    }

    public void ИзменитьВремя(DateTime новаяДата, TimeSpan новоеВремя)
    {
        Дата = новаяДата;
        Время = новоеВремя;
        string message = $"Изменено время записи: Пациент {Пациент.Имя} {Пациент.Фамилия} к врачу {Врач.ФИО} на {новаяДата.ToShortDateString()} в {новоеВремя}.";
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
            Console.WriteLine("5. Изменить время записи");
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
                    Console.WriteLine("Введите дату рождения (гггг, мм, дд):");
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
                    Console.WriteLine("Введите дату приема (гггг, мм, дд):");
                    DateTime дата = DateTime.Parse(Console.ReadLine());
                    Console.WriteLine("Введите время приема (чч:мм):");
                    TimeSpan время = TimeSpan.Parse(Console.ReadLine());
                    поликлиника.ЗаписьНаПрием(patientId, дата, время);
                    break;

                case "5":
                    Console.WriteLine("Введите имя пациента для изменения времени записи:");
                    string имяПациента = Console.ReadLine();
                    Console.WriteLine("Введите новую дату приема (гггг, мм, дд):");
                    DateTime новаяДата = DateTime.Parse(Console.ReadLine());
                    Console.WriteLine("Введите новое время приема (чч:мм):");
                    TimeSpan новоеВремя = TimeSpan.Parse(Console.ReadLine());
                    поликлиника.ИзменитьВремяЗаписи(имяПациента, новаяДата, новоеВремя);
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
