﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Media;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Data;

namespace Familiada.ViewModel
{
    using Base;
    using Model;
    using Model.DAL;
    using System.IO;
    using System.Windows.Controls;

    class MainWindowViewModel: ViewModelBase
    {
        public MenuViewModel Menu { get; set; }
        public BoardViewModel Board { get; set; }
        public QuestionSectionViewModel QuestionSection { get; set; }
        public StrasburgerViewModel Strasburger { get; set; }
        public GameOverViewModel GameOver { get; set; }
        private Question[] questions;
        private int round;
        private string finalMessage;
        private string pathToSave;
        public string FinalMessage
        {
            get => finalMessage;
            set
            {
                finalMessage = value;
                OnPropertyChanged();
            }
        }

        public SoundPlayer Music { get; set; }
        private bool musicOn;
        public bool MusicOn
        {
            get => musicOn;
            set
            {
                musicOn = value;
                this.OnPropertyChanged();
            }
        }


        private ICommand checkAnswer;
        public ICommand CheckAnswer
        {
            get
            {
                if (checkAnswer == null)
                {
                    checkAnswer = new RelayCommand(
                    arg =>
                    {
                    int i = -1;
                    int notIt = 0;
                    foreach (var rightAnswer in Board.RightAnswers)
                    {
                        i++;
                        if (QuestionSection.Answer != "" && rightAnswer.Contains(QuestionSection.Answer.ToLower()) && !Board.DisplayedAnswers.Contains(QuestionSection.Answer.ToUpper()))
                        {
                            Board.Total += Convert.ToInt32(Board.Points[i]);
                            Board.DisplayedAnswers[i] =(i+1)+". "+QuestionSection.Answer.ToUpper();
                            Strasburger.CurrentGifPath = "/GameResources/STRASBURGER_WOW.gif";
                                Strasburger.GetRandomYay();
                            break;
                        }
                        else
                        {
                                notIt++;                                
                        }

                            if (notIt == 6)
                            {
                                Board.Loss++;
                                Board.CrossPaths[Board.Loss-1] = "/GameResources/Cross.gif";
                                Strasburger.CurrentGifPath = "/GameResources/STRASBURGER_Boo.gif";
                                Strasburger.GetRandomBoo();
                            }
                    }
                    if(Board.Loss==3)
                    {
                            if (round == 5)
                            {
                                FinalMessage = "Gratulacje " + Menu.TeamName + "! Twoja drużyna uzyskała " + Board.Total + " punktów.";
                                Board.Visible = "Hidden";
                                QuestionSection.Stopwatch.Stop();
                                SaveTotal();
                                round++;
                            }
                            else
                            {
                                /*Stopwatch time = new Stopwatch();
                                time.Restart();
                                while (2-time.Elapsed.Seconds>0)
                                {
                                    Console.WriteLine("dziala");
                                }
                                time.Stop();*/

                                
                                NewQuestion();
                            }
                    }


                        QuestionSection.Answer = "";
                    },
                    arg => round<=5 && Menu.Visible=="Hidden"
                    );

                }
                return checkAnswer;
            }
        }
        private ICommand musicOnOff;
        public ICommand MusicOnOff
        {
            get
            {
                if (musicOnOff == null)
                {
                    musicOnOff = new RelayCommand(
                        arg =>
                        {
                            if (MusicOn == true)
                            {
                                Music.Stop();
                                MusicOn = false;
                            }
                            else
                            {
                                Music.PlayLooping();
                                MusicOn = true;
                            }
                        },
                        arg => true
                        );
                }
                return musicOnOff;
            }
        }

        private ICommand newRound;

        public ICommand NewRound
        {
            get
            {
                if (newRound == null)
                {
                    newRound = new RelayCommand(
                        arg =>
                        {
                            NewQuestion();
                        },
                        arg => Menu.Visible == "Hidden"
                        );
                }
                return newRound;
            }
        }

