﻿
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Asc
{
    class TestOutlierTrimmingAnalyzer
    {
        [Test]
        public void UC01_RemainingErrorData()
        {
            double[] RemainingErrorComponent = new double[] { 
                0.06, 0.04, -0.68, 1.14, 0.12, 0.12, -0.09, -0.31, -0.40, -0.29, 0.40, 0.38, -0.66, 0.09, 0.69, -0.39, -0.40, -0.10, 0.37, -0.52, -0.56, 0.82, 0.32, -0.49, -0.12, 0.43, 0.73, -0.68, -0.21, 0.88, 0.14, -0.68, 0.62, -0.73, 0.51, -0.46, -0.21, -0.52, 0.74, 0.28, -0.09, -0.20, 0.50, -0.59, -0.32, -0.60, 0.18, 0.61, 0.30, -0.16, 0.14, -0.81, 0.82, 0.26, -1.59, -0.48, 0.72, -0.54, 1.22, 0.23, -0.66, 0.37, 0.77, -0.41, -0.98, -0.93, 2.06, -0.34, -0.70, 1.13, 0.74, -0.39, -0.82, -0.56, -0.49, 0.53, 1.44, -0.32, 0.12, 1.18, 0.08, -0.61, -0.42, -0.39, -0.52, -0.69, 0.48, 0.89, 0.93, -0.49, -1.38, 0.23, 0.26, -0.02, 1.00, -0.52, -0.50, 0.13, 0.20, 0.16, 0.52, -0.60, 0.48, -0.54, -0.63, 0.30, 0.34, 0.48, 0.60, -0.39, -0.31, -0.33, 0.22, 0.16, -1.38, 0.47, 0.73, -0.30, 0.20, -0.63, 0.13, -0.26, -0.09, 0.17, 0.04, 0.11, 0.42, -0.13, -0.60, 0.00, 0.09, 0.40, -0.37, -0.18, 0.42, -0.27, 0.22, -0.69, 0.60, 0.76, -0.04, -0.66, 0.26, 0.39, 0.26, -0.68, 0.29, -0.22, -0.62, -0.18, -0.31, 0.01, 0.43, -0.02, 0.60, -0.28, -0.07, 0.56, -0.38, -0.28, 0.27, -0.36, -0.22, -0.17, -0.23, 0.09, 0.27, -0.34, 0.28, -0.18, 0.58, 0.68, -0.12, 0.12, 0.10, -0.60, 0.20, -0.34, 0.13, 0.16, -0.34, -0.09, 0.88, -0.70, -0.26, 0.30, 0.03, -0.36, 0.04, 0.56, -0.31, 0.09, -0.18, 0.62, -0.47, -0.46, 0.23, -0.42, 0.60, -0.04, -0.89, -0.34, 0.77, 0.66, -0.68, -0.26, -0.63, -0.41, -0.90, 0.13, 0.07, -0.66, 1.26, 0.17, -0.73, -1.11, -0.81, 2.11, 2.87, -0.26, -0.89, 2.59, 3.40, 1.19, 2.20, -2.90, -3.27, -1.96, 0.50, -0.98, -2.06, -0.99, 2.68, 1.23, -1.74, 0.61, -0.90, -1.00, 0.81, 1.09, 1.61, 0.10, -1.29, 0.10, -0.39, -2.54, -1.91, 0.24, 0.54, 0.80, -0.22, 0.28, 1.11, 2.66, -1.23, 0.77, 0.12, 0.36, -0.24, 1.13, 0.63, -1.54, 0.91, -1.72, -1.39, 1.06, 1.53, 1.72, -1.61, -3.83, 1.11, 0.59, 1.21, 0.02, -0.09, -0.42, 0.19, 0.09, -0.03, 0.37, 0.47, 0.71, -0.57, -1.29, -0.87, 0.86, 0.64, 0.24, -1.89, -0.58, 1.70, 0.42, 0.88, 0.67, 0.34, -0.26, -1.44, 0.33, 1.10, 0.61, -0.78, -0.17, -0.94, -0.67, 0.24, 0.57, 1.18, -1.43, -0.83, 1.37, -0.30, 0.11, -1.32, -0.54, -0.17, -0.07, -0.31, 1.23, -0.77, 0.43, 0.60, 0.13, -0.11, -1.16, 0.87, 1.18, -0.48, -1.41, 0.20, -0.18, 1.03, -0.14, 0.43, 0.01, 0.69, -1.06, -0.68, 0.06, 0.68, 0.12, 0.76, -0.02, -0.93, 0.76, -0.04, -0.12, -0.58, -0.54, 0.17, 0.62, -0.44, -0.26, -0.09, 0.62, 0.50, -0.37, -1.37, -0.84, 0.38, 1.02, 2.28, 0.87, -0.73, -1.18, -1.49, 0.90 
            };
            double[] Outliers = new double[] { };
            double Lower;
            double Upper;
            double Mad;

            OutlierTrimmingAnalyzer Oti = new OutlierTrimmingAnalyzer();
            Oti.SetOriginalSeriesAndDoAnalysis(RemainingErrorComponent);
            Oti.GetMad(out Mad);
            Oti.GetConfidenceIntervals(out Lower, out Upper);
            Oti.GetOutliers(out Outliers);
        }
    }
}
