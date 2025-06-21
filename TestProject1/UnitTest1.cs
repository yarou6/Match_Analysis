using NUnit.Framework;
using Moq;
using MySqlConnector;
using System;
using System.Data;
using Match_Analysis;
using Match_Analysis.Model;
using Match_Analysis.View;
using System.Data.Common;

namespace TestProject1
{
    [TestFixture]
    public class TeamDBTests
    {
        private Mock<DbConnection> mockConnection;
        private Mock<MySqlCommand> mockCommand;
        private TeamDB teamDb;

        [SetUp]
        public void Setup()
        {
            mockConnection = new Mock<DbConnection>(MockBehavior.Strict);
            mockCommand = new Mock<MySqlCommand>(MockBehavior.Strict);

            // Создаем TeamDB с замоканным подключением
            // Если конструктор private, нужно использовать рефлексию для создания или изменить видимость
            // В примере предположим, что конструктор публичный для тестов
            teamDb = (TeamDB)Activator.CreateInstance(typeof(TeamDB), true);
            typeof(TeamDB).GetField("connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(teamDb, mockConnection.Object);
        }

        [Test]
        public void Insert_ValidTeam_ReturnsTrueAndSetsId()
        {
            // Arrange
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockCommand.Object);

            mockCommand.Setup(cmd => cmd.Parameters.Add(It.IsAny<MySqlParameter>())).Returns((MySqlParameter p) => p);
            mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns((ulong)123); // id 123 возвращаем

            mockConnection.Setup(c => c.CloseConnection());

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(123, team.Id);

            mockConnection.Verify(c => c.OpenConnection(), Times.Once);
            mockCommand.Verify(cmd => cmd.ExecuteScalar(), Times.Once);
            mockConnection.Verify(c => c.CloseConnection(), Times.Once);
        }

        [Test]
        public void Insert_NullConnection_ReturnsFalse()
        {
            // Arrange
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };
            typeof(TeamDB).GetField("connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(teamDb, null);

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Insert_OpenConnectionFails_ReturnsFalse()
        {
            // Arrange
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            mockConnection.Setup(c => c.OpenConnection()).Returns(false);

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result);

            mockConnection.Verify(c => c.OpenConnection(), Times.Once);
            mockConnection.Verify(c => c.CloseConnection(), Times.Once);
        }

        [Test]
        public void Insert_ExecuteScalarThrowsException_ReturnsFalse()
        {
            // Arrange
            var team = new Team { Title = "TestTeam", Coach = "TestCoach", City = "TestCity" };

            mockConnection.Setup(c => c.OpenConnection()).Returns(true);
            mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockCommand.Object);

            mockCommand.Setup(cmd => cmd.Parameters.Add(It.IsAny<MySqlParameter>())).Returns((MySqlParameter p) => p);
            mockCommand.Setup(cmd => cmd.ExecuteScalar()).Throws(new Exception("DB error"));

            mockConnection.Setup(c => c.CloseConnection());

            // Act
            var result = teamDb.Insert(team);

            // Assert
            Assert.IsFalse(result);

            mockConnection.Verify(c => c.OpenConnection(), Times.Once);
            mockCommand.Verify(cmd => cmd.ExecuteScalar(), Times.Once);
            mockConnection.Verify(c => c.CloseConnection(), Times.Once);
        }
    }
}