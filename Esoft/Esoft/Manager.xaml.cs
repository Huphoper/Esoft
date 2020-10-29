using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data;

namespace Esoft
{
    public partial class Manager : Window
    {
        public User user;
        DataTable datatable; // Таблица являющиеся источником данных для заполнения DataGrid

        public Manager()
        {
            InitializeComponent();
        }

        //Установить начальные значения при запуске
        public void SetValue(string pLogin, int pid)
        {
            Login.Text = pLogin;
            taskstatus.SelectedIndex = 0;
            ReloadTasks(pid.ToString());
            Commons SetV = new Commons();
            SetV.SetListPerformers(taskexec, pid.ToString());

        }

        //Открытие формы коэфицентов
        private void ButtonOpenCoefficients(object sender, RoutedEventArgs a)
        {
            Coefficients Coefficientswindow = new Coefficients
            {
                user = user /*Ввод стандартных переменных*/
            };
            Coefficientswindow.SetValue(user.Login, user.FullName, user.id); //Установить начальные значения
            Coefficientswindow.Show();  // Показать
        }


        //Обновить список задач 
        private void ButtonReloadTask(object sender, RoutedEventArgs a)
        {
            ReloadTasks(user.id.ToString());
        }

        //Удалить выбранные задачи 
        private void ButtonDeletedTask(object sender, RoutedEventArgs a)
        {
            MessageBoxResult reuslt = MessageBox.Show("Вы уверены, что хотите удалить задачу?", "Проверка на удаление"
                , MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
            if (MessageBoxResult.Yes == reuslt)
            {

                string idtask = datatable.Rows[TasksForExecutors.SelectedIndex][0].ToString();
                MySqlConnection db = DBUtils.GetDBConnection();
                try
                {
                    db.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE `task` SET  deleted = 1 WHERE id = " + idtask)
                    { Connection = db };
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось удалить задачу! Информация об ошибке: " + e);
                }
                finally
                {
                    db.Close();
                    db.Dispose();
                    ReloadTasks(user.id.ToString());
                }

            }

        }

        //Кнопка для создания новой задача
        private void ButtonOpenTasks(object sender, RoutedEventArgs a)
        {
            AddTask AddTaskwindow = new AddTask
            {
                /*Ввод стандартных переменных*/
                user = user
            };
            AddTaskwindow.SetValue(user.Login, user.id, user.TypeUser); //Установить начальные значения
            AddTaskwindow.Owner = this; // Установка владельца
            AddTaskwindow.Show();  // Показать
        }

        //Кнопка для просмотра всех исполниителей
        private void ButtonAllPerformers(object sender, RoutedEventArgs a)
        {
            AllExecutors AllPerform = new AllExecutors();
            AllPerform.Show();
        }

        //Кнопка для просмотра всех задач
        private void ButtonAllTasks(object sender, RoutedEventArgs a)
        {
            AllTasks Alliasks = new AllTasks();
            Alliasks.Show();
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

                MySqlCommand command = new MySqlCommand("SELECT id, DateCreate AS Создан, DateCompletion AS `Дата завершения`, Status AS Статус, (SELECT FullName FROM `user` WHERE id = `task`.Performer_id) AS Исполнитель, Name As Задача, TypeWork AS `Характер работы`, Complexity As Сложность, LeadTime AS `Времени на исполнении`, PerformanceDate AS Дедлайн FROM `task` WHERE deleted = 0 AND Performer_id IN (SELECT Performer_id FROM subgroups WHERE deleted = 0 AND Manager_id = " + pid.ToString() + ") "
                    + (taskstatus.SelectedIndex > 0 ? " AND Status = '" + taskstatus.Text + "' " : "")
                    + (taskexec.SelectedIndex > 0 ? " AND Performer_id = '" + taskexec.Text.Split(' ')[0] + "' " : "")
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
