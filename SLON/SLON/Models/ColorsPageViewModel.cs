using Plugin.Maui.SwipeCardView.Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SLON.Models;

public class ColorsPageViewModel : BasePageViewModel
{
    private ObservableCollection<string> _cardItems;

    public ColorsPageViewModel(ObservableCollection<string> Events_or_Users)
    {
        _cardItems = Events_or_Users;

        SwipedCommand = new Command<SwipedCardEventArgs>(OnSwipedCommand);
        DraggingCommand = new Command<DraggingCardEventArgs>(OnDraggingCommand);

        ClearItemsCommand = new Command(OnClearItemsCommand);
        AddItemsCommand = new Command(OnAddItemsCommand);
    }

    public ObservableCollection<string> CardItems
    {
        get => _cardItems;
        set
        {
            _cardItems = value;
            RaisePropertyChanged();
        }
    }

    public ICommand SwipedCommand { get; }

    public ICommand DraggingCommand { get; }

    public ICommand ClearItemsCommand { get; }

    public ICommand AddItemsCommand { get; }

    private void OnSwipedCommand(SwipedCardEventArgs eventArgs)
    {
    }

    private void OnDraggingCommand(DraggingCardEventArgs eventArgs)
    {
        switch (eventArgs.Position)
        {
            case DraggingCardPosition.Start:
                return;

            case DraggingCardPosition.UnderThreshold:
                break;

            case DraggingCardPosition.OverThreshold:
                break;

            case DraggingCardPosition.FinishedUnderThreshold:
                return;

            case DraggingCardPosition.FinishedOverThreshold:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnClearItemsCommand()
    {
        CardItems.Clear();
    }

    private void OnAddItemsCommand()
    {
        for (var i = 1; i <= 5; i++)
        {
            CardItems.Add($"Card {i}");
        }
    }
}