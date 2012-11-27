using System;
using System.Linq;
using System.Windows.Input;
using Awful.Models;
using Awful.Data;
using KollaSoft;
using System.Threading;

namespace Awful.Commands
{
    public class ToggleFavoritesCommand : PropertyChangedBase, ICommand
    {
        public static event EventHandler FavoriteAdded;
        public static event EventHandler FavoriteRemoved;

        private string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; NotifyPropertyChangedAsync("Header"); }
        }

        public bool CanExecute(object parameter)
        {
            var forum = parameter as Models.SAForum;
            if (forum == null) return false;

            this.Header = forum.IsFavorite ? "remove from favorites" : "add to favorites";

            return forum != null;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var forum = parameter as Models.SAForum;
            if (forum == null) return;

            forum.IsFavorite = !forum.IsFavorite;
            if (forum.IsFavorite) { ToggleFavoritesCommand.FavoriteAdded.Fire(forum); }
            else { ToggleFavoritesCommand.FavoriteRemoved.Fire(forum); }
        }
    }
}
