using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data.Common;

namespace Esoft
{
    class Commons
    {


        public bool CheckNumber(string ValS)
        {
            bool ichck = true;
            try
            {
                Convert.ToInt32(ValS);
            }
            catch
            {
                ichck = false;
            }
            return ichck;
        }

        public bool CheckDouble(string ValS)
        {
            bool ichck = true;
            try
            {
                Convert.ToDouble(ValS);
            }
            catch
            {
                ichck = false;
            }
            return ichck;
        }


        public void InstallStyleTextBox(TextBox blocks)
        {
            blocks.Background = Brushes.Red;
        }

        public void UnInstallStyleTextBox(TextBox blocks)
        {
            blocks.Background = Brushes.White;
        }

        public void InstallStyleBorder(Border blocks)
        {
            
            blocks.BorderBrush = Brushes.Red;
            blocks.BorderThickness = new Thickness(2);
        }

        public void UnInstallStyleBorder(Border blocks)
        {
            
            blocks.BorderBrush = Brushes.Gray;
        }
        public void InstallStyleDate(DatePicker blocks)
        {
            
            blocks.BorderBrush = Brushes.Red;
            blocks.BorderThickness = new Thickness(2);
        }
        public void UnInstallStyleDate(DatePicker blocks)
        {
            
            blocks.BorderBrush = Brushes.Gray;
            blocks.BorderThickness = new Thickness(1);
        }

        public void SetListPerformers(ComboBox blocks, string pid)
        {
            MySqlConnection db = DBUtils.GetDBConnection();
            try
            {
                blocks.Items.Clear();
                blocks.Items.Add("");
                db.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT FullName, id, Login FROM `user` WHERE TypeUser = 'Исполнитель' AND deleted = 0 AND id IN (SELECT Performer_id FROM subgroups WHERE deleted = 0 AND Manager_id = " + pid
                    + ") ORDER BY id")
                {
                    Connection = db
                };
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                    blocks.Items.Add(reader.GetString(1) + " " + reader.GetString(0) + " " + reader.GetString(2));
                    };
                };
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось подключиться к базе данных или серверу при получении исполнителей! Информация об ошибке: " + e);
            }
            finally
            {
                db.Close();
                db.Dispose();
            }
        }

    }
}
