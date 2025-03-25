using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Lab_4
{
    public class AdoAssistant
    {
        private string connectionString =
            ConfigurationManager.ConnectionStrings["connectionString_ADO"].ConnectionString;

        private DataTable dtStudents = null;

        public DataTable LoadStudentsTable()
        {
            if (dtStudents != null)
                return dtStudents;

            dtStudents = new DataTable();
            string sql = @"SELECT [Номер_залікової_книги],
                              [ПІБ],
                              [Група],
                              [Адреса]
                       FROM [Студенти]";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(dtStudents);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка підключення або завантаження даних.\n" + ex.Message);
                }
            }
            return dtStudents;
        }

        public void RefreshStudentsTable()
        {
            dtStudents = null;
            LoadStudentsTable();
        }

        public void CreateStudent(string nomerZalik, string pib, string grupa, string address)
        {
            if (string.IsNullOrWhiteSpace(nomerZalik) ||
                string.IsNullOrWhiteSpace(pib) ||
                string.IsNullOrWhiteSpace(grupa) ||
                string.IsNullOrWhiteSpace(address))
            {
                throw new Exception("Усі поля мають бути заповнені!");
            }

            string sql = @"
            INSERT INTO [Студенти]
            ([Номер_залікової_книги], [ПІБ], [Група], [Адреса])
            VALUES (@nzk, @pib, @grp, @adr)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nzk", nomerZalik);
                    cmd.Parameters.AddWithValue("@pib", pib);
                    cmd.Parameters.AddWithValue("@grp", grupa);
                    cmd.Parameters.AddWithValue("@adr", address);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStudent(string oldNomerZalik, string newNomerZalik,
                                  string pib, string grupa, string address)
        {
            if (string.IsNullOrWhiteSpace(newNomerZalik) ||
                string.IsNullOrWhiteSpace(pib) ||
                string.IsNullOrWhiteSpace(grupa) ||
                string.IsNullOrWhiteSpace(address))
            {
                throw new Exception("Усі поля мають бути заповнені!");
            }

            string sql = @"
            UPDATE [Студенти]
            SET [Номер_залікової_книги] = @newNzk,
                [ПІБ] = @pib,
                [Група] = @grp,
                [Адреса] = @adr
            WHERE [Номер_залікової_книги] = @oldNzk";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@newNzk", newNomerZalik);
                    cmd.Parameters.AddWithValue("@pib", pib);
                    cmd.Parameters.AddWithValue("@grp", grupa);
                    cmd.Parameters.AddWithValue("@adr", address);
                    cmd.Parameters.AddWithValue("@oldNzk", oldNomerZalik);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteStudent(string nomerZalik)
        {
            if (string.IsNullOrWhiteSpace(nomerZalik))
            {
                throw new Exception("Номер залікової книги не може бути порожнім!");
            }

            string sql = @"DELETE FROM [Студенти]
                       WHERE [Номер_залікової_книги] = @nzk";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nzk", nomerZalik);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}
