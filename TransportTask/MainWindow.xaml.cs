using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TransportTask
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NorthwestCornerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var (supply, demand, costs) = GetInputData();
                (supply, demand, costs) = BalanceProblem(supply, demand, costs);

                var plan = NorthwestCornerMethod(supply, demand, costs);
                double totalCost = CalculateTotalCost(plan, costs);
                DisplayResult(plan, totalCost, costs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void MinElementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var (supply, demand, costs) = GetInputData();
                (supply, demand, costs) = BalanceProblem(supply, demand, costs);

                var plan = MinElementMethod(supply, demand, costs);
                double totalCost = CalculateTotalCost(plan, costs);
                DisplayResult(plan, totalCost, costs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private (int[], int[], int[,]) GetInputData()
        {
            var supply = SupplyInput.Text.Split(',').Select(int.Parse).ToArray();
            var demand = DemandInput.Text.Split(',').Select(int.Parse).ToArray();
            var costRows = CostsInput.Text.Split(';');
            int m = supply.Length;
            int n = demand.Length;

            int[,] costs = new int[m, n];

            for (int i = 0; i < m; i++)
            {
                var costValues = costRows[i].Split(',').Select(int.Parse).ToArray();
                for (int j = 0; j < n; j++)
                {
                    costs[i, j] = costValues[j];
                }
            }

            return (supply, demand, costs);
        }

        private (int[], int[], int[,]) BalanceProblem(int[] supply, int[] demand, int[,] costs)
        {
            int totalSupply = supply.Sum();
            int totalDemand = demand.Sum();

            if (totalSupply > totalDemand)
            {
                Array.Resize(ref demand, demand.Length + 1);
                demand[demand.Length - 1] = totalSupply - totalDemand;

                int m = supply.Length;
                int[,] newCosts = new int[m, demand.Length];
                Array.Copy(costs, newCosts, costs.Length);

                for (int i = 0; i < m; i++)
                {
                    newCosts[i, demand.Length - 1] = 0;
                }

                costs = newCosts;
            }
            else if (totalDemand > totalSupply)
            {
                Array.Resize(ref supply, supply.Length + 1);
                supply[supply.Length - 1] = totalDemand - totalSupply;

                int n = demand.Length;
                int[,] newCosts = new int[supply.Length, n];
                Array.Copy(costs, newCosts, costs.Length);

                for (int j = 0; j < n; j++)
                {
                    newCosts[supply.Length - 1, j] = 0;
                }

                costs = newCosts;
            }

            return (supply, demand, costs);
        }

        private int[,] NorthwestCornerMethod(int[] supply, int[] demand, int[,] costs)
        {
            int m = supply.Length;
            int n = demand.Length;
            int[,] plan = new int[m, n];

            int i = 0, j = 0;

            while (i < m && j < n)
            {
                int allocation = Math.Min(supply[i], demand[j]);
                plan[i, j] = allocation;
                supply[i] -= allocation;
                demand[j] -= allocation;

                if (supply[i] == 0) i++;
                if (demand[j] == 0) j++;
            }

            return plan;
        }

        private int[,] MinElementMethod(int[] supply, int[] demand, int[,] costs)
        {
            int m = supply.Length;
            int n = demand.Length;
            int[,] plan = new int[m, n];

            while (true)
            {
                int minCost = int.MaxValue;
                int minRow = -1;
                int minCol = -1;

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (supply[i] > 0 && demand[j] > 0 && costs[i, j] < minCost)
                        {
                            minCost = costs[i, j];
                            minRow = i;
                            minCol = j;
                        }
                    }
                }

                if (minRow == -1 || minCol == -1) break;

                int allocation = Math.Min(supply[minRow], demand[minCol]);
                plan[minRow, minCol] = allocation;
                supply[minRow] -= allocation;
                demand[minCol] -= allocation;
            }

            return plan;
        }

        private double CalculateTotalCost(int[,] plan, int[,] costs)
        {
            double totalCost = 0;
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    totalCost += plan[i, j] * costs[i, j];
                }
            }

            return totalCost;
        }

        private void DisplayResult(int[,] plan, double totalCost, int[,] costs)
        {
            int m = plan.GetLength(0);  // Количество поставщиков
            int n = plan.GetLength(1);  // Количество потребителей

            // Список данных для отображения в DataGrid
            List<SupplierTransportData> supplierDataList = new List<SupplierTransportData>();

            for (int i = 0; i < m; i++)
            {
                // Создаем строку данных для одного поставщика
                SupplierTransportData supplierData = new SupplierTransportData
                {
                    SupplierIndex = i + 1,  // Индексация с 1
                    Quantities = new List<int>(),
                    Costs = new List<double>()
                };

                for (int j = 0; j < n; j++)
                {
                    supplierData.Quantities.Add(plan[i, j]);  // Добавляем количество для текущего потребителя
                    supplierData.Costs.Add(plan[i, j] * costs[i, j]);  // Добавляем стоимость для текущего потребителя
                }

                // Добавляем данные для поставщика в список
                supplierDataList.Add(supplierData);
            }

            // Привязываем список данных к DataGrid
            ResultDataGrid.ItemsSource = supplierDataList;

            // Выводим общую стоимость в MessageBox
            MessageBox.Show($"Общая стоимость: {totalCost}");
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            SupplyInput.Text = string.Empty;
            DemandInput.Text = string.Empty;
            CostsInput.Text = string.Empty;
            ResultDataGrid.ItemsSource = null;
        }

        private void Button_Fill_Click(object sender, RoutedEventArgs e)
        {
            SupplyInput.Text = "200,350,300";
            DemandInput.Text = "270,130,190,150,110";
            CostsInput.Text = "24,50,55,27,16;50,47,23,17,21;35,59,55,27,41";
        }
        private void Button_Fill_Click_1(object sender, RoutedEventArgs e)
        {
            // Данные по поставщикам
            SupplyInput.Text = "350,200,300";

            // Данные по потребителям
            DemandInput.Text = "170,140,200,195,145";

            // Матрица затрат (каждая строка представляет затраты для поставщика, каждая колонка — для потребителя)
            CostsInput.Text = "22,14,16,28,30;19,17,26,36,36;37,30,31,39,41";
        }

    }

    public class TransportPlan
    {
        public int SupplierIndex { get; set; }  // Индекс поставщика
        public int ConsumerIndex { get; set; }  // Индекс потребителя
        public int Quantity { get; set; }       // Количество товаров
        public double Cost { get; set; }        // Стоимость за товар
    }
    public class SupplierTransportData
    {
        public int SupplierIndex { get; set; }  // Индекс поставщика
        public List<int> Quantities { get; set; }  // Список количеств для всех потребителей
        public List<double> Costs { get; set; }  // Список стоимостей для всех потребителей
    }


}