        public MainWindowViewModel()
        {
            round = 0;
            Music = new SoundPlayer(@"..\..\Familjadee.wav");
            MusicOn = true;

            Menu = new MenuViewModel();
            Board = new BoardViewModel();
            Strasburger = new StrasburgerViewModel();
            QuestionSection = new QuestionSectionViewModel();

            questions = DataAccess.GetAllQuestions().ToArray();

            Music.PlayLooping();
            Menu.MenuClosed += NewQuestion;
            QuestionSection.TimeOver += NewQuestion;
           
            //NewQuestion();

            //QuestionSection.RealTimer.Elapsed+=GameLoop;
        }

        public async void NewQuestion()
        {
            if(round!=0) await Task.Delay(2000);
                QuestionSection.GetRandomQuestion(questions);
                Board.GetRightAnswers(QuestionSection.Question);
                Board.GetDisplayedAnswers(QuestionSection.Question);
                Strasburger.GetRandomJoke();

                Board.Loss = 0;
                QuestionSection.Stopwatch.Restart();
                round++;
                Strasburger.CurrentGifPath = "/GameResources/STRASBURGER_Talking.gif";


            for (int i = 0; i < 3; i++)
            {
                Board.CrossPaths[i] = "/GameResources/NoCross.gif";
            }
        }

        private void TimerSourceUpdated(object sender, DataTransferEventArgs e)
        {

        }


        public void SaveTotal()
        {
            pathToSave = @"..\..\GameResources\points.txt";
            string dataToSave = File.ReadAllText(@"..\..\GameResources\points.txt");
            dataToSave += $"{Menu.TeamName};{Board.Total}\n";
            File.WriteAllText(pathToSave, dataToSave);
            /*
            int counter = 1;
            
            foreach (string line in File.ReadLines(pathToSave))
            {
                if (line != String.Empty) ++counter;
            }
            string dataToSave = string.Empty;
            dataToSave += $"{counter}.{Menu.TeamName} {Board.Total} pkt";
            if (File.Exists(pathToSave))
            {
                if (new FileInfo(pathToSave).Length == 0)
                {
                    File.WriteAllText(pathToSave, dataToSave);
                    File.AppendAllText(pathToSave, "\n");
                }
                else
                {
                    File.AppendAllText(pathToSave, dataToSave);
                    File.AppendAllText(pathToSave, "\n");
                }

            }
            */
        }

        private ICommand bestScores;
        public ICommand BestScores
        {
            get
            {
                if (bestScores == null)
                {
                    bestScores = new RelayCommand(
                        arg =>
                        {
                            int i = 0;
                            string points = "";
                            List<string[]> scores = new List<string[]> ();
                            foreach (string line in File.ReadLines(@"..\..\GameResources\points.txt"))
                            { 
                                if (line != "")
                                {
                                    scores.Add(line.Split(';'));
                                }
                            }
                            scores = scores.OrderBy(arr => arr[1]).ToList();

                            foreach (string[] player in scores)
                            {
                                points += $"{++i}. {player[0]}   {player[1]} pkt.\n";
                            }
                            MessageBox.Show(points);
                        },
                        arg => true
                        );
                }
                return bestScores;
            }
        }

        private ICommand instruction;
        public ICommand Instruction
        {
            get
            {
                if (instruction == null)
                {
                    instruction = new RelayCommand(
                        arg =>
                        {
                            MessageBox.Show("Gra rozpoczyna się od wpisania nazwy drużyny. W trakcie rozgrywki należy wpisywać najbardziej pasujące do pytania odpowiedzi w różowym polu na dole okna i zatwierdzać je fioletowym przyciskiem.");
                        },
                        arg => true
                        );
                }
                return instruction;
            }
        }

        private ICommand newGame;
        public ICommand NewGame
        {
            get
            {
                if (newGame == null)
                {
                    newGame = new RelayCommand(
                        arg =>
                        {
                            Application.Restart();
                            System.Windows.Application.Current.Shutdown();
                        },
                        arg => true
                        );
                }
                return newGame;
            }
        }

    }
}
