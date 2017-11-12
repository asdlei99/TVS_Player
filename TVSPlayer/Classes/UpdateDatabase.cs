﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TVS.API;

namespace TVSPlayer
{
    class UpdateDatabase {
        public async static Task Update() {
            if (Settings.LastCheck.AddDays(1) < DateTime.Now) { 
                await Task.Run( () => {
                    List<int> ids = Series.GetUpdates(Settings.LastCheck);
                    List<Series> series = Database.GetSeries();
                    ids = ids.Where(x => series.Any(y => y.id == x)).ToList();
                    foreach (int id in ids) {
                        UpdateFullSeries(id);
                    }
                });
                Settings.LastCheck = DateTime.Now;
            }
        }

        public async static void StartUpdateBackground() {
            await Update();
            Timer timer = new Timer(3600000);
            timer.Elapsed += (s, ev) => Update();
            timer.Start();
        }

        private static void UpdateFullSeries(int id) {
            Task.Run(() => {
                UpdateSeries(id);
                UpdateEpisodes(id);
                UpdateEpisodes(id);
                UpdateActors(id);
            });

        }

        private static void UpdateSeries(int id) {
            Task.Run(() => {
                var series = Database.GetSeries(id);
                var newseries = Series.GetSeries(id);
                if (series.Compare(newseries)) {
                    series.Update(newseries);
                    Database.EditSeries(id, series);
                }
            });
        }

        private static void UpdateEpisodes(int id) {
            Task.Run(() => {
                var list = Episode.GetEpisodes(id);
                foreach (var episode in list) {
                    var ep = Database.GetEpisode(id, episode.id);
                    if (ep != null) {
                        if (ep.Compare(episode)) {
                            ep.Update(episode);
                            Database.EditEpisode(id, ep.id, ep);
                        }
                    } else {
                        Database.AddEpisode(id, episode);
                    }
                }
            });
        }

        private static void UpdateActors(int id) {
            Task.Run(() => {
                var list = Actor.GetActors(id);
                foreach (var actor in list) {
                    var ac = Database.GetActor(id, actor.id);
                    if (ac != null) {
                        if (ac.Compare(actor)) {
                            ac.Update(actor);
                            Database.EditActor(id, ac.id, ac);
                        }
                    } else {
                        Database.AddActor(id, actor);
                    }
                }

            });
        }

        private static void UpdatePosters(int id) {
            Task.Run(() => {
                var list = Poster.GetPosters(id);
                foreach (var poster in list) {
                    var po = Database.GetPoster(id,poster.id);
                    if (po != null) {
                        if (po.Compare(poster)) {
                            po.Update(poster);
                            Database.EditPoster(id, po.id, po);
                        }
                    } else {
                        Database.AddPoster(id, poster);
                    }
                }

            });
        }

    }
}