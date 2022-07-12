﻿using CommunityToolkit.WinUI.Notifications;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Xunkong.Hoyolab.DailyNote;

namespace Xunkong.Desktop.Helpers;

internal static class SecondaryTileProvider
{

    private static readonly string BasePath = "ms-appx:///Assets/Images/";
    private static readonly string Commission_64 = "UI_NPCTopIcon_QuestEvent_64.png";
    private static readonly string Domain_64 = "Domain_64.png";
    private static readonly string Resin_64 = "UI_ItemIcon_210.png";
    private static readonly string HomeCoin_64 = "UI_ItemIcon_204.png";
    private static readonly string CondensedResin_64 = "UI_ItemIcon_211.png";
    private static readonly string Transparent = "Transparent.png";




    public static async Task<List<string>> FindAllAsync()
    {
        var tiles = await SecondaryTile.FindAllAsync();
        return tiles.Select(x => x.TileId).ToList();
    }



    public static async Task<bool> RequestPinTileAsync(DailyNoteInfo info)
    {
        SecondaryTile tile = new SecondaryTile($"DailyNote_{info.Uid}",
                                               "寻空",
                                               "dailynote",
                                               new Uri("ms-appx:///Images/Square150x150Logo.png"),
                                               Windows.UI.StartScreen.TileSize.Wide310x150);
        tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Images/Square44x44Logo.png");
        tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Images/Square71x71Logo.png");
        tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Images/Square150x150Logo.png");
        tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Images/Square310x150Logo.png");
        tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Images/Square310x310Logo.png");

        // Pin the tile
        bool isPinned = await tile.RequestCreateAsync();

        if (isPinned)
        {
            UpdatePinnedTile(info);
        }

        return isPinned;
    }



    public static async Task<bool> RequestUnpinTileAsync(DailyNoteInfo info)
    {
        SecondaryTile tile = new SecondaryTile($"DailyNote_{info.Uid}");
        return await tile.RequestDeleteAsync();
    }

    public static async Task<bool> RequestUnpinTileAsync(int uid)
    {
        SecondaryTile tile = new SecondaryTile($"DailyNote_{uid}");
        return await tile.RequestDeleteAsync();
    }


    private static void UpdatePinnedTile(DailyNoteInfo info)
    {
        info = info.Adapt<DailyNoteInfo>();
        if (info.Expeditions.Count < 5)
        {
            do
            {
                info.Expeditions.Add(new Expedition { AvatarSideIcon = Transparent });
            } while (info.Expeditions.Count != 5);
        }
        var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile($"DailyNote_{info.Uid}");
        if (updater != null)
        {
            var content = GetDailyNoteTileContent(info);
            var xml = content.GetXml();
            var notification = new TileNotification(content.GetXml());
            updater.Update(notification);
        }
    }



