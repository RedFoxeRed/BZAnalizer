using BZAnalizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig.AcroForms.Fields;

namespace BZAnalizer.Service
{
    public static class ElementHandler
    {
        public static Dictionary<string, Action<WorkElement>> MethodsForElements = new Dictionary<string, Action<WorkElement>>()
            {
                { "Вентилятор", UpdateInfo_FAN },
                { "Приточный вентилятор", UpdateInfo_FAN },
                { "Вытяжной вентилятор", UpdateInfo_FAN },
                { "Панель с клапаном", UpdateInfo_CLAPAN },
                { "Клапан рециркуляционный", UpdateInfo_CLAPAN },
                { "Блок для установки фильтрующих вставок", UpdateInfo_OTHER},
                { "Фильтр", UpdateInfo_OTHER },
                { "Нагреватель жидкостный", UpdateInfo_OTHER },
                { "Испаритель", UpdateInfo_OTHER },
                { "Прогрев", UpdateInfo_PROGREV },
                { "Освещение", UpdateInfo_LIGHT },
                { "Насос циркуляционный", UpdateInfo_NASOS },
                { "Электропривод клапана", UpdateInfo_UVS },
                { "ЭК", UpdateInfo_EK }
            };

        private static void UpdateInfo_FAN(WorkElement element)
        {
            string check24VorSK = "24В=";
            string semistr = "";

            element.description = "Supply fan /Вентилятор\r\nприточный";
            element.article = "fan";
            element.fullnameElement = "Вент." + check24VorSK + "" + semistr;

            string powerfull = element.parameters.FirstOrDefault(x => x.name == "Мощность двигателя (кВт)").text.Trim() + " кВт; ";
            string nominalA = element.parameters.FirstOrDefault(x => x.name == "Номинальный ток (А)").text.Trim().Replace("Опции", "").Replace("\n", "") + "А; ";
            string fase = "1Ф; ";
            if (element.parameters.FirstOrDefault(x => x.name == "Параметры электропитания (В - тип)").text.Trim().Contains("400"))
                fase = "3Ф; ";

            string kpd = (Convert.ToDouble(element.parameters.FirstOrDefault(x => x.name == "КПД электродвигателя (%)").text.Replace(".", ",").Trim()) / 100).ToString() + "КПД; ";
            string ks = Math.Round((Convert.ToDouble(element.parameters.FirstOrDefault(x => x.name == "Потреб. мощность рабочего колеса (кВт)").text.Replace(".", ",").Trim()) / Convert.ToDouble(element.parameters.FirstOrDefault(x => x.name == "Мощность двигателя (кВт)").text.Replace(".", ",").Trim())), 2).ToString() + "Кс";
            element.stringParameters = (powerfull + nominalA + fase + kpd + ks).Replace(".", ",");

            if(!element.parameters.FirstOrDefault(x => x.name == "Светильник").text.Contains("ет"))
            {
                List<MainBlock> lightParams = new List<MainBlock>();
                lightParams.Add(new MainBlock("Мощность (Вт)", "10"));
                element.childrenElements.Add(new WorkElement("Освещение", "", lightParams, element.MainOrReserve));
            }
            element.printForSila = true;
        }
        private static void UpdateInfo_CLAPAN(WorkElement element)
        {
            if (element.parameters.FirstOrDefault(x => x.name == "Номинальное напряжение (В)") != null)
            {
                if (!element.parameters.FirstOrDefault(x => x.name == "Номинальное напряжение (В)").text.Contains("24"))
                {
                    element.printForSila = true;
                }
            }

            element.description = "Air valve with electric\r\ndrive /Клапан воздушный с\r\nэл. Приводом";
            element.article = "clapan";

            element.fullnameElement = "Заслонка " + element.parameters.FirstOrDefault(x => x.name == "Номинальное напряжение (В)").text.Trim() + "В";

            if (element.parameters.FirstOrDefault(x => x.name == "Возвратная пружина") != null)
            {
                if (element.parameters.FirstOrDefault(x => x.name == "Возвратная пружина").text.ToLower().Contains("есть"))
                {
                    element.fullnameElement += " с пружиной (упр. 24В=)";
                }
                else
                {
                    element.fullnameElement += " без пружины (упр. 24В=)";
                }
            }
            element.stringParameters = (Convert.ToInt32(element.parameters.FirstOrDefault(x => x.name == "Потребляемая мощность (Вт)").text.Trim()) * Convert.ToInt32(element.parameters.FirstOrDefault(x => x.name == "Количество приводов на 1 клапан (шт)").text.Trim())).ToString() + " Вт";

            if (!element.parameters.FirstOrDefault(x => x.name == "Мощность обогрева (Вт)").text.Contains("-"))
            {
                List<MainBlock> progrevParams = new List<MainBlock>();
                progrevParams.Add(new MainBlock("Мощность обогрева (Вт)", element.parameters.FirstOrDefault(x => x.name == "Мощность обогрева (Вт)").text));
                element.childrenElements.Add(new WorkElement("Прогрев", "", progrevParams, element.MainOrReserve));
            }

        }
        private static void UpdateInfo_PROGREV(WorkElement element)
        {
            string div_uzo = "(Диф)";
            string check24VorSK = "24В=";

            element.description = "Heating of the valve /\r\nПодогрев клапана";
            element.article = "progrev";
            element.fullnameElement = "Прогрев " + div_uzo + " " + check24VorSK;
            element.stringParameters = element.parameters.FirstOrDefault(x => x.name == "Мощность обогрева (Вт)").text.Trim() + " Вт; " + "1Ф";
            element.printForSila = true;
        }
        private static void UpdateInfo_LIGHT(WorkElement element)
        {
            element.description = "Освещение/Lighting";
            element.article = "light";
            element.fullnameElement = "Освещение";
            element.stringParameters = "10 Вт";
            element.printForSila = true;
        }
        public static void UpdateInfo_NASOS(WorkElement element)
        {
            string check24VorSK = "24В=";

            string pwr = (Convert.ToDouble(element.parameters.FirstOrDefault(x => x.name == "Потребляемая мощность (Вт)").text.Trim()) / 1000) + "кВт; ";

            string fase = "1Ф; ";

            if (element.parameters.FirstOrDefault(x => x.name == "Параметры электропитания (В) - тип").text.ToLower().Contains("400"))
                fase = "3Ф; ";

            string denomination = element.parameters.FirstOrDefault(x => x.name == "Сила тока (A)").text + "А";

            element.description = "Pump/\r\nНасос";
            element.article = "nasos";
            element.fullnameElement = "Насос " + check24VorSK;

            element.stringParameters = pwr + fase + denomination;
            element.printForSila = true;

        }
        public static void UpdateInfo_UVS(WorkElement element)
        {
            if (element.parameters.FirstOrDefault(x => x.name == "Напряжение питания (В)").text.Contains("230"))
                element.printForSila = true;

            element.description = "Water valve with electric\r\ndrive /Клапан водяной с\r\nэл. Приводом";
            element.article = "clapanUVS";

            element.fullnameElement = "Заслонка 230В без пружины (упр. 24В=)";

            element.stringParameters = element.parameters.FirstOrDefault(x => x.name == "Потребляемая мощность (Вт)").text.Trim() + " Вт";

        }
        public static void UpdateInfo_EK(WorkElement element)
        {
            string check24VorSK = "24В=";


            element.description = $"ЭК Ступень {element.parameters.FirstOrDefault(x => x.name == "Номер ступени").text}";

            element.article = $"ekStep{element.parameters.FirstOrDefault(x => x.name == "Номер ступени").text}";
            element.fullnameElement = "ЭК " + check24VorSK;
            element.stringParameters = "3Ф; " + element.parameters.FirstOrDefault(x => x.name == "Мощность ступени").text + "кВт";

            element.printForSila = true;
        }
        private static void UpdateInfo_OTHER(WorkElement element)
        {

        }

