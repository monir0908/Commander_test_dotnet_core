using System.Collections.Generic;
using Commander.Models;

namespace Commander.Data
{
    public class MockCommanderRepo : ICommanderRepo
    {
        public IEnumerable<Command> GetAppCommands()
        {
            var commands = new List<Command>{
                new Command {Id=1, HowTo="Bol an egg", Line="Kettle", Platform="Kettle"},
                new Command {Id=2, HowTo="Cut Bread", Line="Chopping board" , Platform="Bread"},
                new Command {Id=3, HowTo="Go Shopping", Line="Wallet", Platform="Kettle"}
            };

            return commands;


        }

        public Command GetCommandById(int id)
        {
            return new Command {Id=1, HowTo="Bol an egg", Line="Kettle", Platform="Kettle"};
        }
    }


}