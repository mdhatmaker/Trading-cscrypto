using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;
using TAC = TicTacTec.TA.Library.Core;

namespace DemoCrypto
{
    public class TAResult
    {
        public int BegIdx { get; private set; }
        public int NBElement { get; private set; }
        public List<double> Values { get; private set; }
        public List<double> Values2 { get; private set; }
        public List<double> Values3 { get; private set; }

        public TAResult(int begIdx, int nbElement, double[] values)
        {
            this.BegIdx = begIdx;
            this.NBElement = nbElement;
            this.Values = values.ToList();
        }

        public TAResult(int begIdx, int nbElement, double[] values, double[] values2)
        {
            this.BegIdx = begIdx;
            this.NBElement = nbElement;
            this.Values = values.ToList();
            this.Values2 = values2.ToList();
        }

        public TAResult(int begIdx, int nbElement, double[] values, double[] values2, double[] values3)
        {
            this.BegIdx = begIdx;
            this.NBElement = nbElement;
            this.Values = values.ToList();
            this.Values2 = values2.ToList();
            this.Values3 = values3.ToList();
        }
    }


    /*var e1 = TAC.CandleSettingType.AllCandleSettings;
    var e2 = TAC.Compatibility.Default; // or MetaStock
    var e3 = TAC.FuncUnstId.Adx;
    var e4 = TAC.MAType.Sma;
    var e5 = TAC.RangeType.HighLow;
    var e6 = TAC.RetCode.AllocErr;*/

    public class TechnicalAnalysis
    {
        private double[] inReal;
        private double[] inClose, inHigh, inLow;
        private int startIdx, endIdx;

        // This expects data with NEWEST data first. If not, set the reverseOrder flag to true.
        public TechnicalAnalysis(List<decimal> prices, bool reverseOrder = false)
        {
            int count = prices.Count;
            this.inReal = new double[count];
            for (int i = 0; i < count; ++i)
            {
                if (reverseOrder)
                    inReal[i] = (double) prices[i];
                else
                    inReal[i] = (double) prices[count - 1 - i];
            }
            this.startIdx = 0;
            this.endIdx = count - 1;
        }

        // This expects data with NEWEST data first. If not, set the reverseOrder flag to true.
        public TechnicalAnalysis(List<decimal> prices, List<decimal> highs, List<decimal> lows, bool reverseOrder = false)
        {
            int count = prices.Count;
            this.inReal = new double[count];
            this.inClose = this.inReal;
            this.inHigh = new double[count];
            this.inLow = new double[count];
            for (int i = 0; i < prices.Count; ++i)
            {
                if (reverseOrder)
                {
                    inReal[i] = (double)prices[i];
                    inHigh[i] = (double)highs[i];
                    inLow[i] = (double)lows[i];
                }
                else
                {
                    inReal[i] = (double)prices[count - 1 - i];
                    inHigh[i] = (double)highs[count - 1 - i];
                    inLow[i] = (double)lows[count - 1 - i];
                }
            }
            this.startIdx = 0;
            this.endIdx = count - 1;
        }

        // https://www.marketvolume.com/quotes/calculatersi.asp
        public TAResult Rsi(int timePeriod = 14, int startIndex = -1, int endIndex = -1)
        {
            var outReal = new double[inReal.Length];
            int begIdx, nbElement;
            //var rc = TAC.Rsi(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            var rc = TAC.Rsi(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
            var values = outReal.Take(nbElement).Reverse().ToArray();
            //var values = outReal.Take(outReal.Length).Reverse().ToArray();
            return new TAResult(begIdx, nbElement, values);
        }

        // RSI Stochastic
        /*public TAResult RsiX(int timePeriod = 14, int startIndex = -1, int endIndex = -1)
        {
            var outReal = new double[inReal.Length];
            int begIdx, nbElement;
            //var rc = TAC.Rsi(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            var rc = TAC.StochRsi(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
            var values = outReal.Take(nbElement).Reverse().ToArray();
            //var values = outReal.Take(outReal.Length).Reverse().ToArray();
            return new TAResult(begIdx, nbElement, values);
        }*/

