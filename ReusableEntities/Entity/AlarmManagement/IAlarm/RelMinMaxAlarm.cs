﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Asc
{   
    public class RelMinMaxAlarm : IJudge
    {
        private GenericValue SV;
        private GenericValue AL;
        private GenericValue AH;

        public RelMinMaxAlarm(GenericValue SV, GenericValue AL, GenericValue AH)
        {
            this.SV = SV;
            this.AL = AL;
            this.AH = AH;
        }

        public bool Judge(GenericValue PV)
        {
            throw new NotImplementedException();
        }
    }
}
