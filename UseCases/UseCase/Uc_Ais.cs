﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Asc
{
    public class Uc_Ais
    {
        // CP Date Ranges
        public DateTime CpStartDate;
        public DateTime CpEndDate;

        // Internal Objects
        private Dictionary<string, List<string>> SupplyChainSet;
        private SortedDictionary<string, int> SpikeSet;

        // Variables that need to be set from outside
        public string CpaFilePath;
        public string SpaFilePath;
        public string AisFilePath;

        /// <summary>
        /// Use Case' Standard Interface
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            bool result = true;
            result &= this.IntegratedAnalysis();
            return result;
        }

        private bool IntegratedAnalysis()
        {
            bool res = true;

            // 1. Get Change Point Set Y
            // (Supply Chain Names, CP Dates)
            SupplyChainSet = GetSupplyChainSet();

            // 2. Get Spike Set X
            // (SP Dates, SP Counts)
            SpikeSet = GetSpikeSet();

            // 3. Compute tIDW score
            Dictionary<string, double> Dic_SupplyChain_tIDW = new Dictionary<string, double>();
            // For each Supply Chain, 
            foreach (KeyValuePair<string, List<string>> ChangePointSet in SupplyChainSet)
            {
                double CP_tIDW = 0;
                // For each Change Point Yj (CP Date) in Range (start, end),  
                foreach (string CPDate in ChangePointSet.Value)
                {
                    // 2005.01-2019.09 vs. 2019.10-2022.09
                    DateTime CpDate;
                    DateTime.TryParseExact(CPDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None,
                        out CpDate);
                    if (!IsDateInRange(CpDate, CpStartDate, CpEndDate))
                        continue;

                    // For each Spike Xi (SP Date) occurred before Yj for 7 years, 
                    foreach (KeyValuePair<string, int> Spike in SpikeSet)
                    {
                        // Calulate tIDW(Xi, Yj);
                        string SPDate = Spike.Key;
                        int SPIntensity = Spike.Value;

                        // Determine SpDate Range here: 
                        DateTime SpStartDate = CpDate.AddYears(-7);
                        DateTime SpEndDate = CpDate;
                        DateTime.TryParseExact(SPDate, "yyyy-MM", null, System.Globalization.DateTimeStyles.None,
                        out DateTime SpDate);
                        if (IsDateInRange(SpDate, SpStartDate, SpEndDate))
                            CP_tIDW += tIDW(SPDate, CPDate, SPIntensity);
                    }
                }
                Dic_SupplyChain_tIDW.Add(ChangePointSet.Key, CP_tIDW);
            }

            // 4. Print Dic_SupplyChain_tIDW by order
            string FilePath = AisFilePath;
            FileInfo FI = new FileInfo(FilePath);
            FI.Directory.Create();  // If the directory already exists, this method does nothing.
            using (var file = new StreamWriter(FilePath, false))
            {
                int rank = 1;
                foreach (KeyValuePair<string, double> entry
                    in Dic_SupplyChain_tIDW.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, p => p.Value))
                {
                    // Ranking, Supply Chain, tIDW
                    file.WriteLine(string.Format("{0},{1}, {2}", rank, entry.Key, entry.Value));
                    rank++;
                }
            }
            return res;
        }

        public static bool IsDateInRange(DateTime date, DateTime start, DateTime end)
        {
            //// leap year check
            //DateTime leap = date.AddYears(start.Year - date.Year);
            //if (leap < start)
            //    leap = leap.AddYears(1);

            //return start <= leap && leap <= end && date <= end;
            if (start <= date && date <= end)
                return true;
            else
                return false;
        }

        private Dictionary<string, List<string>> GetSupplyChainSet()
        {
            Dictionary<string, List<string>> SupplyChainSet = new Dictionary<string, List<string>>();
            string tmp_key = "";
            string[] tmp_dates = { };

            //string InputFilePath = AppDomain.CurrentDomain.BaseDirectory
            //            + "Result\\Result_Summary\\" + "Result_Auto_Data_Cpa" + ".csv";
            string InputFilePath = CpaFilePath;

            //bool res = true;
            int LineIndex = 0;
            string Line = "";
            char[] Delimiters = new char[] { ',' };
            int LocationIndex = 1;

            try
            {
                using (FileStream fs = File.Open(InputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    //if (Cfs.FooterLinesCount > 0)
                    //    FileTotalLinesCount = File.ReadLines(FilePath).Count();
                    while ((Line = sr.ReadLine()) != null)
                    {
                        string[] Splits = Line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                        // Header
                        if (LocationIndex == 1)
                        {
                            tmp_key = Splits[0];
                            LocationIndex++;
                        }
                        // Dates
                        else if (LocationIndex == 2)
                        {
                            tmp_dates = Splits;
                            LocationIndex++;
                        }
                        // Indexes
                        else if (LocationIndex == 3)
                        {
                            List<string> List_Date = new List<string>();
                            int cnt = 0;
                            foreach (string s in Splits)
                            {
                                if (s != "0")
                                {
                                    List_Date.Add(tmp_dates[cnt]);
                                }
                                cnt++;
                            }
                            SupplyChainSet.Add(tmp_key, List_Date);
                            LocationIndex = 1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("InputFilePath: " + InputFilePath +
                    "\tLineIndex: " + LineIndex.ToString() + "\tLine: " + Line.ToString());
            }
            finally
            {
                // Do nothing
            }
            return SupplyChainSet;
        }

        private SortedDictionary<string, int> GetSpikeSet()
        {
            //throw new NotImplementedException();
            SortedDictionary<string, int> SpikeSet = new SortedDictionary<string, int>();
            //List<string> DateList = new List<string>();
            //List<string> SpikeCntList = new List<string>();
            string[] DateArr = { };
            string[] SpikeCntArr = { };

            //string InputFilePath = AppDomain.CurrentDomain.BaseDirectory
            //            + "Result\\Result_Summary\\" + "Result_Auto_Data_Spa" + ".csv";
            string InputFilePath = SpaFilePath;
            // Result_Spa_Rr.csv
            // Result_Spa_Tg.csv

            //bool res = true;
            int LineIndex = 0;
            string Line = "";
            char[] Delimiters = new char[] { ',' };
            int LocationIndex = 1;

            try
            {
                using (FileStream fs = File.Open(InputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    //if (Cfs.FooterLinesCount > 0)
                    //    FileTotalLinesCount = File.ReadLines(FilePath).Count();
                    while ((Line = sr.ReadLine()) != null)
                    {
                        string[] Splits = Line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                        // Header
                        if (LocationIndex == 1)
                        {
                            DateArr = Splits;
                            LocationIndex++;
                        }
                        // Dates
                        else if (LocationIndex == 2)
                        {
                            SpikeCntArr = Splits;
                            LocationIndex++;
                        }
                    }
                }   // end of reading file

                //List<string> List_Date = new List<string>();
                int cnt = 0;
                foreach (string Date in DateArr)
                {
                    Int32.TryParse(SpikeCntArr[cnt], out int SpikeCnt);
                    SpikeSet.Add(Date, SpikeCnt);
                    cnt++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("InputFilePath: " + InputFilePath +
                    "\tLineIndex: " + LineIndex.ToString() + "\tLine: " + Line.ToString());
            }
            finally
            {
                // Do nothing
            }
            return SpikeSet;
        }

        private double tIDW(string SPDate, string CPDate, int SpikeIntensity)
        {
            //throw new NotImplementedException();
            DateTime SpDate = DateTime.ParseExact(SPDate, "yyyy-MM",
                                       System.Globalization.CultureInfo.InvariantCulture);
            DateTime CpDate = DateTime.Parse(CPDate, System.Globalization.CultureInfo.InvariantCulture);

            // SpDate > CpDate gives a positive value; SpDate < CpDate (targeted) gives a negative value.
            double Time_Difference = SpDate.Subtract(CpDate).Days / (365.2425 / 12);
            double tIDW = 0;
            // 照理講，若是已確定範圍
            // 即 SpStartDate <= SpDate <= SpEndDate
            // 即 SpDate <= CpDate
            // 則，SpDate.Subtract(CpDate) 必定小於 0
            if (Time_Difference >= 0)
            {
                tIDW = 0;
            }
            else
            {
                double p = 1;
                double d = Math.Abs(Time_Difference);
                tIDW = 1 / Math.Pow(d, p) * SpikeIntensity;
            }
            return tIDW;
        }
    }
}
