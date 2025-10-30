using Interface;
using Super_Awesome_Library_Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks; 

namespace DatabaseService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
      InstanceContextMode = InstanceContextMode.Single)]
    internal class DBServices : DatabaseServiceInterface
    {
        private readonly DatabaseClass _db = new DatabaseClass();

        public DBServices()
        {
            Task.Run( () =>
            {
                _db.GenerateDatabase(100000);
            });


            //_db.GenerateDatabase(100000);
        }
        public int GetNumEntries()
        {
            return _db.GetNumRecords();
        }
        public void GetValuesForEntry(int index, out uint acctNo, out uint pin, out int bal, out string fName, out string lName)
        {
            if (index < 0 || index >= _db.GetNumRecords())
            {
                throw new FaultException<IndexOutOfRangeFault>(
              new IndexOutOfRangeFault { Message = $"Index {index} is out of range." },
              new FaultReason("Index Out Of Range. Index ranges from 1 to " + (GetNumEntries()-1)));
            }
            acctNo = _db.GetAcctNoByIndex(index);
            pin = _db.GetPINByIndex(index);
            bal = _db.GetBalanceByIndex(index);
            fName = _db.GetFirstNameByIndex(index);
            lName = _db.GetLastNameByIndex(index);

        }

        public bool GetFirstByLastName(string lastName, out uint acctNo, out uint pin, out int bal, out string fName, out string lName)
        {
            int numEntries = _db.GetNumRecords();
            acctNo = pin = 0;
            bal = 0;
            fName = lName = string.Empty;
            for (int i = 0; i < numEntries; i++)
            {
                uint tempAcctNo = _db.GetAcctNoByIndex(i);
                uint tempPin = _db.GetPINByIndex(i);
                int tempBal = _db.GetBalanceByIndex(i);
                string tempFname = _db.GetFirstNameByIndex(i);
                string tempLname = _db.GetLastNameByIndex(i);
                if (tempLname.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                {
                    acctNo = tempAcctNo;
                    pin = tempPin;
                    bal = tempBal;
                    fName = tempFname;
                    lName = tempLname;
                    return true;
                }
            }
            return false;
        }

    }
}
