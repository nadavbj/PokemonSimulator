using PokeApiNet;

namespace PokemonSimulator.logic
{
    public interface IPokemonsFightSimulator
    {
        public Task<PokemonFightSummary> GetFightResultsAsync(string pokemon1, string pokemon2);
    }

    public class PokemonsFightSimulator : IPokemonsFightSimulator
    {
        public async Task<PokemonFightSummary> GetFightResultsAsync(string pokemon1name, string pokemon2name)
        {
            // Init local variables
            PokeApiClient pokeClient =new PokeApiClient() ;
            Pokemon pokemon1= await pokeClient.GetResourceAsync<Pokemon>(pokemon1name);
            Pokemon pokemon2= await pokeClient.GetResourceAsync<Pokemon>(pokemon2name);
            Dictionary<Pokemon, double> pokemonHp=new Dictionary<Pokemon, double>();
            pokemonHp[pokemon1] = pokemon1.Stats.First(s => s.Stat.Name == "hp").BaseStat;
            pokemonHp[pokemon2] = pokemon2.Stats.First(s => s.Stat.Name == "hp").BaseStat;
            SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
            PokemonFightSummary result;
            List<Attack> attacks = new List<Attack>();

            // Run the simulation
            while (BothPokemonAlive())
            {
                attacks.Add(await SimulateTurnAsync(pokemon1, pokemon2));
                if (BothPokemonAlive())
                    attacks.Add(await SimulateTurnAsync(pokemon2, pokemon1));
            }
            return new PokemonFightSummary(pokemon1, pokemon2, attacks, GetWinner());

           
            async Task<Attack> SimulateTurnAsync(Pokemon attacker, Pokemon attacked)
            {
                var move = await ChooseMoveAsync(attacker, attacked);
                bool hit = await SimulateAttackAsync(attacker, attacked, move);
                return new Attack(attacker, attacked, move, hit);
            }

            Pokemon GetWinner()
            {
                return IsPokemonAlive(pokemon1) ? pokemon1 : pokemon2;
            }

            async Task<bool> SimulateAttackAsync(Pokemon attacker, Pokemon attacked, Move move)
            {
                Random rnd = new Random();
                if (rnd.Next(0, 100) <= move.Accuracy)
                {
                    var demage = await CalculateDamage(attacker, attacked, move);
                    var defense = attacked.Stats.First(s => s.Stat.Name == "defense").BaseStat;
                    if (demage > defense)
                    {
                        pokemonHp[attacked] -= demage - defense;
                    }
                    Console.WriteLine($"{attacker.Name} attacked {attacked.Name} with {move.Name} and made {demage - defense} damage");
                    return true;
                }
                return false;
            }

            async Task<Move> ChooseMoveAsync(Pokemon attacker, Pokemon attacked)
            {
                var allMovesTasks = attacker.Moves.Select(async m => await pokeClient.GetResourceAsync(m.Move));
                var allMoves = await Task.WhenAll(allMovesTasks);
                var unUsedMoves = allMoves.Where(m => attacks.All(a => a.Move.Name != m.Name));
                //Find which of them makes the most damage by avarage
                var movesDemageByProbability = await Task.WhenAll(unUsedMoves.Select(async m => (m.Accuracy * await CalculateDamage(attacker, attacked, m), m)));
                return movesDemageByProbability.MaxBy(p => p.Item1).Item2;
            }

            async Task<double> CalculateDamage(Pokemon attacker, Pokemon attacked, Move m)
            {
                var moveType = await pokeClient.GetResourceAsync(m.Type);
                var attackedTypes = await Task.WhenAll(attacked.Types.Select(async t => await pokeClient.GetResourceAsync(t.Type)));
                return GetDamageFactor(m, attackedTypes) * (attacker.Stats.First(s => s.Stat.Name == "attack").BaseStat + m.Pp ?? 0);
            }


            double GetDamageFactor(Move m, PokeApiNet.Type[] attackedTypes)
            {
                if (attackedTypes.Any(t => t.DamageRelations.NoDamageFrom.Contains(m.Type)))
                    return 0;
                else if (attackedTypes.Any(t => t.DamageRelations.HalfDamageFrom.Contains(m.Type)))
                    return 0.5;
                else if (attackedTypes.Any(t => t.DamageRelations.DoubleDamageFrom.Contains(m.Type)))
                    return 2;
                return 1;
            }

            bool BothPokemonAlive()
            {
                return IsPokemonAlive(pokemon1) && IsPokemonAlive(pokemon2);
            }

            bool IsPokemonAlive(Pokemon p)
            {
                return pokemonHp[p] > 0;
            }
        }
    }
}
