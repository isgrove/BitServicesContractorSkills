using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;

namespace BitServicesContractorSkills
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // field variables used to make the table
        // available to all methods in this class.
        private DataTable _dtSkillsList; // List of skills for the ComboBox.
        private DataTable _dtSkillIds; // The skill ids that the new contractor has.
        private string _cnnStr; // Connection string to the database
        private SqlConnection _myDbConn; // Connection object for the Database.

        public MainWindow()
        {
            InitializeComponent();

            // Get the connection string to the database
            _cnnStr = ConfigurationManager.ConnectionStrings["cnnStrBitServices"].ConnectionString;

            _myDbConn = new SqlConnection(_cnnStr);

            // Get the skills list
            GetSkillsList();

            // Load the Skills List Combo Box
            LoadSkillsListComboBox();

            // Instantiate the Skill_IDs table
            _dtSkillIds = new DataTable();
            _dtSkillIds.Columns.Add("JobSkill_ID", typeof(Int32));
        }

        private void LoadSkillsListComboBox()
        {
            cboSkillsList.DisplayMemberPath = "JobSkill_Type";
            cboSkillsList.SelectedValuePath = "JobSkill_ID";

            // Add a "-- Select a Skill --" message to the DataTable
            DataRow drSelectMsg;
            drSelectMsg = _dtSkillsList.NewRow();
            drSelectMsg["JobSkill_ID"] = "0";
            drSelectMsg["JobSkill_Type"] = "-- Select a Skill --";
            _dtSkillsList.Rows.InsertAt(drSelectMsg, 0);

            cboSkillsList.ItemsSource = _dtSkillsList.DefaultView;
            cboSkillsList.SelectedIndex = 0;
        }

        private void GetSkillsList()
        {
            // Go to the database and get the list of JobSkill_IDs and JobSkill_Types
            SqlCommand myCmd = new SqlCommand();
            myCmd.CommandText = "usp_GetListOfSkills";
            myCmd.CommandType = CommandType.StoredProcedure;
            myCmd.Connection = _myDbConn;

            // Try and get the data
            try
            {
                _myDbConn.Open();
                _dtSkillsList = new DataTable(); // Instantiate the datatable
                _dtSkillsList.Load(myCmd.ExecuteReader()); // Get the data
            }
            catch (SqlException sqlEx)
            {

                MessageBox.Show($"Database error occured! \n{sqlEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong retrieving the list of skills from the DB. \n{ex.Message}");
            }
            finally
            {
                if (myCmd.Connection.State == ConnectionState.Open)
                {
                    myCmd.Connection.Close();
                }
            }
        }

        private void btnAddSkill_Click(object sender, RoutedEventArgs e)
        {
            if(cboSkillsList.SelectedIndex != 0)
            {
                DataRow drNewSkill_ID;
                drNewSkill_ID = _dtSkillIds.NewRow();
                
                // If there are already items in the list box, check that we're not adding items twice
                if (lstBxSkillsList.Items.Count > 0)
                {
                    bool foundFlag = false;

                    // Loop through the listBox and check if the skill has already been added.
                    foreach (var listBoxItem in lstBxSkillsList.Items)
                    {
                        if (listBoxItem.ToString() == cboSkillsList.Text)
                        {
                            foundFlag = true;
                        }
                    }

                    // if the skill has not been added, add it.
                    if (foundFlag == false)
                    {
                        // Add the item selected in the combobox to the ListBox
                        lstBxSkillsList.Items.Add(cboSkillsList.Text);

                        // Capture the ID of the Skill
                        drNewSkill_ID["JobSkill_ID"] = cboSkillsList.SelectedValue;
                        _dtSkillIds.Rows.Add(drNewSkill_ID);
                    }
                }
                else
                {
                    // Nothing in the listBox, so add it
                    lstBxSkillsList.Items.Add(cboSkillsList.Text);

                    // Add the Skill_Id to our DataTable to pass the stored proc.
                    drNewSkill_ID["JobSkill_ID"] = cboSkillsList.SelectedValue;
                    _dtSkillIds.Rows.Add(drNewSkill_ID);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Define a command object
            SqlCommand myCmd = new SqlCommand();
            myCmd.CommandText = "usp_InsertNewContractorAndSkills";
            myCmd.CommandType = CommandType.StoredProcedure;
            myCmd.Connection = _myDbConn;

            // Passing Arguments to Parameters in a Stored Procedure
            SqlParameter[] sqlParams =  { new SqlParameter("@FName", txtFirstName.Text),
                                        new SqlParameter("@LName", txtLastName.Text),
                                        new SqlParameter("@UserName", txtUserName.Text),
                                        new SqlParameter("@Password", txtPassword.Text),
                                        new SqlParameter("@Address", txtStreetAddress.Text),
                                        new SqlParameter("@Suburb", txtSuburb.Text),
                                        new SqlParameter("@Phone", txtPhone.Text),
                                        new SqlParameter("@Email", txtEmail.Text),
                                        new SqlParameter("@PostCode", txtPostCode.Text),
                                        new SqlParameter("@Active", rbtnActive.IsChecked),
                                        new SqlParameter("@ListJobSkill_IDs", _dtSkillIds) };

            // Add each of the Parameters to the Parameters collection of the Command Object
            foreach (SqlParameter param in sqlParams)
            {
                myCmd.Parameters.Add(param);
            }

            // Try and do the INSERT (i.e. call the stored proc).
            try
            {
                myCmd.Connection.Open();
                int intResult = myCmd.ExecuteNonQuery();
                MessageBox.Show("Added to the database");
            }
            catch(SqlException sqlEx)
            {
                MessageBox.Show("Something went wrong inserting the new Contractor!\n" + sqlEx);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong!\n" + ex);
            }
            finally
            {
                if (myCmd.Connection.State == ConnectionState.Open)
                {
                    myCmd.Connection.Close();
                }
            }
        }
    }
}
