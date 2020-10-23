using System;
using System.Collections.Generic;

namespace QueueingApi.Model
{
    public class QueuePool
    {
        public QueuePool()
        {
            LoadClients(300); // Loads the number of clients
        }

        private readonly List<int> _clientPool = 
            new List<int>();

        private void LoadClients(int maxNo)
        {
            for (var i = 0; i < maxNo + 1; i++)
            {
                _clientPool.Add(i);  
            }
        }

        public int PrevClient(int clientNo)
        {
            var newClientNo = clientNo;
            var index = _clientPool.IndexOf(clientNo);

            if (index <= -1) return newClientNo;

            index--;

            try
            {
                newClientNo = _clientPool[index];
            }
            catch (Exception)
            {
                newClientNo = _clientPool[_clientPool.Count - 1];
            }

            return newClientNo;
        }

        public int NextClient(int clientNo)
        {
            var newClientNo = clientNo;
            var index = _clientPool.IndexOf(clientNo);

            if (index <= -1) return newClientNo;

            index++;

            try
            {
                newClientNo = _clientPool[index];
            }
            catch (Exception)
            {
                newClientNo = _clientPool[0];
            }

            return newClientNo;
        }

    }
}
