using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace NokiaAutoWordComplete_WPF;

public partial class MainWindow : Window
{
    private bool isProcess;
    private bool isBack;

    public ObservableCollection<string> AutoComplete { get; set; }
    public List<string> Words { get; set; }
    

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        if (Words is null)
            Words = new();

        AutoComplete = new();

        if (File.Exists(@"..\..\..\WordsDictionary.json"))
        {
            var json = File.ReadAllText(@"..\..\..\WordsDictionary.json");

            Words = JsonSerializer.Deserialize<List<string>>(json)!;
        }
        else
            Words.Add("Hello");

    }

    private void Text_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txt.Text))
        {
            AutoComplete.Clear();
            return;
        }

        if (isProcess || isBack)
            return;

        Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                AutoComplete.Clear();

                foreach (var word in Words)
                {
                    if (word.ToLower().StartsWith(txt.Text.ToLower()))
                        AutoComplete.Add(word);
                }

                isProcess = true;
                if (AutoComplete.Count > 0)
                {
                    var FilledWord = AutoComplete[0];

                    var startIndex = txt.Text.Length;
                    var lenght = FilledWord.Length - txt.Text.Length;

                    txt.Text += FilledWord.Remove(0, startIndex);
                    txt.Select(startIndex, lenght);

                }
                isProcess = false;
            });
        });

    }

    private void Text_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key;

        bool isNumPad; // When I update nokia keyboard i shall use this

        if (key == Key.Back)
            isBack = true;
        else
            isBack = false;



        if (Enumerable.Range(74, 83).Contains(((int)key))) 
            isNumPad= true;
        else
            isNumPad = false;



        if (AutoComplete.Count != 0)
        {
            var index = AutoList.SelectedIndex;
            isProcess = true;

            switch (key)
            {
                case Key.Up:
                    if (index == 0)
                        index = AutoComplete.Count - 1;
                    else
                        index--;

                    txt.Text = AutoComplete[index];
                    AutoList.SelectedIndex = index;
                    break;

                case Key.Down:
                    if (index == AutoComplete.Count - 1)
                        index = 0;
                    else
                        index++;

                    txt.Text = AutoComplete[index];
                    AutoList.SelectedIndex = index;
                    break;


                case Key.Enter:
                    SelectWords();
                    break;

            }
            isProcess = false;
        }
    }

    private void Btn_Add_Click(object sender, RoutedEventArgs e)
    {
        if (!Words.Contains(txt.Text))
        {
            Words.Add(txt.Text);

            Task.Run(() =>
            {
                var jsonStr = JsonSerializer.Serialize(Words);

                File.WriteAllText(@"..\..\..\WordsDictionary.json", jsonStr);
              
            });
        }
    }



    private void SelectWords()
    {
        isProcess = true;
        if (!string.IsNullOrEmpty(txt.Text))
        {
            txt.Text = AutoList.SelectedItem as string;
            AutoComplete.Clear();
        }
        isProcess = false;
    }

    private void Btn_HangUp_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Application.Current.Shutdown();

    private void AutoList_MouseDoubleClick(object sender, MouseButtonEventArgs e)=> SelectWords();


    private void BtnSoon_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show("It will be updated soon !");

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (((int)e.Key) >=74 && ((int)e.Key) <=83)
            txt.Text+=((int)e.Key - 74).ToString();
    }
}
