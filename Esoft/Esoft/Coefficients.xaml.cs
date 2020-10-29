using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data.Common;
namespace Esoft
{
    /// <summary>
    /// Логика взаимодействия для Manager.xaml
    /// </summary>
    public partial class Coefficients : Window
    {
        public User user;

        public Coefficients()
        {

            InitializeComponent();
        }

        //Кнопка Отмены
        private void ButtonClose(object sender, RoutedEventArgs a)
        {
            this.Close();
        }

        //Изменение значений коэфицентов
        public void UploadChanges(object sender, RoutedEventArgs a)
        {


            bool checkError = false;
            Commons Valid = new Commons();
            TextBox[] blocks = new TextBox[] { junior, middle, senior, analysis, installation, support, TimeRatio, DifficultyRatio, CashEquivalent };

            

            for (int i = 0; i < blocks.Length; i++)
            {
                if (!Valid.CheckDouble(blocks[i].Text))
                {
                    Valid.InstallStyleTextBox(blocks[i]);
                    checkError = true;
                }
                else
                {
                    Valid.UnInstallStyleTextBox(blocks[i]);
                };
            };

            if (checkError)
            {
                MessageBox.Show("Необходимо заполнить выделенные поля!");
            }
            else
            {
                //Подключение к таблице
                MySqlConnection db = DBUtils.GetDBConnection();
                try
                {

                    db.Open();
                    string sql = "UPDATE `managers_coef` SET  junior = '" + junior.Text.ToString().Replace(",", ".")
                        + "' , middle = '" + middle.Text.ToString().Replace(",", ".")
                        + "', senior = '"
                        + senior.Text.ToString().Replace(",", ".")
                        + "' ,analysis= '"
                        + analysis.Text.ToString().Replace(",", ".") + "' ,installation = '"
                        + installation.Text.ToString().Replace(",", ".") + "' ,support= '"
                        + support.Text.ToString().Replace(",", ".") + "' ,time_coef = '"
                        + TimeRatio.Text.ToString().Replace(",", ".")
                        + "',diff_coef = '"
                        + DifficultyRatio.Text.ToString().Replace(",", ".") + "' ,cash_coef= '"
                        + CashEquivalent.Text.ToString().Replace(",", ".")
                        + "' WHERE `managers_coef`.`Manager_id` = " + user.id;
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = db;
                    cmd.CommandText = sql;
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
                    this.Close();
                }
            };
        }

        //Установка начальных значений
        public void SetValue(string pLogin, string pFullName, int pid)
        {

            ManagerName.Text = pLogin;

            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                db.Open();

                //Поиск и получение коэфицентов
                MySqlCommand cmd = new MySqlCommand("SELECT junior,middle,senior,analysis,installation,support,time_coef,diff_coef,cash_coef FROM `managers_coef` WHERE Manager_id = '" + pid + "' AND deleted = 0");
                cmd.Connection = db;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        reader.Close();
                        string sql =
                            "INSERT INTO `managers_coef` (`Manager_id`,`junior`,`middle`,`senior`,`analysis`,`installation`,`support`,`time_coef`,`diff_coef`,`cash_coef`,`deleted`) VALUES ("
                            + user.id + ", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);";
                        MySqlCommand cmda = new MySqlCommand();
                        cmda.Connection = db;
                        cmda.CommandText = sql;
                        cmda.ExecuteNonQuery();
                    }
                    else
                    {
                        junior.Text = reader.GetString(0);
                        middle.Text = reader.GetString(1);
                        senior.Text = reader.GetString(2);
                        analysis.Text = reader.GetString(3);
                        installation.Text = reader.GetString(4);
                        support.Text = reader.GetString(5);
                        TimeRatio.Text = reader.GetString(6);
                        DifficultyRatio.Text = reader.GetString(7);
                        CashEquivalent.Text = reader.GetString(8);
                    };
                };

            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получении и установке начальных значений! Информация об ошибке: " + e);
            }
            finally
            {
                // Закрыть соединение.
                db.Close();
                // Уничтожить объект, освободить ресурс.
                db.Dispose();
            }
        }


        //Заполнение только вещественными
        private void OnlyFloatInput(object sender, TextCompositionEventArgs e)
        {
            if (Char.IsDigit(e.Text, 0) || (e.Text[0] == ',' && ((TextBox)sender).Text.Split(',').Length == 1))
            {
            }
            else
            {
                e.Handled = true;
            }
        }


    }
}
