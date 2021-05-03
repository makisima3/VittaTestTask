using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PaymentsTestTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int selectedOrder = -1, selecetedArrival = -1;

        public MainWindow()
        {
            InitializeComponent();

            UpdateDataGrids();
        }

        //логика вьюшки на обработку клика
        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (selecetedArrival != -1 && selectedOrder != -1)
            {
                if (AddPayment())
                {

                    UpdateDataGrids();

                    selecetedArrival = -1;
                    selectedOrder = -1;

                    MessageBox.Show("Платеж Создан!");
                }
                else
                {
                    MessageBox.Show("Не валидная транзакция!");
                }
            }
            else
            {
                MessageBox.Show("Заказ или приход не выбран!");
            }
        }

        //добавляем новый платеж(здесь только условия его добавления и привязка данных, остальная логика в триггере как и было в тз)
        private bool AddPayment()
        {
            using (var idk = new idkEntities())
            {
                var order = idk.Orders.FirstOrDefault((p) => p.id == selectedOrder);
                var arrival = idk.Arrival.FirstOrDefault((p) => p.id == selecetedArrival);

                if (order.Total_Order_Amount > order.Amount_Paid || arrival.Remains > 0)
                {
                    int lastID = idk.Payments.Count() > 0 ? idk.Payments.Max((p) => p.id) + 1 : 0;

                    Payments payment = new Payments()
                    {
                        id = lastID,
                        Arrival_id = selecetedArrival,
                        Order_id = selectedOrder
                    };

                    idk.Payments.Add(payment);
                    idk.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        //Обновляем данные из бд
        private void UpdateDataGrids()
        {
            using (var idk = new idkEntities())
            {
                OrdersDataGrid.ItemsSource = idk.Orders.ToList();
                ArrivalsDataGrid.ItemsSource = idk.Arrival.ToList();
            }
        }

        #region DataGridsFilters
        private void DropFilters_Click(object sender, RoutedEventArgs e)
        {
            UpdateDataGrids();
        }

        private void FindArrivalButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(ArrivalID_TB.Text, out int result);

            int SearchableID = result;

            using (var idk = new idkEntities())
            {
                var arrival = idk.Arrival.Where((p) => p.id == SearchableID);

                ArrivalsDataGrid.ItemsSource = arrival.ToList();
            }
        }

        private void FindOrderButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(OrderID_TB.Text, out int result);

            int SearchableID = result;

            using (var idk = new idkEntities())
            {
                var order = idk.Orders.Where((p) => p.id == SearchableID);

                OrdersDataGrid.ItemsSource = order.ToList();
            }
        }
        #endregion

        #region DataGridsSelectionChanges
        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersDataGrid.SelectedIndex != -1)
                selectedOrder = (OrdersDataGrid.SelectedItem as Orders).id;
        }

        private void ArrivalsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArrivalsDataGrid.SelectedIndex != -1)
                selecetedArrival = (ArrivalsDataGrid.SelectedItem as Arrival).id;
        }
        #endregion

        #region TextBoxesInputFilter
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }
        #endregion

    }
}