        // https://www.marketvolume.com/technicalanalysis/roc.asp
        // https://www.marketvolume.com/quotes/calculateroc.asp
        public TAResult Roc(int timePeriod = 14, int startIndex = -1, int endIndex = -1)
        {
            var outReal = new double[inReal.Length];
            int begIdx, nbElement;
            var rc = TAC.Roc(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
            var values = outReal.Take(nbElement).Reverse().ToArray();
            //var values = outReal.Take(outReal.Length).Reverse().ToArray();
            return new TAResult(begIdx, nbElement, values);
        }

        /*public TAResult Roc(int timePeriod = 14, int startIndex = -1, int endIndex = -1)
        {
            var outReal = new double[inReal.Length];
            int begIdx, nbElement;
            var rc = TAC.Roc(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
            var values = outReal.Take(nbElement).Reverse().ToArray();
            return new TAResult(begIdx, nbElement, values);
        }*/

        // https://www.marketvolume.com/quotes/macd.asp
        public TAResult Macd(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9, int startIndex = -1, int endIndex = -1)
        {
            var outMACD = new double[inReal.Length];
            var outMACDSignal = new double[inReal.Length];
            var outMACDHist = new double[inReal.Length];
            int begIdx, nbElement;
            var rc = TAC.Macd(startIdx, endIdx, inReal, fastPeriod, slowPeriod, signalPeriod, out begIdx, out nbElement, outMACD, outMACDSignal, outMACDHist);
            //if (rc == TAC.RetCode.Success)
            var values = outMACDHist.Take(nbElement).Reverse().ToArray();
            return new TAResult(begIdx, nbElement, values);
            //var values = outMACDHist.Take(outMACDHist.Length).Reverse().ToArray();
            //var values2 = outMACD.Take(outMACD.Length).Reverse().ToArray();
            //var values3 = outMACDSignal.Take(outMACDSignal.Length).Reverse().ToArray();
            //return new TAResult(begIdx, nbElement, values, values2, values3);
        }

        public void DirectionalMovementIndex(int startIdx, int endIdx, double[] inReal, int timePeriod = 1)
        {
            var outReal = new double[inReal.Length];
            int begIdx, nbElement;
            var rc = TAC.Rsi(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
        }

        public void Adx(int startIdx, int endIdx, double[] inHigh, double[] inLow, double[] inClose, int timePeriod = 1)
        {
            var outReal = new double[inClose.Length];
            int begIdx, nbElement;
            var rc = TAC.Adx(startIdx, endIdx, inHigh, inLow, inClose, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
        }

        public void Trix(int startIdx, int endIdx, double[] inReal, int timePeriod = 1)
        {
            var outReal = new double[inReal.Length];
            int begIdx, nbElement;
            var rc = TAC.Trix(startIdx, endIdx, inReal, timePeriod, out begIdx, out nbElement, outReal);
            //if (rc == TAC.RetCode.Success)
        }

        // https://www.marketvolume.com/quotes/stochastics.asp
        public TAResult Stochastic(int fastKPeriod = 9, int slowKPeriod = 14, TAC.MAType slowKMAType = TAC.MAType.Sma, int slowDPeriod = 20, TAC.MAType slowDMAType = TAC.MAType.Sma)
        {
            var outSlowK = new double[inClose.Length];
            var outSlowD = new double[inClose.Length];
            int begIdx, nbElement;
            var rc = TAC.Stoch(startIdx, endIdx, inHigh, inLow, inClose, fastKPeriod, slowKPeriod, slowKMAType, slowDPeriod, slowDMAType, out begIdx, out nbElement, outSlowK, outSlowD);
            //if (rc == TAC.RetCode.Success)
            var values = outSlowK.Take(outSlowK.Length).Reverse().ToArray();
            var values2 = outSlowD.Take(outSlowD.Length).Reverse().ToArray();
            return new TAResult(begIdx, nbElement, values, values2);
        }



    } // class

} // namespace
