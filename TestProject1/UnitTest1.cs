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
        // ������ ��������� ����������� � ��
        private Mock<IConnectionWrapper> mockConnection;

        // �������� ���������� ������� ��� ������ ��� ��������� ����
        private FakeCommandWrapper fakeCommand;

        // ����������� ������
        private TeamDB teamDb;

        [SetUp]
        public void Setup()
        {
            // ������������� ����� ����� ������ ������
            mockConnection = new Mock<IConnectionWrapper>(MockBehavior.Strict);
            fakeCommand = new FakeCommandWrapper();
            teamDb = new TeamDB(mockConnection.Object);
        }

        [Test]
        //	�������� ������� ������� � ��������� ID
        public void Insert_ValidTeam_ReturnsTrueAndSetsId()
        {
            // Arrange: ������ ������� � ������ ��������� �����������
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            // ��������� �������� ID ����������� ������
            var fakeCommand = new FakeCommandWrapper { ScalarResult = (ulong)123 };

            // ��������� �����������: ��������, �������� �������, ��������
            var mockConnection = new Mock<IConnectionWrapper>();
            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            // �������� ������� � ���������� ������������
            var teamDb = new TeamDB(mockConnection.Object);

            // Act: ��������� �������
            var result = teamDb.Insert(team);

            // Assert: ���������, ��� �� ������ �������
            Assert.IsTrue(result); // ����� ������ true
            Assert.AreEqual(123, team.Id); // id ��� ����������
            Assert.That(fakeCommand.ParametersList.Count, Is.EqualTo(3)); // ��������� ��� ���������
            Assert.That(fakeCommand.ParametersList[0].name, Is.EqualTo("Title")); // �������� ����� ���������
        }

        [Test]
        //������ �� ������� ��� ���������� �����������
        public void Insert_NullConnection_ReturnsFalse()
        {
            // Arrange: ������� ������� � null-������������
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };
            var db = new TeamDB(null); // ������� null ������ �����������

            // Act
            var result = db.Insert(team);

            // Assert
            Assert.IsFalse(result); // ������� ����� ��-�� ���������� �����������
        }

        [Test]
        //	��������� ������, ����� ���������� �� ���������
        public void Insert_OpenConnectionFails_ReturnsFalse()
        {
            // Arrange: ����������� �� �����������
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };
            mockConnection.Setup(c => c.OpenConnection()).Returns(false); // ���������� false

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result); // ������� ����������
            mockConnection.Verify(c => c.OpenConnection(), Times.Once); // ���������, ��� OpenConnection ��������� ���� ���
        }

        [Test]
        //��������� ������ ��� ���������� �������
        public void Insert_ExecuteScalarThrowsException_ReturnsFalse()
        {
            // Arrange: ���������� �������, ������� �������� ���������� ��� ExecuteScalar
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            fakeCommand.ThrowOnExecuteScalar = true; // ���������� ������ ��� ���������� �������

            // ��������� ���� �����������
            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result); // ����� ������ ������� false ��-�� ����������
            mockConnection.Verify(c => c.OpenConnection(), Times.Once); // �������� ������ ��������
            mockConnection.Verify(c => c.CloseConnection(), Times.Once); // �������� ������ ��������
        }
    }



    [TestFixture]
    public class PlayerDBTests
    {
        // ����������� ����� ������� ���������� � �������
        private Mock<IConnectionWrapper> mockConnection; // ��� ��� ���������� ����������� � ��
        private FakeCommandWrapper fakeCommand;          // �������� ������� ��� ��������� ���������� � �������� ����������
        private PlayerDB playerDb;                       // ����������� �����

        // �����, ������� ����������� ����� ������ ������
        [SetUp]
        public void Setup()
        {
            mockConnection = new Mock<IConnectionWrapper>(MockBehavior.Strict); // ������� ��� ���������� � ������� ����������
            fakeCommand = new FakeCommandWrapper();                             // ������� �������� �������
            playerDb = new PlayerDB(mockConnection.Object);                     // ������� ������ ������ � ����������� �����������
        }

        // ���������� ����: �������� ���������� ������� ������
        [Test]
        public void Insert_ValidPlayer_ReturnsTrueAndSetsId()
        {
            var player = new Player { Name = "John", Surname = "Doe", Age = 25, PlayerPosition = "FW", TeamId = 1, Patronymic = "Jr" };
            fakeCommand.ScalarResult = (ulong)42; // ������������ ID

            // ��������� �����
            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Insert(player); // ����� ������

            // �������� ����������
            Assert.IsTrue(result);
            Assert.AreEqual(42, player.Id); // ID ������ ������������
            Assert.That(fakeCommand.ParametersList.Count, Is.EqualTo(6)); // ������ ���� 6 ����������
        }

        // ���������� ����: ����������� = null
        [Test]
        public void Insert_NullConnection_ReturnsFalse()
        {
            var db = new PlayerDB(null); // �������� null ������ ����������
            var result = db.Insert(new Player());
            Assert.IsFalse(result); // ������� ����������
        }

        // ���������� ����: ������ ��� ExecuteScalar
        [Test]
        public void Insert_ThrowsException_ReturnsFalse()
        {
            var player = new Player { Name = "Test", Surname = "Player", Age = 20 };
            fakeCommand.ThrowOnExecuteScalar = true; // ��������� ����������

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Insert(player);
            Assert.IsFalse(result); // ������ ������� false ��-�� ����������
        }

        // ���������� ����: ���������� ���������� ������
        [Test]
        public void Update_ValidPlayer_ReturnsTrue()
        {
            var player = new Player { Id = 1, Name = "Edit", Surname = "Player", Age = 22, PlayerPosition = "MF", TeamId = 1, Patronymic = "Sr" };

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Update(player);
            Assert.IsTrue(result); // ���������� ������
        }

        // ���������� ����: ���������� �������� ������
        [Test]
        public void Remove_ValidPlayer_ReturnsTrue()
        {
            var player = new Player { Id = 99 };

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Remove(player);
            Assert.IsTrue(result); // �������� ������
        }

        // ����������� ������: ������� ������ ����������� (�� �� �� ����������)
        [Test]
        public void Insert_InvalidAgeUnderLimit_ReturnsTrueBecauseDBDoesNotValidateAge()
        {
            var player = new Player { Name = "TooYoung", Surname = "Test", Age = 15, PlayerPosition = "DF", TeamId = 1, Patronymic = "Jr" };
            fakeCommand.ScalarResult = (ulong)100;

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(fakeCommand);
            mockConnection.Setup(c => c.CloseConnection());

            var result = playerDb.Insert(player);

            Assert.IsTrue(result); // ������� �������, �.�. ����������� �������� ���� ������ �� ������ ViewModel
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
                Name = "", // ������ ��� ������� IndexOutOfRange
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

            // ������� PlayerDB.GetDb() �������� �� ����������� �������� (����� �������� � IoC �����������)
            // ����� �� ��������, ��� close ����������
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