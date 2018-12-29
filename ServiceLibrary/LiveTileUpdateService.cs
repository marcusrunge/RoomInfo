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
    public interface ILiveTileUpdateService
    {
        Task<AgendaItem> GetActiveAgendaItem();
        TileContent CreateTile(AgendaItem agendaItem);
        void UpdateTile(TileContent tileContent);
    }
    public class LiveTileUpdateService : ILiveTileUpdateService
    {
        IDatabaseService _databaseService;
        IApplicationDataService _applicationDataService;
        public LiveTileUpdateService(IDatabaseService databaseService, IApplicationDataService applicationDataService)
        {
            _databaseService = databaseService;
            _applicationDataService = applicationDataService;
        }
        public async Task<AgendaItem> GetActiveAgendaItem()
        {
            AgendaItem agendaItem = new AgendaItem()
            {
                Occupancy = _applicationDataService.GetSetting<int>("StandardOccupancy")
            };
            var now = DateTime.Now;
            List<AgendaItem> agendaItems = await _databaseService.GetAgendaItemsAsync(now);
            var result = agendaItems.Where(x => now > x.Start && now < x.End).Select(x => x).FirstOrDefault();
            if (result != null) return result;
            else return agendaItem;
        }

        public TileContent CreateTile(AgendaItem agendaItem)
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse();
            string occupancyIcon = "-";
            string occupancyText = "-";
            string tileBranding = _applicationDataService.GetSetting<string>("RoomName") + " " + _applicationDataService.GetSetting<string>("RoomNumber");
            string timeWindow = agendaItem.Start.TimeOfDay.ToString(@"hh\:mm") + " - " + agendaItem.End.TimeOfDay.ToString(@"hh\:mm");
            TileContent tileContent;
            switch (_applicationDataService.GetSetting<bool>("OccupancyOverridden") ? _applicationDataService.GetSetting<int>("OverriddenOccupancy") : agendaItem.Occupancy)
            {
                case 0:
                    occupancyIcon = "✓";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockFree/Text");
                    break;
                case 1:
                    occupancyIcon = "⧖";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockBusy/Text");
                    break;
                case 2:
                    occupancyIcon = "🗙";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockOccupied/Text");
                    break;
                case 3:
                    occupancyIcon = "∞";
                    occupancyText = resourceLoader.GetString("Info_OccupancyTextBlockAbsent/Text");
                    break;
                default:
                    break;
            }

            if (agendaItem.Id < 1)
            {
                tileContent = new TileContent()
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
                            DisplayName = tileBranding,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = occupancyText,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = agendaItem.Title,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = tileBranding,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = occupancyText
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = tileBranding,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = occupancyText
                                    }
                                }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
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
                            DisplayName = tileBranding,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = occupancyText,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = agendaItem.Title,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = agendaItem.Start.TimeOfDay.ToString(@"hh\:mm"),
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = tileBranding,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = occupancyText
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = agendaItem.Title,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = timeWindow,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = tileBranding,
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = occupancyText
                                    },
                                        new AdaptiveText()
                                    {
                                        Text = agendaItem.Title,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = timeWindow,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    }
                };
            }
            return tileContent;
        }

        public void UpdateTile(TileContent tileContent)
        {
            var tileNotification = new TileNotification(tileContent.GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }
}