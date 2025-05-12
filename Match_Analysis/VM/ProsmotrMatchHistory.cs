using Match_Analysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.VM
{
    internal class ProsmotrMatchHistory : BaseVM
    {

        

        public CommandMvvm Vozvrat { get; set; }
        public ProsmotrMatchHistory()
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