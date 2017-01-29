﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;
using System.Web;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ManageShowList.xaml
    /// </summary>
    public partial class ManageShowList : Page {
        string location;
        public ManageShowList(string loc) {
            InitializeComponent();
            location = loc;
            readFolders();
        }
        List<string> subfolders = new List<String>();
        List<Shows> shows = new List<Shows>();
        public struct Shows {
            public string id;
            public string name;
            public Shows(string id, string name) : this() {
                this.id = id;
                this.name = name;
            }

        }
        private void readFolders() {
            subfolders = Directory.GetDirectories(location).ToList<string>();
            Action list;
            list = () => listFolders();
            Thread thread = new Thread(list.Invoke);
            thread.Name = "List shows";
            thread.Start();

        }
        private void addUI(string show, string folder,int index) {
            JObject jo = JObject.Parse(show);
            DBScanOption option = new DBScanOption();
            string name = jo["data"][0]["seriesName"].ToString();
            string id = jo["data"][0]["id"].ToString();
            option.showName.Text = name;
            option.showLocation.Text = folder;
            option.showInfo.Click += (s, e) => {              
                showInfo(Api.apiGet(Int32.Parse(id))); 
            };
            option.editShow.Click += (s, e) => {
                editShow(index,option);
            };
            option.removeShow.Click += (s, e) => {
                removeShow(s,e,index,option);
            };
            shows.Add(new Shows(id, name));
            panel.Children.Add(option);
        }

        private void listFolders() {
            int i = 0;
            foreach (string folder in subfolders) {           
                string show = Api.apiGet(Path.GetFileName(folder));
                if (show != null) {
                    i++;
                    Dispatcher.Invoke(new Action(() => {
                        addUI(show, folder,i);
                    }), DispatcherPriority.Send);
                }
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            List<int> id = new List<int>();
            List<string> locs = new List<string>();
            for (int i = 0; i < shows.Count(); i++){
                if (shows[i].id != null) { 
                    DatabaseAPI.addShowToDb(shows[i].id, shows[i].name, true);
                    id.Add(Int32.Parse(shows[i].id));
                }
            }
            foreach (DBScanOption fc in panel.Children) {
                locs.Add(fc.showLocation.Text);
            }
            Page page = new TVS_Player.Shows();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
            Renamer.RenameBatch(id,locs,DatabaseAPI.database.libraryLocation);
            ((MainWindow)main).SetFrameView(page);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
        private void showInfo(string specific) {
            Page showPage = new ShowInfoSmall(specific);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }
        private void removeShow(object sender, RoutedEventArgs e, int index, DBScanOption option) {
            shows[index - 1] = new Shows(null, null);
            panel.Children.Remove(option);
        }
        private async void editShow(int index, DBScanOption option) {
            Page showPage = new SelectShow();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
            var show = await Helpers.showSelector();
            string name = show.Item2;
            string id = show.Item1;
            option.showName.Text = name;
            shows[index-1] = new Shows(id, name);

        }

    }
}
