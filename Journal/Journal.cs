using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Journal
{
    public partial class FormJournal : Form
    {
        //Флаг выбранна ли дисцыплина
        bool disciplinSelected = false;

        public FormJournal()
        {
            InitializeComponent();
        }

        //Закгрузка окна
        private void FormJournal_Load(object sender, EventArgs e)
        {
            //Зполнение комбовоксов
            StreamReader sr = new StreamReader("Disciplines.txt");//открыть файл с дисциплинами
            var s = sr.ReadToEnd().Trim('\n').Split('\n');//считать весь файл очитстить от лишнего
            comBoxDiscipline.Items.AddRange(s);//добавить все дисциплины в колекцию выплыв списка
            comBoxDiscipline.AutoCompleteCustomSource.AddRange(s);//добавить все дисциплины в колекцию
                                                                  //автодобавления выплыв списка

            //Заолнение ДатаГридВью
            sr = new StreamReader("Students.txt");//открыть файл со студентами
            s = sr.ReadToEnd().Trim('\n').Split('\n');//считать весь файл очитстить от лишнего
            DGVJournal.Columns.Add("Names", "Студенты");//добавить колонку студентов
            for (int i = 0; i < s.Length; i++)
            {
                DGVJournal.Rows.Add(s[i]);//добавить строку с именем студента
            }
            sr.Close();//закрыть файл
        }

        //Очистка таблицы
        private void ClearDGV() 
        {
            var columnsCount = DGVJournal.Columns.Count;//количество столбцов
            for (int i = 1; i < columnsCount; i++)
                DGVJournal.Columns.RemoveAt(1);//удалить первый столбец
        }

        //Конпка вывести
        private void buttonShow_Click(object sender, EventArgs e)
        {
            ClearDGV();
            var month = comBoxMonth.Text;//считать месяц
            var dis = comBoxDiscipline.Text.Trim('\r').Trim();//считать дисциплину и очистить от лтшнего
            if (String.IsNullOrEmpty(month))//если месяц не выбран
                MessageBox.Show("Вы не выбрали месяц");
            else if (String.IsNullOrEmpty(dis))//если дисциплина не выбрана
                MessageBox.Show("Вы не выбрали дисциплину");
            else
            {
                disciplinSelected = true;//пометить что дисциплина выбрана
                StreamReader sr = new StreamReader($"Months\\{month}.txt");//перейти в папку                                                       //и открыть файл с выбраным месяцем
                if (sr == null) MessageBox.Show("Вы неправильно ввели данные");//если месяц был введён неправильно
                else 
                {
                    var s = sr.ReadLine();//считать строку
                    while (!String.IsNullOrEmpty(s))//пока строка не пустая
                    {
                        if (s[0] == '$')//если строка начинается с доллара
                        {
                            var disTemp = s.Split('$');
                            if (disTemp[1] == dis)//если дисциплина совпадает
                            {
                                var day = sr.ReadLine();//считать сроку с датой и оценками
                                for (int i = 1; day[0] != '&'; i++)//пока не дойдём до &
                                {
                                    var dayTemp = day.Split('#');//разделить по #
                                    var dayColumn = new DataGridViewTextBoxColumn();//новая колонка
                                    dayColumn.Name = dayTemp[0];//имя столбца присвоить число
                                    dayColumn.Width = DGVJournal.RowTemplate.Height;//установить ширину колонок
                                    DGVJournal.Columns.Add(dayColumn);//добавить колонку в таблицу
                                    for (int j = 0; j < dayTemp[1].Length; j++)
                                    {
                                        DGVJournal[i, j].Value = dayTemp[1][j] == '_' ? 
                                            "" : dayTemp[1][j].ToString();//перенести оценки из файла в таблицу
                                    }
                                    day = sr.ReadLine();//считать след строку
                                }                               
                            }
                        }
                        s = sr.ReadLine();//считать след строку
                    }
                }
                sr.Close();//закрыть файл
            }
        }

        //кнопка добавлния
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var date = textBoxDateAdd.Text.Trim();//считать число которое надо добавить

            if (!disciplinSelected)//если дисциплина не выбрана
                MessageBox.Show("Вы не выбрали дисциплину");
            else if (String.IsNullOrEmpty(date))//если поле пустое
                MessageBox.Show("Вы не выбрали число");
            else if (int.Parse(date) > 31 || double.Parse(date) % 1 != 0 || int.Parse(date) < 0)//если введена некоректная дата
                MessageBox.Show("Некорректная дата");
            else 
            {
                var dayColumn = new DataGridViewTextBoxColumn();//новая колонка
                dayColumn.Name = date;//имя столбца присвоить число
                dayColumn.Width = DGVJournal.RowTemplate.Height;//установить ширину колонок

                var cols = new List<string>();//лист с колонками
                var isAdded = false;//добавлена ли колонка
                foreach (DataGridViewColumn col in DGVJournal.Columns)//пройтись по всем колокам
                {
                    if (col.Name != "Names")//если это не колонка с именами
                    {
                        if(Convert.ToInt32(col.Name) == Convert.ToInt32(dayColumn.Name))//если колонка с таким числом уже сущ
                        {
                            MessageBox.Show("Такая дата уже существует");
                            textBoxDateAdd.Text = "";//очитстить текстовое поле
                            return;
                        }
                        if (Convert.ToInt32(col.Name) > Convert.ToInt32(dayColumn.Name) && !isAdded)//если это правильное место для даты
                                                                                                    //и она ещё не добавлена
                        {
                            isAdded = true;//пометить что колонку уже добавили
                            cols.Add(dayColumn.Name);//добавить новую колонку
                        }                       
                        cols.Add(col.Name);//дубавить существующую колонку
                    }
                }
                if (!isAdded)//если колонка ещё не была добавлена
                {
                    cols.Add(dayColumn.Name);//добавить новую колонку
                }

                int colsLength = DGVJournal.ColumnCount + 1, rowsLength = DGVJournal.RowCount;//пометить границы

                var matrJournal = new string[colsLength, rowsLength];//матрица с оценками
                for (int i = 1; i < colsLength-1; i++)
                    for (int j = 0; j < rowsLength; j++)//пройтись по всей матрице
                    {
                        if (Convert.ToInt32(DGVJournal.Columns[i].Name) > Convert.ToInt32(dayColumn.Name))//полсе добавленной колонки 
                            matrJournal[i + 1, j] = DGVJournal[i, j].Value == null ? null : DGVJournal[i, j].Value.ToString();
                            //присвоить на одну колонку леве
                        else
                            matrJournal[i, j] = DGVJournal[i, j].Value == null ? null : DGVJournal[i, j].Value.ToString();
                    }
                ClearDGV();

                for (int i = 1; i < colsLength; i++)
                {
                    var tempColumn = new DataGridViewTextBoxColumn();
                    tempColumn.Name = cols[i - 1];
                    tempColumn.Width = DGVJournal.RowTemplate.Height;
                    DGVJournal.Columns.Add(tempColumn);//добавить колонку из листа
                    for (int j = 0; j < rowsLength; j++)
                        DGVJournal[i, j].Value = matrJournal[i, j] ?? "";//если значение null присвоить пустую строку
                }
            }
            textBoxDateAdd.Text = "";//очистить текстовое поле добавить колонку
        }

        //Кнопка удалить
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            var date = textBoxDateDelete.Text.Trim();//считать с текстового поля удалить колоннку
            if (!disciplinSelected)//если дисциплина не выбрана
                MessageBox.Show("Вы не выбрали дисциплину");
            else if (String.IsNullOrEmpty(date))//если поле пустое
                MessageBox.Show("Вы не выбрали число");
            else if (int.Parse(date) > 31 || double.Parse(date) % 1 != 0 || int.Parse(date) < 0)//если введена некоректная дата
                MessageBox.Show("Некорректная дата");
            else
            {
                for (int i = 1; i < DGVJournal.Columns.Count; i++)
                {
                    if (DGVJournal.Columns[i].Name == date)//если даты совпадают
                    {
                        DGVJournal.Columns.RemoveAt(i);//удалить колонку
                    }
                }
                textBoxDateDelete.Text = "";//очистить поле удалить колонку
            }
        }

        //кнопка сохранения
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!disciplinSelected)//если дисциплина не выбрана
                return;

            int colsLength = DGVJournal.ColumnCount, rowsLength = DGVJournal.RowCount;//отметить границы
            var dicJournal = new Dictionary<string, List<string>>();//словарь в который будет сохраняться информация

            for (int i = 1; i < colsLength; i++)
                for (int j = 0; j < rowsLength; j++)//пройтись по таблице
                {
                    if (!dicJournal.ContainsKey(DGVJournal.Columns[i].Name))//если нет такой даты
                        dicJournal[DGVJournal.Columns[i].Name] = new List<string>();//создать лист
                    dicJournal[DGVJournal.Columns[i].Name]
                        .Add(DGVJournal[i, j].Value == null || String.IsNullOrEmpty(DGVJournal[i, j].Value.ToString()) ? "_" : DGVJournal[i, j].Value.ToString());   
                    //добавить в лист значение из ячейки таблицы
                }

            var month = comBoxMonth.Text;//считать месяц
            var dis = comBoxDiscipline.Text.Trim('\r').Trim();//считать дисциплину

            StreamReader sr = new StreamReader($"Months\\{month}.txt");//открыть файл с выбраным месяцем

            var fileStart = "";//часть файла до дисциплины, которую надо перезаписать
            var newDis = "";//часть файла с новой инф о дисциплине
            var fileEnd = "";//часть файла до дисциплины, которую надо перезаписать
            var isStart = true;//если дисциплина ещё не изменена

            if (sr == null) MessageBox.Show("Вы неправильно ввели данные");//если файл был неправильно выбран
            else
            {               
                var s = sr.ReadLine();//считать строку
                while (!String.IsNullOrEmpty(s))//пока строка не пустая
                {
                    if (s[0] == '$')//если строка начинается с доллара
                    {
                        var disTemp = s.Split('$');
                        if (disTemp[1] == dis)//если дисциплина совпадает
                        {
                            newDis = $"${dis}$\n";//начало новой дисциплины
                            isStart = false;//изменить на дисциплина уже изменена
                            var day = sr.ReadLine();//считать строку
                            while (day[0] != '&')//пока не дошёл до конца дисциплину
                                day = sr.ReadLine();//считать строку
                            foreach (var dayItem in dicJournal)//пройтись по словарю
                            {
                                var dayTemp = dayItem.Key.ToString() + "#" + String.Join("", dayItem.Value) + "\n";
                                newDis += dayTemp;//добавить строку с числом 
                            }
                            newDis += $"&{dis}&\n";//конец новой дисциплины
                            s = sr.ReadLine();//считать строку
                        }
                    }
                    if (isStart)//до дисциплины 
                        fileStart += s+"\n";//добавить строку
                    else
                        fileEnd += s+"\n";//добавить строку
                    s = sr.ReadLine();//считать строку
                }
            }
            sr.Close();//закрыть файл
            File.WriteAllText($"Months\\{month}.txt", fileStart+newDis+fileEnd);//пререзаписать файл
        }
    }
}
