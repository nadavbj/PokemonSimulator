using PokeApiNet;

namespace PokemonSimulator.logic
{
    public class Attack
    {
        public Pokemon Attacker { get ; set ; }
        public Pokemon Attacked { get; set; }
        public Move Move { get; set; }
        public bool Hit { get; set; }

        public Attack(Pokemon attacker, Pokemon attacked, Move move, bool hit)
        {
            this.Attacker = attacker;
            this.Attacked = attacked;
            this.Move = move;
            this.Hit = hit;
        }

    }
}