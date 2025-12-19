using AutofactoryNET;
using DAO;             // Para acceder a MyDBContext
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using BCrypt.Net;

namespace AppAutoFactory.Pages
{
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();

            //// --- 🛠️ CÓDIGO TEMPORAL DE REPARACIÓN 🛠️ ---
            //// Esto forzará que la contraseña en la BD sea exactamente "1234"
            //try
            //{
            //    using (var context = new DAO.MyDBContext())
            //    {
            //        var conn = context.Database.GetDbConnection();
            //        conn.Open();
            //        using (var cmd = conn.CreateCommand())
            //        {
            //            // 1. Generamos un hash limpio y nuevo para "1234"
            //            string nuevoHash = BCrypt.Net.BCrypt.HashPassword("1234");

            //            // 2. Actualizamos el usuario 'admin' (asegúrate de que existe en la BD)
            //            cmd.CommandText = "UPDATE USUARIS SET us_password_hash = :hash WHERE us_username = 'admin'";

            //            var param = cmd.CreateParameter();
            //            param.ParameterName = ":hash";
            //            param.Value = nuevoHash;
            //            cmd.Parameters.Add(param);

            //            int filas = cmd.ExecuteNonQuery();
            //            if (filas > 0)
            //                MessageBox.Show("✅ ¡Contraseña REPARADA! Ahora el usuario 'admin' tiene la clave '1234'.");
            //            else
            //                MessageBox.Show("⚠️ No se encontró el usuario 'admin'. Revisa tu tabla USUARIS.");
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("❌ Error intentando reparar la contraseña: " + ex.Message);
            //}
            //// -----------------------------------------------
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text;
            string pass = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Has d'introduir un usuari i una contrasenya.", "Atenció", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (ValidarCredenciales(user, pass))
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuari o contrasenya incorrectes.", "Error d'accés", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connectant amb la base de dades:\n" + ex.Message, "Error Crític", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCredenciales(string username, string passwordPlana)
        {
            // Creamos una instancia del contexto para comprobar el usuario.
            using (var context = new MyDBContext())
            {
                var conn = context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    // Usamos :usuarioInput en lugar de :user para evitar conflicto con palabras reservadas
                    cmd.CommandText = "SELECT us_password_hash FROM USUARIS WHERE us_username = :usuarioInput";

                    var param = cmd.CreateParameter();
                    param.ParameterName = ":usuarioInput";
                    param.Value = username;
                    cmd.Parameters.Add(param);

                    var result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string hashGuardado = result.ToString();

                        // BCrypt comprueba si la contraseña coincide con el hash
                        return BCrypt.Net.BCrypt.Verify(passwordPlana, hashGuardado);
                    }
                }
            }
            return false; // Usuario no encontrado
        }
    }
}