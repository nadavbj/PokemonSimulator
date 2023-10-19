using FluentAssertions;
using PokemonSimulator.logic;
using Xunit;

namespace PokemonSimulator.tests
{
    public class PokemonsFightSimulatorTests
    {
        [Fact]
        public async void ObviousWinnerTest()
        {
            var simulator = new PokemonsFightSimulator();
            var result = await simulator.GetFightResultsAsync("Charmander", "Omastar");
            result.Winner.Name.Should().Be("Omastar");
        }
    }
}
