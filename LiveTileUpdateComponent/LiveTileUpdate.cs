using Microsoft.Toolkit.Uwp.Notifications;
using ModelComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Notifications;

namespace LiveTileUpdateComponent
{
    public sealed class LiveTileUpdate
    {
        public static async Task<AgendaItem> GetActiveAgendaItem()
        {
            return new AgendaItem();
        }

        private static TileContent CreateTile(AgendaItem agendaItem)
        {
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
                        Text = "Mon",
                        HintAlign = AdaptiveTextAlign.Center
                    },
                    new AdaptiveText()
                    {
                        Text = "22",
                        HintStyle = AdaptiveTextStyle.Body,
                        HintAlign = AdaptiveTextAlign.Center
                    }
                }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        DisplayName = "Monday 22",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Snowboarding with Mark",
                        HintWrap = true,
                        HintMaxLines = 2
                    },
                    new AdaptiveText()
                    {
                        Text = "Fri: 9:00 AM",
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Monday 22",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Snowboarding with Mark"
                    },
                    new AdaptiveText()
                    {
                        Text = "Mt. Baker",
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },
                    new AdaptiveText()
                    {
                        Text = "Tomorrow: 9:00 AM – 5:00 PM",
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Monday 22",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = "Snowboarding with Mark",
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = "Mt. Baker",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = "Tomorrow: 9:00 AM – 5:00 PM",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveText()
                    {
                        Text = ""
                    },
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = "Casper Baby Pants",
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = "Hollywood Bowl",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = "Tomorrow: 8:00 PM – 11:00 PM",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
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