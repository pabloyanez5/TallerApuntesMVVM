using CommunityToolkit.Mvvm.Input;
using PabloYanezApuntes.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PabloYanezApuntes.ViewModels;

internal class PYanezNotesViewModel : IQueryAttributable
{
    public ObservableCollection<ViewModels.PYanezNoteViewModel> AllNotes { get; }
    public ICommand NewCommand { get; }
    public ICommand SelectNoteCommand { get; }

    public PYanezNotesViewModel()
    {
        AllNotes = new ObservableCollection<ViewModels.PYanezNoteViewModel>(Models.PYanezNote.LoadAll().Select(n => new PYanezNoteViewModel(n)));
        NewCommand = new AsyncRelayCommand(NewNoteAsync);
        SelectNoteCommand = new AsyncRelayCommand<ViewModels.PYanezNoteViewModel>(SelectNoteAsync);
    }

    private async Task NewNoteAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.NotePage));
    }

    private async Task SelectNoteAsync(ViewModels.PYanezNoteViewModel note)
    {
        if (note != null)
            await Shell.Current.GoToAsync($"{nameof(Views.NotePage)}?load={note.Identifier}");
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("deleted"))
        {
            string noteId = query["deleted"].ToString();
            PYanezNoteViewModel matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

            // If note exists, delete it
            if (matchedNote != null)
                AllNotes.Remove(matchedNote);
        }
        else if (query.ContainsKey("saved"))
        {
            string noteId = query["saved"].ToString();
            PYanezNoteViewModel matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

            // If note is found, update it
            if (matchedNote != null)
            {
                matchedNote.Reload();
                AllNotes.Move(AllNotes.IndexOf(matchedNote), 0);
            }
            // If note isn't found, it's new; add it.
            else
                AllNotes.Insert(0, new PYanezNoteViewModel(Models.PYanezNote.Load(noteId)));
        }
    }
}
