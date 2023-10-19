namespace PokemonSimulator.logic
{
    public interface IFightsDB
    {
        public Task<PokemonFightSummary> GetFightResultsAsync(int fightId);

        public Task<int> AddFightAsync(PokemonFightSummary fight);
    }
}
