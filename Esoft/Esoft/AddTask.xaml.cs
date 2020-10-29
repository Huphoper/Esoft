using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data.Common;


namespace Esoft
{
    /// <summary>
    /// Логика взаимодействия для AddTask.xaml
    /// </summary>
    public partial class AddTask : Window
    {
        public User user;
        public int id_task = -1;

        public AddTask()
        {
            InitializeComponent();
        }

        //Автоматическая установка даты завершения при изменении статуса задачи
        void SelectedStatus(object sender, EventArgs e)
        {
            if (taskstatus.SelectedIndex == 2)
            {
                DateCompletion.SelectedDate = DateTime.Now;
            }
        }

        //Кнопка Отмены
        private void ButtonClose(object sender, RoutedEventArgs a)
        {
            this.Close();
        }

        //Установить начальные значения
        public void SetValue(string pLogin, int pid, string pTypeUser, int pid_task = -1)
        {


            Taskmanager.Text = pLogin;
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                db.Open();
                //Заполнение списка исполнителей
                MySqlCommand cmd = new MySqlCommand("SELECT FullName, id, Login FROM `user` WHERE TypeUser = 'Исполнитель' AND deleted = 0 AND id IN (SELECT Performer_id FROM subgroups WHERE deleted = 0 AND Manager_id = " + pid.ToString() + ") ORDER BY id");
                cmd.Connection = db;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        taskexec.Items.Add(reader.GetValue(1).ToString() + " " + reader.GetValue(0).ToString());
                    };
                    reader.Close();
                };

