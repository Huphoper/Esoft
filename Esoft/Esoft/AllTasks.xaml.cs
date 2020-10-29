using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data;
using System.Data.Common;

namespace Esoft
{
    /// <summary>
    /// Логика взаимодействия для AllTasks.xaml
    /// </summary>
    public partial class AllTasks : Window
    {
        DataTable datatable; // Таблица для динамического заполнения DataGrid из запроса 
        public AllTasks()
        {
            InitializeComponent();
            ReloadTasks();
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                Manager.Items.Clear();
                Manager.Items.Add("");
                db.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT FullName, id FROM `user` WHERE TypeUser = 'Менеджер' AND deleted = 0 ORDER BY id")
                {
                    Connection = db
                };
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Manager.Items.Add(reader.GetString(1) + " " + reader.GetString(0));
                    };
                    reader.Close();
                };

                taskexec.Items.Clear();
                taskexec.Items.Add("");
                cmd.CommandText = "SELECT FullName, id, Login FROM `user` WHERE TypeUser = 'Исполнитель' AND deleted = 0 ORDER BY id";
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        taskexec.Items.Add(reader.GetString(1) + " " + reader.GetString(0) + " " + reader.GetString(2));
                    };
                };
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получении менеджеров и исполнителей! Информация об ошибке: " + e);
            }
            finally
            {
                db.Close();
                db.Dispose();
            }

        }

        //Обновить список задач 
        private void ButtonReloadTask(object sender, RoutedEventArgs a)
        {
            ReloadTasks();
        }

        //Обновить список задач
        public void ReloadTasks()
        {

            TasksForExecutors.ItemsSource = null;
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                db.Open();
                //Обновить таблицу задач
                datatable = new DataTable("task");

                MySqlCommand command = new MySqlCommand("SELECT id, (SELECT FullName FROM `user` WHERE id = (SELECT Manager_id FROM `subgroups` WHERE Performer_id = `task`.Performer_id)) AS Менеджер, DateCreate AS Создан, DateCompletion AS `Дата завершения`, Status AS Статус, (SELECT FullName FROM `user` WHERE id = `task`.Performer_id) AS Исполнитель, Name As Задача, TypeWork AS `Характер работы`, Complexity As Сложность, LeadTime AS `Времени на исполнении`, PerformanceDate AS Дедлайн FROM `task` WHERE deleted = 0 "
                    + (taskstatus.SelectedIndex > 0 ? " AND Status = '" + taskstatus.Text + "' " : "")
                    + (taskexec.SelectedIndex > 0 ? " AND Performer_id = '" + taskexec.Text.Split(' ')[0] + "' " : "")
                    + (Manager.SelectedIndex > 0 ? " AND Performer_id IN (SELECT Performer_id FROM `subgroups` WHERE Manager_id = " + Manager.Text.Split(' ')[0] + ") " : "")
                    + " ORDER BY id")
                {
                    Connection = db
                };
                command.ExecuteNonQuery();
                //Поставить типы из результат
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                //Обновить строки по таблице
                adapter.Fill(datatable);


                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    datatable.Columns[i].ReadOnly = true;
                };
                //Установить таблицу
                TasksForExecutors.ItemsSource = datatable.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получении задач! Информация об ошибке: " + e);
            }
            finally
            {
                // Закрыть соединение.
                db.Close();
                // Уничтожить объект, освободить ресурс.
                db.Dispose();
            }
        }
    }
}
