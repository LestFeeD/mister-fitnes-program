using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string TrainName { get; set; }
        public static ObservableCollection<int> SelectedExercises = new ObservableCollection<int>();
        public static List<int> SelectedDeletedExercises = new List<int>();

    }
}
