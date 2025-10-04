using System.Collections;
using System.Diagnostics;

namespace FeedReader
{
    public static class Utility
    {
        /// <summary>
        ///   Different machines will need to output to different places 
        ///   due to LoadUnitscal policies
        ///   not a good idea to have the directory as a konstant
        /// </summary>
        /// <returns></returns>


        /// <summary>
        ///   Use this to turn back time
        /// </summary>
        /// <returns></returns>
        public static DateTime CurrentDate()
        {
            //			return new DateTime( 2011, 1, 1 );   // should equate to Week 17 of 2010 season
            //			return new DateTime( 2010, 9, 11 );  // should equate to Week  1 of 2010 season
            return DateTime.Now;
        }

        public static string HostName()
        {
            return Environment.MachineName;
        }


        public static decimal Percent(int quotient, int divisor)
        {
            return 100 * Average(quotient, divisor);
        }

        public static decimal Average(int quotient, int divisor)
        {
            //  need to do decimal other wise INT() will occur
            if (divisor == 0) return 0.0M;
            return (Decimal.Parse(quotient.ToString()) /
                    Decimal.Parse(divisor.ToString()));
        }

        public static string DotNetVersion()
        {
            return IsNet45OrNewer() ? "4.5" : Environment.Version.ToString();
        }

