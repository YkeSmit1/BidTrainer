using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MvvmHelpers.Commands;
using Newtonsoft.Json;

namespace Wpf.BidTrainer.Views
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage
    {
        public List<Lesson> Lessons { get; }

        public Command StartLessonCommand { get; }

        public Lesson Lesson { get; private set; }
        public bool IsContinueWhereLeftOff;

        public StartPage()
        {
            InitializeComponent();
            Lessons = JsonConvert.DeserializeObject<List<Lesson>>(File.ReadAllText("Lessons.json"));
            DataContext = this;
            StartLessonCommand = new Command<int>(ChooseLesson);
            var currentLesson = Lessons.Where(x => x.LessonNr == Settings1.Default.CurrentLesson).ToList();
            Lesson = currentLesson.Count != 0 ? currentLesson.First() : Lessons.First();
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
