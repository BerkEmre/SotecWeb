using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sotec_web.Models
{
    public class SepetKalem
    {
        public int sepet_id;
        public int urun_id;
        public decimal miktar;
        public int[] deger_id;

        public int findMaxID(List<SepetKalem> list)
        {
            if (list.Count == 0)
            {
                return 0;
            }
            int maxValue = 0;
            int value;
            foreach (SepetKalem item in list)
            {
                value = item.sepet_id;
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }
            return maxValue;
        }
    }
}