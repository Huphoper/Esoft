using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data;
using System.Data.Common;

namespace Esoft
{
    /// <summary>
    /// Логика взаимодействия для AllExecutors.xaml
    /// </summary>
    public partial class AllExecutors : Window
    {
        DataTable datatable; // Таблица для динамического заполнения DataGrid из запроса 

        public AllExecutors()
        {
            InitializeComponent();
            ReloadAllPerformer();

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
                };
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получении менеджеров! Информация об ошибке: " + e);
            }
            finally
            {
                db.Close();
                db.Dispose();
            }

        }

        private void ButtonReloadPerformers(object sender, RoutedEventArgs a)
        {
            ReloadAllPerformer();
        }

        public void ReloadAllPerformer()
        {
            AllPerformers.ItemsSource = null;
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                db.Open();
                //Обновить таблицу задач
                datatable = new DataTable("user");

                MySqlCommand command = new MySqlCommand("SELECT FullName AS ФИО, (SELECT Grade FROM `performer` WHERE Performer_id = BT.id) AS Грейд, (SELECT FullName FROM `user` WHERE id = (SELECT Manager_id FROM `subgroups` WHERE Performer_id = BT.id)) AS Менеджер FROM `user` AS BT WHERE TypeUser = 'Исполнитель' AND deleted = 0"
                    + (Manager.SelectedIndex > 0 ? " AND id IN (SELECT Performer_id FROM `subgroups` WHERE Manager_id = " + Manager.Text.Split(' ')[0] + ") " : "") + " ORDER BY id")
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
                AllPerformers.ItemsSource = datatable.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получении испонителец! Информация об ошибке: " + e);
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
