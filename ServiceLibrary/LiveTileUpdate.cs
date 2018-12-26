using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp.Notifications;
using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;

namespace ServiceLibrary
{
    public sealed class LiveTileUpdate
    {
        public static async Task<AgendaItem> GetActiveAgendaItem()
        {
            IDatabaseService databaseService = ServiceLocator.Current.GetInstance<IDatabaseService>();
            IApplicationDataService applicationDataService = ServiceLocator.Current.GetInstance<IApplicationDataService>();
            AgendaItem agendaItem = new AgendaItem()
            {
                Occupancy = applicationDataService.GetSetting<int>("StandardOccupancy")
            };
            var now = DateTime.Now;
            List<AgendaItem> agendaItems = await databaseService.GetAgendaItemsAsync(now);
            var result = agendaItems.Where(x => now > x.Start && now < x.End).Select(x => x).FirstOrDefault();
            if (result != null) return result;
            else return agendaItem;
        }

        private static TileContent CreateTile(AgendaItem agendaItem)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            string occupancyIcon = "-";
            string occupancyText = "-";
            switch (agendaItem.Occupancy)
            {
                case 0:
                    occupancyIcon = "✓";
                    occupancyText = resourceLoader.GetString("Info_OccupancyFree");
                    break;
                case 1:
                    occupancyIcon = "⧖";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockBusy");
                    break;
                case 2:
                    occupancyIcon = "🗙";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockOccupied");
                    break;
                case 3:
                    occupancyIcon = "∞";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockAbsent");
                    break;
                default:
                    break;
            }
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = occupancyIcon,
                                    HintStyle = AdaptiveTextStyle.Body,
                                    HintAlign = AdaptiveTextAlign.Center,
                                }
                            }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        DisplayName = occupancyText,
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = agendaItem.Title,
                                    HintWrap = true,
                                    HintMaxLines = 2
                                },
                                new AdaptiveText()
                                {
                                    Text = agendaItem.Start.TimeOfDay.ToString(),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = occupancyText,
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = agendaItem.Title
                                },
                                new AdaptiveText()
                                {
                                    Text = agendaItem.Start.TimeOfDay.ToString() + " - " + agendaItem.End.TimeOfDay.ToString(),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = occupancyText,
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = agendaItem.Title
                                },
                                new AdaptiveText()
                                {
                                    Text = agendaItem.Start.TimeOfDay.ToString() + " - " + agendaItem.End.TimeOfDay.ToString(),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    }
                }
            };
            return tileContent;
        }

        private static void UpdateTile(TileContent tileContent)
        {
            var tileNotification = new TileNotification(tileContent.GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }
}