using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Model
{
    public class Player
    {
        public IPAddress IP { get; }
        public string Nickname { get; }
        private volatile bool isValid = true;
        public bool IsValid => isValid;

        public Player(IPAddress ip, string nickname)
        {
            IP = ip;
            Nickname = nickname;
        }

        public void Invalidate()
        {
            isValid = false;
        }
    }
}