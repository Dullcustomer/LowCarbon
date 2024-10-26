using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowCarbon
{
    [Serializable]
    /// <summary>
    /// 一个个体代表一组完整的解，即 Xijk 和 Yijk，同时Xijk 和 Yijk 均采用三维实数编码
    /// </summary>
    public class Individual
    {
        public double[,,] aijk { get; set; }
        public double objective1 { get; set; }
        public double objective2 { get; set; }
        public List<List<List<double>>> SofTreeFelling { get; set; }
        public List<List<double>> Tempijk { get; set; }
        /// <summary>
        /// 为了保证约束2成立，SofTreePlanting = SofFellingPerYear[k]*Tempijk / SumofTempijkForK
        /// </summary>
        public List<List<double>> SofTreePlanting { get; set; }
        public List<Individual> donimatedSet { get; set; }                                                                          
        public int numOfDonimateIndividual { get; set; }
        public int frontNumber { get; set; }
        public double distanceOfCrowd { get; set; }                                                                                           //拥挤度
    }
}