        public static bool IsNet45OrNewer()
        {
            // Class "ReflectionContext" exists in .NET 4.5 .
            return Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        public static void Announce(
            string rpt,
            int indent = 3)
        {
            var theLength = rpt.Length + indent;
            rpt = rpt.PadLeft(theLength, ' ');
#if DEBUG
            Debug.WriteLine(rpt);
#endif
        }

        public static bool JustDoIt()
        {
            return true;
        }

        public static string Dtos(
            DateTime theDate)
        {
            return string.Format("{0}{1:00}{2:00}", theDate.Year, theDate.Month, theDate.Day);
        }

        public static void WriteLog(string message)
        {
            Console.WriteLine(
                string.Format("{0}:{1}",
                DateTime.Now.ToString("dd MMM yyyy - hh:mm:ss  "), message));
        }


        public static string CategoryFor(string posdesc)
        {
            var catOut = "?";

            posdesc = posdesc.Trim();

            if (String.IsNullOrEmpty(posdesc)) return "<blank>";

            const string qb = "QB,P,";
            const string rb = "RB,HB,FB,";
            const string wr = "WR,TE,FL,";
            const string pk = "PK,K,";
            const string dl = "DE,DT,LB,NT,ILB,OLB";
            const string db = "DB,CB,FS,SS";
            const string ol = "OT,G,T,C,OG,";

            if (qb.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "1";
            else if (rb.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "2";
            else if (wr.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "3";
            else if (pk.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "4";
            else if (dl.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "5";
            else if (db.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "6";
            else if (ol.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
                catOut = "7";
            return catOut;
        }

        public static string CategoryOut(string strCat)
        {
            string catOut;
            switch (strCat)
            {
                case "1":
                    catOut = "Quarterback";
                    break;

                case "2":
                    catOut = "Runningback";
                    break;

                case "3":
                    catOut = "Receiver";
                    break;

                case "4":
                    catOut = "Kicker";
                    break;

                case "5":
                    catOut = "Linebacker";
                    break;

                case "6":
                    catOut = "Secondary";
                    break;

                case "7":
                    catOut = "Offensive Line";
                    break;

                default:
                    catOut = "???";
                    break;
            }
            return catOut;
        }


        public static string SeedOut(int weekSeed)
        {
            int baseyr = (weekSeed / 22);
            int wk = weekSeed - (baseyr * 22);
            return String.Format("{0}:{1:0#}", baseyr + 1980, wk);
        }

        public static string SeasonSeed(int weekSeed)
        {
            int baseyr = (weekSeed / 22);
            return String.Format("{0}", baseyr + 1980);
        }

        public static void CopyFile(
            string fromFile,
            string targetFile)
        {
            EnsureDirectory(targetFile);

            // To copy a file to another location and
            // overwrite the destination file if it already exists.
            File.Copy(fromFile, targetFile, overwrite: true);
        }


        public static void EnsureDirectory(
            string destFile)
        {
            var directoryInfo = new FileInfo(destFile).Directory;
            if (directoryInfo != null)
            {
                if (!Directory.Exists(
                    directoryInfo.ToString()))
                {
                    string currentWorkingDirectory =
                       Path.GetDirectoryName(
                          System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    Directory.CreateDirectory(
                        directoryInfo.ToString());
                }
            }
        }

        public static string UniversalDate(
            DateTime date)
        {
            var longDate = $"{date.Date:u}";
            var universalDate = longDate.Substring(0, 10);
            //            var year = universalDate.Substring(0, 5);
            //            var month = universalDate.Substring(5, 2);
            //            var day = universalDate.Substring(8, 2);
            //            var usUniversalDate = $"{year}-{day}-{month}";
            return universalDate;
        }

        public static void PrintIndexAndKeysAndValues(Hashtable myList)
        {
            var myEnumerator = myList.GetEnumerator();
            var i = 0;
            Announce("\t-INDEX-\t-KEY-\t-VALUE-");
            while (myEnumerator.MoveNext())
                Announce(String.Format("\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, myEnumerator.Value));
        }


        public static decimal Clip(int w, int l, int t)
        {
            var clip = 0.0M;
            decimal tot = (w + l + t);
            if (tot > 0)
                clip = ((Convert.ToDecimal(w) * 2.0M) + Convert.ToDecimal(t)) / (tot * 2.0M);
            return clip;
        }

        public static string GameKey(string season, string week, string gameCode)
        {
            return string.Format("{0}:{1}-{2}", season, week, gameCode);
        }

        public static bool IsSunday(DateTime dGame)
        {
            return dGame.DayOfWeek == DayOfWeek.Sunday;
        }

        private static DateTime NextSunday(DateTime dGame)
        {
            var testDate = dGame;
            while (!IsSunday(testDate))
                testDate = testDate.AddDays(1);
            return testDate;
        }


        public static TimeSpan StopTheWatch(
            Stopwatch stopwatch, 
            string message)
        {
            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            var elapsedTime = FormatElapsedTime(ts);
            Announce($"{message} took {elapsedTime}");
            return ts;
        }

        public static string FormatElapsedTime(TimeSpan ts)
        {
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                            ts.Hours, ts.Minutes, ts.Seconds,
                                            ts.Milliseconds / 10);
            return elapsedTime;
        }

        public static bool Contains(string subString, string mainString)
        {
            var nSpot = mainString.IndexOf(subString.Trim());
            return nSpot != -1;
        }


        public static bool IsValidCbsCode(
            string code)
        {
            var validCodes =
            new string[] {
                "BAL",
                "BUF",
                "CIN",
                "CLE",
                "DEN",
                "HOU",
                "IND",
                "JAC",
                "KC",
                "LAC",
                "LV",
                "MIA",
                "NE",
                "NYJ",
                "PIT",
                "TEN",
                "ARI",
                "ATL",
                "CAR",
                "CHI",
                "DAL",
                "DET",
                "GB",
                "LAR",
                "MIN",
                "NO",
                "NYG",
                "PHI",
                "SEA",
                "SF",
                "TB",
                "WAS"
            };
            return validCodes.Contains(code);
        }

        public static string TeamMapCbsToTfl(
            string teamCode)
        {
            var tflCode = teamCode;
            if (teamCode == "ARI")
                tflCode = "AC";
            if (teamCode == "BAL")
                tflCode = "BR";
            if (teamCode == "ATL")
                tflCode = "AF";
            if (teamCode == "BUF")
                tflCode = "BB";
            if (teamCode == "CAR")
                tflCode = "CP";
            if (teamCode == "CIN")
                tflCode = "CI";
            if (teamCode == "CHI")
                tflCode = "CH";
            if (teamCode == "CLE")
                tflCode = "CL";
            if (teamCode == "DAL")
                tflCode = "DC";
            if (teamCode == "DEN")
                tflCode = "DB";
            if (teamCode == "DET")
                tflCode = "DL";
            if (teamCode == "HOU")
                tflCode = "HT";
            if (teamCode == "IND")
                tflCode = "IC";
            if (teamCode == "LAR")
                tflCode = "LR";
            if (teamCode == "JAC")
                tflCode = "JJ";
            if (teamCode == "MIN")
                tflCode = "MV";
            if (teamCode == "LV")
                tflCode = "OR";
            if (teamCode == "NYG")
                tflCode = "NG";
            if (teamCode == "LAC")
                tflCode = "LC";
            if (teamCode == "PHI")
                tflCode = "PE";
            if (teamCode == "MIA")
                tflCode = "MD";
            if (teamCode == "NYJ")
                tflCode = "NJ";
            if (teamCode == "PIT")
                tflCode = "PS";
            if (teamCode == "WAS")
                tflCode = "WR";
            if (teamCode == "TEN")
                tflCode = "TT";
            if (teamCode == "SEA")
                tflCode = "SS";
            return tflCode;
        }

        public static bool IsOdd(int number) => number % 2 != 0;

        public static bool IsEven(int number) => !IsOdd(number);

        public static string TeamToCode(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
                return teamName;
            if (teamName.Equals("San Francisco 49ers"))
                return "SF";
            if (teamName.Equals("Los Angeles Rams"))
                return "LR";
            if (teamName.Equals("Los Angeles Chargers"))
                return "LC";
            if (teamName.Equals("Washington Commanders"))
                return "WR";
            if (teamName.Equals("New York Giants"))
                return "NG";
            if (teamName.Equals("New York Jets"))
                return "NJ";
            if (teamName.Equals("Kansas City Chiefs"))
                return "KC";
            if (teamName.Equals("Tennessee Titans"))
                return "TT";
            if (teamName.Equals("New England Patriots"))
                return "NE";
            if (teamName.Equals("Arizona Cardinals"))
                return "AC";
            if (teamName.Equals("Atlanta Falcons"))
                return "AF";
            if (teamName.Equals("Baltimore Ravens"))
                return "BR";
            if (teamName.Equals("Buffalo Bills"))
                return "BB";
            if (teamName.Equals("Carolina Panthers"))
                return "CP";
            if (teamName.Equals("Cincinnati Bengals"))
                return "CI";
            if (teamName.Equals("Chicago Bears"))
                return "CH";
            if (teamName.Equals("Cleveland Browns"))
                return "CL";
            if (teamName.Equals("Dallas Cowboys"))
                return "DC";
            if (teamName.Equals("Denver Broncos"))
                return "DB";
            if (teamName.Equals("Detroit Lions"))
                return "DL";
            if (teamName.Equals("Houston Texans"))
                return "HT";
            if (teamName.Equals("Indianapolis Colts"))
                return "IC";
            if (teamName.Equals("Jacksonville Jaguars"))
                return "JJ";
            if (teamName.Equals("Miami Dolphins"))
                return "MD";
            if (teamName.Equals("Minnesota Vikings"))
                return "MV";
            if (teamName.Equals("Las Vegas Raiders"))
                return "OR";
            if (teamName.Equals("Philadelphia Eagles"))
                return "PE";
            if (teamName.Equals("Pittsburgh Steelers"))
                return "PS";
            if (teamName.Equals("Seattle Seahawks"))
                return "SS";
            if (teamName.Equals("Tampa Bay Buccaneers"))
                return "TB";
            if (teamName.Equals("New Orleans Saints"))
                return "NO";
            if (teamName.Equals("Green Bay Packers"))
                return "GB";
            return teamName;

        }
    }
}