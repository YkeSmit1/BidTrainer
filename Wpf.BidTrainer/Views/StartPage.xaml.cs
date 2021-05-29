using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MvvmHelpers.Commands;
using Newtonsoft.Json;

namespace Wpf.BidTrainer
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Window
    {
        public List<Lesson> Lessons { get; set; }

        public Command StartLessonCommand { get; set; }

        public Lesson Lesson { get; set; }
        public bool IsContinueWhereLeftOff = false;

        public StartPage()
        {
            InitializeComponent();
            Lessons = JsonConvert.DeserializeObject<List<Lesson>>(File.ReadAllText("Lessons.json"));
            DataContext = this;
            StartLessonCommand = new Command<int>(ChooseLesson);
            var currentLesson = Lessons.Where(x => x.LessonNr == Settings1.Default.CurrentLesson);
            Lesson = currentLesson.Any() ? currentLesson.First() : Lessons.First();
        }

        private void Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            Close();
            IsContinueWhereLeftOff = true;
        }

        private void ChooseLesson(int lessonNr)
        {
            Lesson = Lessons.Single(x => x.LessonNr == lessonNr);
            Settings1.Default.CurrentLesson = Lesson.LessonNr;
            Close();
        }
    }
}