    public static async Task<bool> RegisterTaskAsync()
    {
        var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
        if (requestStatus == BackgroundAccessStatus.AlwaysAllowed || requestStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
        {
            UnregisterTask();
            var builder = new BackgroundTaskBuilder();
            builder.Name = "DailyNoteTask";
            builder.TaskEntryPoint = "Xunkong.Desktop.Background.DailyNoteTask";
            builder.SetTrigger(new TimeTrigger(16, false));
            BackgroundTaskRegistration task = builder.Register();
            return true;
        }
        else
        {
            return false;
        }
    }


    public static void UnregisterTask()
    {
        var allTasks = BackgroundTaskRegistration.AllTasks;
        foreach (var item in allTasks)
        {
            if (item.Value.Name == "DailyNoteTask")
            {
                item.Value.Unregister(true);
            }
        }
    }




    private static TileContent GetDailyNoteTileContent(DailyNoteInfo info)
    {
        return new TileContent
        {
            Visual = new TileVisual
            {
                BaseUri = new Uri(BasePath),
                // 小磁贴
                TileSmall = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                            {
                                new AdaptiveImage
                                {
                                    Source = Resin_64,
                                    HintAlign = AdaptiveImageAlign.Center,
                                    HintRemoveMargin = true,
                                },
                                new AdaptiveText
                                {
                                    Text = info.CurrentResin.ToString(),
                                    HintAlign = AdaptiveTextAlign.Center,
                                }
                            }
                    }
                },
                // 中磁贴
                TileMedium = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                            {
                                // 个人信息
                                new AdaptiveText
                                {
                                    Text = info.Nickname,
                                    HintAlign =AdaptiveTextAlign.Left
                                },
                                new AdaptiveText
                                {
                                    Text = $"更新于 {DateTime.Now:HH:mm}",
                                    HintAlign =AdaptiveTextAlign.Left
                                },
                                new AdaptiveText(),
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        // 树脂
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = Resin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.CurrentResin.ToString(),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        // 树脂恢复时间
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = CondensedResin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.ResinFullTime.ToString("HH:mm"),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        // 委托
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight = 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveImage
                                        //        {
                                        //            Source = Commission_64,
                                        //            HintAlign = AdaptiveImageAlign.Stretch,
                                        //            HintRemoveMargin = true,
                                        //        },
                                        //        new AdaptiveText
                                        //        {
                                        //            Text = info.IsExtraTaskRewardReceived? "-" : $"{info.FinishedTaskNumber}/{info.TotalTaskNumber}",
                                        //            HintAlign = AdaptiveTextAlign.Center,
                                        //        }
                                        //    }
                                        //},
                                        // 周本
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight = 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveImage
                                        //        {
                                        //            Source = Domain_64,
                                        //            HintAlign = AdaptiveImageAlign.Stretch,
                                        //            HintRemoveMargin = true,
                                        //        },
                                        //        new AdaptiveText
                                        //        {
                                        //            Text = $"{info.RemainResinDiscountNumber}/{info.ResinDiscountLimitedNumber}",
                                        //            HintAlign = AdaptiveTextAlign.Center,
                                        //        }
                                        //    }
                                        //},
                                        // 洞天宝钱
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = HomeCoin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = $"{info.CurrentHomeCoin}",
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        }
                                    }
                                },
                            }
                    }
                },
                // 宽磁贴
                TileWide = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                            {
                                // 个人信息
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = info.Nickname,
                                                    HintAlign= AdaptiveTextAlign.Left,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text=$"更新于 {DateTimeOffset.Now:HH:mm}",
                                                     HintAlign= AdaptiveTextAlign.Right,
                                                }
                                            }
                                        }
                                    }
                                },
                                // 第二行
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        // 树脂
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = Resin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text =$"{info.CurrentResin}",
                                                    HintAlign = AdaptiveTextAlign.Left,
                                                },
                                            }
                                        },
                                        // 树脂恢复时间
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight= 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = CondensedResin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight= 3,
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = info.ResinFullTime.ToString("HH:mm"),
                                                    HintAlign = AdaptiveTextAlign.Left,
                                                },
                                            }
                                        },
                                        // 委托
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight= 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveImage
                                        //        {
                                        //            Source = Commission_64,
                                        //            HintAlign = AdaptiveImageAlign.Stretch,
                                        //            HintRemoveMargin = true,
                                        //        },
                                        //    }
                                        //},
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight= 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveText
                                        //        {
                                        //            Text = info.IsExtraTaskRewardReceived? "-" : $"{info.FinishedTaskNumber}/{info.TotalTaskNumber}",
                                        //            HintAlign = AdaptiveTextAlign.Left,
                                        //        },
                                        //    }
                                        //},
                                        // 周本
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight = 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveImage
                                        //        {
                                        //            Source = Domain_64,
                                        //            HintAlign = AdaptiveImageAlign.Stretch,
                                        //            HintRemoveMargin = true,
                                        //        },
                                        //    }
                                        //},
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight = 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveText
                                        //        {
                                        //            Text = $"{info.RemainResinDiscountNum}/{info.ResinDiscountNumLimit}",
                                        //            HintAlign = AdaptiveTextAlign.Left,
                                        //        }
                                        //    }
                                        //},
                                        // 洞天宝钱
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight= 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = HomeCoin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight= 3,
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = info.CurrentHomeCoin.ToString(),
                                                    HintAlign = AdaptiveTextAlign.Left,
                                                },
                                            }
                                        },
                                    }
                                },
                                // 派遣
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[0].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[0].Status == null ? "" :(info.Expeditions[0].IsFinished ? "-" : info.Expeditions[0].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[1].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[1].Status == null ? "" :(info.Expeditions[1].IsFinished ? "-" : info.Expeditions[1].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[2].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[2].Status == null ? "" :(info.Expeditions[2].IsFinished ? "-" : info.Expeditions[2].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[3].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[3].Status == null ? "" :(info.Expeditions[3].IsFinished ? "-" : info.Expeditions[3].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[4].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[4].Status == null ? "" :(info.Expeditions[4].IsFinished ? "-" : info.Expeditions[4].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                    }
                },
                // 大磁贴
                TileLarge = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                            {
                                // 个人信息
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = info.Nickname,
                                                    HintAlign= AdaptiveTextAlign.Left,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text=$"更新于 {DateTimeOffset.Now:HH:mm}",
                                                     HintAlign= AdaptiveTextAlign.Right,
                                                }
                                            }
                                        }
                                    }
                                },
                                new AdaptiveText(),
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        // 树脂
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = Resin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = false,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text =$"{info.CurrentResin}",
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                },
                                            }
                                        },
                                        // 树脂恢复时间
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = CondensedResin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = false,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.ResinFullTime.ToString("HH:mm"),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                },
                                            }
                                        },
                                        // 委托
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight= 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = Commission_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = false,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.IsExtraTaskRewardReceived? "-" : $"{info.FinishedTaskNumber}/{info.TotalTaskNumber}",
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                },
                                            }
                                        },
                                        // 周本
                                        //new AdaptiveSubgroup
                                        //{
                                        //    HintWeight = 2,
                                        //    Children =
                                        //    {
                                        //        new AdaptiveImage
                                        //        {
                                        //            Source = Domain_64,
                                        //            HintAlign = AdaptiveImageAlign.Stretch,
                                        //            HintRemoveMargin = false,
                                        //        },
                                        //        new AdaptiveText
                                        //        {
                                        //            Text = $"{info.RemainResinDiscountNumber}/{info.ResinDiscountLimitedNumber}",
                                        //            HintAlign = AdaptiveTextAlign.Center,
                                        //        }
                                        //    }
                                        //},
                                        // 洞天宝钱
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = HomeCoin_64,
                                                    HintAlign = AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = false,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = $"{info.CurrentHomeCoin}",
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        }
                                    }
                                },
                                new AdaptiveText(),
                                // 派遣
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[0].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[0].Status == null ? "" :(info.Expeditions[0].IsFinished ? "-" : info.Expeditions[0].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[1].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[1].Status == null ? "" :(info.Expeditions[1].IsFinished ? "-" : info.Expeditions[1].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[2].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[2].Status == null ? "" :(info.Expeditions[2].IsFinished ? "-" : info.Expeditions[2].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[3].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[3].Status == null ? "" :(info.Expeditions[3].IsFinished ? "-" : info.Expeditions[3].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = info.Expeditions[4].AvatarSideIcon,
                                                    HintAlign= AdaptiveImageAlign.Stretch,
                                                    HintRemoveMargin = true,
                                                },
                                                new AdaptiveText
                                                {
                                                    Text = info.Expeditions[4].Status == null ? "" :(info.Expeditions[4].IsFinished ? "-" : info.Expeditions[4].FinishedTime.ToString("HH:mm")),
                                                    HintAlign = AdaptiveTextAlign.Center,
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                    }
                },
            }
        };
    }




}
