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
using System.Windows.Threading;
using TelHai.CS.Client.Models;
using TelHai.CS.Client.Repositories;

namespace TelHai.CS.Client.View
{
    /// <summary>
    /// Interaction logic for ExamWindow.xaml
    /// </summary>
    public partial class ExamWindow : Window
    {
        Grade grade { get; set; }
        public Exam exam { get; set; }
        public List<int> Answers { get; set; }
        public int Answered { get; set; }
        private DispatcherTimer timer { get; set; }
        private TimeSpan remainingTime { get; set; }

        public ExamWindow(Exam getExam, string studentName, string studentId)
        {
            InitializeComponent();
            DataContext = this;
            this.exam = getExam;
            Answers = new List<int>();
            grade = new Grade { StudentId = studentId, StudentName = studentName , ExamId = exam._id , _grade = 0};
            this.txtStudent.Text = studentName;
            this.txtId.Text = studentId;
            this.Loaded += Load;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            if(this.exam.IsOrderRandom == true) // order of questions will randomize
            {
                Random rnd = new Random();
                exam.Questions = exam.Questions.Select(x => new { value = x, order = rnd.Next() })
                    .OrderBy(x => x.order).Select(x => x.value).ToList();
            }

            foreach (var question in exam.Questions)
            {
                if (question.IsRand == true) // order of answers will randomize
                {
                    Random rnd = new Random();
                    question.Answers = question.Answers.Select(x => new { value = x, order = rnd.Next() })
                        .OrderBy(x => x.order).Select(x => x.value).ToList();
                }
            }
            for (int i = 0; i < this.exam.Questions.Count; i++)
            {
                Answers.Add(-1);
            }
            this.questionsListBox.ItemsSource = exam.Questions;
            this.questionsListBox.SelectedIndex = 0;
            this.txtNumOfQuestions.Text = exam.Questions.Count.ToString();
            Answered = 0;

            timer = new DispatcherTimer();
            double time = (double)exam.TotalTime;
            remainingTime = TimeSpan.FromHours(time);
            timeLabel.Content = remainingTime.ToString(@"hh\:mm\:ss");
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            if (remainingTime < TimeSpan.Zero)
            {
                remainingTime = TimeSpan.Zero;
                timer.Stop();
                MessageBox.Show("Time is over we sorry");
                this.Close();
            }
            timeLabel.Content = remainingTime.ToString(@"hh\:mm\:ss");
        }

        private void questionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Question currQuestion = (Question)questionsListBox.SelectedItem;
            this.txtTitle.Text = currQuestion.Text;

            int questionIndex = this.questionsListBox.SelectedIndex;
            if (questionIndex < 0 || questionIndex >= Answers.Count)
            {
                return;
            }
            List<RadioButton> list = new List<RadioButton>();
            if (Answers[questionIndex] == -1) // NOT answer yet
            {
                foreach (var item in currQuestion.Answers)
                {
                    //RadioButton rb = new RadioButton { Content = new TextBlock { Text = item.Text, TextWrapping = TextWrapping.Wrap}, GroupName = currQuestion.Text };
                    RadioButton rb = new RadioButton { Content = item, GroupName = currQuestion.Text };
                    list.Add(rb);
                }
            }
            else // Have an Answer
            {
                for (int i = 0; i < currQuestion.Answers.Count; i++)
                {
                    if (Answers[questionIndex] == i)
                    {
                        RadioButton rb = new RadioButton { Content = currQuestion.Answers[i], GroupName = currQuestion.Text, IsChecked = true };
                        list.Add(rb);
                    }
                    else
                    {
                        RadioButton rb = new RadioButton { Content = currQuestion.Answers[i], GroupName = currQuestion.Text, IsChecked = false };
                        list.Add(rb);
                    }
                }
            }
            this.answersListBox.ItemsSource = list;
            this.answersListBox.SelectedIndex = -1;
        }

        private void prevQuestion_Click(object sender, RoutedEventArgs e)
        {
            int curr = this.questionsListBox.SelectedIndex;
            if (curr > 0)
            {
                this.questionsListBox.SelectedIndex--;
            }
        }

        private void nextQuestion_Click(object sender, RoutedEventArgs e)
        {
            int curr = this.questionsListBox.SelectedIndex;
            if (curr < this.questionsListBox.Items.Count)
            {
                this.questionsListBox.SelectedIndex++;
            }
        }

        private void answersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.answersListBox.SelectedIndex == -1)
            {
                return;
            }
            // SET THE SELECTED ITEM (RatioButton) checked!!!!
            var answer = this.answersListBox.SelectedItem as RadioButton;
            answer.IsChecked = true;


            // ((RadioButton)this.answersListBox.SelectedItem).IsChecked = true;
            int index = this.questionsListBox.SelectedIndex;
            if (Answers[index] == -1)
            {
                if (Answered < exam.Questions.Count)
                    Answered++;
            }
            Answers[index] = this.answersListBox.SelectedIndex;
            this.txtNumOfAnswered.Text = Answered.ToString();

            // Set Question answered
            ListBoxItem item = questionsListBox.ItemContainerGenerator.ContainerFromItem(questionsListBox.SelectedItem) as ListBoxItem;
            item.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private async void finishExamBtn_Click(object sender, RoutedEventArgs e)
        {
            int totalTrue = 0;
            for (int i = 0; i < exam.Questions.Count; i++)
            {
                string correctAnswer = string.Empty;
                foreach (var ans in exam.Questions[i].Answers)
                {
                    if (ans.IsCorrect)
                    {
                        correctAnswer = ans.Text;
                    }
                }
                if (this.Answers[i] == -1) // not answered
                {
                    Error err = new Error
                    {
                        QuestionTitle = exam.Questions[i].Text,
                        ChosenAnswer = "Not selected answer",
                        CorrectAnswer = correctAnswer
                    };
                    grade.Errors.Add(err);
                    continue;
                }
                if (exam.Questions[i].Answers[this.Answers[i]].IsCorrect) // correct Answer
                {
                    totalTrue += 1;
                }
                else // wrong answer
                {
                    Error err = new Error
                    {
                        QuestionTitle = exam.Questions[i].Text,
                        ChosenAnswer = exam.Questions[i].Answers[this.Answers[i]].Text,
                        CorrectAnswer = correctAnswer
                    };
                    grade.Errors.Add(err);
                }
            }
            grade._grade = ((double)totalTrue / (double)Answers.Count) * 100;
            grade._grade = Math.Round(grade._grade, 2);

            await HttpExamsRepository.Instance.CreateGradeAsync(exam.Id, grade);

            if (Answered != exam.Questions.Count) // Too Early
            {
                string msg = "NOT all Questions are answered, are you sure you finished?";
                MessageBoxResult res = MessageBox.Show(msg, "WAIT", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Close();
                }
            }
            else
            {
                string msg = "Are you sure you finished?";
                MessageBoxResult res = MessageBox.Show(msg, "WAIT", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Close();
                }
            }
        }
    }
}
