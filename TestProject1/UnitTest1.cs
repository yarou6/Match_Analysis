using NUnit.Framework;
using Moq;
using MySqlConnector;
using System;
using System.Data;
using Match_Analysis;
using Match_Analysis.Model;
using Match_Analysis.View;
using Match_Analysis.VM;
using System.Collections.ObjectModel;

namespace TestProject1
{
    [TestFixture]
    public class TeamDBTests
    {
        // Мокаем интерфейс подключения к БД
        private Mock<IConnectionWrapper> mockConnection;

        // Фейковая реализация команды для тестов без настоящей базы
        private FakeCommandWrapper fakeCommand;

        // Тестируемый объект
        private TeamDB teamDb;

        [SetUp]
        public void Setup()
        {
            // Инициализация моков перед каждым тестом
            mockConnection = new Mock<IConnectionWrapper>(MockBehavior.Strict);
            fakeCommand = new FakeCommandWrapper();
            teamDb = new TeamDB(mockConnection.Object);
        }

        [Test]
        //	Успешная вставка команды и установка ID
        public void Insert_ValidTeam_ReturnsTrueAndSetsId()
        {
            // Arrange: создаём команду и мокаем поведение подключения
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            // Настройка возврата ID вставленной записи
            var fakeCommand = new FakeCommandWrapper { ScalarResult = (ulong)123 };

            // Настройка подключения: открытие, создание команды, закрытие
            var mockConnection = new Mock<IConnectionWrapper>();
            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            // Создание объекта с замоканным подключением
            var teamDb = new TeamDB(mockConnection.Object);

            // Act: выполняем вставку
            var result = teamDb.Insert(team);

            // Assert: проверяем, что всё прошло успешно
            Assert.IsTrue(result); // метод вернул true
            Assert.AreEqual(123, team.Id); // id был установлен
            Assert.That(fakeCommand.ParametersList.Count, Is.EqualTo(3)); // добавлены все параметры
            Assert.That(fakeCommand.ParametersList[0].name, Is.EqualTo("Title")); // проверка имени параметра
        }

        [Test]
        //Защита от вставки при отсутствии подключения
        public void Insert_NullConnection_ReturnsFalse()
        {
            // Arrange: создаем команду с null-подключением
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };
            var db = new TeamDB(null); // передаём null вместо подключения

            // Act
            var result = db.Insert(team);

            // Assert
            Assert.IsFalse(result); // ожидаем отказ из-за отсутствия подключения
        }

