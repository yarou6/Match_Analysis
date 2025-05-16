using Match_Analysis.Model;
using Match_Analysis.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Match_Analysis.VM
{
    internal class AddEditPlayerHistory : BaseVM
    {

        public DateTime? EntryDate
        {
            get => NewPlayerHistory.EntryDate;
            set
            {
                if (NewPlayerHistory.EntryDate != value)
                {
                    NewPlayerHistory.EntryDate = value;

                    // Проверка: ReleaseDate >= EntryDate
                    if (NewPlayerHistory.ReleaseDate != null && NewPlayerHistory.ReleaseDate < NewPlayerHistory.EntryDate)
                    {
                        MessageBox.Show("Дата выхода не может быть раньше даты входа.");
                        NewPlayerHistory.ReleaseDate = null;
                        Signal(nameof(ReleaseDate));
                    }

                    Signal();
                    Signal(nameof(EntryDate));
                }
            }
        }

        public DateTime? ReleaseDate
        {
            get => NewPlayerHistory.ReleaseDate;
            set
            {
                if (NewPlayerHistory.ReleaseDate != value)
                {
                    // Проверка: ReleaseDate >= EntryDate
                    if (value != null && NewPlayerHistory.EntryDate != null && value < NewPlayerHistory.EntryDate)
                    {
                        MessageBox.Show("Дата выхода не может быть раньше даты входа.");
                        return;
                    }

                    NewPlayerHistory.ReleaseDate = value;
                    Signal();
                    Signal(nameof(ReleaseDate));
                }
            }
        }
        private PlayerHistory newPlayerHistory = new();

        public PlayerHistory NewPlayerHistory
        {
            get => newPlayerHistory;
            set
            {

                newPlayerHistory = value;
                Signal();
            }
        }

        private PlayerHistory selectedPlayerHistory;
        public PlayerHistory SelectedPlayerHistory
        {
            get => selectedPlayerHistory;
            set
            {
                selectedPlayerHistory = value;
                Signal();

                if (selectedPlayerHistory != null)
                {
                    NewPlayerHistory = new PlayerHistory
                    {
                        Id = selectedPlayerHistory.Id,
                        PlayerId = selectedPlayerHistory.PlayerId,
                        TeamId = selectedPlayerHistory.TeamId,
                        EntryDate = selectedPlayerHistory.EntryDate,
                        ReleaseDate = selectedPlayerHistory.ReleaseDate
                    };

                    SelectedTeam = Teams?.FirstOrDefault(t => t.Id == selectedPlayerHistory.TeamId);
                }
                else
                {
                    NewPlayerHistory = new PlayerHistory();
                    SelectedTeam = null;
                }

            }
        }

        private ObservableCollection<PlayerHistory> playerHistories = new();
        public ObservableCollection<PlayerHistory> PlayerHistories
        {
            get => playerHistories;
            set
            {
                playerHistories = value;
                Signal();
            }
        }

        public List<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                Signal();
            }
        }

        private Player newPlayer = new();

        public Player NewPlayer
        {
            get => newPlayer;
            set
            {
                newPlayer = value;
                Signal();
            }
        }

        private Team selectedTeam;

        public Team SelectedTeam
        {
            get => selectedTeam;
            set
            {
                if (selectedTeam != value)
                {
                    selectedTeam = value;

                    if (selectedTeam != null)
                        NewPlayerHistory.TeamId = selectedTeam.Id;

                    Signal();
                }
            }
        }
        public CommandMvvm AddTeam { get; set; }
        public CommandMvvm RemoveHistory { get; set; }

        public CommandMvvm EditTeam { get; set; }

        public CommandMvvm Exit { get; set; }

        public AddEditPlayerHistory()
        {

            AddTeam = new CommandMvvm(() =>
            {

                // Очистка MinValue
                if (NewPlayerHistory.ReleaseDate == DateTime.MinValue)
                    NewPlayerHistory.ReleaseDate = null;

                var lastHistory = PlayerHistories
                    .Where(p => p.PlayerId == NewPlayer.Id)
                    .OrderByDescending(p => p.EntryDate)
                    .FirstOrDefault();

                if (lastHistory != null &&
                    (lastHistory.ReleaseDate == null || lastHistory.ReleaseDate == DateTime.MinValue))
                {
                    MessageBox.Show("Добавьте предыдущей команде дату выхода.");
                    return;
                }

                // EntryDate новой записи меньше ReleaseDate предыдущей
                if (lastHistory != null && NewPlayerHistory.EntryDate < lastHistory.ReleaseDate)
                {
                    MessageBox.Show("Дата входа не может быть раньше даты выхода предыдущей команды.");
                    return;
                }

                // Проверка на выбор команды
                //if (SelectedTeam == null)
                //{
                //    MessageBox.Show("Выберите команду.");
                //    return;
                //}

                // Установка данных
                NewPlayerHistory.Id = 0;
                NewPlayerHistory.TeamId = SelectedTeam.Id;
                NewPlayerHistory.PlayerId = NewPlayer.Id;

                // Обновляем текущую команду игрока
                NewPlayer.TeamId = NewPlayerHistory.TeamId;

                // Вставка
                PlayerHistoryDB.GetDb().Insert(NewPlayerHistory);

                SelectAll();
                NewPlayerHistory = new PlayerHistory();

            }, () => 
            {
                return NewPlayerHistory.EntryDate != null && SelectedTeam != null;
            });


            EditTeam = new CommandMvvm(() =>
            {
                NewPlayerHistory.TeamId = SelectedTeam.Id;
                NewPlayerHistory.PlayerId = NewPlayer.Id;

                PlayerHistoryDB.GetDb().Update(NewPlayerHistory);
                if (NewPlayerHistory.ReleaseDate == null)
                {
                    NewPlayer.TeamId = SelectedTeam.Id;
                    PlayerDB.GetDb().Update(NewPlayer);
                }

                SelectAll();

                NewPlayerHistory = new PlayerHistory();

            }, () => SelectedPlayerHistory != null);



            RemoveHistory = new CommandMvvm(() =>
            {
                var playervozvrat = MessageBox.Show("Вы уверены что хотите удалить историю?", "Подтверждение", MessageBoxButton.YesNo);

                if (playervozvrat == MessageBoxResult.Yes)
                {
                    PlayerHistoryDB.GetDb().Remove(SelectedPlayerHistory);
                }

                SelectAll();
            }, () => SelectedPlayerHistory != null);

            Exit = new CommandMvvm(() =>
            {
                close?.Invoke();
            }, () => true);

        }



        public void SetPlayer(Player editPlayer)
        {
            NewPlayer = editPlayer;
            SelectAll();
        }

        Action close;
        private List<Team> teams = new();

        internal void SetClose(Action close)
        {
            this.close = close;
        }


        private void SelectAll()
        {
            PlayerHistories = new ObservableCollection<PlayerHistory>(PlayerHistoryDB.GetDb().SelectPlayer(NewPlayer.Id));
            Teams = new List<Team>(TeamDB.GetDb().SelectAll());
        }
    }
}
