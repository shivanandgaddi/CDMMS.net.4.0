using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.Network.Engineering.TypeLibrary
{
    public class TID
    {
        private string elevenCharacterClli = string.Empty;
        private string floor = string.Empty;
        private string aisle = string.Empty;
        private string bay = string.Empty;
        private string relayRack = string.Empty;
        private string alphaShelf = string.Empty;

        public TID()
        {
        }

        public string ElevenCharacterClli
        {
            get
            {
                return elevenCharacterClli;
            }
            set
            {
                elevenCharacterClli = value;
            }
        }

        public override string ToString()
        {
            StringBuilder tid = new StringBuilder(elevenCharacterClli);

            tid.Append(floor);

            if (!string.IsNullOrEmpty(relayRack))
                tid.Append(relayRack);
            else
            {
                tid.Append(aisle);
                tid.Append(bay);
            }

            tid.Append(alphaShelf);

            return tid.ToString();
        }

        public string Floor
        {
            get
            {
                return floor;
            }
            set
            {
                floor = value;
            }
        }

        public string Aisle
        {
            get
            {
                return aisle;
            }
            set
            {
                aisle = value;
            }
        }

        public string Bay
        {
            get
            {
                return bay;
            }
            set
            {
                bay = value;
            }
        }

        public string RelayRack
        {
            get
            {
                return relayRack;
            }
            set
            {
                relayRack = value;
            }
        }

        public string AlphaShelf
        {
            get
            {
                return alphaShelf;
            }
            set
            {
                alphaShelf = value;
            }
        }
    }
}
