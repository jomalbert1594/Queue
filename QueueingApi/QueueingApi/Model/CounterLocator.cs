using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;

namespace QueueingApi.Model
{
    public class CounterLocator
    {
        public CounterLocator()
        {
            IntializePools(); // InitializePools
        }


        #region Properties

        public QueuePool QueuePool1 { get; set; }
        public QueuePool QueuePool2 { get; set; }
        public QueuePool QueuePool3 { get; set; }
        public QueuePool QueuePool4 { get; set; }

        #endregion

        #region Methods

        private void IntializePools()
        {
            QueuePool1 = new QueuePool();
            QueuePool2 = new QueuePool();
            QueuePool3 = new QueuePool();
            QueuePool4 = new QueuePool();
        }
        public int Next(int counterNo, int clientNo)
        {
            switch (counterNo)
            {
                case 1:
                    return QueuePool1.NextClient(clientNo);
                case 2:
                    return QueuePool2.NextClient(clientNo);
                case 3:
                    return QueuePool3.NextClient(clientNo);
                case 4:
                    return QueuePool4.NextClient(clientNo);
            }

            return clientNo;
        }

        public int Prev(int counterNo, int clientNo)
        {
            switch (counterNo)
            {
                case 1:
                    return QueuePool1.PrevClient(clientNo);
                case 2:
                    return QueuePool2.PrevClient(clientNo);
                case 3:
                    return QueuePool3.PrevClient(clientNo);
                case 4:
                    return QueuePool4.PrevClient(clientNo);
            }

            return clientNo;
        }

        #endregion
    }
}
