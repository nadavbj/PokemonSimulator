using PokeApiNet;

namespace PokemonSimulator.logic
{
    public class PokemonFightSummary
    {
        private readonly Pokemon pokemon1;
        private readonly Pokemon pokemon2;
        private readonly IEnumerable<Attack> attacks;
        private readonly Pokemon winner;

        public PokemonFightSummary(Pokemon pokemon1, Pokemon pokemon2, IEnumerable<Attack> attacks, Pokemon winner)
        {
            this.pokemon1 = pokemon1;
            this.pokemon2 = pokemon2;
            this.attacks = attacks;
            this.winner = winner;
        }

        public Pokemon Pokemon1 => pokemon1;

        public Pokemon Pokemon2 => pokemon2;

        public IEnumerable<Attack> Attacks => attacks;

        public Pokemon Winner => winner;

        internal string GetDescription()
        {
            return @$"In the fight between {Pokemon1.Name} and {Pokemon2.Name} the winner is {Winner.Name}
The fight was the following:
" + string.Join('\n', Attacks.Select(a => $"{a.Attacker.Name} attacked {a.Attacked.Name} with {a.Move.Name} and {(a.Hit ? "hit" : "missed")}"));
        }
    }
}