﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using TelHai.CS.Client.Models;
using TelHai.CS.Client.Repositories;

namespace TelHai.CS.Client.View
{
    /// <summary>
    /// Interaction logic for StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        public string StudentName { get; set; }
        public string Id { get; set; }
        private List<Exam> _exams;
        public StudentWindow()
        {
            InitializeComponent();

            StudentName = string.Empty;
            Id = string.Empty;
            this.Loaded += Load;

            this.DataContext = this;
        }

        private async void Load(object sender, RoutedEventArgs e)
        {
            // Load all exams from DB and put in the ListBox
            _exams = await HttpExamsRepository.Instance.GetAllExamsAsync();
            foreach(var exam in _exams)
            {
                this.examsListBox.Items.Add(exam);
            }
        }

        private void txtSearchExam_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the query from the TextBox and find the exams that contains this query
            List<Exam> list = new List<Exam>();
            string query = this.txtSearchExam.Text;
            list = _exams.Where(s => s.ToString().ToLower().Contains(query.ToLower())).ToList();
            this.examsListBox.Items.Clear();
            foreach (Exam exam in list)
            {
                this.examsListBox.Items.Add(exam);
            }
        }

        private void starthExamBtn_Click(object sender, RoutedEventArgs e)
        {
            if ( this.examsListBox.Items.Count > 0 )
            {
                // Get the selected exam
                Exam exam = (Exam)this.examsListBox.SelectedItem;

                if( exam == null)
                {
                    return;
                }
                // Check that the student name and id are correct
                if (StudentName == string.Empty || Id == string.Empty)
                {
                    string msg = "Please enter Name and Id";
                    MessageBox.Show(msg, "WAIT", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (Id.Length != 9)
                {
                    string msg = "Id should be 9 letters";
                    MessageBox.Show(msg, "WAIT", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Check if the time is right for exam (and not too early OR too late)
                int hour = (int)(exam.TotalTime);
                int minute = (int)((exam.TotalTime - hour) * 60);
                hour += (int)exam.DateHour;
                minute += (int)exam.DateMinute;
                if(minute >= 60)
                {
                    minute -= 60;
                    hour++;
                }
                if(hour >= 24)
                {
                    hour = 23;
                }

                DateTime examDateStart = new DateTime((int)exam.DateYear, (int)exam.DateMonth, (int)exam.DateDay, (int)exam.DateHour, (int)exam.DateMinute, 0);
                DateTime examDateEnd = new DateTime((int)exam.DateYear, (int)exam.DateMonth, (int)exam.DateDay, hour, minute, 0);
                DateTime now = DateTime.Now;
                if(DateTime.Compare(now, examDateStart) < 0) // Too Early
                {
                    string msg = "Wait for the Exam Time will Arrive....";
                    MessageBox.Show(msg, "WAIT", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if(DateTime.Compare(examDateEnd, now) < 0) // Too Late
                {
                    string msg = "The Exam Time is over....";
                    MessageBox.Show(msg, "WAIT", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Time is correct - start exam
                ExamWindow ew = new ExamWindow(exam, StudentName, Id);
                ew.ShowDialog();
            }
            else
            {
                MessageBox.Show("There is no Exams to Start!");
            }
        }

        private void examsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.examsListBox.Items.Count > 0)
            {
                Exam? exam = this.examsListBox.SelectedItem as Exam;
                if (exam != null)
                {
                    this.txtExamId.Text = exam._id;
                }
            }
        }
    }
}
