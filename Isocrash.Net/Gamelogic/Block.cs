namespace Isocrash.Net.Gamelogic
{
    public class Block : Item
    {
        public Block(string identifier) : base(identifier) { }

        public virtual void Tick() { }
    }
}