        public static void CheckCHPVent(List<WorkElement> elements)
        {
            var fans = elements.Where(el => el.name == "Вентилятор" || el.name == "Приточный вентилятор" || el.name == "Вытяжной вентилятор").ToList();

            string sinusBypass = "с синусом";

            if (fans.Count > 0) 
            {
                var CHPs = elements.Where(el => el.name.Contains("астотный преоб")).ToList();

                if(CHPs.Count > 0)
                {
                    List<double> kvts = new List<double>();

                    foreach(var chp in CHPs)
                    {
                        int countEl = Convert.ToInt32(chp.parameters.FirstOrDefault(x => x.name == "Кол-во ЧП").text);

                        for (int i = 0; i < countEl; i++)
                        {
                            kvts.Add(Convert.ToDouble(chp.parameters.FirstOrDefault(x => x.name == "Мощность ЧП").text));
                        }
                    }
                    kvts = kvts.OrderByDescending(x => x).ToList();

                    var fansSort = fans.OrderByDescending(fan =>
                    {
                        // Получаем значение параметра "Мощность двигателя"
                        var powerValue = fan.parameters.FirstOrDefault(x => x.name == "Мощность двигателя (кВт)").text;

                        // Пытаемся преобразовать строку в double
                        if (double.TryParse(powerValue.Replace(".", ","), out double power))
                        {
                            return power; // Возвращаем числовое значение
                        }

                        // Если преобразование не удалось, возвращаем значение по умолчанию (например, 0)
                        return 100;
                    }).ToList();
                    int ind = 0;

                    foreach (var f in fansSort)
                    {
                        f.powerfull = kvts[ind].ToString() + "кВт";
                        f.fullnameElement = "ЧП " + CHPs[0].parameters.FirstOrDefault(x => x.name == "Положение ЧП").text.ToLower().Replace("в шкафу", "внутри") + " " + sinusBypass;
                        f.printForSila = true;
                        ind++;

                    }

                    foreach(var chpRemove in CHPs)
                    {
                        elements.Remove(chpRemove);
                    }
                }
            }
        }

    }
}
