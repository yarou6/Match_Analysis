using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.VM
{
    internal class ProsmotrTournStat:  BaseVM
    {

        public CommandMvvm Vozvrat { get; set; }
        public ProsmotrTournStat()
        {

            Vozvrat = new CommandMvvm(() =>
            {

                close?.Invoke();

            }, () => true);


        }
        Action close;

        internal void SetClose(Action close)
        {
            this.close = close;
        }
    }
}
