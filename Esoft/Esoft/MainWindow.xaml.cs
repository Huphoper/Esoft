using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data.Common;


namespace Esoft
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Authorization : Window
    {

        public Authorization()
        {

            InitializeComponent();
            Login.Text = "kremlev";
            Password.Password = "kremlevpassword";


        }

        private void ButtonAuthorization(object sender, RoutedEventArgs a)
        {
            //Подключение к таблице
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                db.Open();
                //Поиск по логину
                MySqlCommand cmd = new MySqlCommand("SELECT id, Login, FullName, TypeUser FROM `user` WHERE Login = '" + Login.Text.Replace("'", "''") + "' AND Password = '" + Password.Password.Replace("'", "''") + "' AND deleted = 0")
                {
                    Connection = db
                };
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        MessageBox.Show("Введен не правильный логин или пароль!");
                    }
                    else
                    {
                        User user = new User
                        {
                            id = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            FullName = reader.GetString(2),
                            TypeUser = reader.GetString(3),
                        };

                        /*Выбор открытия окна по роли*/
                        if (user.TypeUser == "Менеджер")
                        {
                            Manager Userwindow = new Manager
                            {
                                /*Ввод стандартных переменных*/
                                user = user
                            };
                            Userwindow.SetValue(user.Login, user.id); //Установить начальные значения
                            Userwindow.Show(); // Показать
                        }
                        else
                        {
                            Executor Userwindow = new Executor
                            {
                                /*Ввод стандартных переменных*/
                                user = user
                            };
                            Userwindow.SetValue(user.Login, user.id); //Установить начальные значения
                            Userwindow.Show();  // Показать
                        };
                        this.Close(); // Закрыть окно авторизации


                    };
                };
            }
            catch (Exception e)
            {

                MessageBox.Show("Не удалось подключиться к базе данных или серверу! Информация об ошибке: " + e);
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