                //Проверка на редактировании задачи
                if (pid_task != -1)
                {
                    //Получение значений задачи для заполнений
                    cmd.CommandText = "SELECT Name, Definition, Complexity, Status,  LeadTime, DateCompletion, PerformanceDate, DateCreate, TypeWork, Performer_id   FROM `task` WHERE id = "
                        + pid_task.ToString();
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            taskname.Text = reader.GetString(0); // Загаловок
                            definition.Text = reader.GetString(1); // Описание
                            taskdiff.Text = reader.GetString(2); // Сложность
                            taskstatus.Text = reader.GetString(3); // Статус
                            tasktime.Text = reader.GetString(4); // Время на выоплнение
                            if (!reader.IsDBNull(5)) // Проверка на налл
                            {
                                DateCompletion.SelectedDate = reader.GetDateTime(5); // Дата завершения
                            };
                            PerformanceDate.SelectedDate = reader.GetDateTime(6); // Мертвая линиия
                            DateCreate.SelectedDate = reader.GetDateTime(7); // Дата создания
                            typeoftask.Text = reader.GetString(8); // Тип работы

                            //Выбор пользователя в ComboBox
                            string idper = reader.GetString(9);
                            foreach (string i in taskexec.Items)
                            {
                                if (i.Split(' ')[0] == idper)
                                {
                                    taskexec.SelectedIndex = taskexec.Items.IndexOf(i);
                                };
                            };
                        };
                        reader.Close();
                    };
                };
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получение списка исполнителей и подгрузки задачи! Информация об ошибке: " + e);
            }
            finally
            {
                if (pTypeUser != "Менеджер")
                {
                    
                    taskexec.Visibility = Visibility.Hidden; // Исполнитель
                    taskexecBorder.Visibility = Visibility.Hidden;
                    taskexecTextBlock.Visibility = Visibility.Hidden;


                    if (taskstatus.Text == "Завершена" || taskstatus.Text == "Отменена")
                    {
                        taskname.IsEnabled = false; // Загаловок
                        definition.IsEnabled = false; // Описание
                        taskdiff.IsEnabled = false; // Сложность
                        taskstatus.IsEnabled = false; // Статус
                        tasktime.IsEnabled = false; // Время на выоплнение
                        DateCompletion.IsEnabled = false; // Дата завершения
                        PerformanceDate.IsEnabled = false; // Мертвая линиия
                        DateCreate.IsEnabled = false; // Дата создания
                        typeoftask.IsEnabled = false; // Тип работы
                    };
                };
                // Закрыть соединение.
                db.Close();
                // Уничтожить объект, освободить ресурс.
                db.Dispose();
            }
        }

        //Добавление новой задачи или обновлении задачи
        public void uploadTask(object sender, RoutedEventArgs a)
        {

            string checkError = "";
            Commons Valid = new Commons();
            if (taskname.Text.Length == 0) { checkError += "Заголовок, "; Valid.InstallStyleTextBox(taskname); } else { Valid.UnInstallStyleTextBox(taskname); };
            if (!Valid.CheckNumber(taskdiff.Text)) { checkError += "Сложность(от 1 до 50), "; Valid.InstallStyleTextBox(taskdiff); } else { if (Convert.ToInt32(taskdiff.Text) < 51 && Convert.ToInt32(taskdiff.Text) > 0) { Valid.UnInstallStyleTextBox(taskdiff); } else { checkError += "Сложность(от 1 до 50), "; Valid.InstallStyleTextBox(taskdiff); }; };
            if (!Valid.CheckNumber(tasktime.Text)) { checkError += "Время, "; Valid.InstallStyleTextBox(tasktime); } else { Valid.UnInstallStyleTextBox(tasktime); };

            if (taskexec.SelectedIndex == -1 && user.TypeUser != "Исполнитель") { checkError += "Исполнитель, "; Valid.InstallStyleBorder(taskexecBorder); } else { Valid.UnInstallStyleBorder(taskexecBorder); };
            if (taskstatus.SelectedIndex == -1) { checkError += "Статус, "; Valid.InstallStyleBorder(taskstatusBorder); } else { Valid.UnInstallStyleBorder(taskstatusBorder); };
            if (typeoftask.SelectedIndex == -1) { checkError += "ТипЗадачи, "; Valid.InstallStyleBorder(typeoftaskBorder); } else { Valid.UnInstallStyleBorder(typeoftaskBorder); };

            if (PerformanceDate.SelectedDate.HasValue == false) { checkError += "Срок исполнения, "; Valid.InstallStyleDate(PerformanceDate); } else { Valid.UnInstallStyleDate(PerformanceDate); };

            if (checkError != "")
            {
                MessageBox.Show("Необходимо заполнить: " + checkError.Remove(checkError.Length - 2));
            }
            else
            {
                MySqlConnection db = DBUtils.GetDBConnection();
                try
                {
                    db.Open();

                    //Выбранный исполнитель
                    string l_user_id = taskexec.Text.Split(' ')[0];

                    //Добавление ИЛИ Обновление
                    MySqlCommand cmd = new MySqlCommand(
                        (id_task == -1 ? "INSERT INTO " : "UPDATE ")
                            + "`task` SET"
                            + (user.TypeUser == "Менеджер" ? " Performer_id = " + l_user_id + ", " : "")
                            + " Name = '" + taskname.Text.Replace("'", "''") + "'"
                            + ", Definition = '" + definition.Text.Replace("'", "''") + "'"
                            + ", Complexity = " + taskdiff.Text
                            + ", Status = '" + taskstatus.Text + "'"
                            + ", LeadTime = " + tasktime.Text
                            + ", DateCompletion = " + (DateCompletion.SelectedDate == null ? "NULL" : "'" + DateCompletion.SelectedDate.Value.ToString("yyyy-MM-dd 00:00:00") + "'")
                            + ", PerformanceDate = '" + PerformanceDate.SelectedDate.Value.ToString("yyyy-MM-dd 00:00:00") + "'"
                            + ", DateCreate = '" + DateCreate.SelectedDate.Value.ToString("yyyy-MM-dd 00:00:00") + "'"
                            + ", TypeWork = '" + typeoftask.Text + "'"
                            + (id_task == -1 ? ", deleted = 0" : "WHERE id = " + id_task.ToString())
                    )
                    { Connection = db };
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {

                    MessageBox.Show("Не удалось обновить данные! Информация об ошибке: " + e);
                }
                finally
                {
                    db.Close();
                    db.Dispose();

                    if (user.TypeUser == "Менеджер")
                    {
                        Manager OwnerForm = this.Owner as Manager;
                        OwnerForm.ReloadTasks(user.id.ToString());
                    }
                    else
                    {
                        Executor OwnerForm = this.Owner as Executor;
                        OwnerForm.ReloadTasks(user.id.ToString());
                    };
                    this.Close();
                }
            }
        }

        //Заполнение только цифрами
        private void OnlyNumberInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}
