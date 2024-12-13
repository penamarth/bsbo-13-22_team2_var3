
using System;
using System.Collections.Generic;
using System.Linq;

public class Поликлиника
{
    public List<Пациент> Пациенты { get; set; }
    private Dictionary<DateTime, List<TimeSpan>> доступныеВремена;

    public Поликлиника()
    {
        Пациенты = new List<Пациент>();
        доступныеВремена = new Dictionary<DateTime, List<TimeSpan>>();
    }

    public void ПостановкаНаУчет(Администратор администратор, string имя, string фамилия, string отчество, DateTime датаРождения)
    {
        var новыйПациент = new Пациент
        {
            ID = Пациенты.Count + 1,
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

    public void СнятиеСУчета(Администратор администратор)
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

    public bool ПроверитьИЗаписать(Администратор администратор, Пациент пациент, DateTime дата, TimeSpan время)
    {
        if (ПроверитьДоступность(дата, время))
        {
            администратор.ПодтвердитьЗапись(пациент, дата, время);
            ОбновитьДоступностьПослеЗаписи(дата, время);

            // Уведомление для пациента
            Console.WriteLine($"Уведомление для пациента: Вы записаны на приём к врачу на {дата.ToShortDateString()} в {время}.");

            // Уведомление для врача
            Console.WriteLine($"Уведомление для врача: У вас назначен приём с пациентом на {дата.ToShortDateString()} в {время}.");

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
        // Здесь можно добавить логику проверки расписания для подтверждения записи пациента
        return true; // В простейшем варианте всегда возвращаем true
    }
}

public class Пациент
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
}

public class Врач
{
    public string ФИО { get; set; }

    public void ДобавитьЗаписьВКарту(МедКарта медКарта, string запись)
    {
        медКарта.ДобавитьЗапись(запись);
    }
}

public class Администратор
{
    public string ФИО { get; set; }

    public void ПодтвердитьЗапись(Пациент пациент, DateTime дата, TimeSpan время)
    {
        Console.WriteLine($"Запись пациента {пациент.Фамилия} {пациент.Имя} {пациент.Отчество} подтверждена на {дата.ToShortDateString()} в {время}.");
    }

    public void УведомитьОНедоступности()
    {
        Console.WriteLine("Выбранное время недоступно.");
    }

    public void УведомитьОДобавлении()
    {
        Console.WriteLine("Пациент успешно добавлен.");
    }

    public void УведомитьОСуществовании()
    {
        Console.WriteLine("Пациент уже внесен в систему.");
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
}

public class Program
{
    public static void Main(string[] args)
    {
        Поликлиника поликлиника = new Поликлиника();
        Врач врач = new Врач { ФИО = "Иванов Иван" };
        Администратор администратор = new Администратор { ФИО = "Сидоров Алексей" };

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

                        поликлиника.ПостановкаНаУчет(администратор, имя, фамилия, отчество, датаРождения);
                    }
                    else if (выборАдм == "2")
                    {
                        поликлиника.СнятиеСУчета(администратор);
                    }
                    else if (выборАдм == "3")
                    {
                        Console.WriteLine("Введите дату приёма (гггг,мм,дд):");
                        DateTime желаемаяДата = DateTime.Parse(Console.ReadLine());

                       

                        Console.WriteLine("Введите время приёма (чч:мм):");
                        TimeSpan желаемоеВремя = TimeSpan.Parse(Console.ReadLine());

                        Console.WriteLine("Введите ID пациента:");
                        int patientId = int.Parse(Console.ReadLine());
                        var пациент = поликлиника.Пациенты.FirstOrDefault(p => p.ID == patientId);
                        if (пациент != null)
                        {
                            if (!поликлиника.ПроверитьИЗаписать(администратор, пациент, желаемаяДата, желаемоеВремя))
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

                        Пациент пациент = new Пациент { Имя = имя, Фамилия = фамилия, Отчество = отчество, ДатаРождения = датаРождения };
                        пациент.ЗапросНаПостановкуНаУчет();
                    }
                    break;

                case "3":
                    Console.WriteLine("Выбрано: Врач.");
                    Console.WriteLine("1. Учет факта приема пациента");
                    string выборВрач = Console.ReadLine();
                    if (выборВрач == "1")
                    {
                        Console.WriteLine("Введите ID пациента:");
                        int patientId = int.Parse(Console.ReadLine());
                        var пациент = поликлиника.Пациенты.FirstOrDefault(p => p.ID == patientId);
                        if (пациент != null)
                        {
                            МедКарта медКарта = new МедКарта();
                            Console.WriteLine("Введите данные осмотра:");
                            string запись = Console.ReadLine();
                            поликлиника.ОбработатьЗапросНаУчетФактаПриема(врач, пациент, медКарта, запись);
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
