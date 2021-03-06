﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for CurrentTorrents.xaml
    /// </summary>
    public partial class CurrentTorrents : Page {
        public CurrentTorrents() {
            InitializeComponent();
        }
        Dictionary<TorrentUserControl, TorrentDownloader> userControls = new Dictionary<TorrentUserControl, TorrentDownloader>();

        private void Panel_Loaded(object sender, RoutedEventArgs e) {
            Render();
        }
      
        private async void Render() {
            while (IsLoaded) {
                var list = TorrentDownloader.GetTorrents();
                if (list.Count > userControls.Count) {
                    var torrents = new List<Torrent>();
                    userControls.Values.ToList().ForEach(x => torrents.Add(x.TorrentSource));
                    List<TorrentDownloader> changes = list.Where(x => !torrents.Contains(x.TorrentSource)).ToList();
                    foreach (var item in changes) {
                        Dispatcher.Invoke(() => {
                            TorrentUserControl tcu = new TorrentUserControl(item, Panel);
                            tcu.Height = 65;
                            tcu.Opacity = 0;
                            tcu.Margin = new Thickness(10, 0, 10, 0);
                            Panel.Children.Add(tcu);
                            userControls.Add(tcu, item);
                            Storyboard sb = (Storyboard)FindResource("OpacityUp");
                            sb.Begin(tcu);
                        }, DispatcherPriority.Send);
                    }
                }
                if (list.Count < userControls.Count) {
                    var torrents = new List<Torrent>();
                    var secondList = new List<Torrent>();
                    list.ForEach(x => secondList.Add(x.TorrentSource));
                    userControls.Values.ToList().ForEach(x => torrents.Add(x.TorrentSource));
                    List<Torrent> changes = torrents.Except(secondList).ToList();
                    Dictionary<TorrentUserControl, TorrentDownloader> values = userControls.Where(x => changes.Contains(x.Value.TorrentSource)).ToDictionary(x => x.Key, x => x.Value);
                    foreach (var item in values) {
                        Storyboard sb = (Storyboard)FindResource("OpacityDown");
                        var clone = sb.Clone();
                        clone.Completed += (s, ev) => {
                            Dispatcher.Invoke(() => {
                                Panel.Children.Remove(item.Key);
                            });
                        };
                        Application.Current.Dispatcher.Invoke(() => {
                            clone.Begin(item.Key);
                            userControls.Remove(item.Key);
                        }, DispatcherPriority.Send);

                    }
                }
                await Task.Run(() => {
                    Thread.Sleep(250);
                });
            }

        }

    }
}
