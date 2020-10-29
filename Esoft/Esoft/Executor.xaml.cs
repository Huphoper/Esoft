using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data;


namespace Esoft
{

    /// <summary>
    /// Логика взаимодействия для Executor.xaml
    /// </summary>
    public partial class Executor : Window
    {
        public User user;
        DataTable datatable; // Таблица для динамического заполнения DataGrid из запроса 

        //Установить начальные значения при запуске
        public void SetValue(string pLogin, int pid)
        {
            Login.Text = pLogin;

            ReloadTasks(pid.ToString());
        }

        //Обновить список задач 
        private void ButtonReloadTask(object sender, RoutedEventArgs a)
        {
            ReloadTasks(user.id.ToString());
        }



        public Executor()
        {
            InitializeComponent();
        }

        //Отредактировать задачу
        private void ButtonReditTask(object sender, RoutedEventArgs a)
        {
            int idtask = Convert.ToInt32(datatable.Rows[TasksForExecutors.SelectedIndex][0]);
            AddTask AddTaskwindow = new AddTask
            {

                user = user,
                id_task = idtask
            };
            AddTaskwindow.SetValue(user.Login, user.id, user.TypeUser, idtask);
            AddTaskwindow.Owner = this;
            AddTaskwindow.Show();
        }


        //Обновить список задач
        public void ReloadTasks(string pid)
        {

            TasksForExecutors.ItemsSource = null;
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                db.Open();
                //Обновить таблицу задач
                datatable = new DataTable("task");
                MySqlCommand command = new MySqlCommand("SELECT id,(SELECT FullName from user WHERE id=(SELECT Manager_id from subgroups WHERE Performer_id =" + pid + ")) AS Менеджер,DateCreate AS Создан, DateCompletion AS `Дата завершения`, Status AS Статус, Name As Задача, TypeWork AS `Характер работы`, Complexity As Сложность, LeadTime AS `Времени на исполнении`, PerformanceDate AS Дедлайн FROM `task` WHERE deleted = 0 "
                    + (taskstatus.SelectedIndex > 0 ? " AND Status = '" + taskstatus.Text + "' " : "")
                    + " AND Performer_id = " + pid
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
