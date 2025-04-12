﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZAnalizer.Models
{
    public static class DataFields
    {

        #region MainBlockNames
        public static List<string> mainBLockNames = new List<string>
        {
            "Воздушный центральный кондиционер",
            "Габаритные размеры установки",
            "Резервная установка",
            "Рабочая установка",
            "Контрольно-измерительные приборы и элементы автоматики",
            "Требование к автоматике и электромонтажу",
            "Узел водосмесительный",
            "Уровень шума",
            "Компрессорный блок общепромышленного исполнения"
        };

        public static List<string> elementBlockNames = new List<string>
        {
            "Панель с клапаном",
            "Вентилятор",
            "Фильтр",
            "Блок для установки фильтрующих вставок",
            "Нагреватель жидкостный",
            "Испаритель",
            "Приточный вентилятор"
        };
        #endregion

        #region ElementBlockParameters

        public static Dictionary<string, List<string>> BlockParameters = new Dictionary<string, List<string>>()
        {
            {
                "Приточный вентилятор",
                new List<string>
                {
                    "Количество полюсов",
                    "Мощность двигателя (кВт)",
                    "Параметры электропитания (В - тип)",
                    "Частота (об/мин)",
                    "Номинальный ток (А)",
                    "КПД электродвигателя (%)",
                    "КПД рабочего колеса (%)",
                    "Светильник",
                    "Смотровой люк",
                    "Манометр",
                    "Концевой выключатель",
                    "Пакетный выключатель",
                    "Тип двигателя",
                    "Падение давления в агрегате (Па)",
                    "Статическое давление (Па)",
                    "Динамическое давление (Па)",
                    "Потреб. мощность рабочего колеса (кВт)",
                    "Пусковой ток (А)",
                    "Расход воздуха (м³/ч)",
                    "Тип лопаток",
                    "Материал рабочего колеса",
                    "Скорость вращения (об/мин)",
                    "Рабочая частота сети (Гц)",
                    "Эффективность вентилятора (%)",
                    "Ресурс работы подшипников (ч)",
                    "Класс энергоэффективности",
                    "Диаметр посадочного отверстия (мм)",
                    "Защита от перегрева обмотки",
                    "Класс защиты",
                    "Полное давление (Па)",
                    "Макс.скорость вращения (об/мин)"
                }
            },
            {
                "Вентилятор",
                new List<string>
                {
                    "Количество полюсов",
                    "Мощность двигателя (кВт)",
                    "Параметры электропитания (В - тип)",
                    "Частота (об/мин)",
                    "Номинальный ток (А)",
                    "КПД электродвигателя (%)",
                    "КПД рабочего колеса (%)",
                    "Светильник",
                    "Смотровой люк",
                    "Манометр",
                    "Концевой выключатель",
                    "Пакетный выключатель",
                    "Тип двигателя",
                    "Падение давления в агрегате (Па)",
                    "Статическое давление (Па)",
                    "Динамическое давление (Па)",
                    "Потреб. мощность рабочего колеса (кВт)",
                    "Пусковой ток (А)",
                    "Расход воздуха (м³/ч)",
                    "Тип лопаток",
                    "Материал рабочего колеса",
                    "Скорость вращения (об/мин)",
                    "Рабочая частота сети (Гц)",
                    "Эффективность вентилятора (%)",
                    "Ресурс работы подшипников (ч)",
                    "Класс энергоэффективности",
                    "Диаметр посадочного отверстия (мм)",
                    "Защита от перегрева обмотки",
                    "Класс защиты",
                    "Полное давление (Па)",
                    "Макс.скорость вращения (об/мин)"
                }
            },
            {
                "Фильтр",
                new List<string>
                {
                    "Тип фильтра",
                    "Расход воздуха (м³/ч)",
                    "Начальное сопр. (Па)",
                    "Светильник",
                    "Тип крепления фильтров",
                    "Скорость в фильтре (м/с)"
                }
            },
            {
                "Панель с клапаном",
                new List<string>
                {
                    "Расход воздуха (м³/ч)",
                    "Модель привода",
                    "Количество приводов на 1 клапан (шт)",
                    "Номинальное напряжение (В)",
                    "Потребляемая мощность (Вт)",
                    "Возвратная пружина",
                    "Мощность обогрева (Вт)",
                    "Обратная связь",
                    "Время поворота пружины (с)",
                    "Материал корпуса",
                    "Периметральный обогрев",
                    "Расположение привода"
                }
            },
            {
                "Блок для установки фильтрующих вставок",
                new List<string>
                {
                    "Тип крепления фильтров",
                    "Расход воздуха (м³/ч)"
                }
            },
            {
                "Испаритель",
                new List<string>
                {
                    "Расход воздуха (м³/ч)",
                    "Полезная производительность (кВт)",
                    "Количество контуров (шт)"
                }
            },
            {
                "Нагреватель жидкостный",
                new List<string>
                {
                    "Расход воздуха (м³/ч)",
                    "Температура жидкости на выходе (°C)",
                    "Полезная производительность (кВт)"
                }
            },
            {
                "Контрольно-измерительные приборы и элементы автоматики",
                new List<string>
                {
                    "Комнатный датчик температуры",
                    "Канальный датчик температуры",
                    "Датчик температуры обратной воды",
                    "Термостат защиты от замораживания по воздуху",
                    "Датчик перепада давления с дисплеем для контроля запыленности фильтра",
                    "Датчик перепада давления с дисплеем для контроля работы вентилятора",
                    "Датчик давления в нагнетательном воздуховоде с монтажным комплектом",
                    "Датчик избыточного давления с дисплеем для контроля работы насоса",
                    "Частотный преобразователь"
                }
            }
        };


        #endregion
    }
}
