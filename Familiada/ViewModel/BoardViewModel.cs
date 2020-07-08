﻿using Familiada.ViewModel.Base;
using Familiada.Model;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace Familiada.ViewModel
{
    class BoardViewModel: ViewModelBase
    {
        private ObservableCollection<string> rightAnswers;
        private ObservableCollection<string> displayedAnswers;
        private ObservableCollection<string> points;
        private int total;

        public ObservableCollection<string> RightAnswers
        {
            get => rightAnswers;
            set { rightAnswers = value; this.OnPropertyChanged(); }
        }

       public ObservableCollection<string> DisplayedAnswers
        {
            get => displayedAnswers;
            set { displayedAnswers = value; this.OnPropertyChanged(); }
        }
        public ObservableCollection<string> Points
        {
            get => points;
            set { points = value; OnPropertyChanged(); }
        }
        public int Total
        {
            get => total;
            set { total = value; OnPropertyChanged(); }
        }

        public void GetRightAnswers(Question currentQuestion)
        {
            foreach (var answer in currentQuestion.Answers)
            {
                RightAnswers.Add(answer);
            }
            foreach (var point in currentQuestion.Points)
            {
                Points.Add(point);
            }
        }
        
        public BoardViewModel()
        {
            RightAnswers = new ObservableCollection<string>();
            DisplayedAnswers = new ObservableCollection<string>();
            Points = new ObservableCollection<string>();

            for (int i = 0; i < 6; i++)
            {
                DisplayedAnswers.Add((i+1)+". ---------------");
            }
        }



    }
}
