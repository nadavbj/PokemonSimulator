using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PokeApiNet;
using PokemonSimulator.logic;
using System.Collections.Generic;

namespace PokemonSimulator.controllers
{
    [ApiController]
    [Route("api")]
    public class FightSimulatorApi : Controller
    {
        private readonly IFightsDB fightsDB;

        public FightSimulatorApi(IFightsDB fightsDB)
        {
            this.fightsDB = fightsDB;
        }

        [HttpGet("fight")]
        public async Task<string> GetAsync(string pokemon1, string pokemon2)
        {
            IPokemonsFightSimulator pokemonsFightSimulator = new PokemonsFightSimulator();
            var fight = await pokemonsFightSimulator.GetFightResultsAsync(pokemon1, pokemon2);
            var id = await fightsDB.AddFightAsync(fight);
            return $@" fight id for future use id { id}.
{fight.GetDescription()}";
        }

        [HttpGet("fight/{id}")]
        public async Task<string> GetAsync(int id)
        {
            var fight = await fightsDB.GetFightResultsAsync(id);
            return fight.GetDescription();
        }
    }
}
