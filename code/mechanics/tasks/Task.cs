
namespace Lurkers_revamped
{
    public class Task
    {
        public int ID;

        public string Description { get; set; }

        public int Amount { get; set; }

        public int Progression { get; set; }    

        public Task() { }

        public Task(string desc, int amount)
        {
            Description = desc;
            Amount = amount;
        }
    }
}