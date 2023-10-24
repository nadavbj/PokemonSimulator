using FluentAssertions;
using PokeApiNet;
using PokemonSimulator.logic;
using Xunit;

namespace PokemonSimulator.tests
{
    public class PokemonsFightSimulatorTests
    {
        [Fact]
        public async Task ObviousWinnerTest()
        {
            var simulator = new PokemonsFightSimulator();
            await simulator.Init("charmander", "omastar");
            var result = await simulator.SimulateFightAsync();
            result.Winner.Name.Should().Be("omastar");
        }

        // Grass type is sensitive to fire so the first move against it should be fire and not grass type
        [Fact]
        public async Task ChooseTheMostAppropriateMove()
        {
            var simulator = new PokemonsFightSimulator();
            
            var attacker = new Pokemon
            {
                Moves = new List<PokemonMove>
                {
                    new PokemonMove
                    {
                        Move = new NamedApiResource<Move>{
                            Name = "fire-punch",
                            Url = "https://pokeapi.co/api/v2/move/7/"
                        }
                    },
                    new PokemonMove
                    {
                        Move = new NamedApiResource<Move>
                        {
                            Name = "vine-whip",
                            Url = "https://pokeapi.co/api/v2/move/22/"
                        }
                    }
                },
                Stats = new List<PokemonStat>
                {
                    new PokemonStat
                    {
                        Stat = new NamedApiResource<Stat>
                        {
                            Name = "hp",
                            Url = "https://pokeapi.co/api/v2/stat/1/"
                        },
                        BaseStat = 52
                    },
                    new PokemonStat
                    {
                        Stat = new NamedApiResource<Stat>
                        {
                            Name = "attack",
                            Url = "https://pokeapi.co/api/v2/stat/2/"
                        },
                        BaseStat = 43
                    }   
                }
            };
            var attacked = new Pokemon
            {
                Types = new List<PokemonType>
                {
                    new PokemonType
                    {
                        Type = new NamedApiResource<PokeApiNet.Type>
                        {
                            Name = "grass",
                            Url = "https://pokeapi.co/api/v2/type/12/"
                        }
                    }
                },
                Stats = new List<PokemonStat>
                {
                    new PokemonStat
                    {
                        Stat = new NamedApiResource<Stat>
                        {
                            Name = "hp",
                            Url = "https://pokeapi.co/api/v2/stat/1/"
                        },
                        BaseStat = 52
                    }
                }
            };
            await simulator.Init(attacker,attacked);
            var move = await simulator.ChooseMoveAsync(attacker, attacked);
            move.Name.Should().Be("fire-punch");
            move = await simulator.ChooseMoveAsync(attacker, attacked);
            move.Name.Should().Be("vine-whip");
        }
    }
}
