using System.Collections.Immutable;
using System.Diagnostics;
using ArknightsResources.Utility;
using OperatorImageViewer.Models;
using Windows.Storage.Streams;

namespace OperatorImageViewer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        //Key:Image codename; Value:Name
        public readonly ImmutableDictionary<string, string> OperatorImageMapping;

        //Key:Operator codename; Value:Skin Codename List
        public readonly ImmutableDictionary<string, string[]> OperatorSkinCodenameMapping;

        private readonly OperatorIllustResourceHelper operatorIllustResourceHelper = new(OperatorIllustRes.ResourceManager);

        public MainWindowViewModel()
        {
            OperatorTextResourceHelper textResourceHelper = new(OperatorTextRes.ResourceManager);
            OperatorImageMapping = textResourceHelper.GetOperatorCodenameMapping(AvailableCultureInfos.ChineseSimplifiedCultureInfo)
                ?? new Dictionary<string, string>(0).ToImmutableDictionary();
            OperatorSkinCodenameMapping = textResourceHelper.GetOperatorSkinMapping();
        }

        [ObservableProperty]
        private BitmapImage operatorImage = new();

        [ObservableProperty]
        private OperatorType currentOperatorType;

        [ObservableProperty]
        private bool? useCodename = true;

        [ObservableProperty]
        private bool isLoadingImage;

        [ObservableProperty]
        private string infoBarMessage;

        [ObservableProperty]
        private string infoBarTitle;

        [ObservableProperty]
        private bool infoBarOpen;

        [ObservableProperty]
        private InfoBarSeverity infoBarSeverity;

        [ObservableProperty]
        private string skinCodename;

        public List<OperatorType> OperatorTypes { get; } = new()
        {
            OperatorType.Elite0,
            OperatorType.Elite1,
            OperatorType.Elite2,
            OperatorType.Skin
        };

        public async Task SetOperatorImageAsync(OperatorCodenameInfo chosenSuggestion)
        {
            await SetOperatorImageAsync(chosenSuggestion.Codename);
        }

        public async Task SetOperatorImageAsync(string queryText)
        {
            IsLoadingImage = true;
            ResetInfoBar();
            string operatorCodeName;
            switch (CurrentOperatorType)
            {
                case OperatorType.Elite0:
                    operatorCodeName = $"{queryText}_1";
                    break;
                case OperatorType.Elite1:
                    if (queryText == "amiya")
                    {
                        operatorCodeName = $"{queryText}_1+";
                    }
                    else
                    {
                        operatorCodeName = $"{queryText}_1";
                    }
                    break;
                case OperatorType.Elite2:
                    operatorCodeName = $"{queryText}_2";
                    break;
                case OperatorType.Skin:
                    if (string.IsNullOrWhiteSpace(skinCodename))
                    {
                        SetInfoBar(true,"注意","请输入皮肤代号",InfoBarSeverity.Warning);
                        IsLoadingImage = false;
                        return;
                    }
                    operatorCodeName = $"{queryText}_{skinCodename}";
                    break;
                default:
                    operatorCodeName = queryText;
                    break;
            }

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                byte[] image = await operatorIllustResourceHelper.GetOperatorIllustrationAsync(new OperatorIllustrationInfo(string.Empty, string.Empty, operatorCodeName, CurrentOperatorType, string.Empty));
                InMemoryRandomAccessStream stream = new();
                await stream.WriteAsync(image.AsBuffer());
                stream.Seek(0);
                await OperatorImage.SetSourceAsync(stream);
                stopwatch.Stop();
                SetInfoBar(true, "报告", $@"花费时间:{stopwatch.Elapsed:s\.ff}秒", InfoBarSeverity.Informational);
            }
            catch (ArgumentException)
            {
                SetInfoBar(true, "出现问题", "无效的干员代号或干员代号类型。", InfoBarSeverity.Error);
            }
            catch (Exception ex)
            {
                SetInfoBar(true, "出现未知问题", $"错误信息:{ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsLoadingImage = false;
                //GC.Collect();
            }
        }

        private void SetInfoBar(bool isOpen, string title, string message, InfoBarSeverity severity)
        {
            InfoBarOpen = isOpen;
            InfoBarTitle = title;
            InfoBarMessage = message;
            InfoBarSeverity = severity;
        }

        private void ResetInfoBar()
        {
            InfoBarMessage = string.Empty;
            InfoBarOpen = false;
            InfoBarTitle = string.Empty;
            InfoBarSeverity = InfoBarSeverity.Informational;
        }

        public IList<OperatorCodenameInfo> FindOperatorCodename(string text)
        {
            List<OperatorCodenameInfo> target = new(20);
            foreach (var item in OperatorImageMapping)
            {
                if (useCodename == true)
                {
                    if (item.Key.FirstOrDefault() == text.FirstOrDefault() && item.Key.Contains(text))
                    {
                        target.Add(new OperatorCodenameInfo(item.Key, item.Value));
                    }
                }
                else
                {
                    if (item.Value.Contains(text) && item.Value.FirstOrDefault() == text.FirstOrDefault())
                    {
                        target.Add(new OperatorCodenameInfo(item.Key, item.Value));
                    }
                }
            }

            return target;
        }

        public static Visibility CheckIfShowSkinCodenameAutoSuggestBox(OperatorType type) => type switch
        {
            OperatorType.Skin => Visibility.Visible,
            _ => Visibility.Collapsed
        };

        public IList<string> FindOperatorSkinCodename(string opName)
        {
            List<string> target = new(5);
            foreach (var item in OperatorSkinCodenameMapping)
            {
                if (useCodename == true)
                {
                    if (item.Key == opName)
                    {
                        foreach (var val in item.Value)
                        {
                            target.Add(val);
                        }
                    }
                }
                else
                {
                    if (OperatorImageMapping.ContainsValue(opName) is not true)
                    {
                        break;
                    }

                    string opCodename = string.Empty;

                    foreach (var itemCodename in OperatorImageMapping)
                    {
                        if (itemCodename.Value == opName)
                        {
                            opCodename = itemCodename.Key;
                            break;
                        }
                    }

                    if (item.Key == opCodename)
                    {
                        foreach (var val in item.Value)
                        {
                            target.Add(val);
                        }
                    }
                }
            }

            return target;
        }

        public static string GetOperatorTypeString(OperatorType type)
        {
            switch (type)
            {
                case OperatorType.Elite0:
                    return "精零";
                case OperatorType.Elite1:
                    return "精一";
                case OperatorType.Elite2:
                    return "精二";
                case OperatorType.Skin:
                    return "皮肤";
                default:
                    goto default;
            }
        }

        internal static bool ReverseBoolean(bool value) => !value;
    }
}
