using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace AdminPanel
{
    public partial class frmMainMenu : Form
    {
        public frmMainMenu()
        {
            InitializeComponent();
        }

        private void frmMainMenu_Load(object sender, EventArgs e)
        {
            //load some data 
            //vv this gets the total doors right now for today
            string sql = "select COUNT(a.ID) from dbo.door a LEFT OUTER JOIN dbo.door_program AS b ON a.id = b.door_id LEFT OUTER JOIN dbo.door_type AS c ON a.door_type_id = c.id " + //
                "WHERE(b.programed_by_id IS NULL) AND(a.status_id = 1 OR a.status_id = 2) " +
                "AND(c.double_y_n IS NOT  NULL) AND(c.slimline_y_n IS NULL OR c.slimline_y_n = 0) AND(a.date_completion IS NOT NULL) AND(a.door_type_id <> 48) AND(a.door_type_id <> 113) " +
                "AND(a.order_number <> 'Bridge Street Cut Downs') AND a.date_punch <= getdate()";
            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    string getData = Convert.ToString(cmd.ExecuteScalar());
                    if (getData != null)
                        lblDoors.Text = "Number of doors to program: " + getData;
                }
                sql = "select count(string) from dbo.view_department_reverse_concat_office where placement_date = CAST(GETDATE() as date) and department = 'Programming' and String <> '128'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    string getData = Convert.ToString(cmd.ExecuteScalar());
                    if (getData != null)
                        lblMen.Text = "Number of programmers currently assigned: " + getData;
                }

                //here we change button text
                sql = "SELECT programming_override FROM [user_info].dbo.prog_version_numbers WHERE id = 13";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    int getData = Convert.ToInt32(cmd.ExecuteScalar());
                    if (getData == -1)
                        btnOverride.Text = "DISABLE";
                    else
                        btnOverride.Text = "ENABLE";
                }

                conn.Close();
            }
        }

        private void btnOverride_Click(object sender, EventArgs e)
        {
            string sql = "";
            string buttonText = "";
            if (btnOverride.Text == "ENABLE")
            {
                sql = "UPDATE [user_info].dbo.prog_version_numbers SET programming_override = -1 WHERE id = 13";
                buttonText = "DISABLE";
            }
            else
            {
                sql = "UPDATE [user_info].dbo.prog_version_numbers SET programming_override = 0 WHERE id = 13";
                buttonText = "ENABLE";
            }

            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                    btnOverride.Text = buttonText;
                    if (buttonText != "ENABLE")
                    {
                        //fire an email @ gez here
                            //usp_email_programming_OT
                            //here we fire the procedure 
                        using (SqlCommand cmdEmail = new SqlCommand("usp_department_placement_planner_auto_update", conn)
                            { CommandType = System.Data.CommandType.StoredProcedure })
                        {
                            cmdEmail.Parameters.Add("@if_statement", SqlDbType.Int).Value = 4;
                            cmdEmail.ExecuteNonQuery();
                        }

                    }

                }
                conn.Close();
                Application.Exit();
            }
        }
    }
}
