using SkillboxHomework10_1.BankWorkers;

using SkillboxHomework10_1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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




//Количество денег не обновляется сразу после перевода
//
namespace SkillboxHomework10_1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        BankWorker bankWorker;
        IEditClientData workerEditMode;
        List<Department> departmentList;
        string info = "dsfdsdfsd";
       
        public MainWindow()
        {
            AccessWindow accessWindow = new AccessWindow(); //Создание и Запуск окна выбора Прав доступа
            accessWindow.ShowDialog();

            

            departmentList = new List<Department>();
            FillDepartmentList(5);
            InitializeComponent();

            if (accessWindow.DialogResult == true) // Инициализация нужных типов и настройка окна
            {
                if (!accessWindow.isManager)
                {
                    bankWorker = new Consultant();
                    workerEditMode = new Consultant();
                    mainWindow.Title = "Консультант";
                    passportColumn.Visibility = Visibility.Hidden;
                    btnAdd.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                    btnAddAcc.IsEnabled = false;
                }
                else
                { 
                    bankWorker = new Manager();
                    workerEditMode = new Manager();
                    mainWindow.Title = "Менеджер";
                    
                }

            }
            dgClientsList.ItemsSource = bankWorker.Clients;

            cbDeps.ItemsSource = departmentList;
            cbDeps.SelectedIndex = 0;

            //(bankWorker as Manager). AddClientEvent += ShowMessage;
            dgSourceUpdate();
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void FillDepartmentList(int count)
        {
            departmentList.Add(new Department(0, "Все департаменты"));
            for (int i = 1; i <= count; i++)
            {
                departmentList.Add(new Department(i, $"Департамент {i}"));
            }

        }
        private void SaveBtnClick(object sender, RoutedEventArgs e)
        {
            bankWorker.SaveToFile();
        }

        private void ChangeBtnClick(object sender, RoutedEventArgs e)
        {
            if (dgClientsList.SelectedIndex!=-1)
            {
                workerEditMode.EditClientData(dgClientsList.SelectedItem as Client);

                bankWorker.SaveToFile();
            }
           
        }
        private void AddBtnClick(object sender, RoutedEventArgs e)
        {
            
            if (bankWorker is Manager)
            {
                AddClientWindow addClientWindow = new AddClientWindow();
                
                addClientWindow.ShowDialog();

                (bankWorker as Manager).Clients.Add(addClientWindow.client);
               
                
            }
            dgSourceUpdate();
            bankWorker.SaveToFile();
        }
        private void DeleteBtnClick(object sender, RoutedEventArgs e)
        {
            if (bankWorker is Manager)
            {
                (bankWorker as Manager).DeleteClient(dgClientsList.SelectedItem as Client);
                
            }
            dgSourceUpdate();
            bankWorker.SaveToFile();
        }

        private void dgClientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientsList.SelectedIndex!=-1)
            {
                Client client = dgClientsList.SelectedItem as Client;

                tbInfo.Text = client.ChangeInfo;
                lbAccounts.ItemsSource = client.AccList;
                lbAccounts.SelectedIndex = 0;
            }
            

        }

        private void cbDeps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgSourceUpdate();
        }
        private void dgSourceUpdate()
        {
            if (cbDeps.SelectedIndex == 0)
            {
                dgClientsList.ItemsSource = bankWorker.Clients;
            }
            else dgClientsList.ItemsSource = bankWorker.Clients.Where(p => p.DepId == (cbDeps.SelectedItem as Department).DepId);
        }

        private void AddAccClick(object sender, RoutedEventArgs e)
        {
            if (dgClientsList.SelectedIndex!=-1)
            {
                AddAccWindow addAccWindow = new AddAccWindow();

                addAccWindow.ShowDialog();
                if (addAccWindow.DialogResult==true)
                {

                    int amount = addAccWindow.amount;
                    IAddAccount<BankAccount> client = dgClientsList.SelectedItem as Client;
                    if (addAccWindow.cbAccType.SelectedIndex==0)
                    {
                        client.AddAccount(new DepositeAccount(amount));
                    }
                    else
                    {
                        client.AddAccount(new NonDepositeAccount(amount));
                    }

                }

            }



            
        }
        

        private void DelAccClick(object sender, RoutedEventArgs e)
        {
            BankAccount acc = lbAccounts.SelectedItem as BankAccount;
            if (acc != null) 
            {
                (dgClientsList.SelectedItem as Client).AccList.Remove(acc);
                MessageBox.Show("Счёт удален!");
            }
        }

        private void TransferMoneyClick(object sender, RoutedEventArgs e)
        {
            if (lbAccounts.SelectedIndex !=-1)
            {
                DepositeAccount sourceDepAcc;
                NonDepositeAccount sourceNonDepAcc;
                
                IDecreaseMoney<BankAccount> moneyDecrease;
                IIncreaseMoney<BankAccount> moneyIncrease;
                TransferMoneyWindow transferMoneyWindow = new TransferMoneyWindow();
                transferMoneyWindow.cbTargetClient.ItemsSource = bankWorker.Clients;
                transferMoneyWindow.ShowDialog();
                if (transferMoneyWindow.DialogResult == true)
                {
                    int amount = transferMoneyWindow.amount;

                    moneyDecrease = new Bank<BankAccount>();
                    moneyIncrease = new Bank<DepositeAccount>();
                    moneyDecrease.DecreaseMoney(lbAccounts.SelectedItem as BankAccount, amount);
                    moneyIncrease.IncreaseMoney(amount, transferMoneyWindow.cbTargetAcc.SelectedItem as BankAccount);

                   
                } 
            }
        }

        private void IncreaseMoneyClick(object sender, RoutedEventArgs e)
        {

            if (lbAccounts.SelectedIndex!=-1)
            {
                IIncreaseMoney<BankAccount> moneyIncrease;
                moneyIncrease = new Bank<DepositeAccount>();
                moneyIncrease.IncreaseMoney(1000,lbAccounts.SelectedItem as BankAccount);
            }
        }



        protected override void OnClosed(EventArgs e)
        {
            bankWorker.SaveToFile();
            base.OnClosed(e);
        }
    }
}