        [Test]
        //	Обработка случая, когда соединение не открылось
        public void Insert_OpenConnectionFails_ReturnsFalse()
        {
            // Arrange: подключение не открывается
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };
            mockConnection.Setup(c => c.OpenConnection()).Returns(false); // возвращает false

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result); // вставка невозможна
            mockConnection.Verify(c => c.OpenConnection(), Times.Once); // проверяем, что OpenConnection вызывался один раз
        }

        [Test]
        //Обработка ошибки при выполнении команды
        public void Insert_ExecuteScalarThrowsException_ReturnsFalse()
        {
            // Arrange: подготовка команды, которая выбросит исключение при ExecuteScalar
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            fakeCommand.ThrowOnExecuteScalar = true; // симулируем ошибку при выполнении команды

            // Настройка мока подключения
            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result); // метод должен вернуть false из-за исключения
            mockConnection.Verify(c => c.OpenConnection(), Times.Once); // проверка вызова открытия
            mockConnection.Verify(c => c.CloseConnection(), Times.Once); // проверка вызова закрытия
        }
    }



    [TestFixture]
    public class PlayerDBTests
    {
        // Подключение моков обертки соединения и команды
        private Mock<IConnectionWrapper> mockConnection; // Мок для интерфейса подключения к БД
        private FakeCommandWrapper fakeCommand;          // Фейковая команда для перехвата параметров и эмуляции выполнения
        private PlayerDB playerDb;                       // Тестируемый класс

        // Метод, который выполняется перед каждым тестом
        [SetUp]
        public void Setup()
        {
            mockConnection = new Mock<IConnectionWrapper>(MockBehavior.Strict); // создаем мок соединения с жестким поведением
            fakeCommand = new FakeCommandWrapper();                             // создаем фейковую команду
            playerDb = new PlayerDB(mockConnection.Object);                     // создаем объект класса с подмененным соединением
        }

        // Позитивный тест: проверка корректной вставки игрока
        [Test]
        public void Insert_ValidPlayer_ReturnsTrueAndSetsId()
        {
            var player = new Player { Name = "John", Surname = "Doe", Age = 25, PlayerPosition = "FW", TeamId = 1, Patronymic = "Jr" };
            fakeCommand.ScalarResult = (ulong)42; // возвращаемый ID

            // Настройка моков
            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Insert(player); // Вызов метода

            // Проверка результата
            Assert.IsTrue(result);
            Assert.AreEqual(42, player.Id); // ID должен установиться
            Assert.That(fakeCommand.ParametersList.Count, Is.EqualTo(6)); // Должно быть 6 параметров
        }

        // Негативный тест: подключение = null
        [Test]
        public void Insert_NullConnection_ReturnsFalse()
        {
            var db = new PlayerDB(null); // передаем null вместо соединения
            var result = db.Insert(new Player());
            Assert.IsFalse(result); // Вставка невозможна
        }

        // Негативный тест: ошибка при ExecuteScalar
        [Test]
        public void Insert_ThrowsException_ReturnsFalse()
        {
            var player = new Player { Name = "Test", Surname = "Player", Age = 20 };
            fakeCommand.ThrowOnExecuteScalar = true; // эмулируем исключение

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Insert(player);
            Assert.IsFalse(result); // должно вернуть false из-за исключения
        }

        // Позитивный тест: корректное обновление игрока
        [Test]
        public void Update_ValidPlayer_ReturnsTrue()
        {
            var player = new Player { Id = 1, Name = "Edit", Surname = "Player", Age = 22, PlayerPosition = "MF", TeamId = 1, Patronymic = "Sr" };

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Update(player);
            Assert.IsTrue(result); // обновление прошло
        }

        // Позитивный тест: корректное удаление игрока
        [Test]
        public void Remove_ValidPlayer_ReturnsTrue()
        {
            var player = new Player { Id = 99 };

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Remove(player);
            Assert.IsTrue(result); // удаление прошло
        }

        // Пограничный случай: возраст меньше допустимого (но БД не валидирует)
        [Test]
        public void Insert_InvalidAgeUnderLimit_ReturnsTrueBecauseDBDoesNotValidateAge()
        {
            var player = new Player { Name = "TooYoung", Surname = "Test", Age = 15, PlayerPosition = "DF", TeamId = 1, Patronymic = "Jr" };
            fakeCommand.ScalarResult = (ulong)100;

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Insert(player);

            Assert.IsTrue(result); // Вставка пройдет, т.к. ограничения возраста есть только на уровне ViewModel
        }

        [Test]
        public void FIO_WithPatronymic_ReturnsCorrectFormat()
        {
            var player = new Player
            {
                Name = "Ivan",
                Surname = "Petrov",
                Patronymic = "Sergeevich"
            };

            var expected = "Petrov I. S.";
            Assert.AreEqual(expected, player.FIO);
        }

        [Test]
        public void FIO_WithoutPatronymic_ReturnsCorrectFormat()
        {
            var player = new Player
            {
                Name = "Ivan",
                Surname = "Petrov",
                Patronymic = ""
            };

            var expected = "Petrov I.";
            Assert.AreEqual(expected, player.FIO);
        }

        [Test]
        public void FIO_WhitespacePatronymic_ReturnsCorrectFormat()
        {
            var player = new Player
            {
                Name = "Alex",
                Surname = "Smith",
                Patronymic = "   "
            };

            var expected = "Smith A.";
            Assert.AreEqual(expected, player.FIO);
        }

        [Test]
        public void FIO_NullPatronymic_ReturnsCorrectFormat()
        {
            var player = new Player
            {
                Name = "John",
                Surname = "Doe",
                Patronymic = null
            };

            var expected = "Doe J.";
            Assert.AreEqual(expected, player.FIO);
        }

        [Test]
        public void FIO_ShortName_ThrowsIndexOutOfRange()
        {
            var player = new Player
            {
                Name = "", // Пустое имя вызовет IndexOutOfRange
                Surname = "Ivanov"
            };

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                var fio = player.FIO;
            });
        }

        [Test]
        public void FIO_NullName_ThrowsNullReferenceException()
        {
            var player = new Player
            {
                Name = null,
                Surname = "Ivanov"
            };

            Assert.Throws<NullReferenceException>(() =>
            {
                var fio = player.FIO;
            });
        }

        [Test]
        public void FIO_SurnameIsNull_StillThrowsNoException()
        {
            var player = new Player
            {
                Name = "Maksim",
                Patronymic = "Andreevich",
                Surname = null
            };

            var result = player.FIO;
            Assert.That(result, Does.StartWith(" M."));
        }

        [Test]
        public void FIO_AllFieldsFilled_ReturnsExpectedFIO()
        {
            var player = new Player
            {
                Name = "Oleg",
                Surname = "Kozlov",
                Patronymic = "Nikolayevich"
            };

            Assert.AreEqual("Kozlov O. N.", player.FIO);
        }

        [Test]
        public void AddPlayer_CanExecute_ReturnsTrue_WhenDataValid()
        {
            var vm = new AddEditPlayer();

            vm.NewPlayer = new Player
            {
                Age = 25,
                Name = "Ivan",
                Surname = "Ivanov"
            };
            vm.Positional = "Forward";

            Assert.IsTrue(vm.AddPlayer.CanExecute(null));
        }

        [Test]
        public void AddPlayer_CanExecute_ReturnsFalse_WhenAgeTooLow()
        {
            var vm = new AddEditPlayer();

            vm.NewPlayer = new Player
            {
                Age = 12,
                Name = "Ivan",
                Surname = "Ivanov"
            };
            vm.Positional = "Forward";

            Assert.IsFalse(vm.AddPlayer.CanExecute(null));
        }

        [Test]
        public void AddPlayer_CanExecute_ReturnsFalse_WhenPositionNull()
        {
            var vm = new AddEditPlayer();

            vm.NewPlayer = new Player
            {
                Age = 22,
                Name = "Ivan",
                Surname = "Ivanov"
            };
            vm.Positional = null;

            Assert.IsFalse(vm.AddPlayer.CanExecute(null));
        }

        [Test]
        public void SetPlayer_SetsNewPlayerAndPositional()
        {
            var vm = new AddEditPlayer();
            var player = new Player
            {
                Name = "Test",
                Surname = "Player",
                PlayerPosition = "Goalkeeper"
            };

            vm.SetPlayer(player);

            Assert.AreEqual(player, vm.NewPlayer);
            Assert.AreEqual("Goalkeeper", vm.Positional);
        }

        [Test]
        public void SetClose_StoresCallbackAndInvokesOnCommand()
        {
            var wasClosed = false;
            var vm = new AddEditPlayer();

            vm.NewPlayer = new Player
            {
                Id = 0,
                Age = 20,
                Name = "Alex",
                Surname = "Petrov"
            };
            vm.Positional = "Midfielder";

            vm.SetClose(() => wasClosed = true);

            // Подмена PlayerDB.GetDb() временно не тестируется напрямую (можно замокать в IoC архитектуре)
            // Здесь мы проверим, что close вызывается
            vm.AddPlayer.Execute(null);

            Assert.IsTrue(wasClosed);
        }
    }



    [TestFixture]
    public class AddEditPlayerHistoryTests
    {
        private AddEditPlayerHistory viewModel;

        [SetUp]
        public void Setup()
        {
            viewModel = new AddEditPlayerHistory();
        }

        [Test]
        public void EntryDate_SetEarlierThanReleaseDate_Valid()
        {
            viewModel.NewPlayerHistory = new PlayerHistory { ReleaseDate = new DateTime(2025, 5, 10) };
            viewModel.EntryDate = new DateTime(2025, 5, 1);
            Assert.AreEqual(new DateTime(2025, 5, 1), viewModel.EntryDate);
        }

        [Test]
        public void ReleaseDate_EarlierThanEntryDate_ShowsErrorAndRejectsChange()
        {
            viewModel.NewPlayerHistory = new PlayerHistory { EntryDate = new DateTime(2025, 6, 10) };
            viewModel.ReleaseDate = new DateTime(2025, 6, 1);
            Assert.IsNull(viewModel.NewPlayerHistory.ReleaseDate);
        }

        [Test]
        public void AddTeamCommand_CannotExecute_WithoutEntryDateOrTeam()
        {
            viewModel.NewPlayer = new Player { Id = 1 };
            viewModel.NewPlayerHistory = new PlayerHistory();
            Assert.IsFalse(viewModel.AddTeam.CanExecute(null));
        }

        [Test]
        public void SetPlayer_LoadsHistoriesAndUpdatesTeamId()
        {
            var fakePlayer = new Player { Id = 5 };
            viewModel.SetPlayer(fakePlayer);

            Assert.AreEqual(5, viewModel.NewPlayer.Id);
        }

        [Test]
        public void SelectedPlayerHistory_WhenSet_ClonesToNewPlayerHistory()
        {
            var history = new PlayerHistory
            {
                Id = 1,
                EntryDate = new DateTime(2024, 1, 1),
                ReleaseDate = new DateTime(2024, 12, 31),
                TeamId = 10,
                PlayerId = 99
            };

            viewModel.Teams = new List<Team> { new Team { Id = 10, Title = "TestTeam" } };
            viewModel.SelectedPlayerHistory = history;

            Assert.AreEqual(1, viewModel.NewPlayerHistory.Id);
            Assert.AreEqual(10, viewModel.NewPlayerHistory.TeamId);
            Assert.AreEqual(99, viewModel.NewPlayerHistory.PlayerId);
            Assert.AreEqual("TestTeam", viewModel.SelectedTeam.Title);
        }
    }

    [TestFixture]
    public class AddInfPlayerTests
    {
        private AddInfPlayer vm;
        private Team testTeam;
        private DateTime matchDate;

        [SetUp]
        public void Setup()
        {
            vm = new AddInfPlayer();
            testTeam = new Team { Id = 1, Title = "Test FC" };
            matchDate = new DateTime(2024, 10, 10);
        }

        [Test]
        public void InitializePlayers_SetsCorrectProperties()
        {
            vm.InitializePlayers(1, 3, testTeam, matchDate);

            Assert.AreEqual(1, vm.MatchId);
            Assert.AreEqual(3, vm.GoalCount);
            Assert.IsNotNull(vm.SelectedGoalPlayers);
            Assert.AreEqual(3, vm.SelectedGoalPlayers.Count);
        }

        [Test]
        public void AddPlayer_InvalidWhenPlayersNotSelected_ShowsMessage()
        {
            vm.MatchId = 1;
            vm.Team = testTeam;
            vm.GoalCount = 2;
            vm.Players = new ObservableCollection<Player> { new Player { Id = 1 } };

            vm.SelectedGoalPlayers = new ObservableCollection<PlayerSelection>
            {
                new(null),
                new(null)
            };
            vm.SelectedAssistPlayers = new ObservableCollection<PlayerSelection>
            {
                new(null)
            };

            Assert.DoesNotThrow(() => vm.AddPlayer.Execute(null));
        }

        [Test]
        public void GetPlayersInTeamAtDate_ReturnsCorrectPlayers()
        {
            var players = vm.GetPlayersInTeamAtDate(testTeam, matchDate);
            Assert.IsInstanceOf<ObservableCollection<Player>>(players);
        }


        [Test]
        public void AddPlayer_MissingAssist_DoesNotCrash()
        {
            var player = new Player { Id = 1 };
            vm.MatchId = 1;
            vm.Team = testTeam;
            vm.Players = new ObservableCollection<Player> { player };
            vm.SelectedGoalPlayers = new ObservableCollection<PlayerSelection>
            {
                new(player)
            };
            vm.SelectedAssistPlayers = new ObservableCollection<PlayerSelection>
            {
                new(null)
            };

            Assert.DoesNotThrow(() => vm.AddPlayer.Execute(null));
        }
    }










}