﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asc
{
    public class OutlierTrimmingAnalyzer
    {
        private double[] source;
        private bool isSourceSet = false;
        private double ConfidenceInterval_Lower;
        private double ConfidenceInterval_Upper;
        private double Med;
        private double MAD;
        public double MAD_Multiplier = 3;

        public bool SetOriginalSeriesAndDoAnalysis(double[] remainingErrorComponent)
        {
            source = (double[])remainingErrorComponent.Clone();
            isSourceSet = true;
            bool res = OutlierIndicatorProcedure();
            return res;
        }

        private bool OutlierIndicatorProcedure()
        {
            if (isSourceSet == true)
            {
                Array.Sort(source);

                Med = source.Length % 2 == 0
                  ? (source[source.Length / 2 - 1] + source[source.Length / 2]) / 2.0
                  : source[source.Length / 2];

                double[] d = source
                  .Select(x => Math.Abs(x - Med))
                  .OrderBy(x => x)
                  .ToArray();

                MAD = 1.4826 * (d.Length % 2 == 0
                  ? (d[d.Length / 2 - 1] + d[d.Length / 2]) / 2.0
                  : d[d.Length / 2]);

                ConfidenceInterval_Lower = Med - MAD_Multiplier * MAD;
                ConfidenceInterval_Upper = Med + MAD_Multiplier * MAD;

                return true;
            }
            else
            {
                // throw new No Source Exception
                return false;
            }
        }

        public void GetMad(out double Mad)
        {
            Mad = MAD;
        }

        public void GetMed(out double M)
        {
            M = Med;
        }

        public void GetConfidenceIntervals(out double lower, out double upper)
        {
            lower = ConfidenceInterval_Lower;
            upper = ConfidenceInterval_Upper;
        }

        public void GetOutliers(out double[] outliers)
        {
            List<double> outliersList = new List<double>();
            foreach (double s in source)
            {
                if (s <= ConfidenceInterval_Lower || ConfidenceInterval_Upper <= s)
                    outliersList.Add(s);
            }
            outliers = outliersList.ToArray();
        }
    }
}
