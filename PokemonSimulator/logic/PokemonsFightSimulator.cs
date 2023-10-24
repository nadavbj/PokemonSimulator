using PokeApiNet;

namespace PokemonSimulator.logic
{
    public class PokemonsFightSimulator
    {

        // Init local variables
        private PokeApiClient pokeClient = new PokeApiClient();
        private Pokemon pokemon1;
        private Pokemon pokemon2;
        private Dictionary<Pokemon, double> pokemonHp = new Dictionary<Pokemon, double>();
        private PokemonFightSummary result;
        private List<Attack> attacks = new List<Attack>();

        internal PokemonsFightSimulator()
        {
        }

        public static async Task<PokemonFightSummary> SimulateFightAsync(string pokemon1name, string pokemon2name)
        {
            var simulator = new PokemonsFightSimulator();
            await simulator.Init(pokemon1name, pokemon2name);
            return await simulator.SimulateFightAsync();
        }

        internal async Task Init(string pokemon1name, string pokemon2name)
        {
            pokeClient = new PokeApiClient();
            pokemon1 = await pokeClient.GetResourceAsync<Pokemon>(pokemon1name);
            pokemon2 = await pokeClient.GetResourceAsync<Pokemon>(pokemon2name);
            pokemonHp = new Dictionary<Pokemon, double>();
            pokemonHp[pokemon1] = pokemon1.Stats.First(s => s.Stat.Name == "hp").BaseStat;
            pokemonHp[pokemon2] = pokemon2.Stats.First(s => s.Stat.Name == "hp").BaseStat;
            attacks = new List<Attack>();
        }

        // For tests purpuses
        internal async Task Init(Pokemon pokemon1, Pokemon pokemon2)
        {
            pokeClient = new PokeApiClient();
            this.pokemon1 = pokemon1;
            this.pokemon2 = pokemon2;
            pokemonHp = new Dictionary<Pokemon, double>();
            pokemonHp[pokemon1] = pokemon1.Stats.First(s => s.Stat.Name == "hp").BaseStat;
            pokemonHp[pokemon2] = pokemon2.Stats.First(s => s.Stat.Name == "hp").BaseStat;
            attacks = new List<Attack>();
        }

        internal async Task<PokemonFightSummary> SimulateFightAsync()
        {

            // Run the simulation
            while (BothPokemonAlive())
            {
                attacks.Add(await SimulateTurnAsync(pokemon1, pokemon2));
                if (BothPokemonAlive())
                    attacks.Add(await SimulateTurnAsync(pokemon2, pokemon1));
            }
            return new PokemonFightSummary(pokemon1, pokemon2, attacks, GetWinner());

        }
        internal async Task<Attack> SimulateTurnAsync(Pokemon attacker, Pokemon attacked)
        {
            var move = await ChooseMoveAsync(attacker, attacked);
            bool hit = await SimulateAttackAsync(attacker, attacked, move);
            return new Attack(attacker, attacked, move, hit);
        }

        internal Pokemon GetWinner()
        {
            return IsPokemonAlive(pokemon1) ? pokemon1 : pokemon2;
        }

        internal async Task<bool> SimulateAttackAsync(Pokemon attacker, Pokemon attacked, Move move)
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

        internal async Task<Move> ChooseMoveAsync(Pokemon attacker, Pokemon attacked)
        {
            var allMovesTasks = attacker.Moves.Select(async m => await pokeClient.GetResourceAsync(m.Move));
            var allMoves = await Task.WhenAll(allMovesTasks);
            var unUsedMoves = allMoves.Where(m => attacks.All(a => a.Move.Name != m.Name));
            //Find which of them makes the most damage by avarage
            var movesDemageByProbability = await Task.WhenAll(unUsedMoves.Select(async m => (m.Accuracy * await CalculateDamage(attacker, attacked, m), m)));
            return movesDemageByProbability.MaxBy(p => p.Item1).Item2;
        }

        internal async Task<double> CalculateDamage(Pokemon attacker, Pokemon attacked, Move m)
        {
            var moveType = await pokeClient.GetResourceAsync(m.Type);
            var attackedTypes = await Task.WhenAll(attacked.Types.Select(async t => await pokeClient.GetResourceAsync(t.Type)));
            return GetDamageFactor(m, attackedTypes) * (attacker.Stats.First(s => s.Stat.Name == "attack").BaseStat + m.Pp ?? 0);
        }


        internal double GetDamageFactor(Move m, PokeApiNet.Type[] attackedTypes)
        {
            if (attackedTypes.Any(t => t.DamageRelations.NoDamageFrom.Any(d => d.Name == m.Type.Name)))
                return 0;
            else if (attackedTypes.Any(t => t.DamageRelations.HalfDamageFrom.Any(d => d.Name == m.Type.Name)))
                return 0.5;
            else if (attackedTypes.Any(t => t.DamageRelations.DoubleDamageFrom.Any(d => d.Name == m.Type.Name)))
                return 2;
            return 1;
        }

        internal bool BothPokemonAlive()
        {
            return IsPokemonAlive(pokemon1) && IsPokemonAlive(pokemon2);
        }

        internal bool IsPokemonAlive(Pokemon p)
        {
            return pokemonHp[p] > 0;
        }
    }
}

