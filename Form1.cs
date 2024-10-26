using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LowCarbon
{
    public partial class Form1 : Form
    {
        int numofTreeType = 5;
        int numofTreeAges =300;
        int numofMaxFellingYear =100;
        int populationSize =10;
        double alpha = 0.25;
        int maxIter = 3;
        int S = 1000;
        List<int> IndexOfYear = new List<int>();


        public DataTable tableOfCdij = new DataTable();
        public DataTable tableOfaij1 = new DataTable();
        public DataTable tableOfCdpij = new DataTable();
        public DataTable tableOfCdtij = new DataTable();
        public DataTable tableOfCijk = new DataTable();

        Random random = new Random(GetRandomSeed());
        List<Individual> population = new List<Individual>();
        public Form1()
        {
            InitializeComponent();
            readData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //初始化种群
            for (int num = 0; num < populationSize; num++) 
            {
                Individual individual = initialIndividualCode();
                //计算适应值
                calculateFitness(individual);
                //Console.WriteLine(individual.objective1);
                population.Add(individual);
            }
            population = population.OrderByDescending(x=>x.objective1).ToList();
            //for (int m = 0; m < maxIter; m++)
            //{
            //    Console.WriteLine($"------------------第{m+1}次迭代---------------");
            //    population = DeepCopyByBin<List<Individual>>(EvolveByGA(population));
            //}
            Console.WriteLine($"最优解为：{population[0].objective1}");

            Console.WriteLine("-------------------------");
            //double[,] xik = new double[numofTreeType,numofMaxFellingYear];
            //for (int i = 0; i < numofTreeType; i++) 
            //{
            //    for (int k=0; k < numofMaxFellingYear; k++) 
            //    {
            //        for (int j = 0; j <numofTreeAges; j++) 
            //        {
            //            xik[i, k] = xik[i, k] + population[0].SofTreeFelling[i][j][k];
            //        }
            //        Console.WriteLine(xik[i, k]);
            //    }
            //}

            //Console.WriteLine("-------------------------");
            //double[,] yik = new double[numofTreeType, numofMaxFellingYear];
            //for (int i = 0; i < numofTreeType; i++)
            //{
            //    for (int k = 0; k < numofMaxFellingYear; k++)
            //    {
            //        yik[i, k] = population[0].SofTreePlanting[i][k];
            //        Console.WriteLine(yik[i, k]);
            //    }

            //}

            double sum2 = 0;
            double sum3 = 0;
            double[] sum2List = new double[numofMaxFellingYear];
            double[] sum3List = new double[numofMaxFellingYear];
            for (int k = 0; k < numofMaxFellingYear; k++)
            {
                for (int i = 0; i < numofTreeType; i++)
                {
                    for (int j = 0; j < numofTreeAges; j++)
                    {
                        double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
                        double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
                        sum2 += cds * population[0].aijk[i, j, k];
                        sum3 += cdp * population[0].SofTreeFelling[i][j][k];
                    }
                }
                sum2List[k] = sum2;
                sum3List[k] = sum3;
            }
            for (int k = 0; k < numofMaxFellingYear; k++)
            {
                Console.WriteLine(sum2List[k]);
            }
            Console.WriteLine("-------------------------");
            for (int k = 0; k < numofMaxFellingYear; k++)
            {
                Console.WriteLine(sum3List[k]);
            }

            //double sum1 = 0;
            //sum2 = 0;
            //sum3 = 0;

            //for (int i = 0; i < numofTreeType; i++)
            //{
            //    for (int j = 0; j < numofTreeAges; j++)
            //    {
            //        double cdt = Convert.ToDouble(tableOfCdtij.Rows[j][i + 1]);
            //        sum1 += cdt * population[0].aijk[i, j, numofMaxFellingYear - 1];

            //        for (int k = 0; k < numofMaxFellingYear; k++)
            //        {
            //            double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
            //            double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
            //            sum2 += cds * population[0].aijk[i, j, k];
            //            sum3 += cdp * population[0].SofTreeFelling[i][j][k];
            //        }
            //    }
            //}
            //Console.WriteLine("-------------------------");
            //Console.WriteLine(sum2);
            //Console.WriteLine("-------------------------");
            //Console.WriteLine(sum3);
            //Console.WriteLine("-------------------------");

        }
        //读取数据
        public void readData() 
        {
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\模型参数2.0.xlsx;Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
            string strConn1 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\数据cijk.xlsx;Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
            OleDbConnection conn = new OleDbConnection(strConn);
            OleDbDataAdapter myCommand1 = new OleDbDataAdapter("SELECT * FROM [Cd$]", strConn);
            OleDbDataAdapter myCommand4 = new OleDbDataAdapter("SELECT * FROM [Cdt$]", strConn);
            OleDbDataAdapter myCommand2 = new OleDbDataAdapter("SELECT * FROM [aij1$]", strConn);
            OleDbDataAdapter myCommand3 = new OleDbDataAdapter("SELECT * FROM [Cdp$]", strConn);
            OleDbConnection conn1 = new OleDbConnection(strConn1);
            OleDbDataAdapter myCommand5 = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", strConn1);
            try
            {
                myCommand1.Fill(tableOfCdij);
                myCommand2.Fill(tableOfaij1);
                myCommand3.Fill(tableOfCdpij);
                myCommand4.Fill(tableOfCdtij);
                myCommand5.Fill(tableOfCijk);
            }
            catch (Exception ex)
            {
                throw new Exception("表一Excel文件的工作表的名字不正确," + ex.Message);
            }
        }
        public static T DeepCopyByBin<T>(T t)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //序列化成流
                bf.Serialize(ms, t);
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }
        //产生随机种子
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        //利用递推关系生成aijk
        public double[,,] calculateAijk(Individual individual, double[,,] aijk, int k) 
        {
            for (int i = 0; i < numofTreeType; i++)
            {
                for (int j = 0; j < numofTreeAges-1; j++)
                {
                    if (j==0)
                    {
                        aijk[i, 1, k+1] = aijk[i, 0, k] - individual.SofTreeFelling[i][0][k] + individual.SofTreePlanting[i][k];
                    }
                    else 
                    {
                        aijk[i, j + 1, k+1] = aijk[i, j, k] - individual.SofTreeFelling[i][j][k];
                    }
                }
            }
            return aijk;
        }
        //初始化编码
        public Individual produceIndividualCodeForK(Individual individual, double[,,] aijk, int k) 
        {
            double sum = 0;

            for (int i = 0; i < numofTreeType; i++)
            {
                for (int j = 0; j < numofTreeAges; j++)
                {
                    List<double> tempList = new List<double>();
                    //根据约束3生成 SofTreeFelling，即Xij1
                    double value = random.NextDouble() * aijk[i, j, k]*0.8;
                    sum += value;
                    tempList.Add(value);
                    individual.SofTreeFelling[i][j].AddRange(tempList);
                }
            }

            double sumofTemp = 0;
            for (int i = 0; i < numofTreeType; i++)
            {
                List<double> tempForJ = new List<double>();
                double value = random.NextDouble();
                sumofTemp += value;
                tempForJ.Add(value);
                individual.Tempijk[i].AddRange(tempForJ);
            }

            for (int i = 0; i < numofTreeType; i++)
            {
                List<double> SofPlantingForIJK = new List<double>();
                double value = (individual.Tempijk[i][k] / sumofTemp) * sum;
                SofPlantingForIJK.Add(value);
                individual.SofTreePlanting[i].AddRange(SofPlantingForIJK);
            }

            return individual;
        }
        public Individual initialIndividualCode() 
        {
            Individual currentIndividual = new Individual();
            //while (true) 
            {
                double[,,] aijk = new double[numofTreeType, numofTreeAges, numofMaxFellingYear];
                //随机生成初始种群
                Individual individual1 = new Individual();
                individual1.SofTreeFelling = new List<List<List<double>>>();

                double sum = 0;

                for (int i = 0; i < numofTreeType; i++)
                {
                    List<List<double>> SofFellingForIJ = new List<List<double>>();
                    for (int j = 0; j < numofTreeAges; j++)
                    {
                        List<double> SofFellingForIJK = new List<double>();
                        //根据约束3生成 SofTreeFelling，即Xij1
                        aijk[i, j, 0] = Convert.ToDouble(tableOfaij1.Rows[j][i + 1]);
                        double value = random.NextDouble() * aijk[i, j, 0] * 0.08;
                        sum += value;
                        SofFellingForIJK.Add(value);
                        SofFellingForIJ.Add(SofFellingForIJK);
                    }
                    individual1.SofTreeFelling.Add(SofFellingForIJ);
                }

                individual1.Tempijk = new List<List<double>>();
                double sumofTemp = 0;
                for (int i = 0; i < numofTreeType; i++)
                {
                    List<double> tempForIJ = new List<double>();
                    double value = random.NextDouble();
                    sumofTemp += value;
                    tempForIJ.Add(value);
                    individual1.Tempijk.Add(tempForIJ);
                }

                individual1.SofTreePlanting = new List<List<double>>();
                for (int i = 0; i < numofTreeType; i++)
                {
                    List<double> SofPlantingForIJ = new List<double>();
                    double value = (individual1.Tempijk[i][0] / sumofTemp) * sum;
                    SofPlantingForIJ.Add(value);
                    individual1.SofTreePlanting.Add(SofPlantingForIJ);
                }


                Individual individual = new Individual();

                for (int k = 0; k < numofMaxFellingYear - 1; k++)
                {
                    if (k == 0)
                    {
                        for (int i = 0; i < numofTreeType; i++)
                        {
                            for (int j = 0; j < numofTreeAges - 1; j++)
                            {
                                if (j == 0)
                                {
                                    aijk[i, 1, k + 1] = aijk[i, 0, k] - individual1.SofTreeFelling[i][0][k] + individual1.SofTreePlanting[i][k];
                                    if (aijk[i, 1, k + 1]<0)
                                    {
                                        individual1.SofTreeFelling[i][0][k] = aijk[i, 0, k] * 0.5;
                                        
                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual1.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual1.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual1.SofTreePlanting[i1][k] = (individual1.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, 1, k + 1] = aijk[i, 0, k] - individual1.SofTreeFelling[i][0][k] + individual1.SofTreePlanting[i][k];
                                }
                                else
                                {
                                    aijk[i, j + 1, k + 1] = aijk[i, j, k] - individual1.SofTreeFelling[i][j][k];
                                    if (aijk[i, j + 1, k + 1] < 0)
                                    {
                                        individual1.SofTreeFelling[i][j][k] = aijk[i, j, k] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual1.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual1.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual1.SofTreePlanting[i1][k] = (individual1.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, j + 1, k + 1] = aijk[i, j, k] - individual1.SofTreeFelling[i][j][k];
                                }
                            }
                        }
                        individual = DeepCopyByBin < Individual > (produceIndividualCodeForK(individual1, aijk, k));
                    }
                    //更新aijk
                    else
                    {
                        for (int i = 0; i < numofTreeType; i++)
                        {
                            for (int j = 0; j < numofTreeAges - 1; j++)
                            {
                                if (j == 0)
                                {
                                    aijk[i, 1, k + 1] = aijk[i, 0, k] - individual.SofTreeFelling[i][0][k] + individual.SofTreePlanting[i][k];
                                    if (aijk[i, 1, k + 1] <= 0)
                                    {
                                        individual.SofTreeFelling[i][0][k] = aijk[i, 0, k] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual.SofTreePlanting[i1][k] = (individual.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, 1, k + 1] = aijk[i, 0, k] - individual.SofTreeFelling[i][0][k] + individual.SofTreePlanting[i][k];
                                }
                                else
                                {
                                    aijk[i, j + 1, k + 1] = aijk[i, j, k] - individual.SofTreeFelling[i][j][k];
                                    if (aijk[i, j + 1, k + 1] < 0)
                                    {
                                        individual.SofTreeFelling[i][j][k] = aijk[i, j, k] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual.SofTreePlanting[i1][k] = (individual.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, j + 1, k + 1] = aijk[i, j, k] - individual.SofTreeFelling[i][j][k];
                                }
                            }
                        }
                        individual = DeepCopyByBin<Individual> (produceIndividualCodeForK(individual, aijk, k));
                    }
                }
                individual.aijk = aijk;
                currentIndividual = DeepCopyByBin<Individual>(individual);
            }
            return currentIndividual;
        }
        //计算适应值
        public void calculateFitness(Individual individual) 
        {
            //for (int num = 0; num < population.Count; num++) 
            {
                individual.objective1 = 0;
                individual.objective2 = 0;

                double sum1 = 0;
                double sum2 = 0;
                double sum3 = 0;
                double sum4 = 0;

                for (int i = 0; i < numofTreeType; i++)
                {
                    for (int j = 0; j < numofTreeAges; j++)
                    {
                        double cdt = Convert.ToDouble(tableOfCdtij.Rows[j][i + 1]);
                        sum1 += cdt * individual.aijk[i, j, numofMaxFellingYear-1];

                        for (int k = 0; k < numofMaxFellingYear; k++) 
                        {
                            double c = Convert.ToDouble(tableOfCijk.Rows[k][i + 1]);
                            double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
                            double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
                            sum2 += cds* individual.aijk[i, j, k];
                            sum3 += cdp * individual.SofTreeFelling[i][j][k];
                            sum4 += c * individual.SofTreeFelling[i][j][k];
                        }
                    }
                }
                individual.objective1 = sum1 + sum2 + sum3;
                individual.objective2 = sum4;
            }
        }
        // 中间重组
        public Individual crossover(Individual individual1, Individual individual2) 
        {
            Individual offspring = DeepCopyByBin<Individual>(individual1);
            for (int i = 0; i < numofTreeType; i++)
            {
                List<List<double>> SofTreeFellingForJK = new List<List<double>>();
                for (int j = 0; j < numofTreeAges; j++)
                {
                    List<double> SofTreeFellingForK = new List<double>();
                    for (int k = 0; k < numofMaxFellingYear; k++)
                    {
                        offspring.SofTreeFelling[i][j][k] = individual1.SofTreeFelling[i][j][k] + alpha * (individual2.SofTreeFelling[i][j][k] - individual1.SofTreeFelling[i][j][k]);
                    }
                }
            }
            offspring = produceFeasibleSolution(offspring);
            calculateFitness(offspring);
            return offspring;
        }
        //基本位变异
        public List<Individual> mutation(List<Individual> population) 
        {

            for (int num = 0; num < population.Count; num++) 
            { 
                int randI = random.Next(0, numofTreeType);
                int randJ = random.Next(0, numofTreeAges);
                int randK = random.Next(0, numofMaxFellingYear);
                population[num].SofTreeFelling[randI][randJ][randK] = random.NextDouble() * population[num].aijk[randI, randJ, randK];
                population[num] = produceFeasibleSolution(population[num]);
                calculateFitness(population[num]);
            }
            return population;
        }
        //解的可行化
        public Individual produceFeasibleSolution(Individual offspring) 
        {
            //解的可行化
            List<double> sum = new List<double>();
            List<double> sum1 = new List<double>();
            for (int k = 0; k < numofMaxFellingYear; k++)
            {
                double value = 0;
                double sumofTemp = 0;
                for (int i = 0; i < numofTreeType; i++)
                {
                    sumofTemp += offspring.Tempijk[i][k];
                    for (int j = 0; j < numofTreeAges; j++)
                    {
                        value += offspring.SofTreeFelling[i][j][k];
                    }
                }
                sum.Add(value);
                sum1.Add(sumofTemp);
            }

            for (int k = 0; k < numofMaxFellingYear; k++)
            {
                for (int i = 0; i < numofTreeType; i++)
                {
                    offspring.SofTreePlanting[i][k] = (offspring.Tempijk[i][k] / sum1[k]) * sum[k];
                }
            }

            for (int i = 0; i < numofTreeType; i++)
            {
                for (int j = 0; j < numofTreeAges; j++)
                {
                    for (int k = 0; k < numofMaxFellingYear; k++)
                    {
                        if (offspring.aijk[i,j,k]< offspring.SofTreeFelling[i][j][k])
                        {
                            //解的可行化
                            offspring.SofTreeFelling[i][j][k] = offspring.aijk[i, j, k] * 0.5;

                            double value = 0;
                            double sumofTemp1 = 0;

                            for (int i1 = 0; i1 < numofTreeType; i1++)
                            {
                                sumofTemp1 += offspring.Tempijk[i1][k];
                                for (int j1 = 0; j1 < numofTreeAges; j1++)
                                {
                                    value += offspring.SofTreeFelling[i1][j1][k];
                                }
                            }
                            for (int i1 = 0; i1 < numofTreeType; i1++)
                            {
                                offspring.SofTreePlanting[i1][k] = (offspring.Tempijk[i1][k] / sumofTemp1) * value;
                            }
                        }
                    }
                }
            }
            return offspring;
        }
        //通过GA进行进化
        public List<Individual> EvolveByGA(List<Individual> population) 
        {
            List<Individual> offsoringPopulation = new List<Individual>();
            List<Individual> clonePopulation = DeepCopyByBin<List<Individual>>(population);
            int a = 0;
            //通过锦标赛选择个体
            while (clonePopulation.Count > 0)
            {
                Individual parent1 = new Individual();
                Individual parent2 = new Individual();
            //---从留下来的精英中无放回地挑选父母
            laberSwap:
                int number1 = random.Next(0, clonePopulation.Count);
                int number2 = random.Next(0, clonePopulation.Count);
                //---从父代种群和当前子代种群中选择个体
                parent1 = DeepCopyByBin<Individual>(clonePopulation[number1]);
                parent2 = DeepCopyByBin<Individual>(clonePopulation[number2]);
                if (number1 == number2)
                {
                    goto laberSwap;
                }
                //适合实数编码的交叉方式----中间重组 X_子代 = X_父代1 + alpha (X_父代2 - X_父代1)
                Individual offspring1 = crossover(parent1, parent2);
                Individual offspring2 = crossover(parent2, parent1);
                offsoringPopulation.Add(offspring1);
                offsoringPopulation.Add(offspring2);
                if (offsoringPopulation.Count == populationSize)
                {
                    break;
                }
            }
            //offsoringPopulation = mutation(offsoringPopulation);
            offsoringPopulation = offsoringPopulation.OrderByDescending(x => x.objective1).ToList();
            offsoringPopulation.AddRange(clonePopulation);
            offsoringPopulation = offsoringPopulation.OrderByDescending(x => x.objective1).Take(populationSize).ToList();

            //for (int n = 0; n < offsoringPopulation.Count; n++) 
            //{
            //    Console.WriteLine(offsoringPopulation[n].objective1);
            //}
            Console.WriteLine($"当前的最大值为：{offsoringPopulation[0].objective1}");
            return offsoringPopulation;
        }
        //修正
        public Individual correction(Individual individual, int i, int j, int k1) 
        {
            individual.SofTreeFelling[i][j][k1-1] = individual.aijk[i,j,k1-1] * 0.5;
            double value = 0;
            double sumofTemp1 = 0;
            for (int i1 = 0; i1 < numofTreeType; i1++)
            {
                sumofTemp1 += individual.Tempijk[i1][k1 - 1];
                for (int j1 = 0; j1 < numofTreeAges; j1++)
                {
                    value += individual.SofTreeFelling[i1][j1][k1 - 1];
                }
            }
            for (int i1 = 0; i1 < numofTreeType; i1++)
            {
                individual.SofTreePlanting[i1][k1 - 1] = (individual.Tempijk[i1][k1 - 1] / sumofTemp1) * value;
            }
            return individual;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //初始化种群
            for (int num = 0; num < populationSize; num++)
            {
                Individual individual = initialIndividualCode();
                //计算适应值
                calculateFitness(individual);
                //Console.WriteLine(individual.objective1);
                population.Add(individual);
            }
            population = produceNewParentSchesulingByDominatedSort(population);

            //for (int m = 0; m < maxIter; m++)
            //{
            //    Console.WriteLine($"------------------第{m + 1}次迭代---------------");
            //    //population = DeepCopyByBin<List<Individual>>(EvolveByNSGAII(population));
            //    population = produceNewParentSchesulingByDominatedSort(population);
            //    population = population.OrderBy(x => x.frontNumber).ToList();
            //}
            ////快速非支配排序
            //population = getParetoOfPopulation(population);


            Console.WriteLine($"---------------------非支配解--------------------");
            for (int num = 0; num < population.Count; num++) 
            {
                Console.WriteLine($"{population[num].objective1}  {population[num].objective2}");
            }
        }
        public List<Individual> produceNewParentSchesulingByDominatedSort(List<Individual> population)
        {
            List<Individual> frontSet = new List<Individual>();                     //记录按pareto层排序的解集
            List<Individual> newParentSolutions = new List<Individual>();  //新种群

            //------初始化------
            for (int i = 0; i < population.Count; i++)
            {
                Individual iIndividual = population[i];
                iIndividual.frontNumber = 0;                                                                   //每次计算pareto解时，frontNumber重值为0
                iIndividual.numOfDonimateIndividual = 0;                                            //每次计算pareto解时，重值为0
                iIndividual.donimatedSet = new List<Individual>();                     //每次计算pareto解时，重值为空                                                        //每次计算pareto解时，distanceOfCrowd重置为空
                iIndividual.distanceOfCrowd = 0;
            }

            //------生成各个体的支配集和被支配个数，并找出第1层front------
            List<Individual> firstFrontSet = new List<Individual>();       //第1层解集
            for (int p = 0; p < population.Count; p++)
            {
                Individual pIndividual = population[p];
                for (int q = 0; q < population.Count; q++)
                {
                    Individual qIndividual = population[q];
                    if (((pIndividual.objective1 >= qIndividual.objective1) && (pIndividual.objective2 > qIndividual.objective2)) || ((pIndividual.objective1 > qIndividual.objective1) && (pIndividual.objective2 >= qIndividual.objective2)))       //pIndividual 支配qIndividual
                    {
                        pIndividual.donimatedSet.Add(qIndividual);                               //qIndividual加入到pIndividual的支配解中
                    }
                    else if (((qIndividual.objective1 >= pIndividual.objective1) && (qIndividual.objective2 > pIndividual.objective2)) || ((qIndividual.objective1 > pIndividual.objective1) && (qIndividual.objective2 >= pIndividual.objective2)))     //qIndividual 支配pIndividual
                    {
                        pIndividual.numOfDonimateIndividual = pIndividual.numOfDonimateIndividual + 1;
                    }
                }

                if (pIndividual.numOfDonimateIndividual == 0)                             //不被任何个体支配，加入到第1层front中。
                {
                    pIndividual.frontNumber = 1;
                    firstFrontSet.Add(pIndividual);
                } 
            }
            int aa = 0;
            for (int n = 0; n < firstFrontSet.Count; n++) 
            {

                List<double> obj1 = new List<double>();
                List<double> obj2 = new List<double>();
                for (int k = 0; k < numofMaxFellingYear; k++)
                {
                    double sum1 = 0;
                    double sum2 = 0;
                    double sum3 = 0;
                    double sum4 = 0;
                    for (int i = 0; i < numofTreeType; i++) 
                    {
                        for (int j = 0; j < numofTreeAges; j++) 
                        {
                            double c = Convert.ToDouble(tableOfCijk.Rows[k][i + 1]);
                            double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
                            double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
                            double cdt = Convert.ToDouble(tableOfCdtij.Rows[j][i + 1]);
                            sum1 += c * firstFrontSet[n].SofTreeFelling[i][j][k];
                            sum2 += cdt * firstFrontSet[n].aijk[i,j,numofMaxFellingYear-1];
                            sum3 += cds * firstFrontSet[n].aijk[i, j, k];
                            sum4 += cdp * firstFrontSet[n].SofTreeFelling[i][j][k];
                        }
                    }
                    obj1.Add(sum3);
                    obj2.Add(sum1);
                }
                for (int k = 0; k < numofMaxFellingYear; k++)
                {
                    Console.WriteLine(obj1[k]);

                }
                Console.WriteLine("-------------------------------");
                //Console.WriteLine(firstFrontSet[n].objective2);
                for (int k = 0; k < numofMaxFellingYear; k++)
                {
                    Console.WriteLine(obj2[k]);
                }
            }

            //------生成第2层pareto解集------
            //List<Individual> nextFrontSet = DeepCopyByBin<List<Individual>>(produceNextFrontNumber(firstFrontSet));
            ////如果第2 front没有将newParentSolutions填充满，即元素个数小于numOfPopular，加入到新种群中
            //if ((nextFrontSet.Count + newParentSolutions.Count) <= populationSize)
            //{
            //    newParentSolutions.AddRange(nextFrontSet);                                //第2层加入到newParentSolutions
            //    //caculateCrowdDistance_SS(nextFrontSet);                                   //计算第2层crowdDistance
            //}
            //else
            //{
            //    caculateCrowdDistance(nextFrontSet);                                    //计算第2层crowdDistance
            //    nextFrontSet.OrderByDescending(x => x.distanceOfCrowd);
            //    int countOfParent = newParentSolutions.Count;
            //    for (int i = 0; i < populationSize - countOfParent; i++)
            //    {
            //        newParentSolutions.Add(nextFrontSet[i]);
            //    }
            //    return firstFrontSet;
            //    //return newParentSolutions;
            //}

            //while (nextFrontSet.Count != 0)
            //{
            //    nextFrontSet = produceNextFrontNumber(nextFrontSet);                   //继续生成其它层pareto解集
            //    if (nextFrontSet.Count != 0)
            //    {
            //        //如果下层front没有将newParentSolutions填充满，即元素个数小于numOfPopular，加入到新种群中
            //        if ((nextFrontSet.Count + newParentSolutions.Count) <= populationSize)
            //        {
            //            newParentSolutions.AddRange(nextFrontSet);                                           //该层加入到newParentSolutions
            //            //caculateCrowdDistance_SS(nextFrontSet);                                              //计算该层crowdDistance
            //        }
            //        else
            //        {
            //            caculateCrowdDistance(nextFrontSet);                                                //计算该层crowdDistance
            //            nextFrontSet.OrderByDescending(x => x.distanceOfCrowd);
            //            int countOfParent = newParentSolutions.Count;
            //            for (int i = 0; i < populationSize - countOfParent; i++)
            //            {
            //                newParentSolutions.Add(nextFrontSet[i]);
            //            }
            //            return firstFrontSet;
            //            //return newParentSolutions;
            //        }
            //    }
            //}

            return firstFrontSet;
            //return newParentSolutions;
        }
        public void caculateCrowdDistance(List<Individual> currentFrontSet)
        {
            // sort by TTPT
            currentFrontSet.OrderBy(x => x.objective1);
            // set the distance of the first individual which has the minimum of TTPT as maxvalue
            currentFrontSet[0].distanceOfCrowd = double.MaxValue;
            // set the distance of the last individual which has the maximum of TTPT as maxvalue
            currentFrontSet[currentFrontSet.Count - 1].distanceOfCrowd = double.MaxValue;
            double minimumOfTTPT = (currentFrontSet[0]).objective1;
            double maximumOfTTPT = (currentFrontSet[currentFrontSet.Count - 1]).objective1;
            double distanceOfMaxToMinOfTTPT = maximumOfTTPT - minimumOfTTPT;

            for (int i = 1; i < currentFrontSet.Count - 1; i++)
            {
                //一个front中的解都相等，实则是一个
                if (distanceOfMaxToMinOfTTPT == 0)
                {
                    (currentFrontSet[i]).distanceOfCrowd = double.MaxValue;
                }
                else
                    (currentFrontSet[i]).distanceOfCrowd = ((currentFrontSet[i + 1]).objective1 - (currentFrontSet[i - 1]).objective1) / distanceOfMaxToMinOfTTPT;
            }

            // sort by TLT. 
            currentFrontSet.OrderBy(x => x.objective2);
            double minimumOfTLT = (currentFrontSet[0]).objective2;
            double maximumOfTLT = (currentFrontSet[currentFrontSet.Count - 1]).objective2;
            double distanceOfMaxToMinOfTLT = maximumOfTLT - minimumOfTLT;
            for (int i = 1; i < currentFrontSet.Count - 1; i++)
            {
                if (distanceOfMaxToMinOfTLT != 0)
                {
                    (currentFrontSet[i]).distanceOfCrowd = ((currentFrontSet[i]).distanceOfCrowd + ((currentFrontSet[i + 1]).objective2 - (currentFrontSet[i - 1]).objective2) / distanceOfMaxToMinOfTLT) / 2;
                }
            }
        }
        public List<Individual> produceNextFrontNumber(List<Individual> currentFrontSet)
        {
            //produce the individuals in the next front number            
            List<Individual> nextFrontSet = new List<Individual>();                   //记录下1层front解
            for (int p = 0; p < currentFrontSet.Count; p++)
            {
                Individual pIndividual = currentFrontSet[p];
                for (int q = 0; q < pIndividual.donimatedSet.Count; q++)
                {
                    Individual qIndividual = pIndividual.donimatedSet[q];
                    qIndividual.numOfDonimateIndividual = qIndividual.numOfDonimateIndividual - 1;
                    if (qIndividual.numOfDonimateIndividual == 0)
                    {
                        qIndividual.frontNumber = pIndividual.frontNumber + 1;   //记录front层号
                        nextFrontSet.Add(qIndividual);         //加入到下1层front中
                    }
                }
            }
            //loop ends, if nextFrontSet is empty
            if (nextFrontSet.Count != 0)
            {
                return nextFrontSet;
            }
            return nextFrontSet;
        }
        public List<Individual> getParetoOfPopulation(List<Individual> population)
        {
            List<Individual> tempPopulation = new List<Individual>();
            for (int j = 0; j < population.Count; j++)
            {
                Individual currentSolution = population[j];
                if (currentSolution.frontNumber == 1)
                {
                    tempPopulation.Add(DeepCopyByBin<Individual>(currentSolution));
                }
                else
                    break;
            }
            tempPopulation.OrderBy(x => x.objective1);
            //------去除重复的解------
            List<Individual> paretoSolutions = new List<Individual>();
            for (int i = 0; i < tempPopulation.Count; i++)
            {
                //新种群中的individual在父种群有，可以添加的情况
                Individual tempSolution = DeepCopyByBin<Individual>(tempPopulation[i]);

                //新种群中的individual在父种群有，不可以添加的情况
                if (!ifSolutionExistedInPopulationWithObjectiveValue(paretoSolutions, tempSolution))
                {
                    paretoSolutions.Add(tempSolution);
                }
            }
            return paretoSolutions;
        }
        public bool ifSolutionExistedInPopulationWithObjectiveValue(List<Individual> population, Individual individual)
        {
            bool flag = false;
            for (int i = 0; i < population.Count; i++)
            {
                //将population的一个Individual的solutionCode与individual相比

                if ((individual.objective1 == (population[i]).objective1) && (individual.objective2 == (population[i]).objective2))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
        public List<Individual> EvolveByNSGAII(List<Individual> population)
        {
            List<Individual> offsoringPopulation = new List<Individual>();
            List<Individual> clonePopulation = DeepCopyByBin<List<Individual>>(population);

            //通过锦标赛选择个体
            while (clonePopulation.Count > 0)
            {
                Individual parent1 = new Individual();
                Individual parent2 = new Individual();
            //---从留下来的精英中无放回地挑选父母
            laberSwap:
                int number1 = random.Next(0, clonePopulation.Count);
                int number2 = random.Next(0, clonePopulation.Count);
                //---从父代种群和当前子代种群中选择个体
                parent1 = DeepCopyByBin<Individual>(clonePopulation[number1]);
                parent2 = DeepCopyByBin<Individual>(clonePopulation[number2]);
                if (number1 == number2)
                {
                    goto laberSwap;
                }
                //适合实数编码的交叉方式----中间重组 X_子代 = X_父代1 + alpha (X_父代2 - X_父代1)
                Individual offspring1 = crossover(parent1, parent2);
                Individual offspring2 = crossover(parent2, parent1);
                offsoringPopulation.Add(offspring1);
                offsoringPopulation.Add(offspring2);
                if (offsoringPopulation.Count == populationSize)
                {
                    break;
                }
            }
            //offsoringPopulation = mutation(offsoringPopulation);
            //offsoringPopulation = offsoringPopulation.OrderByDescending(x => x.objective1).ToList();
            offsoringPopulation.AddRange(clonePopulation);
            offsoringPopulation = produceNewParentSchesulingByDominatedSort(offsoringPopulation);
            //for (int n = 0; n < offsoringPopulation.Count; n++) 
            //{
            //    Console.WriteLine(offsoringPopulation[n].objective1);
            //}
            //Console.WriteLine($"当前的最大值为：{offsoringPopulation[0].objective1}");
            return offsoringPopulation;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int K = 0;
            while (K < numofMaxFellingYear)
            {
                IndexOfYear.Add(K);
                K = K + 11;
            }
            //初始化种群
            for (int num = 0; num < populationSize; num++)
            {
                Individual individual = initialIndividualCode10();
                //计算适应值
                calculateFitness10(individual);
                //Console.WriteLine(individual.objective1);
                population.Add(individual);
            }
            int a = 0;
             
            population = population.OrderByDescending(x => x.objective1).ToList();
            Console.WriteLine($"最优解为：{population[0].objective1}");

            Console.WriteLine("-------------------------");

            //double sum0 = 0;
            //double sum1 = 0;
            //double sum2 = 0;
            //double sum3 = 0;
            //double[] sum2List = new double[IndexOfYear.Count];
            //double[] sum3List = new double[IndexOfYear.Count];


            //for (int k = 0; k < IndexOfYear.Count; k++)
            //{

            //    for (int i = 0; i < numofTreeType; i++)
            //    {
            //        sum3 += population[0].SofTreePlanting[i][k];

            //        for (int j = 0; j < numofTreeAges; j++)
            //        {
            //            double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
            //            double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
            //            sum0 += cds * population[0].aijk[i,j,IndexOfYear[k]];
            //            sum1 += cdp * population[0].SofTreeFelling[i][j][k];
            //            sum2 += population[0].SofTreeFelling[i][j][k];     
            //        }
            //    }
            //    sum2List[k] = sum2;
            //    sum3List[k] = sum3;
            //}
            //Console.WriteLine("-------------------------");
            //Console.WriteLine($"{sum0}");
            //Console.WriteLine("-------------------------");
            //Console.WriteLine($"{sum1}");
            //Console.WriteLine("-------------------------");

            //for (int k = 0; k < IndexOfYear.Count; k++)
            //{
            //    Console.WriteLine(sum2List[k]);
            //}
            //Console.WriteLine("-------------------------");
            //for (int k = 0; k < IndexOfYear.Count; k++)
            //{
            //    Console.WriteLine(sum3List[k]);
            //}
            //Console.WriteLine("-------------------------");


            double sum2 = 0;
            double sum3 = 0;
            double[] sum2List = new double[IndexOfYear.Count];
            double[] sum3List = new double[IndexOfYear.Count];
            for (int k = 0; k < IndexOfYear.Count; k++)
            {
                for (int i = 0; i < numofTreeType; i++)
                {
                    for (int j = 0; j < numofTreeAges; j++)
                    {
                        double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
                        double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
                        sum2 += cds * population[0].aijk[i, j, IndexOfYear[k]];
                        sum3 += cdp * population[0].SofTreeFelling[i][j][k];
                    }
                }
                sum2List[k] = sum2;
                sum3List[k] = sum3;
            }
            for (int k = 0; k < IndexOfYear.Count; k++)
            {
                Console.WriteLine(sum2List[k]);
            }
            Console.WriteLine("-------------------------");
            for (int k = 0; k < IndexOfYear.Count; k++)
            {
                Console.WriteLine(sum3List[k]);
            }

            double sum1 = 0;
            sum2 = 0;
            sum3 = 0;

            for (int i = 0; i < numofTreeType; i++)
            {
                for (int j = 0; j < numofTreeAges; j++)
                {
                    double cdt = Convert.ToDouble(tableOfCdtij.Rows[j][i + 1]);
                    sum1 += cdt * population[0].aijk[i, j, IndexOfYear.Count - 1];

                    for (int k = 0; k < IndexOfYear.Count; k++)
                    {
                        double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
                        double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
                        sum2 += cds * population[0].aijk[i, j, IndexOfYear[k]];
                        sum3 += cdp * population[0].SofTreeFelling[i][j][k];
                    }
                }
            }
            Console.WriteLine("-------------------------");
            Console.WriteLine(sum2);
            Console.WriteLine("-------------------------");
            Console.WriteLine(sum3);
            Console.WriteLine("-------------------------");
        }
        public Individual initialIndividualCode10()
        {
            Individual currentIndividual = new Individual();
            //while (true) 
            {
                double[,,] aijk = new double[numofTreeType, numofTreeAges, numofMaxFellingYear];
                //随机生成初始种群
                Individual individual1 = new Individual();
                //individual1.aijk = aijk;
                individual1.SofTreeFelling = new List<List<List<double>>>();

                double sum = 0;

                for (int i = 0; i < numofTreeType; i++)
                {
                    List<List<double>> SofFellingForIJ = new List<List<double>>();
                    for (int j = 0; j <numofTreeAges; j++)
                    {
                        List<double> SofFellingForIJK = new List<double>();
                        //根据约束3生成 SofTreeFelling，即Xij1
                        aijk[i, j, 0] = Convert.ToDouble(tableOfaij1.Rows[j][i + 1]);
                        double value = random.NextDouble() * aijk[i, j, 0] * 0.08;
                        sum += value;
                        SofFellingForIJK.Add(value);
                        SofFellingForIJ.Add(SofFellingForIJK);
                    }
                    individual1.SofTreeFelling.Add(SofFellingForIJ);
                }


                individual1.Tempijk = new List<List<double>>();
                double sumofTemp = 0;
                for (int i = 0; i < numofTreeType; i++)
                {
                    List<double> tempForIJ = new List<double>();
                    double value = random.NextDouble();
                    sumofTemp += value;
                    tempForIJ.Add(value);
                    individual1.Tempijk.Add(tempForIJ);
                }

                individual1.SofTreePlanting = new List<List<double>>();
                for (int i = 0; i < numofTreeType; i++)
                {
                    List<double> SofPlantingForIJ = new List<double>();
                    double value = (individual1.Tempijk[i][0] / sumofTemp) * sum;
                    SofPlantingForIJ.Add(value);
                    individual1.SofTreePlanting.Add(SofPlantingForIJ);
                }

                Individual individual = new Individual();

                for (int k = 0; k < IndexOfYear.Count-1; k++)
                {
                    if (k == 0)
                    {
                        for (int i = 0; i < numofTreeType; i++)
                        {
                            for (int j = 0; j < numofTreeAges - 11; j++)
                            {
                                if (j == 0)
                                {
                                    aijk[i, 11, IndexOfYear[k] + 11] = aijk[i, 0, IndexOfYear[k]] - individual1.SofTreeFelling[i][0][k] + individual1.SofTreePlanting[i][k];
                                    if (aijk[i, 11, IndexOfYear[k] + 11] < 0)
                                    {
                                        individual1.SofTreeFelling[i][0][k] = aijk[i, 0, IndexOfYear[k]] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual1.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual1.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual1.SofTreePlanting[i1][k] = (individual1.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, 11, IndexOfYear[k] + 11] = aijk[i, 0, IndexOfYear[k]] - individual1.SofTreeFelling[i][0][k] + individual1.SofTreePlanting[i][k];
                                }
                                else
                                {
                                    aijk[i, j + 11, IndexOfYear[k]+ 11] = aijk[i, j, IndexOfYear[k]] - individual1.SofTreeFelling[i][j][k];
                                    if (aijk[i, j + 11, IndexOfYear[k] + 11] < 0)
                                    {
                                        individual1.SofTreeFelling[i][j][k] = aijk[i, j, IndexOfYear[k]] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual1.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual1.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual1.SofTreePlanting[i1][k] = (individual1.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, j + 11, IndexOfYear[k] + 11] = aijk[i, j, IndexOfYear[k]] - individual1.SofTreeFelling[i][j][k];
                                }
                            }
                        }
                        individual = DeepCopyByBin<Individual>(produceIndividualCodeFor10K(individual1, aijk, k));
                    }
                    //更新aijk
                    else
                    {
                        for (int i = 0; i < numofTreeType; i++)
                        {
                            for (int j = 0; j < numofTreeAges - 11; j++)
                            {
                                if (j == 0)
                                {
                                    aijk[i, 11, IndexOfYear[k] + 11] = aijk[i, 0, IndexOfYear[k]] - individual.SofTreeFelling[i][0][k] + individual.SofTreePlanting[i][k];
                                    if (aijk[i, 11, IndexOfYear[k] + 11] < 0)
                                    {
                                        individual.SofTreeFelling[i][0][k] = aijk[i, 0, IndexOfYear[k]] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual.SofTreePlanting[i1][k] = (individual.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, 11, IndexOfYear[k] + 11] = aijk[i, 0, IndexOfYear[k]] - individual.SofTreeFelling[i][0][k] + individual.SofTreePlanting[i][k];
                                }
                                else
                                {
                                    aijk[i, j + 11, IndexOfYear[k] +11] = aijk[i, j, IndexOfYear[k]] - individual.SofTreeFelling[i][j][k];
                                    if (aijk[i, j + 11, IndexOfYear[k] + 11] < 0)
                                    {
                                        individual.SofTreeFelling[i][j][k] = aijk[i, j, IndexOfYear[k]] * 0.5;

                                        double value = 0;
                                        double sumofTemp1 = 0;

                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            sumofTemp1 += individual.Tempijk[i1][k];
                                            for (int j1 = 0; j1 < numofTreeAges; j1++)
                                            {
                                                value += individual.SofTreeFelling[i1][j1][k];
                                            }
                                        }
                                        for (int i1 = 0; i1 < numofTreeType; i1++)
                                        {
                                            individual.SofTreePlanting[i1][k] = (individual.Tempijk[i1][k] / sumofTemp1) * value;
                                        }
                                    }
                                    aijk[i, j + 11, IndexOfYear[k] + 11] = aijk[i, j, IndexOfYear[k]] - individual.SofTreeFelling[i][j][k];
                                }
                            }
                        }
                        individual = DeepCopyByBin<Individual>(produceIndividualCodeFor10K(individual, aijk, k));
                    }
                }
                individual.aijk = aijk;
                currentIndividual = DeepCopyByBin<Individual>(individual);
            }
            return currentIndividual;
        }
        public void calculateFitness10(Individual individual)
        {
            //for (int num = 0; num < population.Count; num++) 
            {
                individual.objective1 = 0;
                individual.objective2 = 0;

                double sum1 = 0;
                double sum2 = 0;
                double sum3 = 0;
                double sum4 = 0;

                for (int i = 0; i < numofTreeType; i++)
                {
                    for (int j = 0; j < numofTreeAges; j++)
                    {
                        double cdt = Convert.ToDouble(tableOfCdtij.Rows[j][i + 1]);
                        sum1 += cdt * individual.aijk[i, j, numofMaxFellingYear - 1];

                        for (int k = 0; k < IndexOfYear.Count; k++)
                        {
                            double c = Convert.ToDouble(tableOfCijk.Rows[IndexOfYear[k]][i + 1]);
                            double cds = Convert.ToDouble(tableOfCdij.Rows[j][i + 1]);
                            double cdp = Convert.ToDouble(tableOfCdpij.Rows[j][i + 1]);
                            sum2 += cds * individual.aijk[i, j, IndexOfYear[k]];
                            sum3 += cdp * individual.SofTreeFelling[i][j][k];
                            sum4 += c * individual.SofTreeFelling[i][j][k];
                        }
                    }
                }
                individual.objective1 = sum1 + sum2 + sum3;
                individual.objective2 = sum4;
            }
        }
        public Individual produceIndividualCodeFor10K(Individual individual, double[,,] aijk, int k)
        {
            double sum = 0;

            for (int i = 0; i < numofTreeType; i++)
            {
                for (int j = 0; j < numofTreeAges; j++)
                {
                    List<double> tempList = new List<double>();
                    //根据约束3生成 SofTreeFelling，即Xij1
                    double value = random.NextDouble() * aijk[i, j, IndexOfYear[k]] * 0.8;
                    sum += value;
                    tempList.Add(value);
                    individual.SofTreeFelling[i][j].AddRange(tempList);
                }
            }

            double sumofTemp = 0;
            for (int i = 0; i < numofTreeType; i++)
            {
                List<double> tempForJ = new List<double>();
                double value = random.NextDouble();
                sumofTemp += value;
                tempForJ.Add(value);
                individual.Tempijk[i].AddRange(tempForJ);
            }

            for (int i = 0; i < numofTreeType; i++)
            {
                List<double> SofPlantingForIJK = new List<double>();
                double value = (individual.Tempijk[i][k] / sumofTemp) * sum;
                SofPlantingForIJK.Add(value);
                individual.SofTreePlanting[i].AddRange(SofPlantingForIJK);
            }

            return individual;
        }
    }
    

}
